using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using Models;
using Systems;
using System;

namespace UI.Collection {
    public class CreatureCollectionPanel : MonoBehaviour {
        [Header("Panel References")]
        public GameObject panelRoot;
        public CreatureDetailPanel creatureDetailPanel;
        
        [Header("Search & Sort")]
        public TMP_InputField searchInput;
        public TMP_Dropdown sortDropdown;
        
        [Header("Element Filters")]
        public Transform elementFiltersParent;
        public GameObject elementFilterPrefab;
        public Button resetFiltersButton;
        
        [Header("Creatures Scroll View")]
        public ScrollRect creaturesScrollView;
        public Transform creaturesContentParent;
        public GameObject creatureItemPrefab;
        
        // Data
        private List<Creature> allCreatures = new List<Creature>();
        private List<Creature> filteredCreatures = new List<Creature>();
        private List<CreatureElement> activeElementFilters = new List<CreatureElement>();
        private Dictionary<CreatureElement, Toggle> elementToggles = new Dictionary<CreatureElement, Toggle>();
        
        // Settings
        private int currentSortIndex = 0; // Index dla dropdown
        private string currentSearchTerm = "";
        
        // Events
        public Action<Creature> OnCreatureSelected;
        
        // Sort options for dropdown
        private readonly string[] sortOptions = {
            "Name ↑",
            "Name ↓", 
            "Experience ↑",
            "Experience ↓",
            "Element ↑",
            "Element ↓"
        };
        
        private void Awake() {
            InitializeUI();
        }
        
        private void OnEnable() {
            LoadSettingsFromPlayerPrefs();
            RefreshCreaturesList();
            
            // Subskrybuj eventy GameManager
            if (GameManager.Instance != null) {
                GameManager.Instance.OnCreatureAdded += OnCreatureAdded;
                GameManager.Instance.OnCreatureUpdated += OnCreatureUpdated;
            }
        }
        
        private void OnDisable() {
            // Odsubskrybuj eventy GameManager
            if (GameManager.Instance != null) {
                GameManager.Instance.OnCreatureAdded -= OnCreatureAdded;
                GameManager.Instance.OnCreatureUpdated -= OnCreatureUpdated;
            }
        }
        
        private void InitializeUI() {
            // Ensure panel is hidden initially
            if (panelRoot != null) {
                panelRoot.SetActive(false);
            }

            // Search input listener
            if (searchInput != null)
            {
                searchInput.onValueChanged.AddListener(OnSearchChanged);
            }
            
            // Sort dropdown
            if (sortDropdown != null) {
                SetupSortDropdown();
            }
            
            // Reset filters button
            if (resetFiltersButton != null) {
                resetFiltersButton.onClick.AddListener(ResetAllFilters);
            }
            
            // Create element filter toggles
            CreateElementFilterToggles();
        }
        
        private void SetupSortDropdown() {
            sortDropdown.ClearOptions();
            sortDropdown.AddOptions(new List<string>(sortOptions));
            sortDropdown.value = currentSortIndex;
            sortDropdown.onValueChanged.AddListener(OnSortChanged);
        }
        
