using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Models;
using Systems;
using System.Threading.Tasks;

namespace UI {
    public class ZawomonPanelUI : MonoBehaviour {
        [Header("UI Elements")]
        public TextMeshProUGUI zawomonInfoText;
        public TextMeshProUGUI goldText;
        public Transform spellsGridParent;
        public GameObject spellButtonPrefab;
        public TextMeshProUGUI learningStatusText;
        public Slider learningProgressBar;

        private Zawomon zawomon;
        private List<Spell> allSpells;
        private List<GameObject> spellButtons = new List<GameObject>();
        private int playerGold = 100;
        private int lastLearningCount = 0; // Dodaj pole do przechowywania liczby spellów uczących się

        async void Start() {
            // Poczekaj na załadowanie danych z GameManager
            while (GameManager.Instance?.PlayerData == null) {
                await Task.Delay(100);
            }
            
            zawomon = GameManager.Instance.PlayerData.Zawomons[0];
            allSpells = GameManager.Instance.AllSpells;
            
            // Pobierz gold z API
            playerGold = await GameAPI.GetPlayerGoldAsync();
            
            UpdatePanel();
            InvokeRepeating(nameof(UpdateLearningStatus), 0f, 1f);
            
            // Wymuś aktualizację po załadowaniu (dla kolorów nauczonych spellów)
            Invoke(nameof(UpdatePanel), 0.1f);
        }

        void UpdatePanel() {
            UpdateZawomonInfo();
            UpdateGoldInfo();
            RecreateSpellButtons();
            UpdateLearningStatus();
        }

        void UpdateZawomonInfo() {
            zawomonInfoText.text = $"<b>{zawomon.Name}</b>\n" +
                $"Klasa: {zawomon.MainClass}" + (zawomon.SecondaryClass != null ? $" / {zawomon.SecondaryClass}" : "") + "\n" +
                $"Poziom: {zawomon.Level}\nHP: {zawomon.CurrentHP}/{zawomon.MaxHP}\nDMG: {zawomon.Damage}\nKolor: {zawomon.Color}" + "\n" +
                $"Spells: {zawomon.Spells.Count}";
        }

        void UpdateGoldInfo() {
            goldText.text = $"Gold: {playerGold}";
        }

