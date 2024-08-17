using Godot;
using System;

public partial class CardEquip : Control
{
	const int CARD_MAX = 22;
	const int CARD_TOTAL_ROW = 11;
	[Export] HFlowContainer container;
	[Export] RichTextLabel description;
	[Export] Control cardLIndicator;
	[Export] Control cardRIndicator;
	CustomButton[] allButtons;
	Control _lastFocused;
    public override void _Ready()
    {
        base._Ready();
		var spacerTemplate = container.GetChild<Control>(0);
		var cardTemplate = container.GetChild<CustomButton>(1);
		allButtons = new CustomButton[CARD_MAX];
		for (int i = 0; i < CARD_MAX; i++) {
			if (i != 0 && i % CARD_TOTAL_ROW == 0) {
				var spacer = spacerTemplate.Duplicate() as Control;
				spacer.CustomMinimumSize = new Vector2(16,24);
				container.AddChild(spacer);
			}
			if (i == 0) {
				allButtons[i] = cardTemplate;
			} else {
				var inst = cardTemplate.Duplicate() as CustomButton;
				container.AddChild(inst);
				allButtons[i] = inst;
			}
			allButtons[i].Name = "CARD " + i;
			var sprite = allButtons[i].content.GetChild<Sprite2D>(0);
			var x = ((i+1) % 6) * 24;
			var y = ((i+1) / 6) * 32;
			sprite.RegionRect = new Rect2(x,y,24,32);
		}
    }
    public override void _Process(double delta)
    {
        if (!IsVisibleInTree()) return;
		RefreshIndicators();
		RefreshDescription();
		// Card interactions
		if (Input.IsActionJustPressed("card_left")) {
			EquipFocusedCard(0);
			RefreshIndicators();
		}
		if (Input.IsActionJustPressed("card_right")) {
			EquipFocusedCard(1);
			RefreshIndicators();
		}
    }
	private void EquipFocusedCard(int slot) {
		var focused = GetViewport().GuiGetFocusOwner();
		if (focused != null) {
			var card = focused.GetMeta("card").As<BaseCard>();
			if (card != null) {
				Game.State.EquipCard(0, card.GetId());
				AudioManager.PlaySystemSound("decision");
			} else {
				Game.State.EquipCard(0, null);
				AudioManager.PlaySystemSound("decision");
			}
		}
	}
    public void Refresh() {
		foreach (var b in allButtons) SetCardButton(b,false);
		var allCards = Game.State.GetAllCards();
		foreach (var c in allCards) {
			if (c.CardIndex >= 0 && c.CardIndex < CARD_MAX) {
				SetCardButton(allButtons[c.CardIndex-1], true);
				allButtons[c.CardIndex-1].SetMeta("card",c);
			}
		}
		allButtons[0].GrabFocus();
		RefreshDescription();
		RefreshIndicators();
	}
	public void RefreshDescription() {
		var focused = GetViewport().GuiGetFocusOwner();
		if (focused != _lastFocused) {
			for (int i = 0; i < CARD_MAX; i++) {
				if (focused == allButtons[i]) {
					var card = allButtons[i].GetMeta("card").As<BaseCard>();
					if (card != null) {
						SetDescription(card.GetDescription());
					} else {
						SetDescription("");
					}
					return;
				}
			}
			SetDescription("");
		}
	}
	private void RefreshIndicators() {
		var cL = Game.State.GetEquippedCard(0);
		if (cL == null) cardLIndicator.Visible = false;
		else {
			cardLIndicator.Visible = true;
			cardLIndicator.GlobalPosition = allButtons[cL.CardIndex-1].GlobalPosition;
		}
		var cR = Game.State.GetEquippedCard(1);
		if (cR == null) cardRIndicator.Visible = false;
		else {
			cardRIndicator.Visible = true;
			cardRIndicator.GlobalPosition = allButtons[cR.CardIndex-1].GlobalPosition;
		}
	}
	private void SetCardButton(CustomButton b, bool state) {
		b.content.Visible = state;
	}
	public void SetDescription(string v) {
		description.Text = v;
	}
}
