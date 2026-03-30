using UnityEngine;

public class GuestWaitState : IGuestState
{
    // 상태 전환 및 데이터 접근용 컨트롤러 참조
    private readonly GuestController _controller;

    public GuestWaitState(GuestController controller)
    {
        _controller = controller;
    }

    public void Enter()
    {
        Debug.Log("[GuestWaitState] Enter");
    }

    public void Update()
    {

        if (_controller.IsTurnEnding)
        {
            _controller.ChangeToExitState();
            return;
        }

        if (_controller.HasFacilityUseFailed || _controller.HasMovementFailed)
        {
            _controller.ClearCurrentFacilityContext();
            _controller.ChangeToWanderState();
            return;
        }
        if (_controller.CanUseFacility)
        {
            _controller.ChangeToUseState();
            return;
        }
    }

    public void Exit()
    {
        Debug.Log("[GuestWaitState] Exit");
    }
}