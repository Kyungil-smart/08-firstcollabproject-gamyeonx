using UnityEngine;

public class GuestStateMachine
{
    public IGuestState CurrentState { get; private set; }

    public void ChangeState(IGuestState newState)
    {
        if(newState == null) return;

        CurrentState?.Exit();
        CurrentState = newState;
        CurrentState.Enter();
    }

    public void Update()
    {
        CurrentState?.Update();
    }

    
}
