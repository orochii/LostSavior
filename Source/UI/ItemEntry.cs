using Godot;
using System;

public partial class ItemEntry : Button
{
	RichTextLabel label;
	int index;
	int ammount;
	InventoryItem item;
	public void Setup(int idx, InventoryItem itm, int qty=1) {
		item = itm;
		index = idx;
		ammount = qty;
		label = GetChild<RichTextLabel>(0);
		if (label != null) {
			var text = (item==null) ? "???" : item.GetNameSmallIcon();
			if (ammount > 1) text += string.Format(" x{0}", ammount);
			if (Game.State.GetAltWeapon() == item.GetId()) {
				text += " <>";
			}
			label.Text = text;
		}
	}
	public InventoryItem Item => item;
	public int Index => index;
}