        private void CreateElementFilterToggles() {
            if (elementFiltersParent == null || elementFilterPrefab == null) return;
            
            var elements = System.Enum.GetValues(typeof(CreatureElement)).Cast<CreatureElement>();
            
            foreach (var element in elements) {
                GameObject toggleObj = Instantiate(elementFilterPrefab, elementFiltersParent);
                Toggle toggle = toggleObj.GetComponent<Toggle>();
                TMP_Text label = toggleObj.GetComponentInChildren<TMP_Text>();
                
                if (toggle != null && label != null) {
                    label.text = GetElementDisplayName(element);
                    toggle.onValueChanged.AddListener((isOn) => OnElementFilterChanged(element, isOn));
                    elementToggles[element] = toggle;
                }
            }
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
        
        public void ShowPanel() {
            if (panelRoot != null) {
                panelRoot.SetActive(true);
                LoadCreaturesData();
                RefreshCreaturesList();
            }
        }
        
        public void HidePanel() {
            if (panelRoot != null) {
                panelRoot.SetActive(false);
            }
            
            // Always hide detail panel when collection panel is hidden
            if (creatureDetailPanel != null) {
                creatureDetailPanel.HidePanel();
            }
        }
        
        private void LoadCreaturesData() {
            if (GameManager.Instance != null && GameManager.Instance.IsPlayerDataLoaded()) {
                allCreatures = new List<Creature>(GameManager.Instance.GetPlayerData().creatures);
            } else {
                Debug.LogWarning("No creatures data available from GameManager");
                allCreatures.Clear();
            }
        }
        
        private void OnSearchChanged(string searchTerm) {
            currentSearchTerm = searchTerm.ToLower();
            RefreshCreaturesList();
        }
        
        private void OnSortChanged(int sortIndex) {
            currentSortIndex = sortIndex;
            SaveSettingsToPlayerPrefs();
            RefreshCreaturesList();
        }
        
        private void OnElementFilterChanged(CreatureElement element, bool isEnabled) {
            if (isEnabled) {
                if (!activeElementFilters.Contains(element)) {
                    activeElementFilters.Add(element);
                }
            } else {
                activeElementFilters.Remove(element);
            }
            
            SaveSettingsToPlayerPrefs();
            RefreshCreaturesList();
        }
        
        private void ResetAllFilters() {
            activeElementFilters.Clear();
            
            // Reset all toggles
            foreach (var toggle in elementToggles.Values) {
                toggle.isOn = false;
            }
            
            // Reset search
            if (searchInput != null) {
                searchInput.text = "";
            }
            currentSearchTerm = "";
            
            SaveSettingsToPlayerPrefs();
            RefreshCreaturesList();
        }
        
        private void RefreshCreaturesList() {
            // Apply filters and search
            filteredCreatures = FilterCreatures(allCreatures);
            
            // Apply sorting
            filteredCreatures = SortCreatures(filteredCreatures);
            
            // Update UI
            UpdateCreaturesDisplay();
        }
        
        private List<Creature> FilterCreatures(List<Creature> creatures) {
            var filtered = creatures.AsEnumerable();
            
            // Apply search filter
            if (!string.IsNullOrEmpty(currentSearchTerm)) {
                filtered = filtered.Where(c => 
                    c.name.ToLower().Contains(currentSearchTerm) ||
                    GetElementDisplayName(c.mainElement).ToLower().Contains(currentSearchTerm) ||
                    (c.secondaryElement.HasValue && GetElementDisplayName(c.secondaryElement.Value).ToLower().Contains(currentSearchTerm))
                );
            }
            
            // Apply element filters (OR logic)
            if (activeElementFilters.Count > 0) {
                filtered = filtered.Where(c => 
                    activeElementFilters.Contains(c.mainElement) ||
                    (c.secondaryElement.HasValue && activeElementFilters.Contains(c.secondaryElement.Value))
                );
            }
            
            return filtered.ToList();
        }
        
        private List<Creature> SortCreatures(List<Creature> creatures) {
            IOrderedEnumerable<Creature> sorted = null;
            
            switch (currentSortIndex) {
                case 0: // Name ↑
                    sorted = creatures
                        .OrderBy(c => c.name)
                        .ThenBy(c => c.experience)
                        .ThenBy(c => c.mainElement.ToString());
                    break;
                case 1: // Name ↓
                    sorted = creatures
                        .OrderByDescending(c => c.name)
                        .ThenByDescending(c => c.experience)
                        .ThenByDescending(c => c.mainElement.ToString());
                    break;
                case 2: // Experience ↑
                    sorted = creatures
                        .OrderBy(c => c.experience)
                        .ThenBy(c => c.name)
                        .ThenBy(c => c.mainElement.ToString());
                    break;
                case 3: // Experience ↓
                    sorted = creatures
                        .OrderByDescending(c => c.experience)
                        .ThenByDescending(c => c.name)
                        .ThenByDescending(c => c.mainElement.ToString());
                    break;
                case 4: // Element ↑
                    sorted = creatures
                        .OrderBy(c => c.mainElement.ToString())
                        .ThenBy(c => c.experience)
                        .ThenBy(c => c.name);
                    break;
                case 5: // Element ↓
                    sorted = creatures
                        .OrderByDescending(c => c.mainElement.ToString())
                        .ThenByDescending(c => c.experience)
                        .ThenByDescending(c => c.name);
                    break;
                default:
                    sorted = creatures
                        .OrderBy(c => c.name)
                        .ThenBy(c => c.experience)
                        .ThenBy(c => c.mainElement.ToString());
                    break;
            }
            
            return sorted?.ToList() ?? creatures;
        }
        
        private void UpdateCreaturesDisplay() {
            // Clear existing items
            if (creaturesContentParent != null) {
                foreach (Transform child in creaturesContentParent) {
                    Destroy(child.gameObject);
                }
            }
            
            // Create new items
            if (creatureItemPrefab != null && creaturesContentParent != null) {
                foreach (var creature in filteredCreatures) {
                    GameObject itemObj = Instantiate(creatureItemPrefab, creaturesContentParent);
                    SetupCreatureItem(itemObj, creature);
                }
            }
        }
        
        private void SetupCreatureItem(GameObject itemObj, Creature creature) {
            // Find UI components in the prefab
            TMP_Text nameText = itemObj.transform.Find("NameText")?.GetComponent<TMP_Text>();
            TMP_Text levelText = itemObj.transform.Find("LevelText")?.GetComponent<TMP_Text>();
            TMP_Text elementsText = itemObj.transform.Find("ElementsText")?.GetComponent<TMP_Text>();
            TMP_Text statsText = itemObj.transform.Find("StatsText")?.GetComponent<TMP_Text>();
            Image colorImage = itemObj.transform.Find("ColorImage")?.GetComponent<Image>();
            Button selectButton = itemObj.GetComponent<Button>();
            
            // Set data
            if (nameText != null) nameText.text = creature.name;
            if (levelText != null) levelText.text = $"Lvl {creature.level}";
            if (elementsText != null) {
                string elements = GetElementDisplayName(creature.mainElement);
                if (creature.secondaryElement.HasValue) {
                    elements += " / " + GetElementDisplayName(creature.secondaryElement.Value);
                }
                elementsText.text = elements;
            }
            if (statsText != null) statsText.text = $"HP: {creature.currentHP}/{creature.maxHP} | DMG: {creature.damage}";
            if (colorImage != null) colorImage.color = creature.color;
            
            // Set click listener
            if (selectButton != null) {
                selectButton.onClick.AddListener(() => OnCreatureItemClicked(creature));
            }
        }
        
        private void OnCreatureItemClicked(Creature creature) {
            OnCreatureSelected?.Invoke(creature);
            
            // Open creature detail panel
            if (creatureDetailPanel != null) {
                creatureDetailPanel.ShowPanel(creature);
            } else {
                Debug.LogWarning("CreatureDetailPanel reference is not set in CreatureCollectionPanel!");
            }
        }
        
        private void SaveSettingsToPlayerPrefs() {
            PlayerPrefs.SetInt("CreatureCollection_SortIndex", currentSortIndex);
            
            // Save active filters
            string filtersString = string.Join(",", activeElementFilters.Select(e => ((int)e).ToString()));
            PlayerPrefs.SetString("CreatureCollection_ElementFilters", filtersString);
            
            PlayerPrefs.Save();
        }
        
        private void LoadSettingsFromPlayerPrefs() {
            currentSortIndex = PlayerPrefs.GetInt("CreatureCollection_SortIndex", 0);
            
            // Load active filters
            string filtersString = PlayerPrefs.GetString("CreatureCollection_ElementFilters", "");
            activeElementFilters.Clear();
            
            if (!string.IsNullOrEmpty(filtersString)) {
                var filterIndices = filtersString.Split(',');
                foreach (var indexStr in filterIndices) {
                    if (int.TryParse(indexStr, out int index)) {
                        activeElementFilters.Add((CreatureElement)index);
                    }
                }
            }
            
            // Update toggles to match loaded filters
            foreach (var kvp in elementToggles) {
                kvp.Value.isOn = activeElementFilters.Contains(kvp.Key);
            }
            
            // Update dropdown to match loaded sort option
            if (sortDropdown != null) {
                sortDropdown.value = currentSortIndex;
            }
        }
        
        private void OnDestroy() {
            // Clean up event listeners
            if (searchInput != null) {
                searchInput.onValueChanged.RemoveAllListeners();
            }
        }
        
        private void OnCreatureAdded(Creature newCreature) {
            LoadCreaturesData();
            RefreshCreaturesList();
        }

        private void OnCreatureUpdated(Creature updatedCreature) {
            // Odśwież listę stworków po aktualizacji (np. zmianie nazwy)
            LoadCreaturesData();
            RefreshCreaturesList();
        }
    }
}
