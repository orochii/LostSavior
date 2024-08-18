using Godot;
using System;
using System.Collections.Generic;

public partial class ConfigMenu : Control
{
	public int LastHudMode = 0;
	[Export] Control FirstFocus;
	[Export] Container OptionsContainer;
	private Control _lastFocus;
	private List<Control> controls;
    public override void _Ready()
    {
        var children = OptionsContainer.GetChildren();
		controls= new List<Control>();
		foreach(var c in children) {
			if (c is Control) controls.Add(c as Control);
		}
		UIUtils.SetupHBoxList(controls);
		Visible = false;
    }
    public void Refresh() {
		//
		foreach (var c in controls) {
			var m = c.GetType().GetMethod("Refresh");
			if (m != null) m.Invoke(c, null);
		}
		//
		//if (_lastFocus != null) _lastFocus.GrabFocus();
		//else 
		FirstFocus.GrabFocus();
	}
    public override void _Process(double delta)
    {
        base._Process(delta);
		if (!IsVisibleInTree()) return;
		if (Input.IsActionJustPressed("ui_left")) {
			var c = GetViewport().GuiGetFocusOwner();
			var m = c.GetType().GetMethod("MoveLeft");
			if (m != null) m.Invoke(c, null);
			return;
		}
		if (Input.IsActionJustPressed("ui_right")) {
			var c = GetViewport().GuiGetFocusOwner();
			var m = c.GetType().GetMethod("MoveRight");
			if (m != null) m.Invoke(c, null);
			return;
		}
		if (Input.IsActionJustPressed("ui_cancel")) {
			GD.Print("Cancel config.");
			Game.Instance.CloseConfig();
		}
    }
}
