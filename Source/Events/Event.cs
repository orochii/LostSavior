using System.Collections.Generic;
using System.Threading.Tasks;
using Godot;

public partial class Event : Node2D
{
	public enum EKind { NONE, CARD, BOSS }
	public static Dictionary<EKind,List<Event>> ObjectiveDictionary = new Dictionary<EKind, List<Event>>();
	[Export] EKind eventKind;
	private static void RegisterEvent(Event ev) {
		if (ev.eventKind == EKind.NONE) return;
		bool found = ObjectiveDictionary.TryGetValue(ev.eventKind, out var list);
		if (!found) {
			list = new List<Event>();
			ObjectiveDictionary.Add(ev.eventKind, list);
		}
		if (!list.Contains(ev)) list.Add(ev);
	}
	public override void _Ready()
	{
		Refresh();
		RegisterEvent(this);
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
	public void OnInteractEnter(Node2D body) {
		if (body is Player) {
			var p = body as Player;
			p.RegisterCloseEvent(this);
		}
	}
	public void OnInteractExit(Node2D body) {
		if (body is Player) {
			var p = body as Player;
			p.UnregisterCloseEvent(this);
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
	public virtual bool ShowAsObjective() {return true;}
	public async Task<bool> Wait(float duration) {
		await ToSignal(GetTree().CreateTimer(duration, false), SceneTreeTimer.SignalName.Timeout);
		return true;
	}
}
