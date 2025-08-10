using UnityEngine;

namespace Models {
    [System.Serializable]
    public enum SpellTargetType { Enemy, AllEnemies, Ally, AllAllies, Self }
    public enum SpellEffectType { Damage, Heal, BuffInitiative, BuffDamage }

    public class Spell {
        public string Name;
        public SpellType Type;
        public ZawomonClass? RequiredClass; // null = uniwersalny
        public int RequiredLevel;
        public int Power; // np. dmg lub heal
        public string Description;
        public float LearnTimeSeconds = 5f; // czas nauki w sekundach
        public bool RequiresLearning = true; // czy wymaga nauki

        // Nowe pola do efektów
        public SpellTargetType TargetType;
        public SpellEffectType EffectType;
        public int EffectValue;
    }
}