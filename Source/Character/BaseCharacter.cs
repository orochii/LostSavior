using System;
using Godot;

public enum EFaction { NONE, PLAYER, ENEMY }

[GlobalClass]
public partial class BaseCharacter : CharacterBody2D
{
    public const float Gravity_Mult = 2f;
	public const float Apex_Range = 20f;
	public const float Apex_GravityMult = 0.4f;
	public const float MaxFallSpeed = 400f;
    public const float JumpBuffer = 0.1f;
	public const float CoyoteTime = 0.1f;
    [Export] public float Speed = 200.0f;
	[Export] public float JumpVelocity = -420.0f;
    [Export] public AnimatedSprite2D graphic;
    [Export] public AnimationPlayer animation;
    [Export] public EFaction faction;
    //
	protected StringName moveState = "default";
	protected StringName actionState = "default";
	protected bool isGrounded;
    protected float canJump;
	protected float jumpRequest;
	// Attack
	protected float actionDelay = 0;
	protected int actionCounter = 0;
    public override void _Ready()
    {
        base._Ready();
		graphic.AnimationFinished += OnAnimationFinished;
    }
    protected void ProcessGroundMove(double delta, float horz, bool jump, bool jumpHeld)
    {
        if (jump) jumpRequest = JumpBuffer;
        Vector2 velocity = Velocity;
        var lastGrounded = isGrounded;
        isGrounded = IsOnFloor();
        var justLanded = lastGrounded != isGrounded && isGrounded == true;

        if (!isGrounded)
        {
            if (canJump > 0) canJump -= (float)delta;
            if (Math.Abs(velocity.Y) < Apex_Range)
                velocity += GetGravity() * Apex_GravityMult * (float)delta;
            else
                velocity += GetGravity() * Gravity_Mult * (float)delta;
            if (velocity.Y > MaxFallSpeed) velocity.Y = MaxFallSpeed;
            if (velocity.Y > 0)
            {
                moveState = "fall";
            }
            else
            {
                moveState = "jump";
                if (!jumpHeld) velocity.Y = 0;
            }
        }
        else
        {
            if (justLanded) CancelAction(0f);
            canJump = CoyoteTime;
        }
        if (jumpRequest > 0 && canJump > 0 && !IsActing())
        {
            CancelAction(0f);
            velocity.Y = JumpVelocity;
            jumpRequest = 0;
            canJump = 0;
        }

        if (horz != 0)
        {
            if (!isGrounded || !IsActing()) velocity.X = horz * Speed;
            else velocity.X = 0;
            if (isGrounded) moveState = "walk";
            if (!IsActing()) graphic.FlipH = horz < 0;
        }
        else
        {
            velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
            if (isGrounded) moveState = "default";
        }
        Velocity = velocity;
        MoveAndSlide();
        //
        jumpRequest -= (float)delta;
        RefreshAnimation();
    }
    protected void ProcessAttack(double delta, bool attack, Action action) {
		if (actionDelay >= 0) {
			actionDelay -= (float)delta;
			return;
		}
		if (attack && action != null) {
			action.Execute(this);
			lastAction = action;
			actionCounter += 1;
		}
	}
    private void OnAnimationFinished() {
		if (lastAction != null) {
			lastAction.Finish(this);
		}
		else {
			CancelAction(0.1f);
		}
	}
	public void SetAction(StringName state, float delay) {
		actionDelay = delay;
		actionState = state;
		graphic.Animation = "default";
	}
	public void CancelAction(float delay) {
		actionDelay = delay;
		actionState = "default";
		actionCounter = 0;
		lastAction = null;
	}
	public bool IsActing() {
		return actionDelay >= 0 || lastAction != null;
	}
	public int GetHorzDirection() {
		return graphic.FlipH ? -1 : 1;
	}
	protected Action lastAction = null;
	protected void RefreshAnimation() {
		var c = moveState;
		if (actionState != "default") c = actionState;
		if (c != graphic.Animation) graphic.Play(c);
	}

    public virtual int GetStr() {
        return 0;
    }
    public virtual int GetCon() {
        return 0;
    }
    public virtual int GetInt() {
        return 0;
    }
}