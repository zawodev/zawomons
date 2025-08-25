using UnityEngine;
using Models;
using Systems;
using System.Linq;

namespace Tests {
    public class SpellTest : MonoBehaviour {
        void Start() {
            Debug.Log("=== SPELL TEST START ===");
            
            // Test generowania creature z secondary element
            var creature1 = CreatureGenerator.GenerateRandomZawomon(2);
            Debug.Log($"Generated creature: {creature1.name} - Main: {creature1.mainElement}, Secondary: {creature1.secondaryElement}");
            Debug.Log($"Starting spells: {creature1.spells.Count}");
            foreach (var spell in creature1.spells) {
                Debug.Log($"- {spell.name}: {spell.effects.Count} effects");
            }
            
            // Test spelli z wymaganiami elementowymi
            var allSpells = GameAPI.GetAllSpells();
            Debug.Log($"\nAll available spells: {allSpells.Count}");
            
            foreach (var spell in allSpells) {
                bool canLearn = spell.CanCreatureLearn(creature1);
                Debug.Log($"- {spell.name}: Can learn: {canLearn}");
                if (spell.elementRequirements.Count > 0) {
                    Debug.Log($"  Requirements: {string.Join(", ", spell.elementRequirements.Select(req => $"Main:{req.mainElement}, Secondary:{req.secondaryElement}"))}");
                }
                Debug.Log($"  Effects: {string.Join(", ", spell.effects.Select(eff => $"{eff.effectType}({eff.targetType}):{eff.power}"))}");
            }
            
            // Test dodawania instant spella
            var basicAttack = allSpells.FirstOrDefault(s => s.name == "Szybki Cios");
            if (basicAttack != null && !creature1.spells.Contains(basicAttack)) {
                creature1.AddSpellInstantly(basicAttack);
                Debug.Log($"\nAdded instant spell. Total spells now: {creature1.spells.Count}");
            }
            
            // Test wieloefektowego spella
            var multiEffectSpell = allSpells.FirstOrDefault(s => s.effects.Count > 1);
            if (multiEffectSpell != null) {
                Debug.Log($"\nMulti-effect spell: {multiEffectSpell.name}");
                foreach (var effect in multiEffectSpell.effects) {
                    Debug.Log($"  Effect: {effect.effectType} -> {effect.targetType} (power: {effect.power})");
                }
            }
            
            Debug.Log("=== SPELL TEST END ===");
        }
    }
}
