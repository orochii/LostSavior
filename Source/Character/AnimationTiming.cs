using Godot;

[GlobalClass]
public partial class AnimationTiming : Resource {
    [Export] public StringName animationId;
    [Export] public float time;
    [Export] public string eventName;
    [Export] public int param1;
    [Export] public AudioEntry soundQueue;
}