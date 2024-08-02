using Godot;
using System;

public partial class Trigger : Area2D
{
	[Signal] public delegate void OnEnterEventHandler();
	[Export] public bool onlyOnce;
	bool done;
	
	public void OnEntered(Node2D body) {
		if (done) return;
		if (body is Player) {
			EmitSignal(SignalName.OnEnter);
			if (onlyOnce) done = true;
		}
	}
}
