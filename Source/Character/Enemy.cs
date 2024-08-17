using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public partial class Enemy : BaseCharacter, IDamageable
{
	[Export][Range(1,999999)] int MaxHealth = 10;
	[Export][Range(1,999)] int Str = 1;
	[Export][Range(1,999)] int Con = 1;
	[Export][Range(1,999)] int Int = 1;
	[Export] int RewardExp = 2;
	[Export] int RewardMoney = 5;
	[Export] public Action[] attackActions;
	[Export] public Action[] airAttackActions;
	[Export] Area2D detectionArea;
	[Export] float ReactionCooldown = 0.6f;
	[Export] float AggroMemoryCooldown = 2f;
	[Export] float DetectMemoryCooldown = 5f;
	[Export] float FollowDistance = 16f;
	[Export] Control healthDisplay;
	private int currentHealth;
	private Node2D lastTarget;
	private List<Node2D> brainTargets = new List<Node2D>();
	private Vector2 brainTargetPos;
	private float reactionTimer;
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
			/*var refMat = healthDisplay.Material as ShaderMaterial;
			_healthMaterial = new ShaderMaterial();
			_healthMaterial.Shader = refMat.Shader;*/
			_healthMaterial = healthDisplay.Material.Duplicate(true) as ShaderMaterial;
			healthDisplay.Material = _healthMaterial;
		}
		if (graphic.Material != null) {
			/*var refMat = graphic.Material as ShaderMaterial;
			var graphicMat = new ShaderMaterial();
			graphicMat.Shader = refMat.Shader;
			graphicMat.SetShaderParameter("width", refMat.GetShaderParameter("width"));
			graphicMat.SetShaderParameter("height", refMat.GetShaderParameter("height"));*/
			graphic.Material = graphic.Material.Duplicate(true) as Material;
			
		}
    }
    public override void _PhysicsProcess(double delta) {
		float horz = 0;
		float vert = 0;
		bool jump = false;
		bool jumpHeld = true;
		bool attack = false;
		if (CanMove()) {
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
				var cachedDir = GetHorzDirection();
				//Move
				var deltaPos = brainTargetPos - GlobalPosition;
				var reqDir = Math.Sign(deltaPos.X);
				if (Math.Abs(deltaPos.X) > FollowDistance || reqDir != cachedDir) {
					horz = reqDir;
					cachedDir = reqDir;
				}
				if(reactionTimer > 0) reactionTimer -= (float)delta;
				else {
					//Jump and Attack
					attack = CheckAttack(cachedDir);
					if (!attack) jump = CheckJump(cachedDir);
					//
				}
			} else {
				// Regular patrol/wander. TODO
			}
			// Forget the dead (?).
			brainTargets.RemoveAll((n) => {
				if (n is BaseCharacter) {
					BaseCharacter c = n as BaseCharacter;
					return c.IsDead();
				}
				return false;
			});
		}
		// Execute actions.
		ProcessGroundMove(delta, horz, vert, jump, jumpHeld);
		ProcessAttack(delta,attack,attackActions[0]);
		RefreshAnimation(delta);
		detectionArea.Scale = new Vector2(GetHorzDirection(), 1);
	}
	private bool CheckAttack(int cachedDir) {
		var _exclude = new Array<Rid>{GetRid()};
		var dirHorz = 32*cachedDir;
		if (DoQuery(new Vector2(0, 0),new Vector2(dirHorz,0),_exclude)) return true;
		for (int i = 1; i <= 2; i++) {
			var oY = i * 4;
			if (DoQuery(new Vector2(0, oY), new Vector2(dirHorz,oY*2),_exclude)) return true;
			if (DoQuery(new Vector2(0,-oY), new Vector2(dirHorz,-oY*2),_exclude)) return true;
		}
		return false;
	}
	private bool DoQuery(Vector2 offset, Vector2 direction, Array<Rid> exclude) {
		var spaceState = GetWorld2D().DirectSpaceState;
		Vector2 from = GlobalPosition + offset;
		Vector2 to = from + direction;
		var query = PhysicsRayQueryParameters2D.Create(from, to);
		query.CollisionMask = 2+16;
		query.Exclude = exclude;
		var result = spaceState.IntersectRay(query);
		if (result.Count > 0) {
			var other = result["collider"].As<Node2D>();
			if (other is BaseCharacter) {
				var basechar = other as BaseCharacter;
				if (basechar.faction != faction) {
					return true;
				}
			}
		}
		return false;
	}
	private bool CheckJump(int cachedDir) {
		var spaceState = GetWorld2D().DirectSpaceState;
		Vector2 from = GlobalPosition + new Vector2(0, -8);
		Vector2 to = from + (new Vector2(32,0) * cachedDir);
		var query = PhysicsRayQueryParameters2D.Create(from, to);
		query.Exclude = new Godot.Collections.Array<Rid>{GetRid()};
		var result = spaceState.IntersectRay(query);
		if (result.Count > 0) {
			var other = result["collider"].As<Node2D>();
			var moveDir = new Vector2(cachedDir,0);
			var normal = result["normal"].AsVector2();
			var radians = normal.AngleTo(moveDir);
			var angle = Math.Abs(Mathf.RadToDeg(radians));
			if (angle >= 150) { // FloorMaxAngle
				return true;
			}
		}
		return false;
	}
	public void OnBodySeen(Node2D body) {
		if (body == this) return;
		if (body is BaseCharacter) {
			var character = body as BaseCharacter;
			if (!character.IsDead() && character.faction != faction) {
				if (!brainTargets.Contains(body)) {
					brainTargets.Add(body);
					reactionTimer = ReactionCooldown;
				}
			}
		}
	}
	public void OnBodyUnseen(Node2D body) {
		if (brainTargets.Contains(body)) brainTargets.Remove(body);
	}
	public override void ApplyDamage(BaseCharacter source, int baseDmg, EDamageFormula damageFormula) {
		int damage = baseDmg;
		if (damage > 0) {
			int defense = DamageFormulas.CalculateDefense(this, damageFormula);
			damage -= defense;
			if (damage < 0) damage = 0;
			OnBodySeen(source);
		}
		ChangeHealth(-damage);
		Game.DamagePop(GlobalPosition, damage);
		if (IsDead()) {
			if(source is Player) {
				Game.State.ChangeExperience(RewardExp);
				Game.State.ChangeGold(RewardMoney);
			}
			Die();
		}
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
	public override bool IsDead() {
		return currentHealth <= 0;
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
    public override void GetStatus()
    {
        GD.Print("Enemy ",Name,", health:", currentHealth);
    }
}
