using Godot;
using System;

public partial class CardSprite : Sprite2D
{
	[Export] int cardIndex;
	[Export] AnimatedSprite2D burningSprite;
	[Export] public float Percent = 1f;
	ShaderMaterial _mat;
	public override void _Ready()
	{
		_mat = Material.Duplicate(true) as ShaderMaterial;
		Material = _mat;
		UpdatePercent();
	}
    public override void _Process(double delta)
    {
        UpdateCard();
    }
    private void UpdateCard() {
		var card = Game.State.GetEquippedCard(cardIndex);
		if (card == null) {
			Visible = false;
		}
		else {
			Visible = true;
			var i = card.CardIndex-1;
			var x = ((i+1) % 6) * 24;
			var y = ((i+1) / 6) * 32;
			RegionRect = new Rect2(x,y,24,32);
			Percent = 1 - (Game.State.PowerConsumed[cardIndex] / card.MaxPower);
			UpdatePercent();
		}
	}
	private void UpdatePercent() {
		if (Percent >= 1f) {
			burningSprite.Visible = false;
			_mat.SetShaderParameter("percent", 1);
		}
		else {
			burningSprite.Visible = true;
			_mat.SetShaderParameter("percent", Percent);
			burningSprite.Offset = new Vector2(0, Mathf.Floor((1-Percent)*-32f));
		}
	}
}
