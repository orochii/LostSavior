using System.Collections.Generic;
using Godot;
[Tool]
[GlobalClass]
public partial class ShopBox : ContainerBackground {
    [Signal] public delegate void FinishedEventHandler();
    [Export] RichTextLabel shopMessage;
    [Export] Label moneyLabel;
    [Export] Container shopItemsContainer;
    [Export] PackedScene shopEntryTemplate;
    bool active = false;
    bool set = false;
    List<Control> entries = new List<Control>();
    public void SetContents(string message, InventoryItem[] items) {
        active = true;
        shopMessage.Text = message;
        foreach (var i in items) {
            var entry = shopEntryTemplate.Instantiate<ShopEntry>();
            shopItemsContainer.AddChild(entry);
            entry.Setup(i);
            entry.Pressed += () => {
                BuyItem(entry);
            };
            entries.Add(entry);
        }
        UIUtils.SetupHBoxList(entries);
        RefreshMoney();
    }
    private void RefreshMoney() {
        moneyLabel.Text = string.Format("{0}G", Game.State.GetMoney());
    }
    public override void _Process(double delta) {
        if (!active) return;
        RefreshSize();
        if (!set) {
            if (entries.Count > 0) entries[0].GrabFocus();
            set = true;
        }
        if (Input.IsActionJustPressed("ui_left")) {
            ChangeFocusedAmount(-1);
        }
        if (Input.IsActionJustPressed("ui_right")) {
            ChangeFocusedAmount(1);
        }
        if (Input.IsActionJustPressed("ui_cancel")) {
            active = false;
            EmitSignal(SignalName.Finished);
        }
    }
    private void ChangeFocusedAmount(int v) {
        var focused = GetViewport().GuiGetFocusOwner();
        if (focused is ShopEntry) {
            var entry = focused as ShopEntry;
            entry.Amount += v;
        }
    }
    private void BuyItem(ShopEntry entry) {
        if (entry.Price <= Game.State.GetMoney()) {
            Game.State.ChangeMoney(-entry.Price);
            Game.State.ChangeItem(entry.Item.GetId(), entry.Amount);
            AudioManager.PlaySystemSound("decision");
            RefreshMoney();
        } else {
            AudioManager.PlaySystemSound("close");
        }
    }
}