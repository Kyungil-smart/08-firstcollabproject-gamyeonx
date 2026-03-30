using UnityEngine;

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
        if(!_isExitRunning)
        {
            return;
        }

        if(_isMovingToDespawn && !_movementAgent.IsMoving)
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
        if(!ValidateExitPoints())
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

        if(!requested)
        {
            Debug.LogWarning($"[GuestExitFlowHandler] GuildInnerExitPoint 이동 실패 | Cell={exitRoadCell}");
            _controller.HandleExitFlowFailed();
            return;
        }

        Log($"[GuestExitFlowHandler] 퇴장 흐름 시작 | 길 따라 GuildInnerExitPoint로 이동 | Cell={exitRoadCell}");
    }

    public void NotifyEnteredGuildInnerExitTrigger()
    {
        if(!_isExitRunning)
        {
            return;
        }

        if(!_isWaitingInnerExitTrigger)
        {
            return;
        }

        _isWaitingInnerExitTrigger = false;

        _movementAgent.StopMove();
        _movementAgent.TeleportTo(_guildExitPoint);

        if(_despawnPoint != null)
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
        if(_enableDebugLog)
        {
            Debug.Log(message);
        }
    }
}
