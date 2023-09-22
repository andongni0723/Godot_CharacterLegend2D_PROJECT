using Godot;
using System;

public abstract partial class CharacterController : CharacterBody2D
{
    public abstract void TickPhysics(State state, double delta);
    public abstract State GetNextState(State state);
    public abstract void TransitionToState(State from, State to);

}
