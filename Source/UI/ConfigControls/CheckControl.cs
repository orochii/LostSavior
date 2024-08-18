using Godot;
using System;

public partial class CheckControl : Button
{
	[Export] string propertyName;
	[Export] CheckButton checkButton;
    public override void _Ready()
    {
        Pressed += Press;
    }
	public void Refresh() {
		checkButton.ButtonPressed = Value;
	}
    public void Press() {
		if (Value) Move(-1);
		else Move(1);
	}
	public void Move(int dir) {
		checkButton.ButtonPressed = dir>0;
		Value = (bool)checkButton.ButtonPressed;
		Refresh();
		Config.Refresh();
	}
	public void MoveLeft() {
		Move(-1);
	}
	public void MoveRight() {
		Move(1);
	}
	public bool Value {
		get {
			var p = Config.Data.GetType().GetProperty(propertyName);
			if (p == null) return false;
			var value = p.GetValue(Config.Data);
			return (bool)value;
		}
		set {
			var p = Config.Data.GetType().GetProperty(propertyName);
			p.SetValue(Config.Data, value);
		}
	}
}
