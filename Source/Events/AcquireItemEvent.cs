using Godot;
using System;
using System.Runtime.CompilerServices;

public partial class AcquireItemEvent : Event
{
	[Export] string flagId = "ITEM_1";
	[Export] InventoryItem item = null;
	[Export] int amount = 1;
	public override void CheckRefresh(string id, int v) {
		if (id == flagId) Refresh();
	}
	public override void Refresh() {
		if (Game.State.GetProgressFlag(flagId) > 0) {
			Visible = false;
		}
	}
    public override void Execute()
    {
        if (!Visible) return;
		// Add item
		Game.State.ChangeItem(item.GetId(), amount);
		// Show popup
		var absAmount = Math.Abs(amount);
		var tlText = amount >= 0 ? TranslationServer.Translate("Acquired {0}!") : TranslationServer.Translate("Lost {0}!");
		var itemName = item.GetNameSmallIcon();
		if (absAmount > 1) itemName += string.Format(" x {0}", absAmount);
		var text = string.Format(tlText, itemName);
		Game.PopUp.Create(text, 2f);
		// Spawn effect?
		// Set flag
		Game.State.SetProgressFlag(flagId, 1);
    }
}
