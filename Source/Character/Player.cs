using Godot;
using System;
using System.Collections;

public partial class Player : BaseCharacter, IDamageable
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
	[Export] public Action[] defaultAttackActions;
	[Export] public Action[] defaultAirAttackActions;
	public override void _PhysicsProcess(double delta)
    {
		var horz = 0f;
        var jump = false;
        var jumpHeld = false;
		var attack = false;
		if (!IsDead()) {
			horz = Input.GetAxis("move_left", "move_right");
			jump = Input.IsActionJustPressed("jump");
			jumpHeld = Input.IsActionPressed("jump");
			attack = Input.IsActionJustPressed("attack");
			if (CanHold()) attack = Input.IsActionPressed("attack");
		}
        // Execute attacks
        ProcessAttack(delta,attack,GetAttackAction());
		// Execute movement
        ProcessGroundMove(delta, horz, jump, jumpHeld);
    }
	protected bool CanHold() {
		var w = Game.State.GetWeapon();
		if (w != null) return w.CanHold;
		return false;
	}
	public Action[] GetAttackActions() {
		var w = Game.State.GetWeapon();
		if (!isGrounded) {
			if (w != null) return w.AirAttackActions;
			return defaultAirAttackActions;
		}
		if (w != null) return w.AttackActions;
		return defaultAttackActions;
	}
	private Action GetAttackAction() {
		var actions = GetAttackActions();
		if (actions.Length > actionCounter) {
			var action = actions[actionCounter];
			return action;
		}
		return null;
	}
	public void ApplyDamage(BaseCharacter source, int baseDmg, EDamageFormula damageFormula) {
		if (IsDead()) return;
		int damage = baseDmg;
		if (damage > 0) {
			int defense = DamageFormulas.CalculateDefense(this, damageFormula);
			damage -= defense;
			if (damage < 0) damage = 0;
		}
		Game.State.ChangeHealth(-damage);
		Game.DamagePop(GlobalPosition, damage);
		if (IsDead()) Die();
	}
	public bool IsDead() {
		return Game.State.GetHealth() <= 0;
	}
	public void Die() {
		
	}
    public override int GetStr()
    {
        return Game.State.GetStr();
    }
    public override int GetCon()
    {
        return Game.State.GetCon();
    }
    public override int GetInt()
    {
        return Game.State.GetInt();
    }
}
