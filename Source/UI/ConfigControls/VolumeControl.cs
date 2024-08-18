using Godot;
using System;

public partial class VolumeControl : Button
{
	[Export] string propertyName;
	[Export] float increment = 1;
	[Export] HSlider slider;
    public override void _Ready()
    {
        Pressed += Press;
    }
	public void Refresh() {
		slider.Value = Value;
	}
    public void Press() {
		Move(1);
	}
	public void Move(int dir) {
		slider.Value += dir * increment;
		Value = (float)slider.Value;
		Refresh();
		Config.Refresh();
	}
	public void MoveLeft() {
		Move(-1);
	}
	public void MoveRight() {
		Move(1);
	}
	public float Value {
		get {
			var p = Config.Data.GetType().GetProperty(propertyName);
			if (p == null) return 0;
			var value = p.GetValue(Config.Data);
			return (float)value;
		}
		set {
			var p = Config.Data.GetType().GetProperty(propertyName);
			p.SetValue(Config.Data, value);
		}
	}
}
