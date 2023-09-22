using Godot;
using System;

public partial class StateMachine : Node
{
    private int currentState = 1;
    public int CurrentState
    {
        get => currentState;
        set
        {
            currentState = value;
            GetParent<CharacterController>().TransitionToState((State)currentState, (State)value);
        }
    }

    public override async void _Ready()
    {
        await ToSignal(Owner, "ready");
        CurrentState = 0;
    }

    public override void _PhysicsProcess(double delta)
    {
        int nextState = (int)GetParent<CharacterController>().GetNextState((State)CurrentState);
        if (nextState != CurrentState)
        {
            CurrentState = nextState;
        }
        
        GetParent<CharacterController>().TickPhysics((State)CurrentState, delta);
    }
}
