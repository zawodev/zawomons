using System.Collections.Generic;
using System.Threading.Tasks;
using Models;

namespace Systems {
    public static class GameAPI {
        // Placeholder dla ID gracza (później z logowania)
        private static int currentPlayerId = 1;

        // Struktura na globalne zasoby gracza
        public class PlayerResources {
            public int gold;
            public int wood;
            public int stone;
            public int gems;
        }

        // Eventy do powiadamiania UI o zmianach zasobów
        public static event System.Action<PlayerResources> OnPlayerResourcesUpdated;
        public static event System.Action<int> OnPlayerGoldUpdated;
        public static event System.Action<int> OnPlayerWoodUpdated;
        public static event System.Action<int> OnPlayerStoneUpdated;
        public static event System.Action<int> OnPlayerGemsUpdated;

        // Przechowywane zasoby (symulacja bazy)
        private static PlayerResources _resources = new PlayerResources {
            gold = 100,
            wood = 50,
            stone = 30,
            gems = 5
        };

        // Placeholder dla danych gracza
        public static async Task<PlayerData> GetPlayerDataAsync() {
            // Symulacja opóźnienia sieci
            await Task.Delay(100);
            var playerData = new PlayerData();
            for (int i = 0; i < 15; i++)
                playerData.AddCreature(CreatureGenerator.GenerateRandomZawomon());
            return playerData;
        }

        // Pobranie wszystkich zasobów gracza na raz
        public static async Task<PlayerResources> GetPlayerResourcesAsync() {
            await Task.Delay(50);
            return new PlayerResources {
                gold = _resources.gold,
                wood = _resources.wood,
                stone = _resources.stone,
                gems = _resources.gems
            };
        }

        // Aktualizacja pojedynczego zasobu (można wywołać po zakupie itp.)
        public static async Task<bool> UpdatePlayerGoldAsync(int newGold) {
            await Task.Delay(100);
            _resources.gold = newGold;
            OnPlayerGoldUpdated?.Invoke(newGold);
            OnPlayerResourcesUpdated?.Invoke(_resources);
            return true;
        }
        public static async Task<bool> UpdatePlayerWoodAsync(int newWood) {
            await Task.Delay(100);
            _resources.wood = newWood;
            OnPlayerWoodUpdated?.Invoke(newWood);
            OnPlayerResourcesUpdated?.Invoke(_resources);
            return true;
        }
        public static async Task<bool> UpdatePlayerStoneAsync(int newStone) {
            await Task.Delay(100);
            _resources.stone = newStone;
            OnPlayerStoneUpdated?.Invoke(newStone);
            OnPlayerResourcesUpdated?.Invoke(_resources);
            return true;
        }
        public static async Task<bool> UpdatePlayerGemsAsync(int newGems) {
            await Task.Delay(100);
            _resources.gems = newGems;
            OnPlayerGemsUpdated?.Invoke(newGems);
            OnPlayerResourcesUpdated?.Invoke(_resources);
            return true;
        }

        // Pojedyncze gettery (opcjonalnie, jeśli chcesz pobierać osobno)
        public static async Task<int> GetPlayerGoldAsync() {
            await Task.Delay(50);
            return _resources.gold;
        }
        public static async Task<int> GetPlayerWoodAsync() {
            await Task.Delay(50);
            return _resources.wood;
        }
        public static async Task<int> GetPlayerStoneAsync() {
            await Task.Delay(50);
            return _resources.stone;
        }
        public static async Task<int> GetPlayerGemsAsync() {
            await Task.Delay(50);
            return _resources.gems;
        }

        // Placeholder dla wszystkich dostępnych spellów
        public static async Task<List<Spell>> GetAllSpellsAsync() {
            await Task.Delay(50);
            return GetAllSpells();
        }

        // Placeholder dla nauki spella
        public static async Task<bool> LearnSpellAsync(int zawomonId, int spellId) {
            await Task.Delay(200);
            // Symulacja sukcesu/niepowodzenia
            return true;
        }

        // Placeholder dla sprawdzenia postępu nauki
        public static async Task<List<LearningSpellData>> GetLearningProgressAsync(int zawomonId) {
            await Task.Delay(100);
            // Zwróć aktualny postęp z lokalnego GameManager
            if (GameManager.Instance?.PlayerData?.creatures.Count > 0) {
                return GameManager.Instance.PlayerData.creatures[0].learningSpells;
            }
            return new List<LearningSpellData>();
        }

        // Placeholder dla zapisu postępu nauki
        public static async Task<bool> SaveLearningProgressAsync(int zawomonId, List<LearningSpellData> learningSpells) {
            await Task.Delay(150);
            // Symulacja zapisu do bazy danych
            return true;
        }

    // ...pozostałe metody bez zmian...

        // Statyczna lista spellów (na razie lokalna, później z bazy)
        private static List<Spell> GetAllSpells() {
            return new List<Spell> {
                new Spell {
                    name = "Basic Attack",
                    type = SpellType.Attack,
                    requiredClass = null,
                    requiredLevel = 1,
                    power = 10,
                    description = "Basic Attack",
                    learnTimeSeconds = 10f,
                    requiresLearning = true
                },
                new Spell {
                    name = "Basic Attack 2",
                    type = SpellType.Attack,
                    requiredClass = null,
                    requiredLevel = 1,
                    power = 10,
                    description = "Basic Attack",
                    learnTimeSeconds = 10f,
                    requiresLearning = true
                },
                new Spell {
                    name = "Ognisty Atak",
                    type = SpellType.Attack,
                    requiredClass = CreatureElement.Fire,
                    requiredLevel = 1,
                    power = 15,
                    description = "Silny atak ogniem.",
                    learnTimeSeconds = 5f,
                    requiresLearning = true
                },
                new Spell {
                    name = "Wodny Strumień",
                    type = SpellType.Attack,
                    requiredClass = CreatureElement.Water,
                    requiredLevel = 1,
                    power = 13,
                    description = "Atak wodnym strumieniem.",
                    learnTimeSeconds = 5f,
                    requiresLearning = true
                },
                new Spell {
                    name = "Leczenie",
                    type = SpellType.Heal,
                    requiredClass = null,
                    requiredLevel = 2,
                    power = 20,
                    description = "Przywraca HP.",
                    learnTimeSeconds = 5f,
                    requiresLearning = true
                },
                new Spell {
                    name = "Szybki Cios",
                    type = SpellType.Attack,
                    requiredClass = null,
                    requiredLevel = 1,
                    power = 8,
                    description = "Uniwersalny szybki atak.",
                    learnTimeSeconds = 0f,
                    requiresLearning = false
                }
            };
        }
    }
} 