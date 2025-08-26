using UnityEngine;
using Systems;
using Models;

/// <summary>
/// Przykład użycia nowego systemu GameManager i GameAPI
/// Ten skrypt pokazuje jak prawidłowo korzystać z eventów i metod GameManager
/// </summary>
public class GameManagerUsageExample : MonoBehaviour
{
    [Header("Debug Info")]
    public bool showDebugInfo = true;
    
    private void OnEnable()
    {
        // Subskrybuj eventy GameManager
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerDataReady += OnPlayerDataLoaded;
            GameManager.Instance.OnPlayerResourcesUpdated += OnResourcesChanged;
            GameManager.Instance.OnPlayerGoldUpdated += OnGoldChanged;
        }
    }
    
    private void OnDisable()
    {
        // Odsubskrybuj eventy
        if (GameManager.Instance != null)
        {
            GameManager.Instance.OnPlayerDataReady -= OnPlayerDataLoaded;
            GameManager.Instance.OnPlayerResourcesUpdated -= OnResourcesChanged;
            GameManager.Instance.OnPlayerGoldUpdated -= OnGoldChanged;
        }
    }

    private void OnPlayerDataLoaded()
    {
        if (showDebugInfo)
        {
            var playerData = GameManager.Instance.GetPlayerData();
            Debug.Log($"[Example] Dane gracza załadowane! Gracz: {playerData.name}, Złoto: {playerData.gold}");
        }
    }

    private void OnResourcesChanged(PlayerData playerData)
    {
        if (showDebugInfo)
        {
            Debug.Log($"[Example] Zasoby zmienione: Gold:{playerData.gold}, Wood:{playerData.wood}, Stone:{playerData.stone}, Gems:{playerData.gems}");
        }
    }

    private void OnGoldChanged(int newGold)
    {
        if (showDebugInfo)
        {
            Debug.Log($"[Example] Złoto zmienione na: {newGold}");
        }
    }

    // Przykłady jak używać GameManager
    [ContextMenu("Test: Refresh Player Data")]
    public async void TestRefreshPlayerData()
    {
        if (GameManager.Instance != null)
        {
            await GameManager.Instance.RefreshPlayerData();
        }
    }

    [ContextMenu("Test: Add 100 Gold")]
    public void TestAddGold()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsPlayerDataLoaded())
        {
            var currentGold = GameManager.Instance.GetPlayerData().gold;
            GameManager.Instance.UpdateGold(currentGold + 100);
        }
    }

    [ContextMenu("Test: Save Data to API")]
    public async void TestSaveDataToAPI()
    {
        if (GameManager.Instance != null)
        {
            await GameManager.Instance.SavePlayerDataToAPI();
        }
    }
}
