using Godot;
using System;
using System.Collections.Generic;

public partial class InventoryPanel : Control
{
	[Export] PackedScene itemEntryTemplate;
	[Export] ScrollContainer scroll;
	[Export] Container container;
	List<Control> allItems = new List<Control>();
    public override void _Process(double delta)
    {
        base._Process(delta);
		if (!IsVisibleInTree()) return;
		//
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
				var idx = index;
				var itm = item;
				GD.Print("Selected: "+ idx + " " + itm.GetDisplayName());
			};
		}
		if (allItems.Count > 0) allItems[0].GrabFocus();
		UIUtils.SetupHBoxList(allItems);
	}
}
