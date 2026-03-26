using UnityEngine;

/// <summary>
/// 퇴장 상태
/// 길드 입출구 방향으로 이동시키는 실제 코드는 나중에 연결
/// 현재는 즉시 퇴장 완료 처리
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

        _controller.ClearCurrentFacilityContext();

        // [퇴장 이동 시스템 연결 지점]
        // 나중에 출구까지 이동 후 CompleteExit() 호출하도록 변경 가능
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