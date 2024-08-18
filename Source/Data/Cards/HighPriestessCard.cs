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
                    v.Y = 16f;
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
        SetVisuals(p.CardObject[index], state);
    }
    private void SetVisuals(Node2D refObj, bool s) {
        // p.CardObject[index].Visible = state;
        var animator = refObj.GetChild<AnimationPlayer>(0);
        if (animator != null) {
            if (s) animator.Play("idle");
            else animator.Play("RESET");
        }
    }
}