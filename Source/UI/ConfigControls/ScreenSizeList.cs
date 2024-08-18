using Godot;
using System;

public partial class ScreenSizeList : OptionButton
{
	public override void _Ready()
	{
		this.Clear();
		foreach (var size in Config.SCREEN_SIZES) {
			this.AddItem(string.Format("{0}x{1}", size.X, size.Y));
		}
	}
}
