using UnityEngine;

public class GuestExitState : IGuestState
{
    private readonly GuestController _controller;
    private bool _startedExitProcess;

    public GuestExitState(GuestController controller)
    {
        _controller = controller;
    }

    public void Enter()
    {
        _startedExitProcess = false;
        Debug.Log("[GuestExitState] Enter");
    }

    public void Update()
    {
        if (_controller.UpdateStuckWatch())
        {
            _controller.ChangeToWanderState();
            return;
        }

        if (_startedExitProcess)
        {
            return;
        }

        _startedExitProcess = true;

        if (_controller.IsInsideFacility)
        {
            bool startedLeave = _controller.BeginFacilityLeave();

            if (!startedLeave)
            {
                _controller.StartGuildExitFlow();
            }

            return;
        }

        _controller.StartGuildExitFlow();
    }

    public void Exit()
    {
        Debug.Log("[GuestExitState] Exit");
    }
}