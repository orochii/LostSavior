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
		switch (actionState) {
			case "attack1":
			case "attack2":
				CancelAction(0.5f);
				break;
		}
	}
	private void CancelAction(float delay) {
		actionDelay = delay;
		actionState = "default";
		actionCounter = 0;
	}

	public override void _PhysicsProcess(double delta)
	{
		var horz = Input.GetAxis("move_left","move_right");
		var jump = Input.IsActionJustPressed("jump");
		var jumpHeld = Input.IsActionPressed("jump");
		var pause = Input.IsActionJustPressed("pause");
		
		if (pause) TogglePause();

		if (jump) jumpRequest = JumpBuffer;
		ProcessAttack(delta);

		Vector2 velocity = Velocity;

		isGrounded = IsOnFloor();
		
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
			canJump = CoyoteTime;
		}
		if (jumpRequest>0 && canJump>0)
		{
			velocity.Y = JumpVelocity;
			jumpRequest = 0;
			canJump = 0;
		}
		
		if (horz != 0)
		{
			velocity.X = horz * Speed;
			if(isGrounded) moveState = "walk";
			graphic.FlipH = horz < 0;
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

	private void ProcessAttack(double delta) {
		if (actionDelay > 0) {
			actionDelay -= (float)delta;
			return;
		}
		var attack = Input.IsActionJustPressed("attack");
		if (attack) {
			switch (actionCounter) {
				case 0:
					actionDelay = 0.2f;
					actionState = "attack1";
					graphic.Animation = "default";
					actionCounter = 1;
					break;
				default:
					actionDelay = 0.5f;
					actionState = "attack2";
					graphic.Animation = "default";
					actionCounter = 2;
					break;
			}
		}
	}

	private void RefreshAnimation() {
		var c = moveState;
		if (actionState != "default") c = actionState;
		if (c != graphic.Animation) graphic.Play(c);
	}

	public async void TogglePause() {
		if (Worldmap.Instance != null) {
			GetTree().Paused = !GetTree().Paused;
			//var timer = GetTree().CreateTimer(.5f);
			//await ToSignal(timer, "timeout");
			await ToSignal(GetTree(), "process_frame");
			await ToSignal(GetTree(), "process_frame");
			Worldmap.Instance.Reposition();
			Worldmap.Instance.Visible = GetTree().Paused;
			GD.Print("Paused?: ", GetTree().Paused);
		}
	}
}
