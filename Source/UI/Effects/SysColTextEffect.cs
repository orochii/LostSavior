using Godot;

[GlobalClass]
public partial class SysColTextEffect : RichTextEffect {
    public static readonly Color[] SysColors = { 
        Colors.White, 
        new Color("#ff8f")
    };
    string bbcode = "syscol";

    public override bool _ProcessCustomFX(CharFXTransform charFX)
    {
        int idx = 1;
        // Get index
        if (charFX.Env.ContainsKey("i")) {
            if (charFX.Env.TryGetValue("c", out var v)) {
                idx = v.AsInt32();
                if (idx >= SysColors.Length || idx < 0) idx = 0;
            }
        }
        charFX.Color = SysColors[idx];
        return true;
    }
}