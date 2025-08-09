using UnityEngine;

namespace Models {
    [System.Serializable]
    public class Spell {
        public string Name;
        public SpellType Type;
        public ZawomonClass? RequiredClass; // null = uniwersalny
        public int RequiredLevel;
    public int Power; // np. dmg lub heal
    public string Description;
    public float LearnTimeSeconds = 5f; // czas nauki w sekundach
    public bool RequiresLearning = true; // czy wymaga nauki
    // Przykładowe efekty do rozbudowy:
    // public SpellTargetType TargetType; // np. Enemy, AllEnemies, AllAllies, Self
    // public SpellEffectType EffectType; // np. Damage, Heal, BuffInitiative, BuffDamage
    // public int EffectValue;
    }
}