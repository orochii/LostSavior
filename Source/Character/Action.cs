using Godot;
using System;

[GlobalClass]
public partial class Action : Resource
{
    [Export] public string displayName = "Attack";
    // Behavior stuff
    [Export] public float actionDelay = 0.2f;
    [Export] public float endDelay = 0.1f;
    [Export] public StringName actionState = "attack1";
    // Effect spawn
    [Export] public PackedScene effect;

    public void Execute(Player p) {
        GD.Print("Executed: ", displayName);
        p.SetAction(actionState, actionDelay);
        // Spawn effect
        if (effect != null) {
            var i = effect.Instantiate<Effect>();
            i.Setup(p);
        }
    }

    public void Finish(Player p) {
        p.CancelAction(endDelay);
    }
}
