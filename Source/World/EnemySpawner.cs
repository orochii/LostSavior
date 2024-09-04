using Godot;
using System;
using System.Collections.Generic;

public partial class EnemySpawner : Node2D
{
	[Export] public PackedScene EnemyTemplate;
	[Export] public int SpawnsPerFrame = 2;
	BaseCharacter[] livingEnemies;
	bool _busy;
	public override void _Ready()
	{
		var children = GetChildren();
		livingEnemies = new BaseCharacter[children.Count];
	}
	public async void Respawn() {
		if (_busy==true) return;
		_busy = true;
		int remainingSpawns = SpawnsPerFrame;
		var children = GetChildren();
		for (int i = 0; i < children.Count; i++) {
			var c = children[i] as Node2D;
			if (c != null) {
				// Check if enemy is alive.
				bool alive = IsAlive(livingEnemies[i]);
				// Respawn if not alive.
				if (!alive) {
					var template = EnemyTemplate;
					// TODO: Redefine template to use.
					var newEnemy = template.Instantiate<BaseCharacter>();
					Game.World.AddChild(newEnemy);
					newEnemy.GlobalPosition = c.GlobalPosition;
					livingEnemies[i] = newEnemy;
					remainingSpawns -= 1;
					if (remainingSpawns <= 0) {
						//await ToSignal(GetTree(), "process_frame");
						await ToSignal(GetTree().CreateTimer(1.1f, false), SceneTreeTimer.SignalName.Timeout);
						remainingSpawns = SpawnsPerFrame;
					}
				}
			}
		}
		_busy = false;
	}
	public void Clear() {
		GD.Print("Clearing " + Name + "@" + GetParent().Name);
		for (int i=0; i < livingEnemies.Length; i++) {
			if (livingEnemies[i] != null && IsInstanceValid(livingEnemies[i])) {
				livingEnemies[i].QueueFree();
				livingEnemies[i] = null;
			}
		}
	}
	private bool IsAlive(BaseCharacter b) {
		if (b == null) return false;
		if (IsInstanceValid(b)) {
			return !b.IsDead();
		}
		return false;
	}
}
