using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

public partial class Enemy : BaseCharacter, IDamageable
{
	[Export][Range(1,999999)] int MaxHealth = 10;
	[Export][Range(1,999)] int Str = 1;
	[Export][Range(1,999)] int Con = 1;
	[Export][Range(1,999)] int Int = 1;
	[Export] Area2D detectionArea;
	[Export] float AggroMemoryCooldown = 2f;
	[Export] float DetectMemoryCooldown = 5f;
	[Export] float FollowDistance = 16f;
	[Export] Control healthDisplay;
	[Export] CollisionShape2D aliveShape;
	[Export] CollisionShape2D deadShape;
	private int currentHealth;
	private Node2D lastTarget;
	private List<Node2D> brainTargets = new List<Node2D>();
	private Vector2 brainTargetPos;
	private float lastTargetMemory;
	private float brainTargetCooldown;
	private ShaderMaterial _healthMaterial = null;
	Tween _healthTween = null;
    //
    public override void _Ready()
    {
        base._Ready();
		currentHealth = MaxHealth;
		detectionArea.BodyEntered += OnBodySeen;
		detectionArea.BodyExited += OnBodyUnseen;
		if (healthDisplay.Material != null) {
			_healthMaterial = healthDisplay.Material as ShaderMaterial;
			_healthMaterial.SetShaderParameter("percent", currentHealth * 1f / MaxHealth);
		}
		Revive();
    }
    public override void _PhysicsProcess(double delta) {
		float horz = 0;
		bool jump = false;
		bool jumpHeld = true;
		bool attack = false;
		if (!IsDead()) {
			// Detection update.
			if (brainTargets.Count > 0) {
				lastTarget = brainTargets[0];
				lastTargetMemory = AggroMemoryCooldown;
			}
			if (lastTarget != null) {
				brainTargetPos = lastTarget.GlobalPosition;
				brainTargetCooldown = DetectMemoryCooldown;
				lastTargetMemory -= (float)delta;
				if (lastTargetMemory < 0) lastTarget = null;
			}
			// Movement brain.
			if (brainTargetCooldown > 0) {
				brainTargetCooldown -= (float)delta;
				//Move
				var deltaPos = brainTargetPos - GlobalPosition;
				var reqDir = Math.Sign(deltaPos.X);
				if (Math.Abs(deltaPos.X) > FollowDistance || reqDir != GetHorzDirection()) {
					horz = reqDir;
				}
				//Jump
				
			}
			// Attack brain.
			// TODO.
		}
		// Execute actions.
		ProcessAttack(delta,attack,null);
		ProcessGroundMove(delta, horz, jump, jumpHeld);
		detectionArea.Scale = new Vector2(GetHorzDirection(), 1);
	}

	public void OnBodySeen(Node2D body) {
		if (body == this) return;
		if (body is BaseCharacter) {
			var character = body as BaseCharacter;
			if (character.faction != faction) {
				if (!brainTargets.Contains(body)) brainTargets.Add(body);
			}
		}
	}
	public void OnBodyUnseen(Node2D body) {
		if (brainTargets.Contains(body)) brainTargets.Remove(body);
	}
	public void ApplyDamage(BaseCharacter source, int baseDmg, EDamageFormula damageFormula) {
		int damage = baseDmg;
		if (damage > 0) {
			int defense = DamageFormulas.CalculateDefense(this, damageFormula);
			damage -= defense;
			if (damage < 0) damage = 0;
			OnBodySeen(source);
		}
		ChangeHealth(-damage);
		GD.Print(string.Format("{0} receives {1} damage (orig:{2})",Name,damage,baseDmg));
		Game.DamagePop(GlobalPosition, damage);
		if (IsDead()) Die();
	}
	public void ChangeHealth(int v) {
		if (IsDead()) return;
		var oldVal = currentHealth;
		currentHealth = Math.Clamp(currentHealth + v, 0, MaxHealth);
		//
		if (_healthTween != null) _healthTween.Kill();
		_healthTween = CreateTween();
		_healthTween.TweenMethod(Callable.From(
			(float v)=>{
				var h = Mathf.Lerp(oldVal, currentHealth, v);
				_healthMaterial.SetShaderParameter("percent", h * 1f / MaxHealth);
				healthDisplay.Modulate = Colors.White;
				healthDisplay.Visible = true;
			}
			), 0f, 1f, 0.1);
		_healthTween.TweenMethod(Callable.From(
			(float v)=>{
				var mod = new Color(1,1,1,v);
				healthDisplay.Modulate = mod;
				healthDisplay.Visible = mod.A > 0;
			}
			), 1f, 0f, 0.25);
    }
	public bool IsDead() {
		return currentHealth <= 0;
	}
	public void Revive() {
		aliveShape.Disabled = false;
		deadShape.Disabled = true;
	}
	public void Die() {
		aliveShape.Disabled = true;
		deadShape.Disabled = false;
		// TODO: Show death animation.
		actionState = "dead";
		animation.Play("dead");
	}
	public override int GetStr()
    {
        return Str;
    }
    public override int GetCon()
    {
        return Con;
    }
    public override int GetInt()
    {
        return Int;
    }
}
