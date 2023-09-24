using Godot;
using System;

public partial class StateMachine : Node
{
    private int _currentState = 1;
    public int CurrentState
    {
        get => _currentState;
        set
        {
            _currentState = value;
            GetParent<CharacterController>().TransitionToState((State)_currentState, (State)value);
            stateTime = 0f;
        }
    }
    
    public float stateTime = 0f;

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
        stateTime += (float)delta;
    }
}
