using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class AcquireCardEvent : Event
{
	[Export] BaseCard card = null;
	public override void Refresh() {
		if (Game.State.HasCard(card.GetId())) {
			Visible = false;
		}
	}
    public override void Execute()
    {
        if (!Visible) return;
		// Add card
		Game.State.AddCard(card.GetId());
		// Show popup
		var tlText = TranslationServer.Translate("You got the {0} card!");
		var cardName = card.GetNameSmallIcon();
		var text = string.Format(tlText, cardName);
		Game.PopUp.Create(text, 2f);
		// Spawn effect?
		// Refresh
        Refresh();
    }
}
