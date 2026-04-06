using UnityEngine;

public class GuestMoveState : IGuestState
{
    private readonly GuestController _controller;

    public GuestMoveState(GuestController controller)
    {
        _controller = controller;
    }

    public void Enter()
    {
        if (_controller.IsTurnEnding)
        {
            _controller.ChangeToExitState();
            return;
        }

        _controller.ResetMovementAndFacilityFlags();

        bool requestedMove = _controller.RequestMoveToFacilityEntrance();

        if (!requestedMove)
        {
            _controller.SetMovementFailed(true);
        }
    }

    public void Update()
    {
        if (_controller.IsTurnEnding)
        {
            _controller.ChangeToExitState();
            return;
        }

        if (_controller.HasMovementFailed || _controller.HasFacilityUseFailed)
        {
            _controller.ClearCurrentFacilityContext();
            _controller.ChangeToWanderState();
            return;
        }

        if (!_controller.HasArrivedAtFacility)
        {
            return;
        }

        if (_controller.CanUseFacility)
        {
            _controller.ChangeToUseState();
            return;
        }

        if (_controller.ShouldWaitForFacility)
        {
            _controller.ChangeToWaitState();
            return;
        }
    }

    public void Exit()
    {
        Debug.Log("[GuestMoveState] Exit");
    }
}