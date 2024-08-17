using Godot;
[Tool]
[GlobalClass]
public partial class ContainerBackground : NinePatchRect
{
	Container container;
	public override void _Ready()
	{
		FindInnerContainer();
		RefreshSize();
	}
	protected void FindInnerContainer() {
		foreach (var c in GetChildren()) {
			if (c is Container) {
				container = c as Container;
				/*if (!Engine.IsEditorHint()) {
					if (!IsConnected(SignalName.Resized, Callable.From(RefreshSize))) {
						container.Resized += RefreshSize;
					}
				}*/
				return;
			}
		}
	}
	protected void RefreshSize() {
		if (container == null) FindInnerContainer();
		this.Size = container.Size;
		this.CustomMinimumSize = Size;
	}
	public override void _Process(double delta)
	{
		if (Engine.IsEditorHint()) {
			FindInnerContainer();
			RefreshSize();
		}
	}
}
