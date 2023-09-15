using Godot;
using System;

public partial class PlayerController : CharacterBody2D
{
	private const float RUN_SPEED = 200f;
	private const float JUMP_SPEED = 300f;
	private float _gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
	
	// Components
	Sprite2D _sprite => GetNode<Sprite2D>("Sprite2D");
	AnimationPlayer _animationPlayer => GetNode<AnimationPlayer>("AnimationPlayer");

	public override void _PhysicsProcess(double delta)
	{
		// Input
		float _direction = Input.GetAxis("move_left", "move_right");

		// Run
		Velocity = new Vector2(_direction * RUN_SPEED,
			Velocity.Y + _gravity * (float)delta);

		// Jump
		if (IsOnFloor() && Input.IsActionJustPressed("jump"))
		{
			Velocity = new Vector2(Velocity.X, -JUMP_SPEED);
		}
		
		// Animation
		if (IsOnFloor())
		{
			_animationPlayer.Play(_direction != 0 ? "running" : "idle");
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
