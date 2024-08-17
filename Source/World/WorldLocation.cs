using Godot;
using System;

public partial class WorldLocation : Area2D
{
	public string Id => id;
	[Export] public string id;
	[Export] public AudioEntry music;
    public override void _Ready()
    {
        BodyEntered += OnEnter;
		BodyExited += OnExit;
    }
    public void OnEnter(Node2D body) {
		if (body is Player) {
			if (Game.World.EnterLocation(this)) {
				Respawn();
			}
		}
	}
	public void OnExit(Node2D body) {
		if (body is Player) {
			if (Game.World.ExitLocation(this)) {
				//
				//Cleanup();
			}
		}
	}
	//
	private void Respawn() {
		foreach (var c in GetChildren()) {
			if (c is EnemySpawner) {
				var spawner = c as EnemySpawner;
				spawner.CallDeferred("Respawn");
			}
			// TODO Here I could also do other variants, like location sections, etc.
		}
	}
	public void Cleanup() {
		foreach (var c in GetChildren()) {
			if (c is EnemySpawner) {
				var spawner = c as EnemySpawner;
				spawner.CallDeferred("Clear");
			}
			// TODO Here I could also do other variants, like location sections, etc.
		}
	}
}
