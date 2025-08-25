using System.Collections.Generic;
using UnityEngine;
using System;

namespace Models {

    [System.Serializable]
    public class Creature {
        public string name;
        public CreatureElement mainElement;
        public CreatureElement? secondaryElement;
        public Color color;
        public int experience; // Podstawowa wartość EXP
        public int maxHP;
        public int currentHP;
        public int maxEnergy;
        public int currentEnergy;
        public int damage;
        public int initiative; // inicjatywa do walki
        public List<Spell> spells = new List<Spell>();
        public List<LearningSpellData> learningSpells = new List<LearningSpellData>();

        // Level jako computed property na podstawie EXP
        public int level {
            get {
                // Formuła: level = sqrt(EXP / 100) + 1, minimum level 1
                return Mathf.Max(1, Mathf.FloorToInt(Mathf.Sqrt(experience / 100f)) + 1);
            }
        }
        
        // Metoda do obliczenia ile EXP potrzeba na dany level
        public static int GetExpForLevel(int targetLevel) {
            if (targetLevel <= 1) return 0;
            return (targetLevel - 1) * (targetLevel - 1) * 100;
        }
        
        // Metoda do dodania EXP
        public void AddExperience(int expToAdd) {
            int oldLevel = level;
            experience += expToAdd;
            int newLevel = level;
            
            if (newLevel > oldLevel) {
                OnLevelUp(newLevel - oldLevel);
            }
        }

        public void LevelUp() {
            // Stara metoda - teraz dodaje EXP potrzebne na następny level
            int currentLevel = level;
            int expNeededForNextLevel = GetExpForLevel(currentLevel + 1);
            AddExperience(expNeededForNextLevel - experience);
        }
        
        private void OnLevelUp(int levelsGained) {
            maxHP += 10 * levelsGained;
            maxEnergy += 5 * levelsGained;
            damage += 2 * levelsGained;
            initiative += 1 * levelsGained;
            currentHP = maxHP;
            currentEnergy = maxEnergy;
            Debug.Log($"{name} gained {levelsGained} level(s)! Now level {level}");
        }

        public bool LearnSpell(Spell spell) {
            if (!spell.CanCreatureLearn(this)) {
                return false;
            }
            
            // Sprawdź czy zawomon już zna ten spell
            if (!spells.Exists(s => s.name == spell.name) &&
                !learningSpells.Exists(ls => ls.spellName == spell.name)) {
                // Sprawdź czy zawomon już się czegoś uczy
                if (learningSpells.Count > 0) {
                    Debug.Log($"{name} już się uczy innego spella. Poczekaj na zakończenie nauki.");
                    return false;
                }

                if (spell.learnTimeSeconds > 0) {
                    StartLearningSpell(spell);
                }
                else {
                    spells.Add(spell);
                }
                return true;
            }
            return false;
        }
        
        public void AddSpellInstantly(Spell spell) {
            // for debug purposes only, bypass checks
            if (!spells.Exists(s => s.name == spell.name))
            {
                spells.Add(spell);
            }
        }

        public void StartLearningSpell(Spell spell) {
            var data = new LearningSpellData {
                spellName = spell.name,
                startTimeUtc = DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalSeconds,
                learnTimeSeconds = spell.learnTimeSeconds
            };
            learningSpells.Add(data);
            Debug.Log($"{name} zaczyna naukę spella {spell.name} (czas: {spell.learnTimeSeconds} s)");
        }

        public void UpdateLearningSpells(List<Spell> allSpells) {
            double now = DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalSeconds;
            List<LearningSpellData> finished = new List<LearningSpellData>();
            foreach (var ls in learningSpells) {
                if (now - ls.startTimeUtc >= ls.learnTimeSeconds) {
                    Spell spell = allSpells.Find(s => s.name == ls.spellName);
                    if (spell != null) {
                        spells.Add(spell);
                        Debug.Log($"{name} nauczył się spella {spell.name}!");
                    }
                    finished.Add(ls);
                }
            }
            foreach (var f in finished)
                learningSpells.Remove(f);
        }

        // Serializacja/deserializacja nauki spellów
        public string GetLearningSpellsJson() {
            return JsonUtility.ToJson(new LearningSpellListWrapper { LearningSpells = this.learningSpells });
        }
        public void SetLearningSpellsFromJson(string json) {
            var wrapper = JsonUtility.FromJson<LearningSpellListWrapper>(json);
            if (wrapper != null && wrapper.LearningSpells != null)
                this.learningSpells = wrapper.LearningSpells;
        }
        [Serializable]
        private class LearningSpellListWrapper {
            public List<LearningSpellData> LearningSpells;
        }
    }
}