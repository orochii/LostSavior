using Godot;
using System;

public partial class ElderEvent : Event
{
	[Export] BaseCard objectiveCard;
	[Export] InventoryItem[] shopItems;
	public override async void Execute() {
		Game.Player.CancelAction(0,true);
        await ToSignal(GetTree(), "process_frame");
		Game.State.OnHold = true;
		//
		Game.Dialogue.Open();
		await Wait(0.5f);
        Game.Dialogue.SetLeftArt(Game.Dialogue.GetArt("ari"));
		Game.Dialogue.SetRightArt(Game.Dialogue.GetArt("elder"));
		if (!Game.State.HasCard(objectiveCard.GetId())) {
			await Game.Dialogue.SpawnLine("Oh my, who would you be?", ESpeaker.RIGHT);
			await Game.Dialogue.SpawnLine("My name...", ESpeaker.LEFT);
			await Game.Dialogue.SpawnLine("I'm Ari, and I'm lost.", ESpeaker.LEFT);
			await Game.Dialogue.SpawnLine("I see, you indeed don't seem to be from these lands.", ESpeaker.RIGHT);
			await Game.Dialogue.SpawnLine("Are you in search of something?", ESpeaker.RIGHT);
			await Game.Dialogue.SpawnLine("I'm actually... not sure, I don't know anything other than my name.", ESpeaker.LEFT);
			await Game.Dialogue.SpawnLine("Hmmm... that's quite a predicament.", ESpeaker.RIGHT);
			await Game.Dialogue.SpawnLine("Know what, I have an idea. You might find good use of this.", ESpeaker.RIGHT);
			await Game.Dialogue.SpawnLine("This is the card of the Hermit.", ESpeaker.RIGHT);
			//
			// Add card
			Game.State.AddCard(objectiveCard.GetId());
			// Show popup
			var tlText = TranslationServer.Translate("You got the {0} card!");
			var cardName = objectiveCard.GetNameSmallIcon();
			var text = string.Format(tlText, cardName);
			Game.PopUp.Create(text, 2f);
			//
			await Game.Dialogue.SpawnLine("Card?", ESpeaker.LEFT);
			await Game.Dialogue.SpawnLine("Yes, the card of the Hermit is a powerful relic.", ESpeaker.RIGHT);
			await Game.Dialogue.SpawnLine("Legend says the Hermit will help you find whatever your heart longs for.", ESpeaker.RIGHT);
			await Game.Dialogue.SpawnLine("I see, and maybe my heart knows things I don't?", ESpeaker.LEFT);
			await Game.Dialogue.SpawnLine("That's our best bet! Ohoho!", ESpeaker.RIGHT);
			await Game.Dialogue.SpawnLine("Well it's worth a try. Thank you mister.", ESpeaker.LEFT);
			await Game.Dialogue.SpawnLine("Just call me elder like everyone else here.", ESpeaker.RIGHT);
		}
		else {
			await Game.Dialogue.SpawnLine("Hello again Ari.", ESpeaker.RIGHT);
			await Game.Dialogue.SpawnLine("Remember: Use the Hermit to find your path forward.", ESpeaker.RIGHT);
			await Game.Dialogue.SpawnLine("It should prove useful for you.", ESpeaker.RIGHT);
			await Game.Dialogue.SpawnShopLine("In the mean time, might I offer you something?", shopItems);
		}

		Game.Dialogue.Close();
		//
		await Wait(0.5f);
		Game.State.OnHold = false;
	}
}
