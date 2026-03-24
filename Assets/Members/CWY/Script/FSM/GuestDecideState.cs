using UnityEngine;

/// <summary>
/// Utility AI를 실행해서 현재 가장 필요한 행동을 결정하는 상태
/// </summary>
public class GuestDecideState : IGuestState
{
    // 상태 전환 및 데이터 접근용 컨트롤러 참조
    private readonly GuestController _controller;

    public GuestDecideState(GuestController controller)
    {
        _controller = controller;
    }

    public void Enter()
    {
        Debug.Log("[GuestDecideState] Enter");

        // 현재 손님의 가장 급한 욕구와 목표 시설 타입을 계산
        _controller.EvaluateCurrentNeed();

        // 목표 시설 타입이 없으면 다시 Idle로 복귀
        if (_controller.CurrentTargetFacilityType == EFacilityType.None)
        {
            Debug.LogWarning("[GuestDecideState] No target facility type found.");
            _controller.ChangeToIdleState();
            return;
        }

        // ---------------------------------------------------
        // [시설 담당자 연결 지점]
        // 여기서 CurrentTargetFacilityType을 기준으로
        // 실제 목표 시설을 찾는 로직이 들어갈 예정
        //
        // 예:
        // _controller.FindTargetFacility();
        // ---------------------------------------------------

        // 지금은 목표 시설 타입만 결정하고 Move 상태로 이동
        _controller.ChangeToMoveState();
    }

    public void Update()
    {
        // Decide 상태는 진입 시 1회 판단만 하므로 Update는 비워둠
    }

    public void Exit()
    {
        Debug.Log("[GuestDecideState] Exit");
    }
}