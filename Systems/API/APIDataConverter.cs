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
            
            // Konwertuj spells z API response na obiekty Spell i LearningSpellData
            if (creatureResponse.spells != null)
            {
                ConvertApiSpellsToCreature(creature, creatureResponse.spells);
            }
            
            return creature;
        }
        
        private static void ConvertApiSpellsToCreature(Creature creature, SpellDataResponse[] apiSpells)
        {
            var allSpells = Systems.GameManager.Instance?.GetAllSpells();
            
            if (allSpells == null)
            {
                Debug.LogWarning("GameManager lub allSpells nie jest dostępne podczas konwersji spelli");
                return;
            }
            
            foreach (var apiSpell in apiSpells)
            {
                Spell spell = allSpells.Find(s => s.id == apiSpell.spell_id);
                if (spell != null)
                {
                    if (apiSpell.is_learned)
                    {
                        // Spell jest już nauczony - dodaj do znanych spelli
                        if (!creature.spells.Exists(s => s.id == spell.id))
                        {
                            creature.spells.Add(spell);
                        }
                    }
                    else
                    {
                        // Spell się nadal uczy - dodaj do learningSpells
                        if (!creature.learningSpells.Exists(ls => ls.spellName == spell.name))
                        {
                            // Oblicz czas nauki na podstawie start_time i end_time
                            if (System.DateTime.TryParse(apiSpell.start_time, out System.DateTime startTime) &&
                                System.DateTime.TryParse(apiSpell.end_time, out System.DateTime endTime))
                            {
                                double startTimeUnix = startTime.Subtract(System.DateTime.UnixEpoch).TotalSeconds;
                                double learnTimeSeconds = endTime.Subtract(startTime).TotalSeconds;
                                
                                var learningSpellData = new Models.LearningSpellData
                                {
                                    spellName = spell.name,
                                    startTimeUtc = startTimeUnix,
                                    learnTimeSeconds = (float)learnTimeSeconds
                                };
                                
                                creature.learningSpells.Add(learningSpellData);
                            }
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"Nie znaleziono spella o ID: {apiSpell.spell_id}");
                }
            }
        }
        
        public static SpellRequestData[] ConvertCreatureSpellsToApiFormat(Creature creature)
        {
            var spellRequests = new System.Collections.Generic.List<SpellRequestData>();
            
            // Dodaj znane spelle (is_learned = true)
            foreach (var spell in creature.spells)
            {
                var spellRequest = new SpellRequestData
                {
                    spell_id = spell.id,
                    start_time = System.DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                    end_time = System.DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ"), // Dla nauczonych spelli start_time = end_time
                    is_learned = true
                };
                spellRequests.Add(spellRequest);
            }
            
            // Dodaj uczące się spelle (is_learned = false)
            var allSpells = Systems.GameManager.Instance?.GetAllSpells();
            if (allSpells != null)
            {
                foreach (var learningSpell in creature.learningSpells)
                {
                    Spell spell = allSpells.Find(s => s.name == learningSpell.spellName);
                    if (spell != null)
                    {
                        var startTime = System.DateTime.UnixEpoch.AddSeconds(learningSpell.startTimeUtc);
                        var endTime = startTime.AddSeconds(learningSpell.learnTimeSeconds);
                        
                        var spellRequest = new SpellRequestData
                        {
                            spell_id = spell.id,
                            start_time = startTime.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                            end_time = endTime.ToString("yyyy-MM-ddTHH:mm:ssZ"),
                            is_learned = false
                        };
                        spellRequests.Add(spellRequest);
                    }
                }
            }
            
            return spellRequests.ToArray();
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
