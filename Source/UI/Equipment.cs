using Godot;
using System;

public partial class Equipment : Control
{
	[Export] CharStatsPanel charStats;
	[Export] CharEquipsPanel charEquips;
	[Export] InventoryPanel inventory;
	[Export] RichTextLabel description;
	[Export] Label levelLabel;
	[Export] Label expLabel;
	[Export] Control expBar;
	[Export] Label moneyAmount;
    public void Refresh() {
		charStats.Refresh();
		charEquips.Refresh();
		inventory.Refresh();
		SetExp();
		SetGold();
	}
	public void SetDescription(string v) {
		description.Text = v;
	}
	private void SetExp() {
		float percent = Game.State.GetExpPercent();
		if (expBar.Material != null) {
			var mat = expBar.Material as ShaderMaterial;
			mat.SetShaderParameter("percent", percent);
		}
		levelLabel.Text = Game.State.GetLevel().ToString();
		expLabel.Text = string.Format("{0}/{1}", Game.State.GetCurrentLevelExp(), Game.State.GetTotalLevelExp());
	}
	private void SetGold() {
		moneyAmount.Text = Game.State.GetMoney().ToString();
	}
}
