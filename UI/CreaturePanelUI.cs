using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;
using Models;
using Systems;
using System.Threading.Tasks;

namespace UI {
    public class CreaturePanelUI : MonoBehaviour {

        [Header("UI Elements")]
        public TextMeshProUGUI zawomonInfoText;
        public TextMeshProUGUI goldText;
        public Transform spellsGridParent;
        public GameObject spellButtonPrefab;
        public TextMeshProUGUI learningStatusText;
        public Slider learningProgressBar;

        private Creature zawomon;
        private List<Spell> allSpells;
        private List<GameObject> spellButtons = new List<GameObject>();
        private int playerGold = 100;
        private int lastLearningCount = 0; // Dodaj pole do przechowywania liczby spellów uczących się

        async void Start() {
            // Poczekaj na załadowanie danych z GameManager
            while (GameManager.Instance?.PlayerData == null) {
                await Task.Delay(100);
            }
            
            zawomon = GameManager.Instance.PlayerData.creatures[0];
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
            int currentExp = zawomon.experience;
            int expForCurrentLevel = Creature.GetExpForLevel(zawomon.level);
            int expForNextLevel = Creature.GetExpForLevel(zawomon.level + 1);
            int expInCurrentLevel = currentExp - expForCurrentLevel;
            int expNeededForNextLevel = expForNextLevel - expForCurrentLevel;
            
            zawomonInfoText.text = $"<b>{zawomon.name}</b>\n" +
                $"Klasa: {zawomon.mainElement}" + (zawomon.secondaryElement != null ? $" / {zawomon.secondaryElement}" : "") + "\n" +
                $"Poziom: {zawomon.level} ({expInCurrentLevel}/{expNeededForNextLevel} EXP)\n" +
                $"HP: {zawomon.currentHP}/{zawomon.maxHP}\nDMG: {zawomon.damage}\nKolor: {zawomon.color}" + "\n" +
                $"Spells: {zawomon.spells.Count}";
        }

        void UpdateGoldInfo() {
            goldText.text = $"Gold: {playerGold}";
        }

        void RecreateSpellButtons() {
            // Sprawdź czy zawomon już się czegoś uczy
            bool isLearning = zawomon.learningSpells.Count > 0;

            // Jeśli nie ma przycisków, stwórz je
            if (spellButtons.Count == 0) {
                foreach (var spell in allSpells) {
                    var btnGo = Instantiate(spellButtonPrefab, spellsGridParent);
                    spellButtons.Add(btnGo);
                    var btn = btnGo.GetComponent<Button>();
                    var label = btnGo.GetComponentInChildren<TextMeshProUGUI>();
                    label.text = spell.name;

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
                bool alreadyLearned = zawomon.spells.Exists(s => s.name == spell.name);
                bool isCurrentlyLearning = zawomon.learningSpells.Exists(ls => ls.spellName == spell.name);
                
                if (alreadyLearned)
                    reasons.Add("Zawomon już zna ten spell");
                if (isCurrentlyLearning)
                    reasons.Add("Zawomon już uczy się tego spella");
                if (isLearning && !isCurrentlyLearning && !alreadyLearned)
                    reasons.Add("Zawomon już uczy się innego spella");
                if (spell.requiredClass != null && spell.requiredClass != zawomon.mainElement && spell.requiredClass != zawomon.secondaryElement)
                    reasons.Add($"Wymagana klasa: {spell.requiredClass}");
                if (zawomon.level < spell.requiredLevel)
                    reasons.Add($"Wymagany poziom: {spell.requiredLevel}");
                if (playerGold < 10) // przykładowy koszt
                    reasons.Add("Za mało golda (10)");

                bool canLearn = reasons.Count == 0 && spell.requiresLearning;
                btn.interactable = canLearn;
                
                // Debugowanie warunków
                Debug.Log($"Spell {spell.name}: alreadyLearned={alreadyLearned}, isLearning={isLearning}, canLearn={canLearn}, reasons={string.Join(", ", reasons)}");
                
                // Ustaw kolor ikony przez komponent hover
                if (alreadyLearned) {
                    hover.SetIconColor(Color.green); // Zielony dla nauczonych
                    Debug.Log($"Spell {spell.name} - ZIELONY (nauczony)");
                }
                else if (canLearn) {
                    hover.SetIconColor(Color.white); // Biały dla dostępnych
                    Debug.Log($"Spell {spell.name} - BIAŁY (dostępny)");
                }
                else {
                    hover.SetIconColor(Color.gray); // Szary dla niedostępnych
                    Debug.Log($"Spell {spell.name} - SZARY (niedostępny)");
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
            
            if (zawomon.learningSpells.Count > 0) {
                var ls = zawomon.learningSpells[0];
                float elapsed = (float)(System.DateTime.UtcNow.Subtract(System.DateTime.UnixEpoch).TotalSeconds - ls.startTimeUtc);
                float left = Mathf.Max(0, ls.learnTimeSeconds - elapsed);
                learningStatusText.text = $"Uczysz się: {ls.spellName} ({Mathf.CeilToInt(left)}s do końca)";
                learningProgressBar.gameObject.SetActive(true);
                learningProgressBar.maxValue = ls.learnTimeSeconds;
                learningProgressBar.value = Mathf.Clamp(elapsed, 0, ls.learnTimeSeconds);
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
            
            lastLearningCount = zawomon.learningSpells.Count;
        }
    }
}