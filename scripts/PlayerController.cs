using Godot;
using System;

public partial class PlayerController : CharacterBody2D
{
	private const float RUN_SPEED = 160f;
	private const float JUMP_SPEED = 350f;
	private const float FLOOR_ACCELERATION = RUN_SPEED / 0.2f;
	private const float AIR_ACCELERATION = RUN_SPEED / 0.02f;
	private float _gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
	
	// Components
	Sprite2D _sprite => GetNode<Sprite2D>("Sprite2D");
	AnimationPlayer _animationPlayer => GetNode<AnimationPlayer>("AnimationPlayer");

	public override void _PhysicsProcess(double delta)
	{
		// Input
		float _direction = Input.GetAxis("move_left", "move_right");

		// Run
		float _acceleration = IsOnFloor() ? FLOOR_ACCELERATION : AIR_ACCELERATION;
		
		Velocity = new Vector2(Mathf.MoveToward(Velocity.X, _direction * RUN_SPEED, _acceleration * (float)delta),
			Velocity.Y + _gravity * (float)delta);

		// Jump
		if (IsOnFloor() && Input.IsActionJustPressed("jump"))
		{
			Velocity = new Vector2(Velocity.X, -JUMP_SPEED);
		}
		
		// Animation
		if (IsOnFloor())
		{
			_animationPlayer.Play(Mathf.IsZeroApprox(_direction) && Mathf.IsZeroApprox(Velocity.X) ?
				"idle" : "running");
		}
		else
		{
			_animationPlayer.Play("jump");
		}
		
		// Flip sprite
		if(_direction != 0) 
			_sprite.FlipH = _direction < 0;

		MoveAndSlide();
	}
}
