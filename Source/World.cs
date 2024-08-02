using Godot;
using System;

public partial class World : Node2D
{
	[Export] Node2D PlayerStart;

	public void RepositionPlayer() {
		if(PlayerStart != null) Game.Player.Position = PlayerStart.Position;
	}
}
