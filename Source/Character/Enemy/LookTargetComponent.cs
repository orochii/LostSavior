using Godot;
using System;

public partial class LookTargetComponent : Node2D
{
	[Export] Enemy baseCharacter;
	[Export] Node2D element;
	public override void _Process(double delta)
	{
		var target = baseCharacter.GetLastTarget();
		if (target != null) {
			var deltaPos = target.GlobalPosition - element.GlobalPosition;
			var angle = deltaPos.Angle();
			if (element.Scale.X <= 0) element.Rotation = angle - (Mathf.DegToRad(180));
			else element.Rotation = angle;
		} else {
			element.Rotation = 0;
		}
	}
}
