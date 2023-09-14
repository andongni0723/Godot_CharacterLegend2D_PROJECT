using Godot;
using System;

public partial class PlayerController : CharacterBody2D
{
    public float Gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

    public override void _PhysicsProcess(double delta)
    {
        Vector2 velocity = Velocity;
        
        velocity.Y += Gravity * (float)delta;
        
        Velocity = velocity;
        MoveAndSlide();
    }
}
