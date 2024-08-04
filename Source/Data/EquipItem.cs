
using Godot;

public enum EEquipKind {
    NONE, WEAPON, BODY, RELIC
}

[GlobalClass]
public partial class EquipItem : InventoryItem {
    [ExportGroup("Behavior")]
    [Export] public EEquipKind EquipKind;
    [ExportGroup("Actions")]
    [Export] public Action[] AttackActions;
	[Export] public Action[] AirAttackActions;
    [Export] public bool CanHold;
    [ExportGroup("Statistics")]
    [Export] public int HPPlus;
    [Export] public int StrPlus;
    [Export] public int ConPlus;
    [Export] public int IntPlus;
}