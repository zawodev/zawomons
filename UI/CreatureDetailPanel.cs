using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Models;
using Systems;
using System.Linq;

namespace UI {
    public class CreatureDetailPanel : MonoBehaviour {
        [Header("Panel References")]
        public GameObject panelRoot;
        public Button closeButton;
        
        [Header("Tabs Navigation")]
        public Transform tabButtonsParent;
        public Button statsTabButton;
        public Button equipmentTabButton;
        public Button cityTabButton;
        public Button spellsTabButton;
        
        [Header("Tab Content Panels")]
        public GameObject statsTabContent;
        public GameObject equipmentTabContent;
        public GameObject cityTabContent;
        public GameObject spellsTabContent;
        
        [Header("Stats Tab UI")]
        public Image creatureImage;
        public TMP_Text creatureNameText;
        public TMP_Text creatureLevelText;
        public TMP_Text creatureElementsText;
        public Slider hpProgressBar;
        public TMP_Text hpText;
        public Slider energyProgressBar;
        public TMP_Text energyText;
        public Slider expProgressBar;
        public TMP_Text expText;
        public TMP_Text damageText;
        public TMP_Text initiativeText;
        
        [Header("Equipment Tab UI")]
        public Transform equipmentSlotsParent;
        public GameObject equipmentSlotPrefab;
        public Button transferButton;
        public TMP_Dropdown transferTargetDropdown;
        
        [Header("City Tab UI")]
        public TMP_Text cityNameText;
        
        [Header("Spells Tab UI")]
        public Image spellsCreatureImage;
        public Transform spellsGridParent;
        public GameObject spellSlotPrefab;
        
        // Data
        private Creature currentCreature;
        private TabType currentTab = TabType.Stats;
        
        // Tab colors
        [Header("Tab Styling")]
        public Color activeTabColor = Color.white;
        public Color inactiveTabColor = Color.gray;
        
        private enum TabType {
            Stats,
            Equipment,
            City,
            Spells
        }
        
        private void Awake() {
            InitializeUI();
        }
        
        private void InitializeUI() {
            // Close button
            if (closeButton != null) {
                closeButton.onClick.AddListener(HidePanel);
            }
            
            // Tab buttons
            if (statsTabButton != null) {
                statsTabButton.onClick.AddListener(() => SwitchToTab(TabType.Stats));
            }
            if (equipmentTabButton != null) {
                equipmentTabButton.onClick.AddListener(() => SwitchToTab(TabType.Equipment));
            }
            if (cityTabButton != null) {
                cityTabButton.onClick.AddListener(() => SwitchToTab(TabType.City));
            }
            if (spellsTabButton != null) {
                spellsTabButton.onClick.AddListener(() => SwitchToTab(TabType.Spells));
            }
            
            // Transfer button (mock - inactive)
            if (transferButton != null) {
                transferButton.interactable = false; // Mock - no map system yet
                transferButton.onClick.AddListener(OnTransferButtonClicked);
            }
            
            // Initialize as hidden
            if (panelRoot != null) {
                panelRoot.SetActive(false);
            }
        }
        
        public void ShowPanel(Creature creature) {
            if (creature == null) {
                Debug.LogWarning("Cannot show CreatureDetailPanel with null creature!");
                return;
            }
            
            currentCreature = creature;
            
            if (panelRoot != null) {
                panelRoot.SetActive(true);
            }
            
            // Switch to stats tab by default
            SwitchToTab(TabType.Stats);
            
            // Update all content
            UpdateAllTabs();
        }
        
        public void HidePanel() {
            if (panelRoot != null) {
                panelRoot.SetActive(false);
            }
            currentCreature = null;
        }
        
        private void SwitchToTab(TabType tabType) {
            currentTab = tabType;
            
            // Hide all tab contents
            if (statsTabContent != null) statsTabContent.SetActive(false);
            if (equipmentTabContent != null) equipmentTabContent.SetActive(false);
            if (cityTabContent != null) cityTabContent.SetActive(false);
            if (spellsTabContent != null) spellsTabContent.SetActive(false);
            
            // Show selected tab content
            GameObject activeContent = null;
            switch (tabType) {
                case TabType.Stats:
                    activeContent = statsTabContent;
                    break;
                case TabType.Equipment:
                    activeContent = equipmentTabContent;
                    break;
                case TabType.City:
                    activeContent = cityTabContent;
                    break;
                case TabType.Spells:
                    activeContent = spellsTabContent;
                    break;
            }
            
            if (activeContent != null) {
                activeContent.SetActive(true);
            }
            
            // Update tab button visuals
            UpdateTabButtonVisuals();
            
            // Update tab-specific content
            UpdateCurrentTabContent();
        }
        
