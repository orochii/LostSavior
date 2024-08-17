using Godot;
using System;

public partial class InputLabel : RichTextLabel
{
	[Export] string actionName;
	[Export] string format = "{0} Change screen";
	int _cachedlastInput = -1;
	bool _cachedLastVisible = false;
	public override void _Process(double delta)
	{
		var v = IsVisibleInTree();
		if (_cachedLastVisible != v || _cachedlastInput != Game.Instance.LastInput) {
			_cachedlastInput = Game.Instance.LastInput;
			_cachedLastVisible = v;
			RefreshText();
		}
	}
	private void RefreshText() {
		var btnIcons = UIUtils.GetKeyIcon(actionName);
		var tlFormat = TranslationServer.Translate(format);
		this.Text = string.Format(tlFormat, btnIcons);
	}
}
