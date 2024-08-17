using Godot;
using System;

[GlobalClass]
public partial class BaseCard : Resource, IComparable<BaseCard> 
{
    protected static string[] ACTION_NAMES = new string[] {"card_left","card_right"};
	public const string BASE = "res://Data/Cards/";
    public const string EXT = ".tres";
	public const int ICON_INDEX = 0;
	public static StringName GetFullPath(string id) {
        return BASE + id + EXT;
    }
    //
    [ExportGroup("Visuals")]
    [Export] public PackedScene Object = null;
    [ExportGroup("Identity")]
    [Export] public int CardIndex;
    [ExportGroup("Behavior")]
    [Export] public float MaxPower;
    [Export] public float ConsumeSpeed;
    [Export] public bool ConsumeUnscaled;
    [Export] public float RecoverySpeed;
	public StringName GetId() {
        var len = this.ResourcePath.Length - BASE.Length - EXT.Length;
        return this.ResourcePath.Substring(BASE.Length, len);
    }
	public string GetDisplayName() {
        return TranslationServer.Translate(GetId());
    }
	internal string GetDescription()
    {
        return GetId() + "_description";
    }
	public string GetNameSmallIcon()
    {
        int cols = 128 / 8;
        var x = (ICON_INDEX % cols) * 8;
        var y = (ICON_INDEX / cols) * 8;
        var r = new Rect2(x,y,8,8);
        var options = string.Format("region={0},{1},{2},{3}", x,y,8,8);
        var imgCode = string.Format("[img {0}]{1}[/img] ", options, "res://Graphics/System/icons_s.png");
        return imgCode + GetDisplayName();
    }
	public int CompareTo(BaseCard other)
    {
        return CardIndex - other.CardIndex;
    }
    public void VerifyVisuals(Player p, int index) {
        if (p.CardObject[index] == null) {
            // Check that it's the right object
            if (Object == null) return;
            // Create new object
            var obj = Object.Instantiate<Node2D>();
            p.CardContainer.AddChild(obj);
            obj.SetMeta("path",Object.ResourcePath);
            p.CardObject[index] = obj;
        } else {
            // Check that it's the right object
            var path = p.CardObject[index].GetMeta("path").AsString();
            if (path == Object.ResourcePath) return;
            p.CardObject[index].QueueFree();
            // Create new object
            if (Object == null) return;
            var obj = Object.Instantiate<Node2D>();
            p.CardContainer.AddChild(obj);
            obj.Position = Vector2.Zero;
            obj.SetMeta("path",Object.ResourcePath);
            p.CardObject[index] = obj;
        }
        
    }
    public virtual void Process(Player p, int index, double delta) {
        
    }
    protected void ConsumePower(Player p, int index, double delta) {
        //
        if (ConsumeUnscaled) Game.State.PowerConsumed[index] += ConsumeSpeed;
        else Game.State.PowerConsumed[index] += (float)delta * ConsumeSpeed;
        // If depleted.
        if (Game.State.PowerConsumed[index] >= MaxPower) {
            Game.State.PowerConsumed[index] = MaxPower;
            Game.State.PowerInRecovery[index] = true;
        }
    }
    protected void RecoverPower(Player p, int index, double delta) {
        // Sort of clamp.
        if (Game.State.PowerConsumed[index] > MaxPower) Game.State.PowerConsumed[index] = MaxPower;
        // Decrease consumption.
        Game.State.PowerConsumed[index] -= (float)delta * RecoverySpeed;
        // If fully recovered.
        if (Game.State.PowerConsumed[index] < 0) {
            Game.State.PowerConsumed[index] = 0;
            Game.State.PowerInRecovery[index] = false;
        }
    }
    protected bool IsReady(Player p, int index) {
        return Game.State.PowerInRecovery[index] == false;
    }
}
