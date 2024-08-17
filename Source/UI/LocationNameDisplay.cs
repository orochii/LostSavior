using Godot;
using System;

public partial class LocationNameDisplay : Control
{
	[Export] Label label;
	[Export] float AppearTime = 1f;
	[Export] float StayTime = 3f;
	[Export] float DisappearTime = 4f;
	private Tween _tween;
	public void ShowName(string name) {
		if (_tween != null) _tween.Kill();
		Visible = true;
		Modulate = Colors.Transparent;
		label.Text = name;
		_tween = CreateTween();
		_tween.TweenProperty(this, "modulate", Colors.White, AppearTime);
		_tween.TweenInterval(StayTime);
		_tween.TweenProperty(this, "modulate", Colors.Transparent, DisappearTime);
		_tween.TweenCallback(Callable.From(()=>{Visible = false;}));
	}
}
