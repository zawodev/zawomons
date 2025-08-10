using System.Collections.Generic;
using System.Threading.Tasks;
using Models;

namespace Systems {
    public static class GameAPI {
        // Placeholder dla ID gracza (później z logowania)
        private static int currentPlayerId = 1;

        // Placeholder dla danych gracza
        public static async Task<PlayerData> GetPlayerDataAsync() {
            // Symulacja opóźnienia sieci
            await Task.Delay(100);
            
            var playerData = new PlayerData();
            playerData.AddZawomon(ZawomonGenerator.GenerateRandomZawomon());
            playerData.AddZawomon(ZawomonGenerator.GenerateRandomZawomon());
            playerData.AddZawomon(ZawomonGenerator.GenerateRandomZawomon());
            playerData.AddZawomon(ZawomonGenerator.GenerateRandomZawomon());
            playerData.AddZawomon(ZawomonGenerator.GenerateRandomZawomon());
            playerData.AddZawomon(ZawomonGenerator.GenerateRandomZawomon());
            playerData.AddZawomon(ZawomonGenerator.GenerateRandomZawomon());
            playerData.AddZawomon(ZawomonGenerator.GenerateRandomZawomon());
            playerData.AddZawomon(ZawomonGenerator.GenerateRandomZawomon());
            playerData.AddZawomon(ZawomonGenerator.GenerateRandomZawomon());
            playerData.AddZawomon(ZawomonGenerator.GenerateRandomZawomon());
            playerData.AddZawomon(ZawomonGenerator.GenerateRandomZawomon());
            playerData.AddZawomon(ZawomonGenerator.GenerateRandomZawomon());
            playerData.AddZawomon(ZawomonGenerator.GenerateRandomZawomon());
            playerData.AddZawomon(ZawomonGenerator.GenerateRandomZawomon());
            
            // Zawomon zaczyna bez żadnych spellów - musi je nauczyć się sam
            // var zawomon = playerData.Zawomons[0];
            // zawomon.LearnSpell(GetAllSpells()[5]); // Usunięte automatyczne dodawanie
            
            return playerData;
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
            if (GameManager.Instance?.PlayerData?.Zawomons.Count > 0) {
                return GameManager.Instance.PlayerData.Zawomons[0].LearningSpells;
            }
            return new List<LearningSpellData>();
        }

        // Placeholder dla zapisu postępu nauki
        public static async Task<bool> SaveLearningProgressAsync(int zawomonId, List<LearningSpellData> learningSpells) {
            await Task.Delay(150);
            // Symulacja zapisu do bazy danych
            return true;
        }

        // Placeholder dla pobrania golda gracza
        public static async Task<int> GetPlayerGoldAsync() {
            await Task.Delay(50);
            return 100; // Statyczny gold na razie
        }

        // Placeholder dla aktualizacji golda gracza
        public static async Task<bool> UpdatePlayerGoldAsync(int newGold) {
            await Task.Delay(100);
            return true;
        }

        // Statyczna lista spellów (na razie lokalna, później z bazy)
        private static List<Spell> GetAllSpells() {
            return new List<Spell> {
                new Spell {
                    Name = "Basic Attack",
                    Type = SpellType.Attack,
                    RequiredClass = null,
                    RequiredLevel = 1,
                    Power = 10,
                    Description = "Basic Attack",
                    LearnTimeSeconds = 10f,
                    RequiresLearning = true
                },
                new Spell {
                    Name = "Basic Attack 2",
                    Type = SpellType.Attack,
                    RequiredClass = null,
                    RequiredLevel = 1,
                    Power = 10,
                    Description = "Basic Attack",
                    LearnTimeSeconds = 10f,
                    RequiresLearning = true
                },
                new Spell {
                    Name = "Ognisty Atak",
                    Type = SpellType.Attack,
                    RequiredClass = ZawomonClass.Fire,
                    RequiredLevel = 1,
                    Power = 15,
                    Description = "Silny atak ogniem.",
                    LearnTimeSeconds = 5f,
                    RequiresLearning = true
                },
                new Spell {
                    Name = "Wodny Strumień",
                    Type = SpellType.Attack,
                    RequiredClass = ZawomonClass.Water,
                    RequiredLevel = 1,
                    Power = 13,
                    Description = "Atak wodnym strumieniem.",
                    LearnTimeSeconds = 5f,
                    RequiresLearning = true
                },
                new Spell {
                    Name = "Leczenie",
                    Type = SpellType.Heal,
                    RequiredClass = null,
                    RequiredLevel = 2,
                    Power = 20,
                    Description = "Przywraca HP.",
                    LearnTimeSeconds = 5f,
                    RequiresLearning = true
                },
                new Spell {
                    Name = "Szybki Cios",
                    Type = SpellType.Attack,
                    RequiredClass = null,
                    RequiredLevel = 1,
                    Power = 8,
                    Description = "Uniwersalny szybki atak.",
                    LearnTimeSeconds = 0f,
                    RequiresLearning = false
                }
            };
        }
    }
} 