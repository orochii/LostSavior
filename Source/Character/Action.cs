using Godot;
using System;

[GlobalClass]
public partial class Action : Resource
{
    [Export] public string displayName = "Attack";
    // Behavior stuff
    [Export] public float effectSpawnDelay = 0.0f;
    [Export] public float actionDelay = 0.2f;
    [Export] public float endDelay = 0.1f;
    [Export] public StringName actionState = "attack1";
    [Export] public Vector2 force = new Vector2(0,0);
    [Export] public float forceDuration = 0;
    // Effect spawn
    [Export] public PackedScene effect;
    [Export] public Vector3 OffsetAndRotation = new Vector3(0,0,0);
    [Export] public Vector2 RotationVariance = new Vector2(0,0);
    [Export] public int SpawnPositionIndex = -1;
    [Export] public bool FlipH = false;
    [Export] public bool FlipV = false;
    public void Execute(BaseCharacter p) {
        p.SetDelayedSpawn(null, 0);
        p.SetAction(actionState, actionDelay);
        p.ApplyForce(force, forceDuration);
        if (effectSpawnDelay <= 0) SpawnEffect(p);
        else {
            p.SetDelayedSpawn(this,effectSpawnDelay);
        }
    }
    public void SpawnEffect(BaseCharacter p) {
        // Spawn effect
        if (effect != null) {
            if (p.CanMove()) {
                var i = effect.Instantiate<Effect>();
                i.Setup(p, this);
            }
        }
    }
    public void Finish(BaseCharacter p) {
        if(p.IsAction(this)) p.CancelAction(endDelay);
    }
}
