using Godot;
using System;
using System.Collections;

public partial class Player : CharacterBody2D
{
	/*
	TODO/REVISE:
		DONE	Anti-gravity Apex
		DONE	Early Fall
		DONE	Jump Buffering
		Sticky Feet on Land
		Speedy Apex?
		Coyote Time
		DONE	Clamp Fall Speed
		Catch Missed Jump
		Bumped Head on Corner
	*/
	public const float Gravity_Mult = 2f;
	public const float Apex_Range = 20f;
	public const float Apex_GravityMult = 0.4f;
	public const float Speed = 200.0f;
	public const float JumpVelocity = -420.0f;
	public const float MaxFallSpeed = 400f;

	public const float JumpBuffer = 0.1f;
	public const float CoyoteTime = 0.1f;

	[Export] public AnimatedSprite2D graphic;
	[Export] public Action[] defaultAttackActions;
	[Export] public Action[] defaultAirAttackActions;

	private bool isGrounded;
	private float canJump;
	private float jumpRequest;
	//
	private StringName moveState = "default";
	private StringName actionState = "default";
	
	// Attack
	private float actionDelay = 0;
	private int actionCounter = 0;

    public override void _Ready()
    {
        base._Ready();
		graphic.AnimationFinished += OnAnimationFinished;
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

	public override void _PhysicsProcess(double delta)
	{
		var horz = Input.GetAxis("move_left","move_right");
		var jump = Input.IsActionJustPressed("jump");
		var jumpHeld = Input.IsActionPressed("jump");

		if (jump) jumpRequest = JumpBuffer;
		ProcessAttack(delta);

		Vector2 velocity = Velocity;
		var lastGrounded = isGrounded;
		isGrounded = IsOnFloor();
		var justLanded = lastGrounded != isGrounded && isGrounded==true;
		
		if (!isGrounded)
		{
			if(canJump > 0) canJump -= (float)delta;
			if (Math.Abs(velocity.Y) < Apex_Range)
				velocity += GetGravity() * Apex_GravityMult * (float)delta;
			else
				velocity += GetGravity() * Gravity_Mult * (float)delta;
			if (velocity.Y > MaxFallSpeed) velocity.Y = MaxFallSpeed;
			if (velocity.Y > 0) {
				moveState = "fall";
			} else {
				moveState = "jump";
				if (!jumpHeld) velocity.Y = 0;
			}
		} else {
			if (justLanded) CancelAction(0f);
			canJump = CoyoteTime;
		}
		if (jumpRequest>0 && canJump>0 && !IsActing())
		{
			CancelAction(0f);
			velocity.Y = JumpVelocity;
			jumpRequest = 0;
			canJump = 0;
		}
		
		if (horz != 0)
		{
			if(!isGrounded || !IsActing()) velocity.X = horz * Speed;
			else velocity.X = 0;
			if(isGrounded) moveState = "walk";
			if (!IsActing()) graphic.FlipH = horz < 0;
		}
		else
		{
			velocity.X = Mathf.MoveToward(Velocity.X, 0, Speed);
			if(isGrounded) moveState = "default";
		}
		Velocity = velocity;
		MoveAndSlide();
		//
		jumpRequest -= (float)delta;
		RefreshAnimation();
	}

	public bool IsActing() {
		return actionDelay >= 0 || lastAction != null;
	}
	public int GetHorzDirection() {
		return graphic.FlipH ? -1 : 1;
	}

	private void ProcessAttack(double delta) {
		if (actionDelay >= 0) {
			actionDelay -= (float)delta;
			return;
		}
		var attack = Input.IsActionJustPressed("attack");
		if (CanHold()) attack = Input.IsActionPressed("attack");
		if (attack) {
			var _actions = GetAttackActions();
			if (_actions.Length > actionCounter) {
				var action = _actions[actionCounter];
				action.Execute(this);
				lastAction = action;
			}
			actionCounter += 1;
		}
	}

	private Action lastAction = null;
	private bool CanHold() {
		var w = Game.State.GetWeapon();
		if (w != null) return w.CanHold;
		return false;
	}
	private Action[] GetAttackActions() {
		var w = Game.State.GetWeapon();
		if (!isGrounded) {
			if (w != null) return w.AirAttackActions;
			return defaultAirAttackActions;
		}
		if (w != null) return w.AttackActions;
		return defaultAttackActions;
	}
	private void RefreshAnimation() {
		var c = moveState;
		if (actionState != "default") c = actionState;
		if (c != graphic.Animation) graphic.Play(c);
	}

}
