using Godot;
using System;

public partial class DmgPop : Node2D
{
	[Export] Label label;
	public void Setup(int dmg) {
		label.Text = dmg.ToString();
	}
	public override void _Process(double delta)
	{
	}
}
