using System;
using System.Collections;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using TMPro;
using Models;
using Systems;
using UnityEngine.UI;

public class ApiTokenTest : MonoBehaviour
{
    [Header("UI Elements")]
    public Button getPlayerDataButton;
    public TMP_Text playerDataText;

    [Header("API Settings")]
    public string baseUrl = "http://127.0.0.1:8000/api/v1/games/zawomons";

    void Start()
    {
        getPlayerDataButton.onClick.AddListener(GetPlayerData);
        playerDataText.text = "Kliknij przycisk aby pobrać dane gracza";
    }

    // Przycisk: Pobierz dane gracza
    public void GetPlayerData()
    {
        string token = GameAPI.GetAuthToken();

        if (string.IsNullOrEmpty(token))
        {
            playerDataText.text = "BŁĄD: Brak tokena autoryzacji!\nUżytkownik nie jest zalogowany.";
            Debug.LogError("Brak tokena autoryzacji!");
            return;
        }

        StartCoroutine(GetPlayerDataCoroutine(token));
    }

    IEnumerator GetPlayerDataCoroutine(string authToken)
    {
        string url = baseUrl + "/player-data/";
        playerDataText.text = "Ładowanie danych gracza...";

        using (UnityWebRequest request = UnityWebRequest.Get(url))
        {
            // Dodaj header autoryzacji
            request.SetRequestHeader("Authorization", "Bearer " + authToken);

            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                Debug.Log("Player Data Response: " + jsonResponse);

                try
                {
                    // Parse JSON response
                    PlayerDataResponse playerData = JsonUtility.FromJson<PlayerDataResponse>(jsonResponse);

                    playerDataText.text = $"✅ DANE GRACZA POBRANE POMYŚLNIE!\n\n" +
                                         $"👤 Gracz: {playerData.username}\n" +
                                         $"📝 Nazwa: {playerData.name}\n" +
                                         $"💰 Złoto: {playerData.gold}\n" +
                                         $"🌲 Drewno: {playerData.wood}\n" +
                                         $"🗿 Kamień: {playerData.stone}\n" +
                                         $"💎 Gemy: {playerData.gems}\n" +
                                         $"🐉 Creatures: {playerData.creatures.Length}\n" +
                                         $"⏰ Ostatnio grał: {FormatDate(playerData.last_played)}";
                }
                catch (Exception e)
                {
                    Debug.LogError("Błąd parsowania JSON: " + e.Message);
                    playerDataText.text = "BŁĄD: Nie udało się sparsować odpowiedzi serwera.\n\n" +
                                         "Raw response:\n" + jsonResponse;
                }
            }
            else
            {
                Debug.LogError("Error getting player data: " + request.error);
                string errorMessage = "❌ BŁĄD POBIERANIA DANYCH!\n\n";

                if (request.responseCode == 401)
                {
                    errorMessage += "🔒 Błąd autoryzacji (401)\nToken może być nieprawidłowy lub wygasły.";
                }
                else if (request.responseCode == 404)
                {
                    errorMessage += "🔍 Nie znaleziono (404)\nSprawdź czy endpoint istnieje.";
                }
                else if (request.responseCode == 500)
                {
                    errorMessage += "🛠️ Błąd serwera (500)\nSprawdź logi Django.";
                }
                else
                {
                    errorMessage += $"📡 Kod błędu: {request.responseCode}\n{request.error}";
                }

                errorMessage += "\n\nSzczegóły:\n" + request.downloadHandler.text;

                playerDataText.text = errorMessage;
            }
        }
    }

    private string FormatDate(string isoDate)
    {
        try
        {
            DateTime dateTime = DateTime.Parse(isoDate);
            return dateTime.ToString("dd.MM.yyyy HH:mm");
        }
        catch
        {
            return isoDate;
        }
    }
}
