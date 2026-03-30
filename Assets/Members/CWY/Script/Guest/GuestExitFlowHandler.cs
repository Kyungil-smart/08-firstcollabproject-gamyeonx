using UnityEngine;

/// <summary>
/// 손님의 퇴장 연출 흐름만 담당한다.
/// 1. 퇴장 이벤트 발생 시 GuildInnerExitPoint 위치를 기준으로 길 따라 이동
/// 2. 길드 안 출구 Trigger에 들어가면
/// 3. GuildExitPoint로 순간이동
/// 4. DespawnPoint까지 이동
/// 5. 도착 시 제거 요청
/// </summary>
[RequireComponent(typeof(GuestController))]
[RequireComponent(typeof(GuestMovementAgent))]
public class GuestExitFlowHandler : MonoBehaviour
{
    [Header("퇴장 연출 포인트")]
    [SerializeField] private Transform _guildInnerExitPoint;
    [SerializeField] private Transform _guildExitPoint;
    [SerializeField] private Transform _despawnPoint;

    [Header("그리드 참조")]
    [SerializeField] private Grid _grid;

    [Header("디버그")]
    [SerializeField] private bool _enableDebugLog = true;

    private GuestController _controller;
    private GuestMovementAgent _movementAgent;

    private bool _isExitRunning;
    private bool _isWaitingInnerExitTrigger;
    private bool _isMovingToDespawn;

    public bool IsExitRunning => _isExitRunning;
    public bool IsWaitingInnerExitTrigger => _isWaitingInnerExitTrigger;

    private void Awake()
    {
        _controller = GetComponent<GuestController>();
        _movementAgent = GetComponent<GuestMovementAgent>();
    }

    private void Update()
    {
        if (!_isExitRunning)
        {
            return;
        }

        if (_isMovingToDespawn && !_movementAgent.IsMoving)
        {
            _isExitRunning = false;
            _isMovingToDespawn = false;
            _isWaitingInnerExitTrigger = false;

            Log("[GuestExitFlowHandler] 디스폰 위치 도착 완료");
            _controller.HandleExitFlowCompleted();
        }
    }

    public void BeginExitFlow()
    {
        if (!ValidateExitPoints())
        {
            Debug.LogWarning("[GuestExitFlowHandler] 퇴장 포인트가 올바르지 않아 퇴장 흐름을 시작할 수 없습니다.");
            _controller.HandleExitFlowFailed();
            return;
        }

        _isExitRunning = true;
        _isWaitingInnerExitTrigger = true;
        _isMovingToDespawn = false;

        _movementAgent.StopMove();

        Vector3Int exitRoadCell = _grid.WorldToCell(_guildInnerExitPoint.position);
        bool requested = _movementAgent.MoveToRoadCell(exitRoadCell);

        if (!requested)
        {
            Debug.LogWarning($"[GuestExitFlowHandler] GuildInnerExitPoint 이동 실패 | Cell={exitRoadCell}");
            _controller.HandleExitFlowFailed();
            return;
        }

        Log($"[GuestExitFlowHandler] 퇴장 흐름 시작 | 길 따라 GuildInnerExitPoint로 이동 | Cell={exitRoadCell}");
    }

    public void NotifyEnteredGuildInnerExitTrigger()
    {
        if (!_isExitRunning)
        {
            return;
        }

        if (!_isWaitingInnerExitTrigger)
        {
            return;
        }

        _isWaitingInnerExitTrigger = false;

        _movementAgent.StopMove();
        _movementAgent.TeleportTo(_guildExitPoint);

        if (_despawnPoint != null)
        {
            _movementAgent.MoveInsideTo(_despawnPoint);
            _isMovingToDespawn = true;
        }
        else
        {
            _controller.HandleExitFlowCompleted();
        }

        Log("[GuestExitFlowHandler] 길드 안 출구 Trigger 진입 | GuildExitPoint로 순간이동");
    }

    private bool ValidateExitPoints()
    {
        return _guildInnerExitPoint != null
            && _guildExitPoint != null
            && _grid != null;
    }

    private void Log(string message)
    {
        if (_enableDebugLog)
        {
            Debug.Log(message);
        }
    }
}

/*
[Unity 구현 방법]
1. Guest 프리팹에 이 스크립트를 붙입니다.
2. _guildInnerExitPoint, _guildExitPoint, _despawnPoint를 직접 드래그해서 연결합니다.
3. _grid에는 씬의 Grid 오브젝트를 연결합니다.
4. 퇴장 이벤트가 발생하면 _guildInnerExitPoint.position을 Grid 셀로 변환해서
   MoveToRoadCell()로 길 따라 이동합니다.
5. 즉, 인스펙터에서는 위치를 직접 넣고, 내부적으로만 셀 좌표로 바꿔 쓰는 구조입니다.
*/