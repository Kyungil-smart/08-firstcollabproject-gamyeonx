using UnityEngine;

/// <summary>
/// 시설을 실제로 이용하는 상태
/// 이용 시간이 끝나면 시설 효과를 적용하고 Idle 상태로 돌아감
/// </summary>
public class GuestUseState : IGuestState
{
    // 상태 전환 및 데이터 접근용 컨트롤러 참조
    private readonly GuestController _controller;

    // 시설 이용 경과 시간
    private float _elapsedTime;

    public GuestUseState(GuestController controller)
    {
        _controller = controller;
    }

    public void Enter()
    {
        // 상태 진입 시 시간 초기화
        _elapsedTime = 0f;

        Debug.Log("[GuestUseState] Enter");
    }

    public void Update()
    {
        // 매 프레임 시간 누적
        _elapsedTime += Time.deltaTime;

        // 시설 이용 시간이 끝나면 효과 적용 후 Idle 상태로 이동
        if (_elapsedTime >= _controller.UseDuration)
        {
            // ---------------------------------------------------
            // [시설 효과 적용 연결 지점]
            // 여기서 현재 이용한 시설의 효과를 찾아서
            // GuestStates에 적용하면 됨
            //
            // 예:
            // _controller.ApplyCurrentFacilityEffect();
            // ---------------------------------------------------

            _controller.ChangeToIdleState();
        }
    }

    public void Exit()
    {
        Debug.Log("[GuestUseState] Exit");
    }
}