using Godot;
using System;

public partial class ParticleTextureFix : GpuParticles2D
{
	[Export] Texture2D facingL;
	[Export] Texture2D facingR;
    public void RefreshTexture() {
		if (GlobalScale.Y >= 0) {
			this.Texture = facingR;
		}
		else {
			this.Texture = facingL;
		}
	}
}
