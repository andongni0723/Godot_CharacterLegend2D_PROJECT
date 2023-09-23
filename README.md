# Godot 遊戲開發 學習筆記

[toc]

![](https://hackmd.io/_uploads/ryWC5JQJa.png)
## Godot 編輯器

### Parallax Background 視差背景

1. 新增 **ParallaxBackground** Node, 在子節點中新增 **ParallaxLayer**。
2. 把圖片拖到 **ParallaxLayer** 子節點中。
3. 把 Offset>Centered -> off，and Reset Position
4. 在 **ParallaxLayer** 中，修改 Motion>Scale 的值調整視差距離。


## 程式(C#)

### Player

#### Gravity

```csharp
private float _defaultGravity = 
    ProjectSettings.GetSetting("physics/2d/default_gravity").AsSingle();
```
設定重力

#### Move

```csharp
public override void _PhysicsProcess(double delta)
{
    // Input
	float _direction = Input.GetAxis("move_left", "move_right");

    // Run
	float _acceleration = IsOnFloor() ? FLOOR_ACCELERATION : AIR_ACCELERATION;
		
	Velocity = new Vector2(Mathf.MoveToward(Velocity.X, _direction * RUN_SPEED, _acceleration * (float)delta),
			        Velocity.Y + _gravity * (float)delta);
}
```
**void _PhysicsProcess(double delta)** :
物理方面的Update

**Mathf.MoveToward(from, to, delta)** : 
把當前v的量值(from) 改變到結束值(to)， 期間花(delta)秒。
> 因為是在**PhysicsProcess**裡執行， 所以不管是加速還是減速都會有緩衝的效果

![](https://imgur.com/3YAr8zw.png)

#### Jump

```csharp
//...
public override void _PhysicsProcess(double delta)
{
    // Jump
    _canJump = IsOnFloor() || _coyoteTimer.TimeLeft > 0;
    _isInputJump = _canJump && _jumpRequestTimer.TimeLeft > 0;

    if (_isInputJump)
    {
        Velocity = new Vector2(Velocity.X, -JUMP_SPEED);
        _coyoteTimer.Stop();
        _jumpRequestTimer.Stop();
    }
    
    //...
    
    _isOnFloorBeforeMove = IsOnFloor();
		
    MoveAndSlide();
		
    // Coyote time 
    // (Can jump after falling off a platform)
    if (IsOnFloor() != _isOnFloorBeforeMove)
    {
        if (_isOnFloorBeforeMove && !_isInputJump)
            _coyoteTimer.Start();
        else
            _coyoteTimer.Stop();
    }
}	
```


#### Animation
```csharp
public override void _PhysicsProcess(double delta)
{
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
}
```
**Mathf.IsZeroApprox(float)** : 
當參數的值(float) **接近或是等於0時**，此函數會返回**true**

**[_animationPlayer].Play("name")** : 
播放動畫("name")


### State Machine


#### StateMachine.cs (Base)

```csharp
using Godot;
using System;

public partial class StateMachine : Node
{
    private int currentState = 1; // Use int to replace the enum ,
                                  // because the other state enum name is not different
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

```

**GetParent\<T\>()** :
得到父類的T組件 (GodotScript是用 "owner") 
    
**await**:
像是Unity的 yield return
    
**ToSignal(Object, signalName)**:
確定物件的基本函數已經執行

#### Player狀態機 架構
    
```csharp
public enum State
{
	IDLE //...
}

public partial class PlayerController : CharacterController
{
    /// <summary>
    /// What do you do in this frame by State?
    /// </summary>
    /// <param name="state"></param>
    /// <param name="delta"></param>
    public override void TickPhysics(State state, double delta)
    {
        switch (state)
        {
            case State.IDLE:
                // doing Physics (like move)
                break;
                
            //...
        }         
    }
    
    /// <summary>
    /// Can to change State?
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    public override State GetNextState(State state)
    {
        switch(state)
        {
            case State.IDLE:
                // Check to let 'State.IDLE' to other state
                break;
            
            //...
        }
    }
    
    /// <summary>
    /// What do you do when you change State? (a frame)
    /// </summary>
    /// <param name="from"></param>
    /// <param name="to"></param>
    public override void TransitionToState(State from, State to)
    {
        switch(to)
        {
            case State.IDLE:
                // do something (like animation)
                break;
                
            //...
        }
    }
}
```

#### 小技巧 (抽象類)

```csharp
using Godot;
using System;

public abstract partial class CharacterController : CharacterBody2D
{
    public abstract void TickPhysics(State state, double delta);
    public abstract State GetNextState(State state);
    public abstract void TransitionToState(State from, State to);

}

```

讓所有角色類Script都繼承 "CharacterController"， 強制讓他們都必須要Override那三個函式
