using Godot;
using System;

public partial class PopUpManager : Control
{
	[Export] Control popupTemplate;
	[Export] Container popupContainer;
    public override void _Ready()
    {
        popupTemplate.Visible = false;
    }
	public void Create(string text, float duration) {
		var inst = popupTemplate.Duplicate() as Control;
		inst.Visible = true;
		var label = inst.GetChild(0) as RichTextLabel;
		if (label != null) {
			label.Text = "[center]" + text;
		}
		popupContainer.AddChild(inst);
		var tween = CreateTween();
		tween.TweenInterval(duration);
		tween.TweenProperty(inst, "modulate", Colors.Transparent, .5f);
		tween.TweenCallback(Callable.From(inst.QueueFree));
		tween.Play();
	}
}
