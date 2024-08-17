using System.Collections;
using Godot;
[Tool]
[GlobalClass]
public partial class DialogueBox : ContainerBackground
{
	[Export] RichTextLabel contents;
	[Export] float lettersBySecond = 32;
	private float _visibleChars = 0;
	private ESpeaker speaker;
	public ESpeaker Speaker => speaker;
	public void SetContents(string text, ESpeaker speaker) {
		contents.Text = text;
		this.speaker = speaker;
		switch (speaker) {
			case ESpeaker.LEFT:
				SizeFlagsHorizontal = SizeFlags.ShrinkBegin;
				break;
			case ESpeaker.RIGHT:
				SizeFlagsHorizontal = SizeFlags.ShrinkEnd;
				break;
			default:
				SizeFlagsHorizontal = SizeFlags.ShrinkCenter;
				break;
		}
		RefreshSize();
		//contents.VisibleCharacters = 0;
		contents.Modulate = Colors.Transparent;
		_visibleChars = 0;
	}
	public void ShowAll() {
		_visibleChars = contents.GetTotalCharacterCount();
		contents.VisibleCharacters = (int)_visibleChars;
		contents.Modulate = Colors.White;
	}
    public override void _Process(double delta)
    {
        base._Process(delta);
		if (Engine.IsEditorHint()) return;
		if (!Finished) {
			_visibleChars += (float)delta * lettersBySecond;
			contents.VisibleCharacters = (int)_visibleChars;
			contents.Modulate = Colors.White;
			RefreshSize();
		}
    }
	public bool Finished => _visibleChars >= contents.GetTotalCharacterCount();
}
