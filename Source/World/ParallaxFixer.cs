using Godot;
using System;

public partial class ParallaxFixer : Sprite2D
{
	public override void _Ready()
	{
		// Get parent, 
		var parent = GetParent<Parallax2D>();
		if (parent != null) {
			Position = Position * parent.ScrollScale;
		}
	}

}
