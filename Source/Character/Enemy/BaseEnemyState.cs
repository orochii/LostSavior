using Godot;

[GlobalClass]
public partial class BaseEnemyState : Resource {
    [Export] public string id;
    public virtual void Enter(Enemy e, double delta) {
        //
    }
    public virtual Enemy.AIOutputs Update(Enemy e, double delta, Enemy.AIOutputs outputs) {
        //
        return outputs;
    }
    public virtual void Exit(Enemy e, double delta) {
        //
    }
}