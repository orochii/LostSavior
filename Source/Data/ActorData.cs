using System.ComponentModel.DataAnnotations;
using Godot;

[GlobalClass]
public partial class ActorData : Resource {
    [ExportCategory("Stats")]
    [Export] public Vector2 HealthGrowth;
    [Export] public Vector2 StrengthGrowth;
    [Export] public Vector2 ConstitutionGrowth;
    [Export] public Vector2 IntelligenceGrowth;
    [ExportCategory("Default Actions")]
    [Export] public Action[] defaultActions;
	[Export] public Action[] defaultAirActions;
    [Export] public Action[] defaultUpActions;
	[Export] public Action[] defaultDownActions;
    [Export] public Action dashAction;
    [ExportCategory("Starting Setup")]
    [Export] [Range(1,GameState.MAX_LEVEL)] public int StartingLevel = 1;
    [Export] public EquipItem[] StartingEquipment;
    [Export] public InventoryItem[] StartingItems;
    [Export] public int StartingMoney;
    [ExportCategory("Jingles")]
    [Export] public AudioEntry OnDeathJingle;
    [Export] public AudioEntry OnLevelUpJingle;
}