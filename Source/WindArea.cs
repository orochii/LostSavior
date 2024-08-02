using Godot;
using System;
using System.Collections.Generic;

public partial class WindArea : Area2D
{
	[Export] Vector2 forceDirection;
	[Export] Texture2D strengthTexture;
	[Export] float strengthMultiplier = 1;
	[Export] float timerMultiplier = 0.2f;
	
	VisibleOnScreenNotifier2D notifier;
	CollisionShape2D shape;
	double timer = 0;
	bool onScreen = false;

	List<PhysicsBody2D> bodiesInside = new List<PhysicsBody2D>();

    public override void _Ready()
    {
        base._Ready();
		Setup();
    }
	private void Setup() {
		// Look up dependencies.
		foreach (var c in GetChildren()) {
			if (c is CollisionShape2D) shape = c as CollisionShape2D;
			if (c is VisibleOnScreenNotifier2D) notifier = c as VisibleOnScreenNotifier2D;
		}
		// Configure
		if (shape != null) {
			if (notifier != null) {
				notifier.ScreenEntered += EnteredScreen;
				notifier.ScreenExited += ExitedScreen;
				var r = shape.Shape.GetRect();
				r.Position -= shape.Position;
				notifier.Rect = r;
			}	
		}
	}

    public override void _Process(double delta)
	{
		// Ignore if not on screen.
		if (!onScreen) return;
		// Process objects within area.
		timer += delta * timerMultiplier;
		var tw = strengthTexture.GetWidth();
		if (timer > tw) timer -= tw;
		foreach (var b in bodiesInside) {
			if (b is RigidBody2D) {
				var x = b.Position.X + timer;
				if (x > tw) x -= tw;
				var y = b.Position.Y;
				var c = strengthTexture.GetImage().GetPixel((int)x,(int)y);
				var s = c.R * c.R * strengthMultiplier;
				var rb = b as RigidBody2D;
				rb.ApplyForce(forceDirection * s);
				//GD.Print(s);
			}
		}
	}

	public void OnBodyEntered(Node2D body) {
		if (body is RigidBody2D || body is CharacterBody2D) {
			var pb = body as PhysicsBody2D;
			if (!bodiesInside.Contains(pb)) {
				bodiesInside.Add(pb);
				GD.Print("Added ", pb.Name);
			}
		}
	}
	public void OnBodyExited(Node2D body) {
		if (body is RigidBody2D || body is CharacterBody2D) {
			var pb = body as PhysicsBody2D;
			if (bodiesInside.Contains(pb)) {
				bodiesInside.Remove(pb);
				GD.Print("Removed ", pb.Name);
			}
		}
	}

	void EnteredScreen() {
		onScreen = true;
	}
	void ExitedScreen() {
		onScreen = false;
	}
}
