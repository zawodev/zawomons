using UnityEngine;
using System.Linq;
using Systems.Creatures.Models;
using Systems.Game.Core;

namespace Systems.Creatures.Core {
    public static class CreatureGenerator {
        private static string[] namePrefixes = {"Za", "Wo", "Mon", "Ka", "Lu", "Pi", "Ra", "Su"};
        private static string[] nameSuffixes = {"mon", "gon", "rus", "lek", "tor", "nix", "zar"};

        public static Creature GenerateRandomZawomon(int level = 1) {
            CreatureElement creatureElement = (CreatureElement)Random.Range(0, System.Enum.GetValues(typeof(CreatureElement)).Length);
            string name = namePrefixes[Random.Range(0, namePrefixes.Length)] + nameSuffixes[Random.Range(0, nameSuffixes.Length)];
            Color color = GetColorForElement(creatureElement);
            
            // Oblicz EXP na podstawie poziomu
            int targetExp = Creature.GetExpForLevel(level);
            // Dodaj trochę losowego EXP w ramach tego poziomu
            int maxExpForThisLevel = Creature.GetExpForLevel(level + 1);
            int randomExpBonus = Random.Range(0, maxExpForThisLevel - targetExp);
            int finalExp = targetExp + randomExpBonus;
            
            // Losowo wybierz czy ma secondary element (50% szans)
            CreatureElement? secondaryElement = null;
            if (Random.Range(0, 2) == 1) {
                // 30% szans na taki sam element (podwójny element)
                if (Random.Range(0, 100) < 30) {
                    secondaryElement = creatureElement;
                } else {
                    // Wybierz losowy inny element
                    CreatureElement randomSecondary = (CreatureElement)Random.Range(0, System.Enum.GetValues(typeof(CreatureElement)).Length);
                    secondaryElement = randomSecondary;
                }
            }
            
            // Stwórz creature z EXP, level będzie automatycznie obliczony
            Creature creature = new Creature {
                name = name,
                mainElement = creatureElement,
                secondaryElement = secondaryElement,
                color = color,
                experience = finalExp,
                maxHP = Random.Range(30, 51) + level * 5,
                maxEnergy = Random.Range(20, 31) + level * 3,
                damage = Random.Range(5, 11) + level * 2,
            };
            
            // Ustaw currentHP i currentEnergy na max
            creature.currentHP = creature.maxHP;
            creature.currentEnergy = creature.maxEnergy;
            
            // Dodaj losowe proste spelle na starcie
            AddRandomStarterSpells(creature);
            
            return creature;
        }

        private static void AddRandomStarterSpells(Creature creature)
        {
            // Pobierz wszystkie spelle z GameAPI
            var allSpells = GameManager.Instance.GetAllSpells();

            // Znajdź proste spelle które creature może się nauczyć (poziom 1 lub 0)
            var starterSpells = allSpells.Where(s =>
                s.requiredLevel <= 1 &&
                s.CanCreatureLearn(creature) &&
                s.learnTimeSeconds <= 5f // tylko szybko uczące się spelle na start
            ).ToList();

            // Wybierz losowo 2-4 spelle z tej listy
            int spellsToLearn = Random.Range(2, 5);
            var selectedSpells = starterSpells.OrderBy(x => Random.value).Take(spellsToLearn).ToList();
            foreach (var spell in selectedSpells)
            {
                creature.AddSpellInstantly(spell);
            }
        }

        public static Color GetColorForElement(CreatureElement creatureElement) {
            switch (creatureElement) {
                case CreatureElement.Fire:
                    return new Color(1f, 0.3f, 0.1f);
                case CreatureElement.Water:
                    return new Color(0.2f, 0.5f, 1f);
                case CreatureElement.Ice:
                    return new Color(0.6f, 0.9f, 1f);
                case CreatureElement.Stone:
                    return new Color(0.5f, 0.4f, 0.3f);
                case CreatureElement.Nature:
                    return new Color(0.2f, 0.8f, 0.2f);
                case CreatureElement.Magic:
                    return new Color(0.7f, 0.2f, 1f);
                case CreatureElement.DarkMagic:
                    return new Color(0.2f, 0.1f, 0.3f);
                default:
                    return Color.white;
            }
        }
    }
}
