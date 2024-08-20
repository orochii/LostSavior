using Godot;
using System;

public partial class ShopEntry : Button
{
	RichTextLabel label;
	int price;
	int amount = 1;
	InventoryItem item;
	public void Setup(InventoryItem itm) {
		item = itm;
		price = itm.Price;
		label = GetChild<RichTextLabel>(0);
		RefreshText();
	}
	private void RefreshText() {
		if (label != null) {
			var text = (item==null) ? "???" : item.GetNameSmallIcon();
			if (amount > 1) text += string.Format(" x {0}", amount);
			text += string.Format(" ({0}G)", price * amount);
			label.Text = text;
		}
	}
	public InventoryItem Item => item;
	public int Price => (price * amount);
	public int Amount {
		get { return amount; }
		set {
			amount = Math.Clamp(value, 1, 99);
			RefreshText();
		}
	}
}
