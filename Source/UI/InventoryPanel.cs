using Godot;
using System;
using System.Collections.Generic;

public partial class InventoryPanel : Control
{
	[Export] Equipment parentScreen;
	[Export] PackedScene itemEntryTemplate;
	[Export] ScrollContainer scroll;
	[Export] Container container;
	List<Control> allItems = new List<Control>();
	Control lastFocus = null;
    public override void _Process(double delta)
    {
        base._Process(delta);
		if (!IsVisibleInTree()) return;
		// Update current focus.
		var focused = GetViewport().GuiGetFocusOwner();
		if (lastFocus != focused) {
			var e = focused as ItemEntry;
			if (e != null) {
				parentScreen.SetDescription(e.Item.GetDescription());
			} 
			else {
				parentScreen.SetDescription("");
			}
			lastFocus = focused;
		}
    }
    public void Refresh() {
		foreach (var v in allItems) v.QueueFree();
		allItems.Clear();
		var items = Game.State.GetAllInventory();
		for (int i = 0; i < items.Length; i++) {
			var entry = itemEntryTemplate.Instantiate<ItemEntry>();
			var index = i;
			var item = items[i];
			entry.Setup(i, item);
			container.AddChild(entry);
			allItems.Add(entry);
			entry.Pressed += () => {
				UseItem(index, item);
			};
		}
		if (allItems.Count > 0) allItems[0].GrabFocus();
		UIUtils.SetupHBoxList(allItems);
	}

	public void UseItem(int idx, InventoryItem itm) {
		GD.Print("Selected: "+ idx + " " + itm.GetDisplayName());
		switch (itm) {
			case EquipItem:
				var equip = itm as EquipItem;
				//
				GD.Print("Equip on: ",equip.EquipKind);
				break;
		}
	}
}
