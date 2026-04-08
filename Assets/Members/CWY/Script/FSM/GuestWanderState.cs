using UnityEngine;

public class GuestWanderState : IGuestState
{
    private readonly GuestController _controller;

    private float _needTickTimer;
    private float _eventCheckTimer;
    private float _repathDelayTimer;

    public GuestWanderState(GuestController controller)
    {
        _controller = controller;
    }

    public void Enter()
    {
        _needTickTimer = 0f;
        _eventCheckTimer = 0f;
        _repathDelayTimer = 0f;

        Debug.Log("[GuestWanderState] Enter");

        TryStartRandomWanderMove();
    }

    public void Update()
    {
        if (_controller.IsTurnEnding)
        {
            _controller.ChangeToExitState();
            return;
        }

        if (_controller.UpdateStuckWatch())
        {
            _controller.ChangeToWanderState();
            return;
        }

        _needTickTimer += Time.deltaTime;
        _eventCheckTimer += Time.deltaTime;
        _repathDelayTimer += Time.deltaTime;

        if (_needTickTimer >= _controller.WanderNeedTickInterval)
        {
            _needTickTimer -= _controller.WanderNeedTickInterval;
            _controller.ApplyWanderNeedTick();

            if (_controller.GuestStates.HasAnyNeedReachedMax())
            {
                _controller.MovementAgent.StopMove();
                _controller.ChangeToDecideState();
                return;
            }
        }

        if (_eventCheckTimer >= _controller.WanderEventCheckInterval)
        {
            _eventCheckTimer -= _controller.WanderEventCheckInterval;

            if (_controller.ShouldStartFacilitySearchNow())
            {
                _controller.MovementAgent.StopMove();
                _controller.ChangeToDecideState();
                return;
            }

            if (_controller.ShouldExitFromWander())
            {
                _controller.MovementAgent.StopMove();
                _controller.ChangeToExitState();
                return;
            }
        }

        if (!_controller.MovementAgent.IsMoving && _repathDelayTimer >= 0.2f)
        {
            _repathDelayTimer = 0f;
            TryStartRandomWanderMove();
        }
    }

    public void Exit()
    {
        Debug.Log("[GuestWanderState] Exit");
    }

    private void TryStartRandomWanderMove()
    {
        bool started = _controller.RequestRandomWanderMove();

        if (!started)
        {
            Debug.Log("[GuestWanderState] 랜덤 배회 이동 시작 실패");
        }
    }
}