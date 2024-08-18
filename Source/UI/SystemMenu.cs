using Godot;
using System;

public partial class SystemMenu : Control
{
	[Export] Control FirstSelected;
	public void Refresh() {
		FirstSelected.GrabFocus();
	}
	bool _lastVisible;
    public override void _Process(double delta)
    {
        base._Process(delta);
		var visible = IsVisibleInTree();
		if (_lastVisible != visible) {
			if (visible) Refresh();
			_lastVisible = visible;
		}
    }
}
