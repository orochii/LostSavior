using Godot;

public interface IDamageable {
    public void ApplyDamage(BaseCharacter source, int baseDmg, EDamageFormula damageFormula);
    public bool IsDead();
    public void Die();
}