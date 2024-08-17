using Godot;
using System;

public partial class CharHUD : TextureRect
{
	[Export] Control HealthContainer;
	[Export] Control HealthBar;
	[Export] Label currentHealth;
	[Export] Label maxHealth;
	[Export] TextureRect weaponIcon;
	[Export] Control ExpBar;
	int _currHealth = 0;
	int _maxHealth = 0;
	public override void _Ready()
	{
		Resize();
	}
	public override void _Process(double delta)
    {
		var _parent = GetParent() as Control;
		if (_parent != null) _parent.Visible = !Game.State.OnHold;
		//
        if (_maxHealth != GetMaxHealth()) Resize();
        if (_currHealth != GetCurrentHealth()) SetHealthPercent();
        //
        RefreshIcon();
		SetExpPercent();
    }
    private int GetCurrentHealth() {
		return Game.State.GetHealth();
	}
	private int GetMaxHealth() {
		return Game.State.GetMaxHealth();
	}
	private int GetHealthContainerWidth() {
		int healthAdd = GetMaxHealth() / 4;
		return Math.Min(16 + healthAdd, 256);
	}
	private int GetWeaponIcon() {
		var w = Game.State.GetWeapon();
		if (w == null) return 0;
		return w.IconIndex;
	}
	private void Resize() {
		var s = HealthContainer.Size;
		s.X = GetHealthContainerWidth();
		HealthContainer.Size = s;
		if (HealthBar.Material != null) {
			var mat = HealthBar.Material as ShaderMaterial;
			mat.SetShaderParameter("width", HealthBar.Size.X);
		}
		_maxHealth = GetMaxHealth();
		maxHealth.Text = _maxHealth.ToString();
		SetHealthPercent();
	}
	private void SetHealthPercent() {
		float percent = GetCurrentHealth() * 1f / GetMaxHealth();
		if (HealthBar.Material != null) {
			var mat = HealthBar.Material as ShaderMaterial;
			mat.SetShaderParameter("percent", percent);
		}
		_currHealth = GetCurrentHealth();
		currentHealth.Text = _currHealth.ToString();
	}
	private void RefreshIcon()
	{
        var atlas = weaponIcon.Texture as AtlasTexture;
        if (atlas != null)
        {
            int idx = GetWeaponIcon();
            int _x = (idx % 16) * 16;
            int _y = (idx / 16) * 16;
            atlas.Region = new Rect2(_x, _y, 16, 16);
        }
    }
	private void SetExpPercent() {
		float percent = Game.State.GetExpPercent();
		if (ExpBar.Material != null) {
			var mat = ExpBar.Material as ShaderMaterial;
			mat.SetShaderParameter("percent", percent);
		}
	}
}
