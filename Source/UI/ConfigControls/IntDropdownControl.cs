using Godot;
using System;

public partial class IntDropdownControl : Button
{
	[Export] string propertyName;
	[Export] OptionButton optionButton;
    public override void _Ready()
    {
        Pressed += Press;
    }
	public void Refresh() {
		optionButton.Selected = Value;
	}
    public void Press() {
		Move(1);
	}
	public void Move(int dir) {
		optionButton.Selected = (optionButton.Selected + dir + optionButton.ItemCount) % optionButton.ItemCount;
		Value = optionButton.Selected;
		Refresh();
		Config.Refresh();
	}
	public void MoveLeft() {
		Move(-1);
	}
	public void MoveRight() {
		Move(1);
	}
	public int Value {
		get {
			var p = Config.Data.GetType().GetProperty(propertyName);
			if (p == null) return 0;
			var value = p.GetValue(Config.Data);
			return (int)value;
		}
		set {
			var p = Config.Data.GetType().GetProperty(propertyName);
			p.SetValue(Config.Data, value);
		}
	}
}
