using UnityEngine;

/// <summary>
/// 목표 시설로 이동하는 상태
/// 실제 A* 이동은 나중에 연결
/// </summary>
public class GuestMoveState : IGuestState
{
    // 상태 전환 및 데이터 접근용 컨트롤러 참조
    private readonly GuestController _controller;

    public GuestMoveState(GuestController controller)
    {
        _controller = controller;
    }

    public void Enter()
    {
        Debug.Log("[GuestMoveState] Enter");

        // ---------------------------------------------------
        // [A* 담당자 연결 지점]
        // 여기서 다른 팀원이 만든 A* 이동 요청 코드를 호출하면 됨
        //
        // 예:
        // _controller.RequestMoveToTarget();
        // ---------------------------------------------------
    }

    public void Update()
    {
        // ---------------------------------------------------
        // [도착 체크 연결 지점]
        // A* 이동이 끝났는지 검사하고,
        // 도착했다면 Wait 상태로 전환
        // ---------------------------------------------------
        if (_controller.HasArrivedAtFacility)
        {
            _controller.ChangeToWaitState();
        }
    }

    public void Exit()
    {
        Debug.Log("[GuestMoveState] Exit");
    }
}