using UnityEngine;

public class GuestUseState : IGuestState
{
    private readonly GuestController _controller;
    private float _effectTickTimer;
    private bool _startedUseEffect;

    public GuestUseState(GuestController controller)
    {
        _controller = controller;
    }

    public void Enter()
    {
        _effectTickTimer = 0f;
        _startedUseEffect = false;

        _controller.MoveToFacilityUsePoint();
    }

    public void Update()
    {
        if (_controller.IsTurnEnding && !_startedUseEffect)
        {
            _controller.ChangeToExitState();
            return;
        }

        if (!_startedUseEffect)
        {
            if (_controller.MovementAgent.IsMoving)
            {
                return;
            }

            _startedUseEffect = true;
        }

        _effectTickTimer += Time.deltaTime;

        if (_effectTickTimer < _controller.UseEffectTickInterval)
        {
            return;
        }

        _effectTickTimer -= _controller.UseEffectTickInterval;

        _controller.ApplyCurrentFacilityEffect();

        if (_controller.IsCurrentFacilityGoalReached())
        {
            _controller.FinishCurrentFacilityUse();
            _controller.ExitFacilityToOutside();

            if (_controller.IsTurnEnding)
            {
                _controller.ChangeToExitState();
            }
            else
            {
                _controller.ChangeToWanderState();
            }
        }
    }

    public void Exit()
    {
        Debug.Log("[GuestUseState] Exit");
    }
}