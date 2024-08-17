using System.Threading.Tasks;
using Godot;

public partial class Event : Node2D
{
	public override void _Ready()
	{
		Refresh();
	}
	public override void _EnterTree()
    {
        base._EnterTree();
		if (!Game.Instance.IsConnected(Game.SignalName.OnFlagUpdate, Callable.From<string,int>(CheckRefresh))) {
			Game.Instance.Connect(Game.SignalName.OnFlagUpdate, Callable.From<string,int>(CheckRefresh));
		}
    }
    public override void _ExitTree()
    {
        base._ExitTree();
		if (Game.Instance.IsConnected(Game.SignalName.OnFlagUpdate, Callable.From<string,int>(CheckRefresh))) {
			Game.Instance.Disconnect(Game.SignalName.OnFlagUpdate, Callable.From<string,int>(CheckRefresh));
		}
    }
	public void ExecuteOnTouch(Node2D body) {
		if (body is Player) Execute();
	}
	public virtual void CheckRefresh(string id, int v) {

	}
	public virtual void Refresh() {
		
	}
	public virtual void Execute() {
		
	}

	public async Task<bool> Wait(float duration) {
		await ToSignal(GetTree().CreateTimer(duration, false), SceneTreeTimer.SignalName.Timeout);
		return true;
	}
}
