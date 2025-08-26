using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Models;
using Unity.VisualScripting;

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
        public event System.Action<PlayerData> OnPlayerResourcesUpdated;
        public event System.Action<int> OnPlayerGoldUpdated;
        public event System.Action<int> OnPlayerWoodUpdated;
        public event System.Action<int> OnPlayerStoneUpdated;
        public event System.Action<int> OnPlayerGemsUpdated;

        public async void SetAuthToken(string token)
        {
            //authToken = token;
            Debug.Log("Otrzymano token: " + (string.IsNullOrEmpty(token) ? "PUSTY" : token.Substring(0, 10) + "..."));
            GameAPI.SetAuthToken(token);

            playerData = await GameAPI.GetPlayerDataAsync();
            if (playerData != null)
            {
                OnPlayerDataReady?.Invoke();
                OnPlayerResourcesUpdated?.Invoke(playerData);
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
        //player data getter
        public PlayerData GetPlayerData()
        {
            return playerData;
        }

        // Metoda do odświeżania danych gracza z API
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
                OnPlayerResourcesUpdated?.Invoke(playerData);
                OnPlayerGoldUpdated?.Invoke(playerData.gold);
                OnPlayerWoodUpdated?.Invoke(playerData.wood);
                OnPlayerStoneUpdated?.Invoke(playerData.stone);
                OnPlayerGemsUpdated?.Invoke(playerData.gems);
            }
        }

        // Metody do aktualizacji poszczególnych zasobów (w przyszłości będą wysyłać do API)
        public void UpdateGold(int newGold)
        {
            if (playerData != null)
            {
                playerData.gold = newGold;
                OnPlayerGoldUpdated?.Invoke(newGold);
                OnPlayerResourcesUpdated?.Invoke(playerData);
            }
        }

        public void UpdateWood(int newWood)
        {
            if (playerData != null)
            {
                playerData.wood = newWood;
                OnPlayerWoodUpdated?.Invoke(newWood);
                OnPlayerResourcesUpdated?.Invoke(playerData);
            }
        }

        public void UpdateStone(int newStone)
        {
            if (playerData != null)
            {
                playerData.stone = newStone;
                OnPlayerStoneUpdated?.Invoke(newStone);
                OnPlayerResourcesUpdated?.Invoke(playerData);
            }
        }

        public void UpdateGems(int newGems)
        {
            if (playerData != null)
            {
                playerData.gems = newGems;
                OnPlayerGemsUpdated?.Invoke(newGems);
                OnPlayerResourcesUpdated?.Invoke(playerData);
            }
        }

        // Metoda pomocnicza do sprawdzania czy dane są załadowane
        public bool IsPlayerDataLoaded()
        {
            return playerData != null;
        }

        // Metody do synchronizacji z API (w przyszłości będą wysyłać dane na serwer)
        public async Task SavePlayerDataToAPI()
        {
            if (playerData != null)
            {
                bool success = await GameAPI.SavePlayerDataAsync(playerData);
                if (success)
                {
                    Debug.Log("Dane gracza zapisane pomyślnie na serwerze");
                }
                else
                {
                    Debug.LogError("Błąd podczas zapisywania danych gracza");
                }
            }
        }

        public async Task UpdateResourcesOnAPI()
        {
            if (playerData != null)
            {
                bool success = await GameAPI.UpdatePlayerResourcesAsync(
                    playerData.gold, 
                    playerData.wood, 
                    playerData.stone, 
                    playerData.gems
                );
                
                if (success)
                {
                    Debug.Log("Zasoby gracza zaktualizowane na serwerze");
                }
                else
                {
                    Debug.LogError("Błąd podczas aktualizacji zasobów gracza");
                }
            }
        }


        public List<Spell> GetAllSpells()
        {
            return allSpells;
        }

        public List<Spell> GetAllSpellsFromAPI()
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