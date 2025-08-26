using System.Collections.Generic;
using UnityEngine;
using Models;

namespace Systems.API {
    /// <summary>
    /// APIDataConverter - klasa odpowiedzialna za konwersję danych między formatami API a modelami gry
    /// 
    /// Odpowiedzialności:
    /// - Konwersja PlayerDataResponse na PlayerData
    /// - Parsowanie elementów creatures
    /// - Konwersja kolorów i innych typów danych
    /// </summary>
    public static class APIDataConverter
    {
        public static PlayerData ConvertToPlayerData(PlayerDataResponse apiResponse)
        {
            PlayerData playerData = new PlayerData();
            playerData.id = apiResponse.id;
            playerData.username = apiResponse.username;
            playerData.gold = apiResponse.gold;
            playerData.wood = apiResponse.wood;
            playerData.stone = apiResponse.stone;
            playerData.gems = apiResponse.gems;
            playerData.lastPlayed = apiResponse.last_played;
            playerData.createdAt = apiResponse.created_at;
            playerData.can_claim_start_creature = apiResponse.can_claim_start_creature;

            // Convert creatures
            playerData.creatures = new List<Creature>();
            foreach (var creatureResponse in apiResponse.creatures)
            {
                Creature creature = ConvertToCreature(creatureResponse);
                playerData.creatures.Add(creature);
            }

            return playerData;
        }

        public static Creature ConvertToCreature(CreatureDataResponse creatureResponse)
        {
            Creature creature = new Creature();
            creature.id = creatureResponse.id;
            creature.name = creatureResponse.name;
            creature.mainElement = ParseElement(creatureResponse.main_element);
            creature.secondaryElement = ParseElement(creatureResponse.secondary_element);
            
            // Konwertuj string koloru na Color
            if (ColorUtility.TryParseHtmlString(creatureResponse.color, out Color parsedColor))
            {
                creature.color = parsedColor;
            }
            else
            {
                creature.color = Color.white; // Domyślny kolor
            }
            
            creature.experience = creatureResponse.experience;
            creature.maxHP = creatureResponse.max_hp;
            creature.currentHP = creatureResponse.current_hp;
            creature.maxEnergy = creatureResponse.max_energy;
            creature.currentEnergy = creatureResponse.current_energy;
            creature.damage = creatureResponse.damage;
            creature.initiative = creatureResponse.initiative;
            
            return creature;
        }

        private static CreatureElement ParseElement(string elementString)
        {
            if (string.IsNullOrEmpty(elementString))
                return CreatureElement.None;
                
            if (System.Enum.TryParse<CreatureElement>(elementString, true, out CreatureElement element))
                return element;
            
            return CreatureElement.None;
        }
    }
}
