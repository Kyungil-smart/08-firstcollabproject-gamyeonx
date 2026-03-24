using UnityEngine;

/// <summary>
/// 가만히 대기하는 상태
/// 일정 시간이 지나면 Wander 상태로 이동
/// </summary>
public class GuestIdleState : IGuestState
{
    // 상태 전환 및 데이터 접근용 컨트롤러 참조
    private readonly GuestController _controller;

    // 현재 상태에서 경과 시간
    private float _elapsedTime;

    public GuestIdleState(GuestController controller)
    {
        _controller = controller;
    }

    public void Enter()
    {
        // 상태 진입 시 시간 초기화
        _elapsedTime = 0f;

        Debug.Log("[GuestIdleState] Enter");
    }

    public void Update()
    {
        // 매 프레임 시간 누적
        _elapsedTime += Time.deltaTime;

        // Idle 시간이 끝나면 Wander 상태로 전환
        if (_elapsedTime >= _controller.IdleDuration)
        {
            _controller.ChangeToWanderState();
        }
    }

    public void Exit()
    {
        Debug.Log("[GuestIdleState] Exit");
    }
}