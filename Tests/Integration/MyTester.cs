using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Systems;
using Systems.Game.Core;
using Systems.Creatures.Models;
using Systems.Creatures.Core;

public class MyTester : MonoBehaviour
{
    [Header("UI Elements")]
    public Button addCreatureButton;
    public TMP_Text statusText;

    [Header("Settings")]
    public bool testActive = true;
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
        if (addCreatureButton != null)
        {
            addCreatureButton.onClick.AddListener(OnAddCreatureClicked);
        }

        if (statusText != null)
        {
            statusText.text = "Status: Ready";
        }
    }
    
    private void OnEnable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerDataReady += OnPlayerDataLoaded;
            GameManager.Instance.OnCreatureAdded += OnCreatureAdded;
        }
    }
    
    private void OnDisable()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerDataReady -= OnPlayerDataLoaded;
            GameManager.Instance.OnCreatureAdded -= OnCreatureAdded;
        }
    }

    private void OnPlayerDataLoaded()
    {
        if (GameManager.Instance.IsPlayerDataLoaded())
        {
            var playerData = GameManager.Instance.GetPlayerData();
            UpdateStatus($"Dane gracza załadowane! Obecne złoto: {playerData.gold}");
        }
    }
    
    private void OnCreatureAdded(Creature creature)
    {
        UpdateStatus($"Nowy stworek dodany: {creature.name} ({creature.mainElement}, poziom {creature.level})");
    }
    
    private async void OnAddCreatureClicked()
    {
        if (!testActive)
        {
            for (int i = 0; i < 10; i++)
            {
                var randomCreature = CreatureGenerator.GenerateRandomZawomon();
                GameManager.Instance.GetPlayerData().AddCreature(randomCreature);
            }
            return;
        }

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
        
        UpdateStatus("Claimuję darmowego stworka...");
        
        // Użyj nowego endpointu do claim darmowego stworka
        Creature claimedCreature = await Systems.API.Core.GameAPI.Creatures.ClaimFreeCreatureAsync();
        
        if (claimedCreature != null)
        {
            UpdateStatus($"Pomyślnie otrzymano stworka: {claimedCreature.name} (Poziom {claimedCreature.level})");
            
            // Dodaj stworka do lokalnych danych gracza
            GameManager.Instance.GetPlayerData().AddCreature(claimedCreature);
            
            // Powiadom UI o dodaniu nowego stworka
            GameManager.Instance.NotifyCreatureAdded(claimedCreature);
        }
        else
        {
            UpdateStatus("Błąd podczas claimu stworka - możliwe że już claimowano dzisiaj lub wystąpił błąd API");
        }
    }
    
    private void UpdateStatus(string message)
    {
        if (statusText != null)
        {
            statusText.text = $"[{System.DateTime.Now:HH:mm:ss}] {message}";
        }
        Debug.Log($"[MyTester] {message}");
    }
}
