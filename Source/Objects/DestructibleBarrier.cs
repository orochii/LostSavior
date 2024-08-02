using Godot;
using System;

public partial class DestructibleBarrier : Node2D
{
	[Export] Vector2 forceDirection;
	[Export] float forceRandomness;
	[Export] bool useForcePosition;
	[Export] Vector2 forcePositionOffset;
	bool destroyed;

    public override async void _Ready()
    {
        base._Ready();
		await ToSignal(GetTree(), SceneTree.SignalName.ProcessFrame);
		var cs = GetChildren();
		foreach (var c in cs) {
			var rb = c as RigidBody2D;
			if (rb != null) {
				//
			}
		}
    }

    public void Destroy() {
		if (destroyed) return;
		var cs = GetChildren();
		var rbc = 0;
		foreach (var c in cs) {
			var rb = c as RigidBody2D;
			if (rb != null) {
				rb.GravityScale = 1;
				var rf = new Vector2(Random.Shared.NextSingle(), Random.Shared.NextSingle()).Normalized() * forceRandomness;
				var df = forceDirection;
				if (useForcePosition) {
					var p = GlobalPosition + forcePositionOffset;
					rb.ApplyForce(df + rf, p);
				} else {
					rb.ApplyForce(df + rf);
				}
				rbc++;
			}
		}
		destroyed = true;
	}
}
