using System.Collections.Generic;
using UnityEngine;
using System;

namespace Models {
    [System.Serializable]
    public class LearningSpellData {
        public string SpellName;
        public double StartTimeUtc;
        public float LearnTimeSeconds;
    }

    [System.Serializable]
    public class Zawomon {
        public string Name;
        public ZawomonClass MainClass;
        public ZawomonClass? SecondaryClass;
        public Color Color;
        public int Level;
        public int MaxHP;
        public int CurrentHP;
        public int Damage;
        public int Initiative; // inicjatywa do walki
        public List<Spell> Spells = new List<Spell>();
        public List<LearningSpellData> LearningSpells = new List<LearningSpellData>();

        public void LevelUp() {
            Level++;
            MaxHP += 10;
            Damage += 2;
            Initiative += 1;
            CurrentHP = MaxHP;
        }

    public bool LearnSpell(Spell spell) {
            if (Level >= spell.RequiredLevel &&
                (spell.RequiredClass == null ||
                spell.RequiredClass == MainClass ||
                spell.RequiredClass == SecondaryClass)) {
                // Sprawdź czy zawomon już zna ten spell
                if (!Spells.Exists(s => s.Name == spell.Name) &&
                !LearningSpells.Exists(ls => ls.SpellName == spell.Name)) {
                    // Sprawdź czy zawomon już się czegoś uczy
                    if (LearningSpells.Count > 0) {
                        Debug.Log($"{Name} już się uczy innego spella. Poczekaj na zakończenie nauki.");
                        return false;
                    }

                    if (spell.RequiresLearning) {
                        StartLearningSpell(spell);
                    }
                    else {
                        Spells.Add(spell);
                    }
                    return true;
                }
            }
            return false;
        }

        public void StartLearningSpell(Spell spell) {
            var data = new LearningSpellData {
                SpellName = spell.Name,
                StartTimeUtc = DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalSeconds,
                LearnTimeSeconds = spell.LearnTimeSeconds
            };
            LearningSpells.Add(data);
            Debug.Log($"{Name} zaczyna naukę spella {spell.Name} (czas: {spell.LearnTimeSeconds} s)");
        }

        public void UpdateLearningSpells(List<Spell> allSpells) {
            double now = DateTime.UtcNow.Subtract(DateTime.UnixEpoch).TotalSeconds;
            List<LearningSpellData> finished = new List<LearningSpellData>();
            foreach (var ls in LearningSpells) {
                if (now - ls.StartTimeUtc >= ls.LearnTimeSeconds) {
                    Spell spell = allSpells.Find(s => s.Name == ls.SpellName);
                    if (spell != null) {
                        Spells.Add(spell);
                        Debug.Log($"{Name} nauczył się spella {spell.Name}!");
                    }
                    finished.Add(ls);
                }
            }
            foreach (var f in finished)
                LearningSpells.Remove(f);
        }

        // Serializacja/deserializacja nauki spellów
        public string GetLearningSpellsJson() {
            return JsonUtility.ToJson(new LearningSpellListWrapper { LearningSpells = this.LearningSpells });
        }
        public void SetLearningSpellsFromJson(string json) {
            var wrapper = JsonUtility.FromJson<LearningSpellListWrapper>(json);
            if (wrapper != null && wrapper.LearningSpells != null)
                this.LearningSpells = wrapper.LearningSpells;
        }
        [Serializable]
        private class LearningSpellListWrapper {
            public List<LearningSpellData> LearningSpells;
        }
    }
}