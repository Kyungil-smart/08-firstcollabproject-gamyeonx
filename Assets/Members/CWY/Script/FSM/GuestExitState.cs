using UnityEngine;

/// <summary>
/// 퇴장 상태
/// 현재는 즉시 제거
/// </summary>
public class GuestExitState : IGuestState
{
    private readonly GuestController _controller;
    private bool _exitStarted;

    public GuestExitState(GuestController controller)
    {
        _controller = controller;
    }

    public void Enter()
    {
        _exitStarted = false;
        Debug.Log("[GuestExitState] Enter");

        if (_controller.IsInsideFacility)
        {
            _controller.ExitFacilityToOutside();
        }
    }

    public void Update()
    {
        if (_exitStarted)
        {
            return;
        }

        _exitStarted = true;
        _controller.CompleteExit();
    }

    public void Exit()
    {
        Debug.Log("[GuestExitState] Exit");
    }
}