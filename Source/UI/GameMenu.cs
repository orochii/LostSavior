using Godot;
using System;

public partial class GameMenu : Control
{
	[Export] Control[] screens;
	[Export] int currentScreenIdx;

    public override void _Ready()
    {
        base._Ready();
		Visible = false;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
		var pause = Input.IsActionJustPressed("pause");
		if (pause) Game.TogglePause();

		if (!GetTree().Paused) return;

		var dash = Input.IsActionJustPressed("dash");
		if (dash) SwitchScreen(1);
    }

    private void SwitchScreen(int v)
    {
		currentScreenIdx = (currentScreenIdx + v + screens.Length) % screens.Length;
		RefreshCurrentScreen();
    }

	private void RefreshCurrentScreen() {
		for (int i = 0; i < screens.Length; i++) {
			if (i == currentScreenIdx) {
				screens[i].Visible = true;
				var m = screens[i].GetType().GetMethod("Refresh");
				if (m != null) m.Invoke(screens[i], null);
			}
			else {
				screens[i].Visible = false;
			}
		}
	}

    public void Open() {
		Visible = true;
		RefreshCurrentScreen();
	}
	public void Close() {
		Visible = false;
	}
}
