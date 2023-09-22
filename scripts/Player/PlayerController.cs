using Godot;
using System;
using System.Collections.Generic;

public enum State
{
	IDLE, RUNNING, JUMPING, FALLING
}

public partial class PlayerController : CharacterController
{
	private const float RUN_SPEED = 160f;
	private const float JUMP_SPEED = 350f;
	private const float FLOOR_ACCELERATION = RUN_SPEED / 0.2f;
	private const float AIR_ACCELERATION = RUN_SPEED / 0.02f;
	private static readonly List<State> groundStates = new List<State> {State.IDLE, State.RUNNING};
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

	public override void TickPhysics(State state, double delta)
	{
		// switch enum State
		switch (state)
		{
			case State.IDLE:
				Move(delta);
				break;

			case State.RUNNING:
				Move(delta);
				break;

			case State.JUMPING:
				Move(delta);
				break;

			case State.FALLING:
				Move(delta);
				break;
		}
		
	}

	private void Move(double delta)
	{
		// Input
		float _direction = Input.GetAxis("move_left", "move_right");

		// Run
		float _acceleration = IsOnFloor() ? FLOOR_ACCELERATION : AIR_ACCELERATION;
		
		Velocity = new Vector2(Mathf.MoveToward(Velocity.X, _direction * RUN_SPEED, _acceleration * (float)delta),
			Velocity.Y + _gravity * (float)delta);

		// Flip sprite
		if(_direction != 0) 
			_sprite.FlipH = _direction < 0;

		_isOnFloorBeforeMove = IsOnFloor();
		MoveAndSlide();	
	}


	public override State GetNextState(State state)
	{
		float direction = Input.GetAxis("move_left", "move_right");
		bool isIdle = Mathf.IsZeroApprox(direction) && Mathf.IsZeroApprox(Velocity.X);
		_canJump = IsOnFloor() || _coyoteTimer.TimeLeft > 0;
		_isInputJump = _canJump && _jumpRequestTimer.TimeLeft > 0;

		// GD.Print(_coyoteTimer.TimeLeft);
		// GD.Print(_isInputJump);
		if (_isInputJump)
		{
			return State.JUMPING;
		}
		
		switch (state)
		{
			case State.IDLE:
				if (!IsOnFloor())
					return State.FALLING;
				if (!isIdle)
					return State.RUNNING;
				break;
			
			case State.RUNNING:
				if (!IsOnFloor())
					return State.FALLING;
				if (isIdle)
					return State.IDLE;
				break;
			
			case State.JUMPING:
				if (Velocity.Y >= 0)
					return State.FALLING;
				break;
			
			case State.FALLING:
				if (IsOnFloor())
					return State.IDLE;
				break;
		}
		
		return state;
	}

	public override void TransitionToState(State from, State to)
	{
		if(!groundStates.Contains(from) && groundStates.Contains(to))
			_coyoteTimer.Stop();

		switch (to)
		{
			case State.IDLE:
				_animationPlayer.Play("idle");
				break;
			
			case State.RUNNING:
				_animationPlayer.Play("running");
				break;
			
			case State.JUMPING:
				_animationPlayer.Play("jump");
				Velocity = new Vector2(Velocity.X, -JUMP_SPEED);
				_coyoteTimer.Stop();
				_jumpRequestTimer.Stop();
				break;
			
			case State.FALLING:
				_animationPlayer.Play("fall");
				if (groundStates.Contains(from) && !_isInputJump)
				{
					GD.Print("fall coyote time start");
					_coyoteTimer.Start();	
				}
				break;
		}
	}
}
