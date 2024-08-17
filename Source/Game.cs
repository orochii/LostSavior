using Godot;
using System;
using System.Collections;
using System.Collections.Generic;

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
	public static World World {
		get {
			if (game == null) return null;
			return game._world;
		}
	}
	public static DialogueManager Dialogue {
		get {
			if (game == null) return null;
			return game.dialogueManager;
		}
	}
	public static PopUpManager PopUp {
		get {
			if (game == null) return null;
			return game.popupManager;
		}
	}
	public static LocationNameDisplay LocationName {
		get {
			if (game == null) return null;
			return game.locationNameDisplay;
		}
	}
	[Signal] public delegate void OnFlagUpdateEventHandler(string id, int v);
	[Export] public PackedScene playerTemplate;
	[Export] public PackedScene worldTemplate;
	[Export] public PackedScene dmgpopTemplate;
	[Export] public Godot.Collections.Array<Key> keyboardMappings;
	[Export] public Godot.Collections.Array<JoyButton> joyButtonMappings;
	[Export] public Godot.Collections.Array<JoyAxis> joyAxisMappings;
	[Export] public CameraControl cameraControl;
	
	[Export] public SubViewport viewport;
	[Export] CanvasLayer Canvas;
	[Export] Control UIParent;
	[Export] TitleMenu TitleScreen;
	//[Export] ConfigMenu ConfigScreen;
	[Export] GameMenu gameMenu;
	[Export] DialogueManager dialogueManager;
	[Export] PopUpManager popupManager;
	[Export] LocationNameDisplay locationNameDisplay;
	Player _player;
	World _world;
	private string SnapPath() {
		return "user://Snap/";
	}
    public override void _Ready()
	{
		game = this;
		DirAccess.MakeDirRecursiveAbsolute(SnapPath());
		ProcessMappings();
		// Create general components.
		Config.Init();
		AudioManager.Init();
		// Set HUD Mode to System
		SetHudMode(1);
	}
	public void StartGame() {
		// Go to gameplay mode.
		SetHudMode(0);
		// Create world
		var world = worldTemplate.Instantiate();
		viewport.AddChild(world);
		_world = world as World;
		SpawnPlayer(false);
	}
	public void SetHudMode(int idx) {
		for (int i = 0; i < UIParent.GetChildCount(); i++) {
			var c = UIParent.GetChild(i) as Control;
			if (c != null) c.Visible = i == idx;
		}
	}
	public void Resize(int width, int height, int scale) {
		Vector2I screenSize = new Vector2I(width,height);
		// Window size
		GetWindow().Size = screenSize * scale;
		// Viewport size
		viewport.Size2DOverride = screenSize;
		// UI: set parent and canvas scale
		Canvas.Scale = new Vector2(scale,scale);
		UIParent.AnchorRight = 1f / scale;
		UIParent.AnchorBottom= 1f / scale;
	}
	public async void SpawnPlayer(bool showAppear, float delay=0) {
		if(delay > 0) {
			await ToSignal(GetTree().CreateTimer(delay, false), SceneTreeTimer.SignalName.Timeout);
		}
		Game.State.MuteLocation = false;
		// Destroy current player (if any).
		if (_player != null) {
			// Disappear
			await _player.Disappear();
			// Destroy
			_player.QueueFree();
			_player = null;
		}
		// Create player
		_player = playerTemplate.Instantiate<Player>();
		viewport.AddChild(_player);
		_player.Position = new Vector2(160,128);
		// Position player
		if (_world != null) {
			_world.InitializePlayer();
		}
		if (showAppear) {
			Game.State.RecoverAll();
			_player.Appear();
		}
		_world.ReplayLocationMusic();
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
	public static void DamagePop(Vector2 pos, int dmg) {
		if (game == null) return;
		var dmgpop = game.dmgpopTemplate.Instantiate<DmgPop>();
		dmgpop.Setup(dmg);
		game._world.AddChild(dmgpop);
		dmgpop.Position = pos;
	}
	Dictionary<Key,int> _keyboardMappings;
	Dictionary<JoyButton,int> _joyButtonMappings;
	Dictionary<JoyAxis,int> _joyAxisMappings;
	private void ProcessMappings() {
		_keyboardMappings = new Dictionary<Key, int>();
		for (int i = 0; i < keyboardMappings.Count; i++) {
			var m = keyboardMappings[i];
			_keyboardMappings.Add(m, i);
		}
		_joyButtonMappings = new Dictionary<JoyButton, int>();
		for (int i = 0; i < joyButtonMappings.Count; i++) {
			var m = joyButtonMappings[i];
			_joyButtonMappings.Add(m, i);
		}
		_joyAxisMappings = new Dictionary<JoyAxis, int>();
		for (int i = 0; i < joyAxisMappings.Count; i++) {
			var m = joyAxisMappings[i];
			_joyAxisMappings.Add(m, i);
		}
	}
	public int GetKeyboardMappingIndex(Key k) {
		bool f = _keyboardMappings.TryGetValue(k, out var idx);
		if (f) return idx;
		return 0;
	}
	public int GetJoyButtonMappingIndex(JoyButton b) {
		bool f = _joyButtonMappings.TryGetValue(b, out var idx);
		if (f) return idx;
		return 0;
	}
	public int GetJoyAxisMappingIndex(JoyAxis a, float v) {
		bool f = _joyAxisMappings.TryGetValue(a, out var idx);
		if (f) {
			if (v > 0) return idx;
			else return idx+8;
		}
		return 0;
	}
    //
	private int _lastInput = 0;
	public int LastInput => _lastInput;
    public override void _Input(InputEvent @event)
    {
        base._Input(@event);
		switch (@event) {
			case InputEventKey:
				// Print screen
				var key = @event as InputEventKey;
				if (key.Pressed) {
					if (key.Keycode == Key.F9) TakeScreenshot();
				}
				_lastInput = 0;
				break;
			case InputEventJoypadButton:
				_lastInput = 1;
				break;
			case InputEventJoypadMotion:
				_lastInput = 1;
				break;
		}
    }
	public void TakeScreenshot() {
		ulong timestamp = (ulong)(Time.GetUnixTimeFromSystem() * 1000);
		var name = SnapPath() + timestamp.ToString() + ".png";
		var img = GetViewport().GetTexture().GetImage();
		img.SavePng(name);
	}
}
