using UnityEngine;

namespace Models {
    public static class CreatureGenerator {
        private static string[] namePrefixes = {"Za", "Wo", "Mon", "Ka", "Lu", "Pi", "Ra", "Su"};
        private static string[] nameSuffixes = {"mon", "gon", "rus", "lek", "tor", "nix", "zar"};

        public static Creature GenerateRandomZawomon(int level = 1) {
            CreatureElement zawomonClass = (CreatureElement)Random.Range(0, System.Enum.GetValues(typeof(CreatureElement)).Length);
            string name = namePrefixes[Random.Range(0, namePrefixes.Length)] + nameSuffixes[Random.Range(0, nameSuffixes.Length)];
            Color color = GetColorForElement(zawomonClass);
            
            // Oblicz EXP na podstawie poziomu
            int targetExp = Creature.GetExpForLevel(level);
            // Dodaj trochę losowego EXP w ramach tego poziomu
            int maxExpForThisLevel = Creature.GetExpForLevel(level + 1);
            int randomExpBonus = Random.Range(0, maxExpForThisLevel - targetExp);
            int finalExp = targetExp + randomExpBonus;
            
            // Stwórz creature z EXP, level będzie automatycznie obliczony
            Creature zawomon = new Creature {
                name = name,
                mainElement = zawomonClass,
                secondaryElement = null,
                color = color,
                experience = finalExp,
                maxHP = Random.Range(30, 51) + level * 5,
                maxEnergy = Random.Range(20, 31) + level * 3,
                damage = Random.Range(5, 11) + level * 2,
            };
            
            // Ustaw currentHP i currentEnergy na max
            zawomon.currentHP = zawomon.maxHP;
            zawomon.currentEnergy = zawomon.maxEnergy;
            
            return zawomon;
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
