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
	[Export] float MaxLife = 0;

	List<Node2D> registeredBodies = new List<Node2D>();
	List<Node2D> affectedBodies = new List<Node2D>();
	Player source = null;
	float timer;

    internal void Setup(Player src)
    {
		source = src;
		if (source == null) {
			QueueFree();
			return;
		}
		// Add itself to same space as source.
		source.GetParent().AddChild(this);
		// Position over source.
		Position = source.Position;
		// Face the right direction.
        var s = Scale;
		s.X = source.GetHorzDirection();
		Scale = s;
    }

	public void OnAnimationFinished() {
		QueueFree();
	}

	public override void _Process(double delta)
	{
		if (source == null) return;
		if (FollowSource) Position = source.Position;
		var active = AlwaysActive || (timer > effectTimeRange.X && timer <= effectTimeRange.Y);
		if (active) {
			ApplyEffect();
		}
		timer += (float)delta;
		// Destroy after time.
		if (MaxLife > 0 && timer > MaxLife) QueueFree();
		//
		var move = speed * (float)delta;
		move.X = move.X * Scale.X;
		Position += move;
	}

	private void ApplyEffect() {
		bool applied = false;
		//
		foreach (var b in registeredBodies) {
			if (!affectedBodies.Contains(b)) {
				// Damage, etc.
				GD.Print(b.Name);
				applied = true;
				affectedBodies.Add(b);
			}
		}
		if (applied) {
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
