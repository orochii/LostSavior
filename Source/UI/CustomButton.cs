using Godot;
using System;

public partial class CustomButton : Button
{
	[Export] public Control content;
	[Export] Node selector;
	Tween _selectTween;
	public override void _Ready()
	{
		FocusEntered += OnFocusEnter;
		FocusExited += OnFocusExit;
		OnFocusExit();
	}
	public void OnFocusEnter() {
		//if (_selectTween == null) SetupTween();
		switch (selector)
		{
			case Node2D:
				(selector as Node2D).Visible = true;
				break;
			case Control:
				(selector as Control).Visible = true;
				break;
		}
	}
	public void OnFocusExit() {
		//if (_selectTween != null) DestroyTween();
		switch (selector)
		{
			case Node2D:
				(selector as Node2D).Visible = false;
				break;
			case Control:
				(selector as Control).Visible = false;
				break;
		}
	}

	private void SetupTween() {
		_selectTween = new Tween();
	}
	private void DestroyTween() {
		_selectTween.Kill();
		_selectTween = null;
	}
}
