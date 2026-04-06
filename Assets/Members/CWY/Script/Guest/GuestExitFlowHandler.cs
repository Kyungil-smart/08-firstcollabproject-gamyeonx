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

            _controller.HandleExitFlowCompleted();
        }
    }

    public void BeginExitFlow()
    {
        if (!ValidateExitPoints())
        {
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
            _controller.HandleExitFlowFailed();
            return;
        }
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