        private void UpdateTabButtonVisuals() {
            // Reset all buttons to inactive color
            SetButtonColor(statsTabButton, inactiveTabColor);
            SetButtonColor(equipmentTabButton, inactiveTabColor);
            SetButtonColor(cityTabButton, inactiveTabColor);
            SetButtonColor(spellsTabButton, inactiveTabColor);
            
            // Set active button color
            switch (currentTab) {
                case TabType.Stats:
                    SetButtonColor(statsTabButton, activeTabColor);
                    break;
                case TabType.Equipment:
                    SetButtonColor(equipmentTabButton, activeTabColor);
                    break;
                case TabType.City:
                    SetButtonColor(cityTabButton, activeTabColor);
                    break;
                case TabType.Spells:
                    SetButtonColor(spellsTabButton, activeTabColor);
                    break;
            }
        }
        
        private void SetButtonColor(Button button, Color color) {
            if (button != null) {
                var colors = button.colors;
                colors.normalColor = color;
                button.colors = colors;
            }
        }
        
        private void UpdateAllTabs() {
            UpdateStatsTab();
            UpdateEquipmentTab();
            UpdateCityTab();
            UpdateSpellsTab();
        }
        
        private void UpdateCurrentTabContent() {
            switch (currentTab) {
                case TabType.Stats:
                    UpdateStatsTab();
                    break;
                case TabType.Equipment:
                    UpdateEquipmentTab();
                    break;
                case TabType.City:
                    UpdateCityTab();
                    break;
                case TabType.Spells:
                    UpdateSpellsTab();
                    break;
            }
        }
        
        private void UpdateStatsTab() {
            if (currentCreature == null) return;
            
            // Basic info
            if (creatureNameText != null) {
                creatureNameText.text = currentCreature.name;
            }
            
            if (creatureLevelText != null) {
                creatureLevelText.text = $"Level {currentCreature.level}";
            }
            
            if (creatureElementsText != null) {
                string elements = GetElementDisplayName(currentCreature.mainElement);
                if (currentCreature.secondaryElement.HasValue) {
                    elements += " / " + GetElementDisplayName(currentCreature.secondaryElement.Value);
                }
                creatureElementsText.text = elements;
            }
            
            // HP Progress Bar
            if (hpProgressBar != null) {
                hpProgressBar.maxValue = currentCreature.maxHP;
                hpProgressBar.value = currentCreature.currentHP;
            }
            if (hpText != null) {
                hpText.text = $"{currentCreature.currentHP} / {currentCreature.maxHP}";
            }
            
            // Energy Progress Bar
            if (energyProgressBar != null) {
                energyProgressBar.maxValue = currentCreature.maxEnergy;
                energyProgressBar.value = currentCreature.currentEnergy;
            }
            if (energyText != null) {
                energyText.text = $"{currentCreature.currentEnergy} / {currentCreature.maxEnergy}";
            }
            
            // EXP Progress Bar
            if (expProgressBar != null && expText != null) {
                int currentExp = currentCreature.experience;
                int expForCurrentLevel = Creature.GetExpForLevel(currentCreature.level);
                int expForNextLevel = Creature.GetExpForLevel(currentCreature.level + 1);
                int expInCurrentLevel = currentExp - expForCurrentLevel;
                int expNeededForNextLevel = expForNextLevel - expForCurrentLevel;
                
                expProgressBar.maxValue = expNeededForNextLevel;
                expProgressBar.value = expInCurrentLevel;
                expText.text = $"{expInCurrentLevel} / {expNeededForNextLevel} EXP";
            }
            
            // Other stats
            if (damageText != null) {
                damageText.text = $"Damage: {currentCreature.damage}";
            }
            if (initiativeText != null) {
                initiativeText.text = $"Initiative: {currentCreature.initiative}";
            }
            
            // Creature image (set color for now)
            if (creatureImage != null) {
                creatureImage.color = currentCreature.color;
            }
        }
        
