
public enum EDamageFormula {
	BASE, PHYSICAL, MAGICAL, HEALING
}

public static class DamageFormulas {
    public static int CalculateDamage(BaseCharacter source, EDamageFormula damageFormula, int damageBase) {
		switch (damageFormula) {
			case EDamageFormula.BASE:
				return damageBase;
			case EDamageFormula.PHYSICAL:
				return damageBase + source.GetStr();
			case EDamageFormula.MAGICAL:
				return damageBase + (source.GetInt() * 2);
            case EDamageFormula.HEALING:
                return -damageBase - (source.GetInt() * 2);
			default:
				return 0;
		}
	}
    public static int CalculateDefense(BaseCharacter source, EDamageFormula damageFormula) {
        switch (damageFormula) {
			case EDamageFormula.BASE:
				return 0;
			case EDamageFormula.PHYSICAL:
				return source.GetCon();
			case EDamageFormula.MAGICAL:
				return source.GetInt();
			default:
				return 0;
		}
    }
}