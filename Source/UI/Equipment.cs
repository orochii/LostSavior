using Godot;
using System;

public partial class Equipment : Control
{
	[Export] CharStatsPanel charStats;
	[Export] CharEquipsPanel charEquips;
	[Export] InventoryPanel inventory;
	[Export] RichTextLabel description;

	public void Refresh() {
		charStats.Refresh();
		charEquips.Refresh();
		inventory.Refresh();
	}

	public void SetDescription(string v) {
		description.Text = v;
	}
}
