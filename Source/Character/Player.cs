using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

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
	[Export] public AnimationPlayer levelUpAnimation;
	[Export] public AnimationPlayer gameoverAnimation;
	[Export] public Node2D CardContainer;
	[Export] public Node2D InteractIndicator;
	public Node2D[] CardObject = new Node2D[GameState.MAX_CARD_SLOTS];
	public override void _PhysicsProcess(double delta)
    {
		var horz = 0f;
		var vert = 0f;
        var jump = false;
        var jumpHeld = false;
		var attack = false;
		if (CanMove()) {
			//
			if (Input.IsActionJustPressed("swap_weapon")) {
				Game.State.SwitchWeapon();
			}
			//
			horz = Input.GetAxis("move_left", "move_right");
			vert = Input.GetAxis("move_up", "move_down");
			jump = Input.IsActionJustPressed("jump");
			jumpHeld = Input.IsActionPressed("jump");
			attack = Input.IsActionJustPressed("attack");
			if (CanHold()) attack = Input.IsActionPressed("attack");
			// Process cards
			ProcessCards(delta);
			// Process dash
			ProcessDash();
		}
        // Execute attacks
		FindClosestEvent();
		if (attack && CheckCloseEvents()) {

		} else {
			ProcessAttack(delta,attack,GetAttackAction());
		}
		// Execute movement
        ProcessGroundMove(delta, horz, vert, jump, jumpHeld);
		CardContainer.Scale = new Vector2(GetHorzDirection(),1);
		// Refresh animation.
		RefreshAnimation(delta);
    }
	private void ProcessDash() {
		if (isGrounded && !IsDashing()) {
			if (Input.IsActionJustPressed("dash")) {
				ExecuteAction(GetDashingAction());
			}
		}
	}
	protected override Action GetDashingAction() {
		return Game.State.Data.dashAction;
	}
	private void ProcessCards(double delta) {
		for (int i = 0; i < GameState.MAX_CARD_SLOTS; i++) {
			var card = Game.State.GetEquippedCard(i);
			if (card != null) {
				card.VerifyVisuals(this, i);
				card.Process(this, i, delta);
			}
			else {
				if (CardObject[i] != null) {
					CardObject[i].QueueFree();
					CardObject[i] = null;
				}
			}
		}
	}
	protected bool CanHold() {
		var w = Game.State.GetWeapon();
		if (w != null) return w.CanHold;
		return false;
	}
	public Action[] GetAttackActions() {
		var vert = Input.GetAxis("move_up", "move_down");
		// Do weapon actions if you have any.
		var w = Game.State.GetWeapon();
		if (w != null) {
			if (vert > 0) {
				if (w.DownActions != null && w.DownActions.Length > 0) return w.DownActions;
			} else if (vert < 0) {
				if (w.UpActions != null && w.UpActions.Length > 0) return w.UpActions;
			}
			if (!isGrounded) if (w != null) return w.AirAttackActions;
			return w.AttackActions;
		}
		// Do default actions.
		if (vert > 0) {
			if (Game.State.Data.defaultDownActions != null && Game.State.Data.defaultDownActions.Length > 0) return Game.State.Data.defaultDownActions;
		} else if (vert < 0) {
			if (Game.State.Data.defaultUpActions != null && Game.State.Data.defaultUpActions.Length > 0) return Game.State.Data.defaultUpActions;
		}
		if (!isGrounded) return Game.State.Data.defaultAirActions;
		return Game.State.Data.defaultActions;
	}
	private Action GetAttackAction() {
		var actions = GetAttackActions();
		if (actions.Length > actionCounter) {
			var action = actions[actionCounter];
			return action;
		}
		return null;
	}
	public override void ApplyDamage(BaseCharacter source, int baseDmg, EDamageFormula damageFormula) {
		if (IsDead()) return;
		int damage = baseDmg;
		if (damage > 0) {
			int defense = DamageFormulas.CalculateDefense(this, damageFormula);
			damage -= defense;
			if (damage < 1) damage = 1;
		}
		Game.State.ChangeHealth(-damage);
		Game.DamagePop(GlobalPosition, damage);
		if (IsDead()) Die();
		else {
			animation.Play("damage");
		}
	}
	public override bool IsDead() {
		return Game.State.GetHealth() <= 0;
	}
	public override void Die() {
		base.Die();
		Game.State.MuteLocation = true;
		AudioManager.StopMusic();
		AudioManager.PlayJingle(Game.State.Data.OnDeathJingle);
		gameoverAnimation.Play("standby");
		// show "selection" maybe.
		// when selected, respawn.
		Game.Instance.SpawnPlayer(true, 3f);
	}
	public void ShowLevelUp() {
		levelUpAnimation.Play("standby");
		AudioManager.PlayJingle(Game.State.Data.OnLevelUpJingle);
	}
	public async Task<bool> Disappear() {
		animation.Play("disappear");
		await ToSignal(animation, AnimationPlayer.SignalName.AnimationFinished);
		return true;
	}
	public async Task<bool> Appear() {
		animation.Play("appear");
		await ToSignal(animation, AnimationPlayer.SignalName.AnimationFinished);
		return true;
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
	//
	private List<Event> _closebyEvents = new List<Event>();
	private Event _closestEvent = null;
	public void RegisterCloseEvent(Event ev) {
		if (!_closebyEvents.Contains(ev)) _closebyEvents.Add(ev);
	}
	public void UnregisterCloseEvent(Event ev) {
		if (_closebyEvents.Contains(ev)) _closebyEvents.Remove(ev);
	}
	private void FindClosestEvent() {
		_closestEvent = null;
		float dst = 0;
		foreach (var ev in _closebyEvents) {
			if (_closestEvent == null) {
				_closestEvent = ev;
				dst = ev.GlobalPosition.DistanceSquaredTo(GlobalPosition);
			} else {
				var d = ev.GlobalPosition.DistanceSquaredTo(GlobalPosition);
				if (d < dst) {
					_closestEvent = ev;
					dst = d;
				}
			}
		}
		//
		if (_closestEvent == null) InteractIndicator.Visible = false;
		else {
			InteractIndicator.Visible = true;
			InteractIndicator.GlobalPosition = _closestEvent.GlobalPosition;
		}
	}
	private bool CheckCloseEvents() {
		if (!CanMove()) return false;
		if (_closestEvent != null) {
			_closestEvent.Execute();
			return true;
		}
		return false;
	}
}
