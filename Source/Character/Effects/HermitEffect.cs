using Godot;
using System;

public partial class HermitEffect : Node2D
{
	const float MAX_DIST = 2880f;
	const float MAX_DIST_SQUARED = MAX_DIST * MAX_DIST;
	[Export] Node2D arrowTemplate;
	[Export] int maxArrows;
	private Node2D[] arrows;
	private Node2D[] closerEvts;
	public override void _Ready()
	{
		arrows = new Node2D[maxArrows];
		closerEvts = new Node2D[maxArrows];
		arrows[0] = arrowTemplate;
		arrowTemplate.Visible = false;
		for (int i = 1; i < maxArrows; i++) {
			var n = arrowTemplate.Duplicate() as Node2D;
			AddChild(n);
			arrows[i] = n;
		}
	}
	public override void _Process(double delta)
	{
		GetCloserObjectives();
		for(int i = 0; i < maxArrows; i++) {
			var curr = closerEvts[i];
			if (curr == null) arrows[i].Visible = false;
			else {
				arrows[i].Visible = true;
				arrows[i].GlobalPosition = curr.GlobalPosition;
				arrows[i].Rotation = arrows[i].Position.Angle();
				var p = arrows[i].Position;
				var hw = Config.GetScreenWidth() / 2 - 32;
				var hh = Config.GetScreenHeight() / 2;
				p.X = Math.Clamp(p.X, -hw, hw);
				p.Y = Math.Clamp(p.Y, -hh+8, hh-64);
				arrows[i].Position = p;
			}
		}
	}
	private void GetCloserObjectives() {
		for(int i = 0; i < maxArrows; i++) closerEvts[i] = null;
		if (Event.ObjectiveDictionary.TryGetValue(Event.EKind.CARD, out var events)) {
			foreach(var ev in events) {
				if (!ev.Visible) continue;
				if (!ev.ShowAsObjective()) continue;
				float dst = ev.GlobalPosition.DistanceSquaredTo(GlobalPosition);
				if (dst > MAX_DIST_SQUARED) continue;
				for(int i = 0; i < maxArrows; i++) {
					if (closerEvts[i] == null) {
						closerEvts[i] = ev;
						break;
					} else {
						float d = closerEvts[i].GlobalPosition.DistanceSquaredTo(GlobalPosition);
						if (d < dst) {
							closerEvts[i] = ev;
							break;
						}
					}
				}
			}
		}
	}
}
