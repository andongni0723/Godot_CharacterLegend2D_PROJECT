using Godot;
using System;
using System.Collections.Generic;

public enum State
{
	IDLE, RUNNING, JUMPING, FALLING, LANDING, WALL_SLIDING, WALL_JUMPING
}

public partial class PlayerController : CharacterController
{
	private const float RUN_SPEED = 160f;
	private const float JUMP_SPEED = 350f;
	private static readonly Vector2 WALL_JUMP_SPEED = new Vector2(500, -300);
	private const float FLOOR_ACCELERATION = RUN_SPEED / 0.2f;
	private const float AIR_ACCELERATION = RUN_SPEED / 0.02f;
	private static readonly List<State> groundStates = new List<State> {State.IDLE, State.RUNNING, State.LANDING};
	private float _defaultGravity = ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();

	private bool _isFirstFrame = false; // if true, then jump velocity not change by gravity a frame
	private bool _canJump = false;
	private bool _isInputJump = false;
	private bool _isOnFloorBeforeMove = false;
	
	// Components
	Node2D _graphics => GetNode<Node2D>("Graphics");
	AnimationPlayer _animationPlayer => GetNode<AnimationPlayer>("AnimationPlayer");
	Timer _coyoteTimer => GetNode<Timer>("CoyoteTimer");
	Timer _jumpRequestTimer => GetNode<Timer>("JumpRequestTimer");
	RayCast2D _handChecker => GetNode<RayCast2D>("Graphics/HandChecker");
	RayCast2D _footChecker => GetNode<RayCast2D>("Graphics/FootChecker");

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

	/// <summary>
	/// What do you do in this frame by State?
	/// </summary>
	/// <param name="state"></param>
	/// <param name="delta"></param>
	public override void TickPhysics(State state, double delta)
	{
		// switch enum State
		switch (state)
		{
			case State.IDLE:
				Move(_defaultGravity, delta);
				break;

			case State.RUNNING:
				Move(_defaultGravity, delta);
				break;

			case State.JUMPING:
				Move(_isFirstFrame? 0 : _defaultGravity, delta);
				break;

			case State.FALLING:
				Move(_defaultGravity, delta);
				break;
			
			case State.LANDING:
				Move(_defaultGravity, delta);
				break;
			
			case State.WALL_SLIDING:
				Move(_defaultGravity / 3, delta);
				_graphics.Scale = new Vector2(GetWallNormal().X, Scale.Y);
				break;
			
			case State.WALL_JUMPING:
				Move(_isFirstFrame? 0 : _defaultGravity, delta);
				break;
		}

		_isFirstFrame = false;
	}

	private void Move(float gravity, double delta)
	{
		// Input
		float _direction = Input.GetAxis("move_left", "move_right");

		// Run
		float _acceleration = IsOnFloor() ? FLOOR_ACCELERATION : AIR_ACCELERATION;
		
		Velocity = new Vector2(Mathf.MoveToward(Velocity.X, _direction * RUN_SPEED, _acceleration * (float)delta),
			Velocity.Y + gravity * (float)delta);

		// Flip sprite
		if(_direction != 0) 
			_graphics.Scale = new Vector2(_direction > 0 ? 1 : -1, Scale.Y);

		_isOnFloorBeforeMove = IsOnFloor();
		MoveAndSlide();	
	}


	/// <summary>
	/// Can to change State?
	/// </summary>
	/// <param name="state"></param>
	/// <returns></returns>
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
					return State.LANDING;	

				if (IsOnWall() && _handChecker.IsColliding() && _footChecker.IsColliding())
					return State.WALL_SLIDING;

				
				break;
			
			case State.LANDING:
				if (!_animationPlayer.IsPlaying())
					return State.IDLE;
				break;
			
			case State.WALL_SLIDING:
				if (_jumpRequestTimer.TimeLeft > 0)
					return State.WALL_JUMPING;
				if(IsOnFloor())
					return State.IDLE;
				if (!IsOnWall())
					return State.FALLING;
				break;
			
			case State.WALL_JUMPING:
				if (Velocity.Y >= 0)
					return State.FALLING;	
				break;
		}
		
		return state;
	}
	
	/// <summary>
	/// What do you do when you change State? (a frame)
	/// </summary>
	/// <param name="from"></param>
	/// <param name="to"></param>
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
			
			case State.LANDING:
				_animationPlayer.Play("landing");
				break;
			
			case State.WALL_SLIDING:
				_animationPlayer.Play("wall_sliding");
				break;
			
			case State.WALL_JUMPING:
				_animationPlayer.Play("jump");
				Velocity = new Vector2(WALL_JUMP_SPEED.X * GetWallNormal().X, WALL_JUMP_SPEED.Y);
				_jumpRequestTimer.Stop();
				break;
		}

		if (to == State.WALL_JUMPING)
			Engine.TimeScale = 0.3;
		if (from == State.WALL_JUMPING)
			Engine.TimeScale = 1;
	
		_isFirstFrame = true;
	}
}
