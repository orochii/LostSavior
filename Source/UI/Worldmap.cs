using Godot;
using System;

public partial class Worldmap : Control
{
	[Export] SubViewport thisViewport;
	[Export] Camera2D minimapCamera;
	[Export] Node2D playerSprite;
	
	public async override void _Ready()
	{
		await ToSignal(GetTree(), "process_frame");
		thisViewport.World2D = Game.Instance.viewport.World2D;
	}

	public void Reposition() {
		if (Game.Player != null) {
			minimapCamera.Position = Game.Player.Position;
		}
	}

	public override void _Process(double delta)
	{
		if (!IsVisibleInTree()) return;

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

	public void Refresh() {
		Reposition();
	}
}
