using Godot;
using System;
using System.Collections.Generic;

public partial class EnemySpawner : Node2D
{
	[Export] public PackedScene EnemyTemplate;
	BaseCharacter[] livingEnemies;
	public override void _Ready()
	{
		var children = GetChildren();
		livingEnemies = new BaseCharacter[children.Count];
	}
	public void Respawn() {
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
				}
			}
		}
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
