using Godot;
using System;

public partial class Game : Control
{
	static Game game;
	public static GameState State = new GameState();
	public static Game Instance => game;
	public static Player Player {
		get {
			if (game == null) return null;
			return game._player;
		}
	}
	
	[Export] public PackedScene playerTemplate;
	[Export] public PackedScene worldTemplate;
	[Export] public SubViewport viewport;
	[Export] public Camera2D camera;
	[Export] public Vector2 cameraOffset;
	[Export] GameMenu gameMenu;
	
	Player _player;
	World _world;
	
	public override void _Ready()
	{
		game = this;
		// Create world
		var world = worldTemplate.Instantiate();
		viewport.AddChild(world);
		_world = world as World;
		// Create player
		var instance = playerTemplate.Instantiate<Node2D>();
		viewport.AddChild(instance);
		instance.Position = new Vector2(160,128);
		_player = instance as Player;
		// Position player
		if (_world != null) {
			_world.RepositionPlayer();
		}
	}

	public override void _Process(double delta)
	{
		if(_player != null) camera.Position = _player.Position + cameraOffset;
	}

	public static void TogglePause() {
		if (game != null) game.TogglePauseInternal();
	}
	public async void TogglePauseInternal() {
		if (gameMenu != null) {
			GetTree().Paused = !GetTree().Paused;
			await ToSignal(GetTree(), "process_frame");
			await ToSignal(GetTree(), "process_frame");
			if (GetTree().Paused) gameMenu.Open();
			else gameMenu.Close();
		}
	}
}
