using Godot;

public partial class CameraControl : Node {
    public Vector2 Position {
        get {
            return _position;
        }
        set { 
            _position = value;
            camera.Position = _position + cameraOffset;
            listener.GlobalPosition = camera.GlobalPosition;
        }
    }
    private Vector2 _position;
	[Export] Camera2D camera;
	[Export] AudioListener2D listener;
	[Export] Vector2 cameraOffset;
    public override void _Process(double delta)
	{
		if(Game.Player != null) Position = Game.Player.Position;
	}
}