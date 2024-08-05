using Godot;
using System;

public partial class ItemEntry : Button
{
	RichTextLabel label;
	int index;
	InventoryItem item;
	public void Setup(int idx, InventoryItem itm) {
		item = itm;
		index = idx;
		label = GetChild<RichTextLabel>(0);
		if (label != null) {
			if (item != null) {
				label.Text = item.GetNameSmallIcon();
			}
			else {
				label.Text = "???";
			}
		}
	}
	public InventoryItem Item => item;
	public int Index => index;
}
