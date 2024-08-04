using Godot;
using System;

public partial class CharStatsPanel : Control
{
	[Export] Label statsLabel;
	
	public void Refresh() {
		// Prepare stats
		string newState = Game.State.GetHpString();
		GD.Print(newState);
		newState += System.Environment.NewLine;
		newState += Game.State.GetStr();
		newState += System.Environment.NewLine;
		newState += Game.State.GetCon();
		newState += System.Environment.NewLine;
		newState += Game.State.GetInt();
		// Set
		statsLabel.Text = newState;
	}
}
