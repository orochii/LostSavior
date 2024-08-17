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
		// Check if set to weapon change.
		if (Input.IsActionJustPressed("swap_weapon")) {
			if (allItems.Contains(focused)) {
				var entry = focused as ItemEntry;
				if (entry.Item is EquipItem) {
					var equip = entry.Item as EquipItem;
					if (equip.EquipKind == EEquipKind.WEAPON) {
						if (equip.GetId() == Game.State.GetAltWeapon()) {
							Game.State.SetAltWeapon(null);
							parentScreen.Refresh();
							Select(entry.Index);
							AudioManager.PlaySystemSound("decision");
						} else {
							Game.State.SetAltWeapon(equip.GetId());
							parentScreen.Refresh();
							Select(entry.Index);
							AudioManager.PlaySystemSound("decision");
						}
					}
				}
			}
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
			var ammount = Game.State.GetItemCount(item.GetId());
			entry.Setup(i, item, ammount);
			container.AddChild(entry);
			allItems.Add(entry);
			entry.Pressed += () => {
				AudioManager.PlaySystemSound("decision");
				UseItem(index, item);
			};
		}
		if (allItems.Count > 0) allItems[0].GrabFocus();
		UIUtils.SetupHBoxList(allItems);
	}
	public void Select(int idx) {
		if (allItems.Count > idx) allItems[idx].GrabFocus();
		else if (allItems.Count > 0) {
			allItems[allItems.Count-1].GrabFocus();
		}
	}
	public void UseItem(int idx, InventoryItem itm) {
		switch (itm) {
			case EquipItem:
				var equip = itm as EquipItem;
				//
				Game.State.Equip(equip);
				parentScreen.Refresh();
				Select(idx);
				break;
		}
	}
}
