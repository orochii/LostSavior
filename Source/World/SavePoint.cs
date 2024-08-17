using Godot;
using System;

public partial class SavePoint : Node2D
{
	const float SAVE_COOLDOWN = 2f;
	[Export] Node2D playerStart;
	[Export] AnimationPlayer animation;
	[Export] Node2D activeIndicator;
	[Export] AudioEntry activationSound;
	float timer = 0;
	private bool IsActive {
		get {
			return Game.State.LastCheckpoint == playerStart.Name;
		}
	}
    public override void _Process(double delta)
    {
		if (timer > 0) timer -= (float)delta;
        activeIndicator.Visible = IsActive;
    }
    public void OnEnter(Node2D body) {
		if (body is Player) {
			if (timer <= 0) {
				Game.State.RecoverAll();
				Game.State.SetLastCheckpoint(playerStart.Name);
				Game.State.SaveGame();
				animation.Play("activation");
				AudioManager.PlaySound2D(activeIndicator.GlobalPosition, activationSound);
				timer = SAVE_COOLDOWN;
			}
		}
	}
}
