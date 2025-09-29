using UnityEngine;
using System.Collections.Generic;

namespace Systems.Creatures.Models {
    [System.Serializable]
    public enum SpellTargetType { Enemy, AllEnemies, Ally, AllAllies, Self }
    public enum SpellEffectType { Damage, Heal, BuffInitiative, BuffDamage }

    [System.Serializable]
    public class SpellElementRequirement 
    {
        public CreatureElement? mainElement; // null = nie wymagany
        public CreatureElement? secondaryElement; // null = nie wymagany
        
        public SpellElementRequirement(CreatureElement? main = null, CreatureElement? secondary = null)
        {
            mainElement = main;
            secondaryElement = secondary;
        }
        
        public bool CanCreatureLearn(Creature creature)
        {
            bool mainMatch = mainElement == null || creature.mainElement == mainElement;
            bool secondaryMatch = secondaryElement == null || creature.secondaryElement == secondaryElement;
            return mainMatch && secondaryMatch;
        }
    }
    
    [System.Serializable]
    public class SpellEffect
    {
        public SpellTargetType targetType;
        public SpellEffectType effectType;
        public int power; // uniwersalna wartość efektu
        
        public SpellEffect(SpellTargetType target, SpellEffectType effect, int powerValue)
        {
            targetType = target;
            effectType = effect;
            power = powerValue;
        }
    }

    public class Spell
    {
        public int id; // unikalny identyfikator spella
        public string name;
        public string description;
        public List<SpellElementRequirement> elementRequirements = new List<SpellElementRequirement>(); // lista możliwych kombinacji elementów
        public int requiredLevel;
        public List<SpellEffect> effects = new List<SpellEffect>(); // lista efektów spella
        public float learnTimeSeconds = 5f; // czas nauki w sekundach
        
        public bool CanCreatureLearn(Creature creature)
        {
            // Sprawdź poziom
            if (creature.level < requiredLevel) return false;
            
            // Jeśli brak wymagań elementowych, każdy może się nauczyć
            if (elementRequirements.Count == 0) return true;
            
            // Sprawdź czy creature spełnia którekolwiek z wymagań elementowych
            foreach (var requirement in elementRequirements)
            {
                if (requirement.CanCreatureLearn(creature))
                    return true;
            }
            
            return false;
        }
    }
}