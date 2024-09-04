using System;
using Godot;

[GlobalClass]
public partial class ApproachTarget : BaseEnemyState {
    [Export] Vector2 FollowDistance = new Vector2(0, 16f);
	[Export] string NextStateId;
    public override Enemy.AIOutputs Update(Enemy e, double delta, Enemy.AIOutputs outputs)
    {
        var cachedDir = e.GetHorzDirection();
		//Move
		var deltaPos = e.BrainTargetPos - e.GlobalPosition;
		var reqDir = Math.Sign(deltaPos.X);
		var deltaOffX = Math.Abs(deltaPos.X);
		if (deltaOffX < FollowDistance.X) {
			outputs.horz = -reqDir;
			cachedDir = 0;
		} else if (deltaOffX >= FollowDistance.Y || reqDir != cachedDir) {
			outputs.horz = reqDir;
			cachedDir = reqDir;
		} else {
			e.ChangeChaseState(NextStateId, delta);
		}
		outputs.jump = e.CheckJump(cachedDir);
        return outputs;
    }
}