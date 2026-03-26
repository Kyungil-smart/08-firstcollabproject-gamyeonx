using UnityEngine;

/// <summary>
/// 배회 상태
/// 2초마다 모든 Need가 1 증가하고,
/// 배회 중에만 시설 이용 이벤트 / 일반 퇴장 이벤트를 판정
/// </summary>
public class GuestWanderState : IGuestState
{
    private readonly GuestController _controller;

    private float _needTickTimer;
    private float _eventCheckTimer;

    public GuestWanderState(GuestController controller)
    {
        _controller = controller;
    }

    public void Enter()
    {
        _needTickTimer = 0f;
        _eventCheckTimer = 0f;

        Debug.Log("[GuestWanderState] Enter");
    }

    public void Update()
    {
        if (_controller.IsTurnEnding)
        {
            _controller.ChangeToExitState();
            return;
        }

        _needTickTimer += Time.deltaTime;
        _eventCheckTimer += Time.deltaTime;

        if (_needTickTimer >= _controller.WanderNeedTickInterval)
        {
            _needTickTimer -= _controller.WanderNeedTickInterval;
            _controller.ApplyWanderNeedTick();

            if (_controller.GuestStates.HasAnyNeedReachedMax())
            {
                _controller.ChangeToDecideState();
                return;
            }
        }

        if (_eventCheckTimer >= _controller.WanderEventCheckInterval)
        {
            _eventCheckTimer -= _controller.WanderEventCheckInterval;

            if (_controller.ShouldStartFacilitySearchNow())
            {
                _controller.ChangeToDecideState();
                return;
            }

            if (_controller.ShouldExitFromWander())
            {
                _controller.ChangeToExitState();
            }
        }
    }

    public void Exit()
    {
        Debug.Log("[GuestWanderState] Exit");
    }
}