using Godot;

[GlobalClass]
public partial class DialogueLine : Resource {
    [Export] public ESpeaker Speaker;
    [Export] public string Text;
}