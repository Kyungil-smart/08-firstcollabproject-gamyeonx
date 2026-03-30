using UnityEngine;

/// <summary>
/// 손님의 입장 연출 흐름만 담당한다.
/// 1. SpawnPoint에서 시작
/// 2. GuildEntrancePoint까지 이동
/// 3. 길드 입구 Trigger에 들어가면
/// 4. GuildInnerEntrancePoint로 순간이동
/// 5. 도착하면 GuestController에 Wander 시작 요청
/// </summary>
[RequireComponent(typeof(GuestController))]
[RequireComponent(typeof(GuestMovementAgent))]
public class GuestEntryFlowHandler : MonoBehaviour
{
    [Header("입장 연출 포인트")]
    [SerializeField] private Transform _spawnPoint;
    [SerializeField] private Transform _guildEntrancePoint;
    [SerializeField] private Transform _guildInnerEntrancePoint;

    [Header("디버그")]
    [SerializeField] private bool _enableDebugLog = true;

    private GuestController _controller;
    private GuestMovementAgent _movementAgent;

    private bool _isEntryRunning;
    private bool _isWaitingEntranceTrigger;
    private bool _isMovingToEntrancePoint;

    public bool IsEntryRunning => _isEntryRunning;
    public bool IsWaitingEntranceTrigger => _isWaitingEntranceTrigger;

    private void Awake()
    {
        _controller = GetComponent<GuestController>();
        _movementAgent = GetComponent<GuestMovementAgent>();
    }

    public void BeginEntryFlow()
    {
        if (!ValidateEntryPoints())
        {
            Debug.LogWarning("[GuestEntryFlowHandler] 입장 포인트가 올바르지 않아 입장 흐름을 시작할 수 없습니다.");
            _controller.HandleEntryFlowFailed();
            return;
        }

        _isEntryRunning = true;
        _isWaitingEntranceTrigger = true;
        _isMovingToEntrancePoint = true;

        _movementAgent.StopMove();
        _movementAgent.TeleportTo(_spawnPoint);
        _movementAgent.MoveInsideTo(_guildEntrancePoint);

        Log("[GuestEntryFlowHandler] 입장 흐름 시작 | Spawn -> 길드 입구");
    }

    public void NotifyEnteredGuildEntranceTrigger()
    {
        if (!_isEntryRunning)
        {
            return;
        }

        if (!_isWaitingEntranceTrigger)
        {
            return;
        }

        _isWaitingEntranceTrigger = false;
        _isMovingToEntrancePoint = false;

        _movementAgent.StopMove();
        _movementAgent.TeleportTo(_guildInnerEntrancePoint);

        Log("[GuestEntryFlowHandler] 길드 입구 Trigger 진입 | 길드 안 입구로 순간이동");
        _controller.HandleEntryFlowCompleted();
    }

    private bool ValidateEntryPoints()
    {
        return _spawnPoint != null
            && _guildEntrancePoint != null
            && _guildInnerEntrancePoint != null;
    }

    private void Log(string message)
    {
        if (_enableDebugLog)
        {
            Debug.Log(message);
        }
    }
}