using Godot;
using System;

public partial class StringDropdownControl : Button
{
	[Export] string propertyName;
	[Export] OptionButton optionButton;
    public override void _Ready()
    {
        Pressed += Press;
    }
	public void Refresh() {
		var v = Value;
		for (int i = 0; i < optionButton.ItemCount; i++) {
			if (optionButton.GetItemText(i) == v)
				optionButton.Selected = i;
		}
	}
    public void Press() {
		Move(1);
	}
	public void Move(int dir) {
		optionButton.Selected = (optionButton.Selected + dir + optionButton.ItemCount) % optionButton.ItemCount;
		Value = optionButton.GetItemText(optionButton.Selected);
		Refresh();
		Config.Refresh();
	}
	public void MoveLeft() {
		Move(-1);
	}
	public void MoveRight() {
		Move(1);
	}
	public string Value {
		get {
			var p = Config.Data.GetType().GetProperty(propertyName);
			if (p == null) return "";
			var value = p.GetValue(Config.Data);
			return (string)value;
		}
		set {
			var p = Config.Data.GetType().GetProperty(propertyName);
			p.SetValue(Config.Data, value);
		}
	}
}