        private void UpdateEquipmentTab() {
            if (currentCreature == null) return;
            
            // Mock equipment slots - clear existing
            if (equipmentSlotsParent != null) {
                foreach (Transform child in equipmentSlotsParent) {
                    Destroy(child.gameObject);
                }
                
                // Create mock equipment slots
                string[] equipmentSlots = {"Weapon", "Armor", "Accessory", "Boots", "Ring", "Necklace"};
                
                foreach (string slotName in equipmentSlots) {
                    if (equipmentSlotPrefab != null) {
                        GameObject slot = Instantiate(equipmentSlotPrefab, equipmentSlotsParent);
                        TMP_Text slotText = slot.GetComponentInChildren<TMP_Text>();
                        if (slotText != null) {
                            slotText.text = $"{slotName}: Empty";
                        }
                    }
                }
            }
            
            // Transfer dropdown (mock - empty)
            if (transferTargetDropdown != null) {
                transferTargetDropdown.ClearOptions();
                transferTargetDropdown.AddOptions(new List<string> {"No creatures nearby"});
                transferTargetDropdown.interactable = false;
            }
        }
        
        private void UpdateCityTab() {
            // Mock city system
            if (cityNameText != null) {
                cityNameText.text = "No city nearby";
                // In future: check creature location and display actual city name
            }
        }
        
        private void UpdateSpellsTab() {
            if (currentCreature == null) return;
            
            // Spells creature image
            if (spellsCreatureImage != null) {
                spellsCreatureImage.color = currentCreature.color;
            }
            
            // Clear existing spell slots
            if (spellsGridParent != null) {
                foreach (Transform child in spellsGridParent) {
                    Destroy(child.gameObject);
                }
                
                // Get all possible spells for this creature's elements
                var allSpells = Systems.GameAPI.GetAllSpells();
                var possibleSpells = GetPossibleSpellsForCreature(currentCreature, allSpells);
                
                // Create spell slots
                foreach (var spell in possibleSpells) {
                    if (spellSlotPrefab != null) {
                        GameObject spellSlot = Instantiate(spellSlotPrefab, spellsGridParent);
                        SetupSpellSlot(spellSlot, spell, currentCreature.spells.Contains(spell));
                    }
                }
            }
        }
        
        private List<Spell> GetPossibleSpellsForCreature(Creature creature, List<Spell> allSpells) {
            return allSpells.Where(spell => spell.CanCreatureLearn(creature)).ToList();
        }
        
        private void SetupSpellSlot(GameObject spellSlot, Spell spell, bool isKnown) {
            TMP_Text spellNameText = spellSlot.transform.Find("SpellName")?.GetComponent<TMP_Text>();
            Image spellBackground = spellSlot.GetComponent<Image>();
            
            if (spellNameText != null) {
                spellNameText.text = spell.name;
            }
            
            if (spellBackground != null) {
                spellBackground.color = isKnown ? Color.green : Color.gray;
            }
            
            // Add button component for future spell details
            Button spellButton = spellSlot.GetComponent<Button>();
            if (spellButton == null) {
                spellButton = spellSlot.AddComponent<Button>();
            }
            
            spellButton.onClick.RemoveAllListeners();
            spellButton.onClick.AddListener(() => OnSpellClicked(spell, isKnown));
        }
        
        private void OnSpellClicked(Spell spell, bool isKnown) {
            Debug.Log($"Clicked on spell: {spell.name} (Known: {isKnown})");
            // Future: Show spell details or learning options
        }
        
        private void OnTransferButtonClicked() {
            Debug.Log("Transfer button clicked - but no creatures nearby (mock)");
            // Future: Handle equipment transfers between creatures
        }
        
        private string GetElementDisplayName(CreatureElement element) {
            switch (element) {
                case CreatureElement.Fire: return "Fire";
                case CreatureElement.Water: return "Water";
                case CreatureElement.Ice: return "Ice";
                case CreatureElement.Stone: return "Stone";
                case CreatureElement.Nature: return "Nature";
                case CreatureElement.Magic: return "Magic";
                case CreatureElement.DarkMagic: return "Dark Magic";
                default: return element.ToString();
            }
        }
        
        private void OnDestroy() {
            // Clean up event listeners
            if (closeButton != null) {
                closeButton.onClick.RemoveAllListeners();
            }
        }
    }
}
