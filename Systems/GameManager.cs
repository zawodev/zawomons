using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Models;
using Unity.VisualScripting;
using Systems.API;

namespace Systems {
    /// <summary>
    /// GameManager - główny manager stanu gry i interfejs do GameAPI
    /// 
    /// Odpowiedzialności:
    /// - Przechowywanie danych gracza (PlayerData) 
    /// - Zarządzanie eventami i powiadamianie UI o zmianach
    /// - Interfejs do komunikacji z GameAPI
    /// - Synchronizacja lokalnych zmian z serwerem
    /// 
    /// Eventy:
    /// - OnPlayerDataReady: gdy dane gracza zostały pobrane z API
    /// - OnPlayerResourcesUpdated: gdy zasoby gracza się zmieniły 
    /// - OnPlayerGoldUpdated/Wood/Stone/Gems: gdy konkretny zasób się zmienił
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }
        public PlayerData playerData { get; private set; }
        //public MapData CurrentMap { get; private set; } // in future
        public List<Spell> allSpells = new List<Spell>();

        // Eventy do powiadamiania innych systemów o zmianach danych gracza
        public event System.Action OnPlayerDataReady;
        public event System.Action<int> OnPlayerGoldUpdated;
        public event System.Action<int> OnPlayerWoodUpdated;
        public event System.Action<int> OnPlayerStoneUpdated;
        public event System.Action<int> OnPlayerGemsUpdated;
        public event System.Action<Creature> OnCreatureAdded;

        public async void SetAuthToken(string token)
        {
            //authToken = token;
            Debug.Log("Otrzymano token: " + (string.IsNullOrEmpty(token) ? "PUSTY" : token.Substring(0, 10) + "..."));
            GameAPI.SetAuthToken(token);

            playerData = await GameAPI.GetPlayerDataAsync();
            if (playerData != null)
            {
                // Sprawdź czy gracz może otrzymać startowego stworka
                if (playerData.can_claim_start_creature)
                {
                    await GiveStarterCreature();
                }
                
                OnPlayerDataReady?.Invoke();
                OnPlayerGoldUpdated?.Invoke(playerData.gold);
                OnPlayerWoodUpdated?.Invoke(playerData.wood);
                OnPlayerStoneUpdated?.Invoke(playerData.stone);
                OnPlayerGemsUpdated?.Invoke(playerData.gems);
            }
        }

        private void Awake()
        {
            //Debug.Log("GameManager Awake");
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);

            allSpells = GetAllSpellsFromAPI();
        }

        public PlayerData GetPlayerData()
        {
            return playerData;
        }
        public bool IsPlayerDataLoaded()
        {
            return playerData != null;
        }
        public async Task RefreshPlayerData()
        {
            if (string.IsNullOrEmpty(GameAPI.GetAuthToken()))
            {
                Debug.LogError("Brak tokena autoryzacji!");
                return;
            }

            PlayerData newData = await GameAPI.GetPlayerDataAsync();
            if (newData != null)
            {
                playerData = newData;
                OnPlayerDataReady?.Invoke();
                OnPlayerGoldUpdated?.Invoke(playerData.gold);
                OnPlayerWoodUpdated?.Invoke(playerData.wood);
                OnPlayerStoneUpdated?.Invoke(playerData.stone);
                OnPlayerGemsUpdated?.Invoke(playerData.gems);
            }
        }

        // Metody do aktualizacji poszczególnych zasobów z automatycznym zapisem do API
        public async void UpdateGold(int newGold)
        {
            if (playerData != null)
            {
                playerData.gold = newGold;
                OnPlayerGoldUpdated?.Invoke(newGold);
                
                // Automatycznie zapisz do API
                await GameAPI.SetSingleResourceAsync("gold", newGold);
            }
        }

        public async void UpdateWood(int newWood)
        {
            if (playerData != null)
            {
                playerData.wood = newWood;
                OnPlayerWoodUpdated?.Invoke(newWood);
                
                // Automatycznie zapisz do API
                await GameAPI.SetSingleResourceAsync("wood", newWood);
            }
        }

        public async void UpdateStone(int newStone)
        {
            if (playerData != null)
            {
                playerData.stone = newStone;
                OnPlayerStoneUpdated?.Invoke(newStone);
                
                // Automatycznie zapisz do API
                await GameAPI.SetSingleResourceAsync("stone", newStone);
            }
        }

        public async void UpdateGems(int newGems)
        {
            if (playerData != null)
            {
                playerData.gems = newGems;
                OnPlayerGemsUpdated?.Invoke(newGems);
                
                // Automatycznie zapisz do API
                await GameAPI.SetSingleResourceAsync("gems", newGems);
            }
        }

        // Metoda do dawania startowego stworka nowym graczom
        private async Task GiveStarterCreature()
        {
            if (playerData == null || !playerData.can_claim_start_creature)
                return;

            Debug.Log("Dajemy startowego stworka nowemu graczowi!");
            
            // Wygeneruj losowego stworka (poziom 1)
            Creature starterCreature = CreatureGenerator.GenerateRandomZawomon(1);
            
            // Wyślij stworka do backendu
            bool success = await GameAPI.AddCreatureAsync(starterCreature);
            
            if (success)
            {
                Debug.Log($"Pomyślnie dodano startowego stworka: {starterCreature.name} (Poziom {starterCreature.level})");
                
                // Dodaj stworka do lokalnych danych gracza
                playerData.AddCreature(starterCreature);
                
                // Zmień flagę na false lokalnie
                playerData.can_claim_start_creature = false;
                
                // Powiadom UI o dodaniu nowego stworka
                OnCreatureAdded?.Invoke(starterCreature);
            }
            else
            {
                Debug.LogError("Błąd podczas zapisywania startowego stworka do API");
            }
        }

        // Wygodne metody do dodawania/odejmowania zasobów
        public void AddGold(int amount)
        {
            if (playerData != null)
            {
                int newAmount = Mathf.Max(0, playerData.gold + amount);
                UpdateGold(newAmount);
            }
        }

        public void AddWood(int amount)
        {
            if (playerData != null)
            {
                int newAmount = Mathf.Max(0, playerData.wood + amount);
                UpdateWood(newAmount);
            }
        }

        public void AddStone(int amount)
        {
            if (playerData != null)
            {
                int newAmount = Mathf.Max(0, playerData.stone + amount);
                UpdateStone(newAmount);
            }
        }

        public void AddGems(int amount)
        {
            if (playerData != null)
            {
                int newAmount = Mathf.Max(0, playerData.gems + amount);
                UpdateGems(newAmount);
            }
        }
        
        // Metoda do dodawania stworka i wywołania eventu
        public void NotifyCreatureAdded(Creature creature)
        {
            OnCreatureAdded?.Invoke(creature);
        }


        public List<Spell> GetAllSpells()
        {
            return allSpells;
        }

        private List<Spell> GetAllSpellsFromAPI()
        {
            // tymczasowo zwracamy hardcoded spelle
            return new List<Spell> {
                new Spell {
                    id = 0,
                    name = "Basic Attack",
                    elementRequirements = new List<SpellElementRequirement>(), // uniwersalny
                    requiredLevel = 1,
                    description = "Basic Attack",
                    learnTimeSeconds = 10f,
                    effects = new List<SpellEffect> {
                        new SpellEffect(SpellTargetType.Enemy, SpellEffectType.Damage, 10)
                    }
                },
                new Spell {
                    id = 1,
                    name = "Basic Attack 2",
                    elementRequirements = new List<SpellElementRequirement>(), // uniwersalny
                    requiredLevel = 1,
                    description = "Basic Attack",
                    learnTimeSeconds = 10f,
                    effects = new List<SpellEffect> {
                        new SpellEffect(SpellTargetType.Enemy, SpellEffectType.Damage, 10)
                    }
                },
                new Spell {
                    id = 2,
                    name = "Ognisty Atak",
                    elementRequirements = new List<SpellElementRequirement> {
                        new SpellElementRequirement(CreatureElement.Fire, null) // głównie ognisty
                    },
                    requiredLevel = 1,
                    description = "Silny atak ogniem.",
                    learnTimeSeconds = 5f,
                    effects = new List<SpellEffect> {
                        new SpellEffect(SpellTargetType.Enemy, SpellEffectType.Damage, 15)
                    }
                },
                new Spell {
                    id = 3,
                    name = "Wodny Strumień",
                    elementRequirements = new List<SpellElementRequirement> {
                        new SpellElementRequirement(CreatureElement.Water, null) // głównie wodny
                    },
                    requiredLevel = 1,
                    description = "Atak wodnym strumieniem.",
                    learnTimeSeconds = 5f,
                    effects = new List<SpellEffect> {
                        new SpellEffect(SpellTargetType.Enemy, SpellEffectType.Damage, 13)
                    }
                },
                new Spell {
                    id = 4,
                    name = "Leczenie",
                    elementRequirements = new List<SpellElementRequirement>(), // uniwersalny
                    requiredLevel = 0,
                    description = "Przywraca HP.",
                    learnTimeSeconds = 0f,
                    effects = new List<SpellEffect> {
                        new SpellEffect(SpellTargetType.Self, SpellEffectType.Heal, 20)
                    }
                },
                new Spell {
                    id = 5,
                    name = "Szybki Cios",
                    elementRequirements = new List<SpellElementRequirement>(), // uniwersalny
                    requiredLevel = 1,
                    description = "Uniwersalny szybki atak.",
                    learnTimeSeconds = 0f,
                    effects = new List<SpellEffect> {
                        new SpellEffect(SpellTargetType.Enemy, SpellEffectType.Damage, 8)
                    }
                },
                new Spell {
                    id = 6,
                    name = "Lodowo-Wodny Wir",
                    elementRequirements = new List<SpellElementRequirement> {
                        new SpellElementRequirement(CreatureElement.Ice, CreatureElement.Water), // lód główny, woda secondary
                        new SpellElementRequirement(CreatureElement.Water, CreatureElement.Ice)  // woda główny, lód secondary
                    },
                    requiredLevel = 3,
                    description = "Potężny atak łączący moc lodu i wody.",
                    learnTimeSeconds = 15f,
                    effects = new List<SpellEffect> {
                        new SpellEffect(SpellTargetType.AllEnemies, SpellEffectType.Damage, 18)
                    }
                },
                new Spell {
                    id = 7,
                    name = "Grupowe Leczenie i Wzmocnienie",
                    elementRequirements = new List<SpellElementRequirement>(), // uniwersalny
                    requiredLevel = 4,
                    description = "Leczy wszystkich sojuszników i zwiększa ich inicjatywę.",
                    learnTimeSeconds = 20f,
                    effects = new List<SpellEffect> {
                        new SpellEffect(SpellTargetType.AllAllies, SpellEffectType.Heal, 15),
                        new SpellEffect(SpellTargetType.AllAllies, SpellEffectType.BuffInitiative, 5)
                    }
                }
            };
        }
    }
}