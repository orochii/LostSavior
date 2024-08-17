using System.Diagnostics;
using Godot;

[GlobalClass]
public partial class HighPriestessCard : BaseCard {
    public override void Process(Player p, int index, double delta) {
        bool state = false;
        // Get action
        var actionName = ACTION_NAMES[index];
        // Glide action
        if (IsReady(p,index)) {
            if (Input.IsActionPressed(actionName)) {
                if (!p.IsGrounded && !p.IsActing()) {
                    // Consume power
                    ConsumePower(p,index,delta);
                    // Effect
                    var v = p.Velocity;
                    v.Y = 0.2f;
                    p.Velocity = v;
                    state = true;
                }
            } else {
                RecoverPower(p,index,delta);
            }
        } else {
            RecoverPower(p,index,delta);
        }
        // Update visuals
        p.CardObject[index].Visible = state;
    }
    
}