using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Systems;
using Models;

/// <summary>
/// Testowy skrypt do testowania systemu zasobów i stworków
/// Dodaje 5 golda lub losowego stworka za każdym kliknięciem i automatycznie zapisuje do API
/// 
/// SETUP:
/// 1. Dodaj ten skrypt do GameObject
/// 2. Stwórz Button i przypisz do addGoldButton (opcjonalne)
/// 3. Stwórz Button i przypisz do addCreatureButton (opcjonalne)
/// 4. Stwórz Text/TextMeshPro i przypisz do statusText
/// 5. Kliknij przycisk żeby dodać goldAmountToAdd golda lub losowego stworka
/// 
/// FUNKCJONALNOŚĆ:
/// - Automatyczne zapisywanie do API przez GameManager
/// - Aktualizacja UI przez system eventów
/// - Context Menu do szybkich testów różnych zasobów i stworków
/// - Status tracking z timestampami
/// - Dodawanie stworków z podstawowymi spellami
/// </summary>
public class ResourceTester : MonoBehaviour
{
    [Header("UI Elements")]
    public Button addGoldButton;
    public Button addCreatureButton;
    public TMP_Text statusText;

    [Header("Settings")]
    public bool testActive = true;
    public int goldAmountToAdd = 5;
    public string token = "";

    void Awake()
    {
        if (!testActive)
            return;
            
        if (!string.IsNullOrEmpty(token) && GameManager.Instance != null)
            GameManager.Instance.SetAuthToken(token);
    }
    private void Start()
    {
        if (addGoldButton != null)
        {
            addGoldButton.onClick.AddListener(OnAddGoldClicked);
        }
        
        if (addCreatureButton != null)
        {
            addCreatureButton.onClick.AddListener(OnAddCreatureClicked);
        }

        if (statusText != null)
        {
            statusText.text = "Gotowy do testowania zasobów";
        }
    }
    
    private void OnEnable()
    {
        // Subskrybuj eventy GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerDataReady += OnPlayerDataLoaded;
            GameManager.Instance.OnPlayerGoldUpdated += OnGoldUpdated;
            GameManager.Instance.OnCreatureAdded += OnCreatureAdded;
        }
    }
    
    private void OnDisable()
    {
        // Odsubskrybuj eventy
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerDataReady -= OnPlayerDataLoaded;
            GameManager.Instance.OnPlayerGoldUpdated -= OnGoldUpdated;
            GameManager.Instance.OnCreatureAdded -= OnCreatureAdded;
        }
    }

    private void OnAddGoldClicked()
    {
        if (GameManager.Instance == null)
        {
            UpdateStatus(">GameManager nie jest dostępny!");
            return;
        }
        
        if (!GameManager.Instance.IsPlayerDataLoaded())
        {
            UpdateStatus(">Dane gracza nie są załadowane!");
            return;
        }
        
        var currentGold = GameManager.Instance.GetPlayerData().gold;
        UpdateStatus($"Dodaję {goldAmountToAdd} golda... (obecne: {currentGold})");
        GameManager.Instance.AddGold(goldAmountToAdd);
    }
    
    private void OnPlayerDataLoaded()
    {
        if (GameManager.Instance.IsPlayerDataLoaded())
        {
            var playerData = GameManager.Instance.GetPlayerData();
            UpdateStatus($"Dane gracza załadowane! Obecne złoto: {playerData.gold}");
        }
    }
    
    private void OnGoldUpdated(int newGold)
    {
        UpdateStatus($"Gold zaktualizowany! Nowa wartość: {newGold}");
    }
    
    private void OnCreatureAdded(Creature creature)
    {
        UpdateStatus($"Nowy stworek dodany: {creature.name} ({creature.mainElement}, poziom {creature.level})");
    }
    
    private async void OnAddCreatureClicked()
    {
        if (GameManager.Instance == null)
        {
            UpdateStatus(">GameManager nie jest dostępny!");
            return;
        }
        
        if (!GameManager.Instance.IsPlayerDataLoaded())
        {
            UpdateStatus(">Dane gracza nie są załadowane!");
            return;
        }
        
        UpdateStatus("Generuję nowego stworka...");
        
        // Wygeneruj losowego stworka (poziom 1-3)
        int randomLevel = Random.Range(1, 4);
        Creature newCreature = CreatureGenerator.GenerateRandomZawomon(randomLevel);
        
        // Wyślij stworka do backendu
        bool success = await Systems.API.GameAPI.AddCreatureAsync(newCreature);
        
        if (success)
        {
            UpdateStatus($"Pomyślnie dodano stworka: {newCreature.name} (Poziom {newCreature.level})");
            
            // Dodaj stworka do lokalnych danych gracza
            GameManager.Instance.GetPlayerData().AddCreature(newCreature);
            
            // Powiadom UI o dodaniu nowego stworka
            GameManager.Instance.NotifyCreatureAdded(newCreature);
        }
        else
        {
            UpdateStatus("Błąd podczas dodawania stworka do API");
        }
    }
    
    private void UpdateStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = $"[{System.DateTime.Now:HH:mm:ss}] {message}";
        }
        Debug.Log($"[ResourceTester] {message}");
    }
    
    // Dodatkowe metody testowe - można wywołać z Context Menu
    [ContextMenu("Test: Add 10 Gold")]
    public void TestAdd10Gold()
    {
        goldAmountToAdd = 10;
        OnAddGoldClicked();
        goldAmountToAdd = 5; // Przywróć domyślną wartość
    }
    
    [ContextMenu("Test: Add 100 Gold")]
    public void TestAdd100Gold()
    {
        goldAmountToAdd = 100;
        OnAddGoldClicked();
        goldAmountToAdd = 5; // Przywróć domyślną wartość
    }
    
    [ContextMenu("Test: Add Wood")]
    public void TestAddWood()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsPlayerDataLoaded())
        {
            GameManager.Instance.AddWood(5);
            UpdateStatus("Dodano 5 drewna!");
        }
    }
    
    [ContextMenu("Test: Add Stone")]
    public void TestAddStone()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsPlayerDataLoaded())
        {
            GameManager.Instance.AddStone(5);
            UpdateStatus("Dodano 5 kamienia!");
        }
    }
    
    [ContextMenu("Test: Add Gems")]
    public void TestAddGems()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsPlayerDataLoaded())
        {
            GameManager.Instance.AddGems(1);
            UpdateStatus("Dodano 1 gem!");
        }
    }
    
    [ContextMenu("Test: Add Random Creature")]
    public void TestAddRandomCreature()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsPlayerDataLoaded())
        {
            OnAddCreatureClicked();
        }
        else
        {
            UpdateStatus("GameManager nie jest dostępny lub dane gracza nie są załadowane!");
        }
    }
}
