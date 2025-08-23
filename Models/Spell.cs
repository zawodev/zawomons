using UnityEngine;

namespace Models {
    [System.Serializable]
    public enum SpellTargetType { Enemy, AllEnemies, Ally, AllAllies, Self }
    public enum SpellEffectType { Damage, Heal, BuffInitiative, BuffDamage }

    public class Spell {
        public string name;
        public SpellType type;
        public CreatureElement? requiredClass; // null = uniwersalny
        public int requiredLevel;
        public int power; // np. dmg lub heal
        public string description;
        public float learnTimeSeconds = 5f; // czas nauki w sekundach
        public bool requiresLearning = true; // czy wymaga nauki

        // Nowe pola do efektów
        public SpellTargetType targetType;
        public SpellEffectType effectType;
        public int effectValue;
    }
}