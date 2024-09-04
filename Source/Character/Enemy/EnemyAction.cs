using Godot;

[GlobalClass]
public partial class EnemyAction : Resource {
    [Export] public Vector2 MaxActionDeltas = new Vector2(32,32);
    [Export] public bool IsGrounded;
    [Export] public Action[] Actions;
}