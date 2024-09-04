using Godot;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

public partial class Enemy : BaseCharacter, IDamageable
{
	public struct AIOutputs {
		public float horz;
		public float vert;
		public bool jump;
		public bool jumpHeld;
		public bool attack;
		public Action[] actions;
	}
	//
	[Export][Range(1,999999)] int MaxHealth = 10;
	[Export][Range(1,999)] int Str = 1;
	[Export][Range(1,999)] int Con = 1;
	[Export][Range(1,999)] int Int = 1;
	[Export] int RewardExp = 2;
	[Export] int RewardMoney = 5;
	[Export] public BaseEnemyState[] chaseStates;
	private Dictionary<string,BaseEnemyState> _chaseStates = new Dictionary<string,BaseEnemyState>();
	[Export] Area2D detectionArea;
	[Export] float ReactionCooldown = 0.6f;
	[Export] float AggroMemoryCooldown = 2f;
	[Export] float DetectMemoryCooldown = 5f;
	[Export] Control healthDisplay;
	private int currentHealth;
	private Node2D lastTarget;
	private List<Node2D> brainTargets = new List<Node2D>();
	private Vector2 originalPos;
	private Vector2 brainTargetPos;
	public Vector2 BrainTargetPos => brainTargetPos;
	private float reactionTimer;
	private float lastTargetMemory;
	private float brainTargetCooldown;
	private ShaderMaterial _healthMaterial = null;
	Tween _healthTween = null;
    //
	public Node2D GetLastTarget() {
		return lastTarget;
	}
    public override void _Ready()
    {
        base._Ready();
		originalPos = GlobalPosition;
		currentHealth = MaxHealth;
		detectionArea.BodyEntered += OnBodySeen;
		detectionArea.BodyExited += OnBodyUnseen;
		if (healthDisplay.Material != null) {
			_healthMaterial = healthDisplay.Material.Duplicate(true) as ShaderMaterial;
			healthDisplay.Material = _healthMaterial;
		}
		if (graphic.Material != null) {
			graphic.Material = graphic.Material.Duplicate(true) as Material;
			
		}
		InitializeStates();
    }
	private void InitializeStates() {
		foreach (var s in chaseStates) _chaseStates.Add(s.id, s);
	}
	public override void _PhysicsProcess(double delta) {
		AIOutputs outputs = new AIOutputs();
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
				outputs = UpdateChaseBrain(delta, outputs);
			} else {
				outputs = UpdatePatrolBrain(delta, outputs);
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
		ProcessGroundMove(delta, outputs.horz, outputs.vert, outputs.jump, outputs.jumpHeld);
		ProcessAttack(delta,outputs.attack,GetAttackAction(outputs.actions));
		RefreshAnimation(delta);
		detectionArea.Scale = new Vector2(GetHorzDirection(), 1);
	}
	private AIOutputs UpdatePatrolBrain(double delta, AIOutputs outputs) {
		// Regular patrol/wander. TODO
		return outputs;
	}
	public bool IsReactionCooldown() {
		return reactionTimer > 0;
	}
	private string currentChaseState = "";
	private float currentChaseStateTimer = 0f;
	public float ChaseStateTimer=>currentChaseStateTimer;
	public BaseEnemyState GetCurrentChaseState() {
		if (_chaseStates.TryGetValue(currentChaseState, out var state)) return state;
		return null;
	}
	public void ChangeChaseState(string newStateId, double delta) {
		var curr = GetCurrentChaseState();
		if (curr != null) curr.Exit(this, delta);
		currentChaseState = newStateId;
		currentChaseStateTimer = 0f;
		var newState = GetCurrentChaseState();
		if (newState != null) newState.Enter(this, delta);
	}
	private AIOutputs UpdateChaseBrain(double delta, AIOutputs outputs) {
		if(reactionTimer > 0) reactionTimer -= (float)delta;
		currentChaseStateTimer += (float)delta;
		//
		var curr = GetCurrentChaseState();
		if (curr == null) {
			ChangeChaseState(chaseStates[0].id, delta);
		} else {
			outputs = curr.Update(this, delta, outputs);
		}
		//
		return outputs;
	}
	private Action GetAttackAction(Action[] selectedActions) {
		if (selectedActions == null) return null;
		if (selectedActions.Length > actionCounter) {
			var action = selectedActions[actionCounter];
			return action;
		}
		return null;
	}
	public bool CheckTargetDelta(Vector2 maxDeltas) {
		if (lastTarget==null) return false;
		var dirTarget = lastTarget.GlobalPosition - GlobalPosition;
		if (Math.Abs(dirTarget.X) > maxDeltas.X) return false;
		if (Math.Abs(dirTarget.Y) > maxDeltas.Y) return false;
		return true;
	}
	public bool CheckAttack(int cachedDir, Vector2 maxDeltas) {
		if (lastTarget==null) return false;
		if (cachedDir == 0) return false;
		var _exclude = new Godot.Collections.Array<Rid>{GetRid()};
		var dirTarget = lastTarget.GlobalPosition - GlobalPosition;
		dirTarget.X = Math.Clamp(dirTarget.X, -maxDeltas.X, maxDeltas.X);
		dirTarget.Y = Math.Clamp(dirTarget.Y, -maxDeltas.Y, maxDeltas.Y);
		if (DoQuery(new Vector2(0, 0),dirTarget,_exclude)) return true;
		return false;
	}
	private bool DoQuery(Vector2 offset, Vector2 direction, Godot.Collections.Array<Rid> exclude) {
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
	public bool CheckJump(int cachedDir) {
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
			if (damage < 1) damage = 1;
			OnBodySeen(source);
		}
		ChangeHealth(-damage);
		Game.DamagePop(GlobalPosition, damage);
		if (IsDead()) {
			if(source is Player) {
				Game.State.ChangeExperience(RewardExp);
				Game.State.ChangeMoney(RewardMoney);
			}
			Die();
		} else {
			animation.Play("damage");
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
