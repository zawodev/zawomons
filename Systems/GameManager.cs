using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Models;

namespace Systems {
    public class GameManager : MonoBehaviour {
        public static GameManager Instance { get; private set; }
        public PlayerData PlayerData { get; private set; }
        public List<Spell> AllSpells = new List<Spell>();
        private string learningFilePath;

        public event System.Action OnPlayerDataReady;

        private async void Awake() {
            Debug.Log("GameManager Awake");
            if (Instance != null && Instance != this) {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            // Pobierz dane z API (placeholder)
            await LoadGameDataAsync();
        }

        private async Task LoadGameDataAsync() {
            // Pobierz dane gracza z API
            PlayerData = await GameAPI.GetPlayerDataAsync();

            Debug.Log("Załadowano dane gracza: " + PlayerData.Name);
            // Powiadom subskrybentów, że PlayerData jest gotowe
            OnPlayerDataReady?.Invoke();

            // Pobierz wszystkie spelle z API
            AllSpells = await GameAPI.GetAllSpellsAsync();

            // Pobierz postęp nauki z API
            await LoadLearningSpellsAsync();

            Debug.Log("Twój pierwszy zawomon: " + PlayerData.Zawomons[0].Name + ", klasa: " + PlayerData.Zawomons[0].MainClass + ", kolor: " + PlayerData.Zawomons[0].Color);
        }

        private void OnApplicationQuit() {
            SaveLearningSpellsAsync();
        }

        private async Task SaveLearningSpellsAsync() {
            if (PlayerData?.Zawomons.Count > 0) {
                var zawomon = PlayerData.Zawomons[0];
                await GameAPI.SaveLearningProgressAsync(1, zawomon.LearningSpells); // 1 = placeholder zawomon ID
                Debug.Log("Zapisano postęp nauki spellów do API");
            }
        }

        private async Task LoadLearningSpellsAsync() {
            if (PlayerData?.Zawomons.Count > 0) {
                var zawomon = PlayerData.Zawomons[0];
                var learningSpells = await GameAPI.GetLearningProgressAsync(1); // 1 = placeholder zawomon ID
                zawomon.LearningSpells = learningSpells;
                Debug.Log("Wczytano postęp nauki spellów z API");
            }
        }
    }
}