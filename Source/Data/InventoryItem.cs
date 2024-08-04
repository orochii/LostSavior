
using Godot;

[GlobalClass]
public partial class InventoryItem : Resource {
    public const string BASE = "res://Data/Items/";
    public const string EXT = ".tres";
    //
    [ExportGroup("Visuals")]
    [Export] public int IconIndex;
    
    public static StringName GetFullPath(string id) {
        return BASE + id + EXT;
    }
    public StringName GetId() {
        var len = this.ResourcePath.Length - BASE.Length - EXT.Length;
        return this.ResourcePath.Substring(BASE.Length, len);
    }
    public string GetDisplayName() {
        return TranslationServer.Translate(GetId());
    }
    public string GetNameSmallIcon()
    {
        int cols = 128 / 8;
        var x = (IconIndex % cols) * 8;
        var y = (IconIndex / cols) * 8;
        var r = new Rect2(x,y,8,8);
        var options = string.Format("region={0},{1},{2},{3}", x,y,8,8);
        var imgCode = string.Format("[img {0}]{1}[/img] ", options, "res://Graphics/System/icons_s.png");
        return imgCode + GetDisplayName();
    }
    public string GetNameBigIcon()
    {
        int cols = 128 / 8;
        var x = (IconIndex % cols) * 8;
        var y = (IconIndex / cols) * 8;
        var r = new Rect2(x,y,8,8);
        var options = string.Format("region={0},{1},{2},{3}", x,y,8,8);
        var imgCode = string.Format("[img {0}]{1}[/img] ", options, "res://Graphics/System/icons.png");
        return imgCode + GetDisplayName();
    }
}