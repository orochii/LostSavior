using Godot;
using System;

public partial class CharEquipsPanel : Control
{
	[Export] RichTextLabel statsLabel;
	
	public void Refresh() {
		// Prepare stats
		var equips = Game.State.GetAllEquipment();
		string newState = "";
		foreach (var e in equips) {
			if (e != null) {
				newState += e.GetNameSmallIcon();
			}
			else {
				newState += "---";
			}
			newState += System.Environment.NewLine;
		}
		// Set
		statsLabel.Text = newState;
	}
}
