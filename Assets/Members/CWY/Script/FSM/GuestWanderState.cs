using UnityEngine;

/// <summary>
/// 배회 상태
/// 아직 실제 이동 구현은 없고, 일정 시간 대기 후 Decide 상태로 넘어감
/// </summary>
public class GuestWanderState : IGuestState
{
    // 상태 전환 및 데이터 접근용 컨트롤러 참조
    private readonly GuestController _controller;

    // 현재 상태에서 경과 시간
    private float _elapsedTime;

    public GuestWanderState(GuestController controller)
    {
        _controller = controller;
    }

    public void Enter()
    {
        // 상태 진입 시 시간 초기화
        _elapsedTime = 0f;

        Debug.Log("[GuestWanderState] Enter");
    }

    public void Update()
    {
        // 매 프레임 시간 누적
        _elapsedTime += Time.deltaTime;

        // Wander 시간이 끝나면 Decide 상태로 전환
        if (_elapsedTime >= _controller.WanderDuration)
        {
            _controller.ChangeToDecideState();
        }
    }

    public void Exit()
    {
        Debug.Log("[GuestWanderState] Exit");
    }
}