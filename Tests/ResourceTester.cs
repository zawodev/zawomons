using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Systems;

/// <summary>
/// Testowy skrypt do testowania systemu zasobów
/// Dodaje 5 golda za każdym kliknięciem i automatycznie zapisuje do API
/// 
/// SETUP:
/// 1. Dodaj ten skrypt do GameObject
/// 2. Stwórz Button i przypisz do addGoldButton
/// 3. Stwórz Text/TextMeshPro i przypisz do statusText
/// 4. Kliknij przycisk żeby dodać goldAmountToAdd golda
/// 
/// FUNKCJONALNOŚĆ:
/// - Automatyczne zapisywanie do API przez GameManager
/// - Aktualizacja UI przez system eventów
/// - Context Menu do szybkich testów różnych zasobów
/// - Status tracking z timestampami
/// </summary>
public class ResourceTester : MonoBehaviour
{
    [Header("UI Elements")]
    public Button addGoldButton;
    public TMP_Text statusText;
    
    [Header("Settings")]
    public int goldAmountToAdd = 5;
    public string token = "";

    void Awake()
    {
        if (!string.IsNullOrEmpty(token) && GameManager.Instance != null)
            GameManager.Instance.SetAuthToken(token);
    }
    private void Start()
    {
        if (addGoldButton != null)
        {
            addGoldButton.onClick.AddListener(OnAddGoldClicked);
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
        }
    }
    
    private void OnDisable()
    {
        // Odsubskrybuj eventy
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerDataReady -= OnPlayerDataLoaded;
            GameManager.Instance.OnPlayerGoldUpdated -= OnGoldUpdated;
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
        
        // Dodaj gold - automatycznie zapisze się do API
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
}
