using UnityEngine;


public class GuestDecideState : IGuestState
{
    private readonly GuestController _controller;

    public GuestDecideState(GuestController controller)
    {
        _controller = controller;
    }

    public void Enter()
    {
        if(_controller.IsTurnEnding)
        {
            _controller.ChangeToExitState();
            return;
        }

        _controller.EvaluateCurrentNeed();

        if(_controller.CurrentTargetFacilityType == EFacilityType.None)
        {
            Debug.Log("[GuestDecideState] 목표 시설 타입이 없어서 다시 배회로 복귀");
            _controller.ChangeToWanderState();
            return;
        }

        bool found = _controller.TryFindTargetFacility();

        if(!found)
        {
            Debug.Log("[GuestDecideState] 사용할 수 있는 목표 시설을 찾지 못해서 다시 배회로 복귀");
            _controller.ChangeToWanderState();
            return;
        }

        _controller.ChangeToMoveState();
    }

    public void Update()
    {
    }

    public void Exit()
    {
        Debug.Log("[GuestDecideState] 퇴장");
    }
}