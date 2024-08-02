using Godot;
using System;

public partial class Worldmap : Control
{
	public static Worldmap Instance;

	[Export] SubViewport thisViewport;
	[Export] SubViewport parentViewport;
	[Export] Camera2D minimapCamera;
	[Export] Node2D playerSprite;
	
	public override void _Ready()
	{
		Instance = this;
		thisViewport.World2D = parentViewport.World2D;
	}

	public void Reposition() {
		if (Game.Player != null) {
			minimapCamera.Position = Game.Player.Position;
		}
	}

	public override void _Process(double delta)
	{
		if (!Visible) return;

		var pause = Input.IsActionJustPressed("pause");
		if (pause) Game.Player.TogglePause();

		if (Game.Player != null) {
			var pos = Game.Player.Position;
			var center = minimapCamera.Position;
			var relative = pos - center;
			playerSprite.Position = relative * minimapCamera.Zoom - new Vector2(0,2);

			playerSprite.Visible = true;
		} else {
			playerSprite.Visible = false;
		}

		var horz = Input.GetAxis("move_left","move_right");
		var vert = Input.GetAxis("move_up","move_down");
		minimapCamera.Position += new Vector2(horz,vert) * (float)delta * 256;
	}
}
