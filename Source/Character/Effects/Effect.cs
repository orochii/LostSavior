using Godot;
using System;
using System.Collections.Generic;

public partial class Effect : Area2D
{
	[Export] bool FollowSource;
	[Export] Vector2 speed;
	[Export] bool AlwaysActive;
	[Export] Vector2 effectTimeRange = new Vector2(0.1f,0.15f);
	[Export] bool KillAfterEffect;
	[Export] AudioEntry onSpawnSound;
	[Export] AudioEntry onHitSound;
	[Export] float MaxLife = 0;
	[Export] EDamageFormula damageFormula;
	[Export] int damageBase;
	List<Node2D> registeredBodies = new List<Node2D>();
	List<Node2D> affectedBodies = new List<Node2D>();
	BaseCharacter source = null;
	float timer = 0;
	Vector2 offset;
    internal void Setup(BaseCharacter src, Action _action)
    {
		source = src;
		if (source == null) {
			QueueFree();
			return;
		}
		var dir = source.GetHorzDirection();
		if (_action.FlipH) dir *= -1;
		// Add itself to same space as source.
		source.GetParent().AddChild(this);
		// Position over source.
		offset = new Vector2(_action.OffsetAndRotation.X, _action.OffsetAndRotation.Y);
		RotationDegrees = _action.OffsetAndRotation.Z * dir;
		// Face the right direction.
		var s = Scale;
		s.X = dir;
		if (_action.FlipV) s.Y = -1;
		Scale = s;
		InitPosition();
		if (onSpawnSound != null) AudioManager.PlaySound2D(GlobalPosition, onSpawnSound);
    }
	public void InitPosition() {
		var _off = new Vector2(offset.X * Scale.X, offset.Y);
		Position = source.Position + _off;
	}
	public void UpdatePosition(double delta) {
		if (FollowSource) {
			InitPosition();
		}
		else {
			var move = speed * (float)delta;
			move.X = move.X * Scale.X;
			Position += move.Rotated(Rotation);
		}
	}
	public void OnAnimationFinished() {
		QueueFree();
	}
	public override void _Process(double delta)
	{
		if (source == null) return;
		var active = AlwaysActive || (timer > effectTimeRange.X && timer <= effectTimeRange.Y);
		if (active) ApplyEffect();
		timer += (float)delta;
		// Destroy after time.
		if (MaxLife > 0 && timer > MaxLife) QueueFree();
		//
		UpdatePosition(delta);
	}
	private void ApplyEffect() {
		bool applied = false;
		//
		foreach (var b in registeredBodies) {
			if (!affectedBodies.Contains(b)) {
				// Damage, etc.
				if (b is IDamageable) {
					var d = b as IDamageable;
					if (d.IsDead()) continue;
					var dmg = DamageFormulas.CalculateDamage(source,damageFormula,damageBase);
					d.ApplyDamage(source, dmg, damageFormula);
				}
				applied = true;
				affectedBodies.Add(b);
			}
		}
		if (applied) {
			if (onHitSound != null) AudioManager.PlaySound2D(GlobalPosition, onHitSound);
			if (KillAfterEffect) QueueFree();
		}
	}
	public void OnBodyEntered(Node2D body) {
		if (body == source) return;
		if (!registeredBodies.Contains(body)) registeredBodies.Add(body);
	}
	public void OnBodyExited(Node2D body) {
		if (registeredBodies.Contains(body)) registeredBodies.Remove(body);
	}
}
