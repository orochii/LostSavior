using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using Godot;

public enum EFaction { NONE, PLAYER, ENEMY }

public partial class BaseCharacter : CharacterBody2D, IDamageable
{
    public bool IsGrounded => isGrounded;
    public const float Gravity_Mult = 2f;
	public const float Apex_Range = 20f;
	public const float Apex_GravityMult = 0.4f;
	public const float MaxFallSpeed = 640f;
    public const float JumpBuffer = 0.1f;
	public const float CoyoteTime = 0.1f;
    [Export] public float Speed = 200.0f;
    [Export] public float Deaccel = 2048.0f;
	[Export] public float JumpVelocity = -420.0f;
    [Export] public AnimatedSprite2D graphic;
    [Export] public GpuParticles2D walkParticles;
	[Export] CollisionShape2D aliveShape;
	[Export] CollisionShape2D deadShape;
    [Export] public AnimationTiming[] animationTimings;
    [Export] public Node2D[] spawnLocations;
    [Export] public AudioEntry onJumpSound;
    [Export] public AudioEntry onLandSound;
    [Export] public AnimationPlayer animation;
    [Export] public Node2D UnderSourceContainer;
    [Export] public EFaction faction;
    //
    private Dictionary<StringName,List<AnimationTiming>> timingsDictionary;
	protected StringName moveState = "default";
	protected StringName actionState = "default";
	protected bool isGrounded;
    protected float canJump;
	protected float jumpRequest;
	// Attack
    protected float lastAnimationTimer = -1;
    protected float animationTimer = 0;
	protected float actionDelay = 0;
    protected float fallSoundQueueDelay = 0.2f;
	protected int actionCounter = 0;
    protected float dropDownTimer = 0;
    protected uint _origCollLayers = 0;
    protected uint _origCollMask = 0;
    public override void _Ready()
    {
        base._Ready();
        PreprocessTimings();
        graphic.AnimationChanged += OnAnimationChanged;
		graphic.AnimationFinished += OnAnimationFinished;
        graphic.AnimationLooped += OnAnimationLooped;
        _origCollLayers = CollisionLayer;
        _origCollMask = CollisionMask;
    }
    private void PreprocessTimings() {
        timingsDictionary = new Dictionary<StringName, List<AnimationTiming>>();
        if (animationTimings == null) return;
        foreach (var at in animationTimings) {
            bool found = timingsDictionary.TryGetValue(at.animationId, out var list);
            if (!found) {
                list = new List<AnimationTiming>();
                timingsDictionary.Add(at.animationId, list);
            }
            list.Add(at);
        }
    }
    protected void ProcessGroundMove(double delta, float horz, float vert, bool jump, bool jumpHeld)
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
            if (justLanded) {
                CancelAction(0f,true);
                if (fallSoundQueueDelay <= 0) {
                    if (onLandSound != null) AudioManager.PlaySound2D(GlobalPosition, onLandSound);
                    fallSoundQueueDelay = .2f;
                }
            }
            canJump = CoyoteTime;
        }
        if (jumpRequest > 0 && canJump > 0 && !IsActing())
        {
            if (vert > 0 && CheckDropDown()) { // holding Down
                DropDown();
            } else {
                CancelAction(0f,true);
                velocity.Y = JumpVelocity;
                canJump = 0;
                if (onJumpSound != null) AudioManager.PlaySound2D(GlobalPosition, onJumpSound);
            }
            jumpRequest = 0;
        }

        if (horz != 0 && forceTimer <= 0)
        {
            if (!isGrounded || (!IsActing())) velocity.X = horz * Speed;
            else velocity.X = Mathf.MoveToward(Velocity.X, 0, Deaccel*(float)delta);
            if (isGrounded) moveState = "walk";
            if (!IsActing()) {
                var s = graphic.Scale;
                s.X = horz;
                graphic.Scale = s; //horz < 0
            }
        }
        else
        {
            velocity.X = Mathf.MoveToward(Velocity.X, 0, Deaccel*(float)delta);
            if (isGrounded) moveState = "default";
        }
        Velocity = velocity;
        MoveAndSlide();
        //
        forceTimer -= (float)delta;
        fallSoundQueueDelay -= (float)delta;
        jumpRequest -= (float)delta;
        UpdateParticles();
    }
    private void UpdateParticles() {
        if (walkParticles != null) { 
            var d = Math.Sign(Velocity.X);
            walkParticles.Emitting = (d != 0 && isGrounded);
            var shader = walkParticles.ProcessMaterial as ShaderMaterial;
            if (shader != null) {
                shader.SetShaderParameter("direction", -d);
            }
        }
    }
    public float forceTimer = 0;
    public void ApplyForce(Vector2 force, float duration) {
        var v = Velocity;
        if (Math.Abs(force.Y)> 0) v.Y = force.Y;
        v.X = force.X * GetHorzDirection();
        Velocity = v;
        forceTimer = duration;
    }
    protected void ProcessAttack(double delta, bool attack, Action action) {
        if (_delayedSpawn != null) {
            if (_delayedSpawnTimer > 0) {
                _delayedSpawnTimer -= (float)delta;
            } else {
                _delayedSpawn.SpawnEffect(this);
                _delayedSpawn = null;
            }
        }
		if (actionDelay >= 0) {
			actionDelay -= (float)delta;
			return;
		}
        if (CanMove()) {
            if (attack && action != null) {
                ExecuteAction(action);
            }
        }
	}
    public void ExecuteAction(Action action) {
        lastAction = action;
        action.Execute(this);
        actionCounter += 1;
    }
    private void OnAnimationChanged() {
        animationTimer = 0;
        lastAnimationTimer = -1;
    }
    private void OnAnimationFinished() {
        if (actionState == "dead") return;
        animationTimer = 0;
        lastAnimationTimer = -1;
		if (lastAction != null) {
			lastAction.Finish(this);
            lastAction = null;
		}
		else {
			CancelAction(0.1f);
		}
	}
    private void OnAnimationLooped() {
        animationTimer = 0;
        lastAnimationTimer = -1;
    }
	public void SetAction(StringName state, float delay) {
        dropDownTimer = 0;
		actionDelay = delay;
		actionState = state;
		graphic.Animation = "default";
        RefreshAnimation(0);
	}
	public bool IsAction(Action origin) {
        return lastAction==origin;
    }
    public void CancelAction(float delay, bool unsetAction=false) {
		dropDownTimer = 0;
        actionDelay = delay;
		if(CanMove()) actionState = "default";
		actionCounter = 0;
		if (unsetAction) lastAction = null;
        _delayedSpawn = null;
	}
	public bool IsActing() {
		return actionDelay >= 0 || lastAction != null;
	}
	public bool IsDashing() {
        if (lastAction==null) return false;
        return (lastAction==GetDashingAction()) && actionDelay >= 0;
	}
    public int GetHorzDirection() {
		return (int)graphic.Scale.X; // ? -1 : 1
	}
	protected Action lastAction = null;
	protected void RefreshAnimation(double delta) {
        var c = moveState;
		if (actionState != "default") c = actionState;
		if (c != graphic.Animation) graphic.Play(c);
        RefreshAnimationTimings();
        lastAnimationTimer = animationTimer;
        animationTimer += (float)delta;
	}
    protected virtual void RefreshAnimationTimings() {
        // Get list of timings for animation N
        var id = graphic.Animation;
        bool found = timingsDictionary.TryGetValue(id, out var list);
        if (found) {
            foreach (var at in list) {
                // Check if timing is within last and current timer.
                if (at.time > lastAnimationTimer && at.time <= animationTimer) {
                    // Emit event.
                    //GD.Print("ANIM:",id," ev:",at.eventName," p1:",at.param1);
                    // I'll just play sound queues like this too.
                    if(at.soundQueue != null) AudioManager.PlaySound2D(GlobalPosition, at.soundQueue);
                }
            }
        }
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
    Action _delayedSpawn;
    float _delayedSpawnTimer;
    internal void SetDelayedSpawn(Action action, float effectSpawnDelay)
    {
        _delayedSpawn = action;
        _delayedSpawnTimer = effectSpawnDelay;
    }
    public virtual void ApplyDamage(BaseCharacter source, int baseDmg, EDamageFormula damageFormula)
    {
        
    }
    public bool CanMove() {
        if (Game.State.OnHold) return false;
        if (IsDead()) return false;
        return true;
    }
    public virtual bool IsDead()
    {
        return false;
    }
    public virtual void Die()
    {
        CollisionLayer = 4;
        if(aliveShape != null) aliveShape.Disabled = true;
		if(deadShape != null) deadShape.Disabled = false;
		SetAction("dead", 0);
		if(animation != null) {
            if (animation.HasAnimation("dead")) animation.Play("dead");
        }
    }
    public void Revive() {
        CollisionLayer = _origCollLayers;
        CollisionMask = _origCollMask;
		if(aliveShape != null) aliveShape.Disabled = false;
		if(deadShape != null) deadShape.Disabled = true;
	}
    public virtual void GetStatus()
    {
        
    }
    private bool CheckDropDown() {
        var spaceState = GetWorld2D().DirectSpaceState;
		Vector2 from = GlobalPosition;
		Vector2 to = from + (Vector2.Down * 4f);
		var query = PhysicsRayQueryParameters2D.Create(from, to);
		query.CollisionMask = 0b1000;
		var result = spaceState.IntersectRay(query);
		if (result.Count > 0) {
			return true;
		}
		return false;
    }
    private async void DropDown() {
        // Disable collision with layer 4
        CollisionMask = _origCollMask & 0b11111111111111111111111111110111;
        // Wait N time
        await ToSignal(GetTree().CreateTimer(0.2f, false), SceneTreeTimer.SignalName.Timeout);
        // Enable collision with layer 4
        CollisionMask = _origCollMask;
    }
    protected virtual Action GetDashingAction() {
		return null;
	}
    public Node2D GetSpawnLocation(int idx) {
        if (spawnLocations==null) return this;
        if (idx < 0 || idx >= spawnLocations.Length) return this;
        return spawnLocations[idx];
    }
}