using UnityEngine;

/// <summary>
/// 시설 앞에서 대기하는 상태
/// 시설 사용 가능해지면 Use 상태로 이동
/// </summary>
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
        // ---------------------------------------------------
        // [시설 담당자 연결 지점]
        // 시설이 사용 가능하면 Use 상태로 전환
        // 예:
        // if (_controller.CanUseFacility)
        // ---------------------------------------------------
        if (_controller.IsTurnEnding)
        {
            _controller.ChangeToExitState();
            return;
        }

        if (_controller.HasFacilityUseFailed || _controller.HasMovementFailed)
        {
            Debug.LogWarning("[GuestWaitState] 대기 중 실패 처리로 다시 배회");
            _controller.ClearCurrentFacilityContext();
            _controller.ChangeToWanderState();
            return;
        }
        if (_controller.CanUseFacility)
        {
            _controller.ChangeToUseState();
        }
    }

    public void Exit()
    {
        Debug.Log("[GuestWaitState] Exit");
    }
}