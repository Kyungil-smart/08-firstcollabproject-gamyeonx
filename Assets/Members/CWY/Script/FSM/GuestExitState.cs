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
        if (_startedExitProcess)
        {
            return;
        }

        _startedExitProcess = true;

        // 시설 내부에 있으면 먼저 시설 내부 종료 흐름 처리
        if (_controller.IsInsideFacility)
        {
            bool startedLeave = _controller.BeginFacilityLeave();

            if (!startedLeave)
            {
                _controller.StartGuildExitFlow();
            }

            return;
        }

        // 시설 밖이면 길드 최종 퇴장 처리
        _controller.StartGuildExitFlow();
    }

    public void Exit()
    {
        Debug.Log("[GuestExitState] 퇴장");
    }
}