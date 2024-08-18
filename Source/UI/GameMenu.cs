using Godot;
using System;

public partial class GameMenu : Control
{
	[Export] Control[] screens;
	[Export] int currentScreenIdx;
	private Control lastFocused = null;

    public override void _Ready()
    {
        base._Ready();
		Visible = false;
    }

    public override void _Process(double delta)
    {
        base._Process(delta);
		// Selection change.
		var focused = GetViewport().GuiGetFocusOwner();
		if (focused != lastFocused) {
			if (focused != null) {
				if (lastFocused != null && IsInstanceValid(lastFocused) && lastFocused.Visible) {
					AudioManager.PlaySystemSound("cursor");
				}
			}
			lastFocused = focused;
		}
		if (!GetParent<Control>().IsVisibleInTree()) return;
		if (Game.Player == null) return;
		// Open/close pause menu.
		if (Game.Player.CanMove()) {
			var pause = Input.IsActionJustPressed("pause");
			if (pause) Game.TogglePause();
		}
		// Don't continue unless paused.
		if (!GetTree().Paused) return;
		// Change screen.
		var dash = Input.IsActionJustPressed("dash");
		if (dash) {
			AudioManager.PlaySystemSound("change");
			SwitchScreen(1);
		}
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
		AudioManager.PlaySystemSound("open");
		Visible = true;
		RefreshCurrentScreen();
	}
	public void Close() {
		AudioManager.PlaySystemSound("close");
		Visible = false;
	}
}
