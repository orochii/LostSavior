using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

public enum ESpeaker { LEFT, MIDDLE, RIGHT }

public partial class DialogueManager : Control
{
	const int LINES_FULLY_VISIBLE = 1;
	const int LINES_FADING_TO_INVISIBLE = 4;
	const float BLINK_TIME_OPEN = 2f;
	const float BLINK_TIME_CLOSE = .1f;
	const float MOUTH_TIME_CHANGE = .1f;
	[Export] PackedScene dialogueBoxTemplate;
	[Export] PackedScene shopBoxTemplate;
	[Export] Container container;
	[Export] AnimationPlayer animation;
	[Export] Sprite2D leftArt;
	[Export] Sprite2D leftEyelids;
	[Export] Sprite2D leftMouth;
	[Export] Sprite2D rightArt;
	[Export] Sprite2D rightEyelids;
	[Export] Sprite2D rightMouth;
	List<Control> existingLines = new List<Control>();
	DialogueBox _lastLine = null;
	[Signal] public delegate void KeyPressEventHandler();
	bool waitingInput = false;
	Tween _leftArtTween = null;
	Tween _rightArtTween = null;
	float leftBlinkTimer = 0;
	float rightBlinkTimer = 0;
	float mouthTimer = 0;
	bool mouthState = false;
    public override void _Process(double delta)
    {
        base._Process(delta);
		if (leftBlinkTimer > 0) leftBlinkTimer -= (float)delta;
		else {
			leftEyelids.Visible = !leftEyelids.Visible;
			if (leftEyelids.Visible) leftBlinkTimer = BLINK_TIME_CLOSE;
			else leftBlinkTimer = BLINK_TIME_OPEN + Random.Shared.NextSingle();
		}
		if (rightBlinkTimer > 0) rightBlinkTimer -= (float)delta;
		else {
			rightEyelids.Visible = !rightEyelids.Visible;
			if (rightEyelids.Visible) rightBlinkTimer = BLINK_TIME_CLOSE;
			else rightBlinkTimer = BLINK_TIME_OPEN + Random.Shared.NextSingle();
		}
		if (waitingInput && _lastLine != null) {
			if (_lastLine.Finished) {
				leftMouth.Visible = false;
				rightMouth.Visible = false;
			} else {
				mouthTimer -= (float)delta;
				if (mouthTimer < 0) {
					mouthState = !mouthState;
					mouthTimer = MOUTH_TIME_CHANGE;
				}
				leftMouth.Visible = _lastLine.Speaker == ESpeaker.LEFT && mouthState;
				rightMouth.Visible = _lastLine.Speaker == ESpeaker.RIGHT && mouthState;
			}
			if (Input.IsActionJustPressed("ui_accept")) {
				if (_lastLine.Finished) EmitSignal(SignalName.KeyPress);
				else {
					_lastLine.ShowAll();
				}
			}
		} else {
			mouthTimer = 0;
			mouthState = false;
			leftMouth.Visible = false;
			rightMouth.Visible = false;
		}
    }
    public void Open(Texture2D l=null, Texture2D r=null) {
		leftArt.Texture = l;
		leftEyelids.Texture = l;
		leftMouth.Texture = l;
		rightArt.Texture = r;
		rightEyelids.Texture = r;
		rightMouth.Texture = r;
		FlushLines();
		animation.Play("enter");
	}
	public void Close() {
		animation.Play("exit");
	}
	public void SetLeftArt(Texture2D t) {
		leftArt.Texture = t;
		leftEyelids.Texture = t;
		leftMouth.Texture = t;
		if (_leftArtTween != null) _leftArtTween.Kill();
		_leftArtTween = TweenArt(leftArt);
	}
	public void SetRightArt(Texture2D t) {
		rightArt.Texture = t;
		rightEyelids.Texture = t;
		rightMouth.Texture = t;
		if (_rightArtTween != null) _rightArtTween.Kill();
		_rightArtTween = TweenArt(rightArt);
	}
	private Tween TweenArt(Sprite2D art) {
		var startPos = new Vector2(16,16);
		art.Position = startPos;
		art.Modulate = Colors.Transparent;
		var tw = CreateTween();
		tw.TweenMethod(Callable.From((float i)=>{
			art.Modulate = Colors.Transparent.Lerp(Colors.White, i);
			art.Position = startPos.Lerp(Vector2.Zero, i);
		}),0f,1f,0.5f);
		return tw;
	}
	public Texture2D GetArt(string name) {
		if (name.Length == 0) return null;
		var path = string.Format("res://Graphics/Artwork/{0}.png", name);
		return OZResourceLoader.Load<Texture2D>(path);
	}
	public async Task<bool> SpawnLine(string text, ESpeaker speaker) {
		var line = dialogueBoxTemplate.Instantiate<DialogueBox>();
		line.SetContents(text, speaker);
		_lastLine = line;
		container.AddChild(line);
		existingLines.Add(line);
		RefreshLineSettings();
		waitingInput = true;
		await ToSignal(this, SignalName.KeyPress);
		waitingInput = false;
		return true;
	}
	public async Task<bool> SpawnShopLine(string message, InventoryItem[] items) {
		var shop = shopBoxTemplate.Instantiate<ShopBox>();
		shop.SetContents(message, items);
		container.AddChild(shop);
		existingLines.Add(shop);
		RefreshLineSettings();
		waitingInput = true;
		await ToSignal(shop, ShopBox.SignalName.Finished);
		waitingInput = false;
		return true;
	}
	public void FlushLines() {
		foreach(var line in existingLines) line.QueueFree();
		existingLines.Clear();
	}
	private void RefreshLineSettings() {
		for (int i = 0; i < existingLines.Count; i++) {
			int reverseIdx = existingLines.Count - i - 1;
			if (reverseIdx < LINES_FULLY_VISIBLE) {
				existingLines[i].Modulate = Colors.White;
			} else {
				float a = (LINES_FADING_TO_INVISIBLE - (reverseIdx-LINES_FULLY_VISIBLE)) / (LINES_FADING_TO_INVISIBLE * 1f);
				existingLines[i].Modulate = new Color(1,1,1,a);
			}
		}
	}
}
