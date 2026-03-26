using UnityEngine;


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

        if (_controller.IsCurrentFacilityGoalReached())
        {
            Debug.Log("[GuestUseState] Goal already reached on enter.");
            _controller.ClearCurrentFacilityContext();
            _controller.ChangeToIdleState();
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

        Debug.Log("[GuestUseState] Applied facility effect tick.");

       
        if (_controller.IsCurrentFacilityGoalReached())
        {
            Debug.Log("[GuestUseState] Facility goal reached. Exit Use state.");

            _controller.ClearCurrentFacilityContext();
            _controller.ChangeToIdleState();
        }
    }

    public void Exit()
    {
        Debug.Log("[GuestUseState] Exit");
    }
}