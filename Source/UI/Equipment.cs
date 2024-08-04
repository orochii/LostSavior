using Godot;
using System;

public partial class Equipment : Control
{
	[Export] CharStatsPanel charStats;
	[Export] CharEquipsPanel charEquips;
	[Export] InventoryPanel inventory;

	public void Refresh() {
		charStats.Refresh();
		charEquips.Refresh();
		inventory.Refresh();
	}
}
