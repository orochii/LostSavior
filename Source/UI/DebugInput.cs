using Godot;
using System;

public partial class DebugInput : Control
{
	[Export] Control[] controls;
	[Export] StringName[] actions;
	public override void _Ready()
	{
		foreach (var c in controls) c.Modulate = Colors.Transparent;
	}
	public override void _Process(double delta)
	{
		if (!IsVisibleInTree()) return;
		for (int i = 0; i < controls.Length; i++) {
			if (actions.Length > i) {
				var v = Input.IsActionPressed(actions[i]);
				if (v) controls[i].Modulate = Colors.White;
				else controls[i].Modulate = Colors.Transparent;
			}
		}
	}
}
