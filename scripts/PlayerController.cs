using Godot;
using System;

public partial class PlayerController : CharacterBody2D
{
	private const float RUN_SPEED = 160f;
	private const float JUMP_SPEED = 350f;
	private const float FLOOR_ACCELERATION = RUN_SPEED / 0.2f;
	private const float AIR_ACCELERATION = RUN_SPEED / 0.02f;
	private float _gravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

	private bool _canJump = false;
	private bool _isInputJump = false;
	private bool _isOnFloorBeforeMove = false;
	
	// Components
	Sprite2D _sprite => GetNode<Sprite2D>("Sprite2D");
	AnimationPlayer _animationPlayer => GetNode<AnimationPlayer>("AnimationPlayer");
	Timer _coyoteTimer => GetNode<Timer>("CoyoteTimer");
	Timer _jumpRequestTimer => GetNode<Timer>("JumpRequestTimer");

	public override void _UnhandledInput(InputEvent @event)
	{
		if(@event.IsActionPressed("jump"))
			_jumpRequestTimer.Start();

		if (@event.IsActionReleased("jump"))
		{
			_jumpRequestTimer.Stop();
			
			if(Mathf.Abs(Velocity.Y) > JUMP_SPEED / 2)
				Velocity = new Vector2(Velocity.X, -JUMP_SPEED / 2f);
		}
			
	}

	public override void _PhysicsProcess(double delta)
	{
		// Input
		float _direction = Input.GetAxis("move_left", "move_right");

		// Run
		float _acceleration = IsOnFloor() ? FLOOR_ACCELERATION : AIR_ACCELERATION;
		
		Velocity = new Vector2(Mathf.MoveToward(Velocity.X, _direction * RUN_SPEED, _acceleration * (float)delta),
							   Velocity.Y + _gravity * (float)delta);

		// Jump
		_canJump = IsOnFloor() || _coyoteTimer.TimeLeft > 0;
		_isInputJump = _canJump && _jumpRequestTimer.TimeLeft > 0;
		
		if (_isInputJump)
		{
			Velocity = new Vector2(Velocity.X, -JUMP_SPEED);
			_coyoteTimer.Stop();
			_jumpRequestTimer.Stop();
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
		

		_isOnFloorBeforeMove = IsOnFloor();
		
		MoveAndSlide();
		
		// Coyote time (Can jump after falling off a platform)
		if (IsOnFloor() != _isOnFloorBeforeMove)
		{
			if (_isOnFloorBeforeMove && !_isInputJump)
				_coyoteTimer.Start();
			else
				_coyoteTimer.Stop();
		}
	}
}
