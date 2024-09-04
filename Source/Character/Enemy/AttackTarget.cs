using System;
using Godot;

[GlobalClass]
public partial class AttackTarget : BaseEnemyState {
    [Export] public EnemyAction[] Actions;
    [Export] public float MaxDistance;
    [Export] public float MaxStateTime;
    [Export] string NextStateId = "";
    public override Enemy.AIOutputs Update(Enemy e, double delta, Enemy.AIOutputs outputs)
    {
        var cachedDir = e.GetHorzDirection();
        // Look towards general direction
        var deltaPos = e.BrainTargetPos - e.GlobalPosition;
        if (deltaPos.X != 0) {
            var reqDir = Math.Sign(deltaPos.X);
            if (reqDir != cachedDir) outputs.horz = reqDir;
        }
        // Attack
		if (!e.IsReactionCooldown()) {
            if (Math.Abs(deltaPos.X) > MaxDistance) {
                e.ChangeChaseState(NextStateId, delta);
            } else {
                EnemyAction currAction = SelectAction(e);
                if (currAction != null) {
                    outputs.actions = currAction.Actions;
                    outputs.attack = e.CheckAttack(cachedDir, currAction.MaxActionDeltas);
                }
            }
		}
        if (MaxStateTime>0 && e.ChaseStateTimer > MaxStateTime) {
            e.ChangeChaseState(NextStateId, delta);
        }
        return outputs;
    }
    private EnemyAction SelectAction(Enemy e) {
		foreach (var a in Actions) {
			if (a.IsGrounded != e.IsGrounded) continue;
			if (!e.CheckTargetDelta(a.MaxActionDeltas)) continue;
			return a;
		}
		return null;
	}
}