        void RecreateSpellButtons() {
            // Sprawdź czy zawomon już się czegoś uczy
            bool isLearning = zawomon.LearningSpells.Count > 0;

            // Jeśli nie ma przycisków, stwórz je
            if (spellButtons.Count == 0) {
                foreach (var spell in allSpells) {
                    var btnGo = Instantiate(spellButtonPrefab, spellsGridParent);
                    spellButtons.Add(btnGo);
                    var btn = btnGo.GetComponent<Button>();
                    var label = btnGo.GetComponentInChildren<TextMeshProUGUI>();
                    label.text = spell.Name;

                    // Dodaj komponent hover do przycisku
                    var hover = btnGo.GetComponent<SpellButtonHover>();
                    if (hover == null)
                        hover = btnGo.AddComponent<SpellButtonHover>();
                    hover.spell = spell;
                    hover.zawomon = zawomon;
                    hover.playerGold = playerGold;
                }
            }

            // Aktualizuj istniejące przyciski
            for (int i = 0; i < allSpells.Count && i < spellButtons.Count; i++) {
                var spell = allSpells[i];
                var btnGo = spellButtons[i];
                var btn = btnGo.GetComponent<Button>();
                var hover = btnGo.GetComponent<SpellButtonHover>();
                
                // Zaktualizuj referencje
                hover.spell = spell;
                hover.zawomon = zawomon;
                hover.playerGold = playerGold;

                // Sprawdź warunki nauki
                List<string> reasons = new List<string>();
                bool alreadyLearned = zawomon.Spells.Exists(s => s.Name == spell.Name);
                bool isCurrentlyLearning = zawomon.LearningSpells.Exists(ls => ls.SpellName == spell.Name);
                
                if (alreadyLearned)
                    reasons.Add("Zawomon już zna ten spell");
                if (isCurrentlyLearning)
                    reasons.Add("Zawomon już uczy się tego spella");
                if (isLearning && !isCurrentlyLearning && !alreadyLearned)
                    reasons.Add("Zawomon już uczy się innego spella");
                if (spell.RequiredClass != null && spell.RequiredClass != zawomon.MainClass && spell.RequiredClass != zawomon.SecondaryClass)
                    reasons.Add($"Wymagana klasa: {spell.RequiredClass}");
                if (zawomon.Level < spell.RequiredLevel)
                    reasons.Add($"Wymagany poziom: {spell.RequiredLevel}");
                if (playerGold < 10) // przykładowy koszt
                    reasons.Add("Za mało golda (10)");

                bool canLearn = reasons.Count == 0 && spell.RequiresLearning;
                btn.interactable = canLearn;
                
                // Debugowanie warunków
                Debug.Log($"Spell {spell.Name}: alreadyLearned={alreadyLearned}, isLearning={isLearning}, canLearn={canLearn}, reasons={string.Join(", ", reasons)}");
                
                // Ustaw kolor ikony przez komponent hover
                if (alreadyLearned) {
                    hover.SetIconColor(Color.green); // Zielony dla nauczonych
                    Debug.Log($"Spell {spell.Name} - ZIELONY (nauczony)");
                }
                else if (canLearn) {
                    hover.SetIconColor(Color.white); // Biały dla dostępnych
                    Debug.Log($"Spell {spell.Name} - BIAŁY (dostępny)");
                }
                else {
                    hover.SetIconColor(Color.gray); // Szary dla niedostępnych
                    Debug.Log($"Spell {spell.Name} - SZARY (niedostępny)");
                }

                // Usuń stare listenery i dodaj nowe
                btn.onClick.RemoveAllListeners();
                if (canLearn) {
                    btn.onClick.AddListener(async () => {
                        playerGold -= 10;
                        zawomon.LearnSpell(spell);
                        
                        // Zaktualizuj gold w API
                        await GameAPI.UpdatePlayerGoldAsync(playerGold);
                        
                        // Odśwież tylko informacje o zawomonie i przyciski (bez rekurencji)
                        UpdateZawomonInfo();
                        UpdateGoldInfo();
                        RecreateSpellButtons();
                    });
                }
            }
        }

        void UpdateLearningStatus() {
            // Sprawdź i zaktualizuj naukę spellów
            zawomon.UpdateLearningSpells(allSpells);
            
            if (zawomon.LearningSpells.Count > 0) {
                var ls = zawomon.LearningSpells[0];
                float elapsed = (float)(System.DateTime.UtcNow.Subtract(System.DateTime.UnixEpoch).TotalSeconds - ls.StartTimeUtc);
                float left = Mathf.Max(0, ls.LearnTimeSeconds - elapsed);
                learningStatusText.text = $"Uczysz się: {ls.SpellName} ({Mathf.CeilToInt(left)}s do końca)";
                learningProgressBar.gameObject.SetActive(true);
                learningProgressBar.maxValue = ls.LearnTimeSeconds;
                learningProgressBar.value = Mathf.Clamp(elapsed, 0, ls.LearnTimeSeconds);
            }
            else {
                learningStatusText.text = "Nie uczysz się żadnego spella";
                learningProgressBar.gameObject.SetActive(false);
                
                // Sprawdź czy właśnie skończyła się nauka (była w poprzedniej klatce)
                if (lastLearningCount > 0) {
                    Debug.Log("Nauka spella zakończona! Odświeżam UI...");
                    // Finalne odświeżenie po zakończeniu nauki
                    UpdateZawomonInfo();
                    UpdateGoldInfo();
                    RecreateSpellButtons();
                }
            }
            
            lastLearningCount = zawomon.LearningSpells.Count;
        }
    }
}