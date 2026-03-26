using UnityEngine;

/// <summary>
/// 시설을 이용하는 상태
/// 틱마다 시설 효과를 적용하고,
/// 타겟 컨디션이 0이 되면 이용 종료 후 배회 또는 퇴장으로 전환
/// </summary>
public class GuestUseState : IGuestState
{
    private readonly GuestController _controller;
    private float _effectTickTimer;

    public GuestUseState(GuestController controller)
    {
        _controller = controller;
    }

    public void Enter()
    {
        _effectTickTimer = 0f;

        Debug.Log("[GuestUseState] Enter");

        // [시설 점유 시작 / 애니메이션 시작 연결 지점]

        if (_controller.IsCurrentFacilityGoalReached())
        {
            Debug.Log("[GuestUseState] 진입 시점에 이미 목표 달성됨");
            _controller.FinishCurrentFacilityUse();

            if (_controller.IsTurnEnding)
            {
                _controller.ChangeToExitState();
            }
            else
            {
                _controller.ChangeToWanderState();
            }
        }
    }

    public void Update()
    {
        _effectTickTimer += Time.deltaTime;

        if (_effectTickTimer < _controller.UseEffectTickInterval)
        {
            return;
        }

        _effectTickTimer -= _controller.UseEffectTickInterval;

        _controller.ApplyCurrentFacilityEffect();
        Debug.Log("[GuestUseState] 시설 효과 틱 적용");

        if (_controller.IsCurrentFacilityGoalReached())
        {
            Debug.Log("[GuestUseState] 타겟 컨디션이 0이 되어 이용 종료");

            // [시설 점유 해제 / 재화 지급 / 평판 반영 연결 지점]

            _controller.FinishCurrentFacilityUse();

            if (_controller.IsTurnEnding)
            {
                _controller.ChangeToExitState();
            }
            else
            {
                _controller.ChangeToWanderState();
            }
        }
    }

    public void Exit()
    {
        Debug.Log("[GuestUseState] Exit");
    }
}