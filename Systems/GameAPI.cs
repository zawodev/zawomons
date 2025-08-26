using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.Networking;
using Models;

namespace Systems {
    /// <summary>
    /// GameAPI - statyczna klasa do komunikacji z backendem API
    /// 
    /// Odpowiedzialności:
    /// - Wykonywanie HTTP requestów do API
    /// - Parsowanie odpowiedzi z API do modeli gry
    /// - Zarządzanie tokenem autoryzacji
    /// - Konwersja między formatami API a modelami gry
    /// 
    /// Metody:
    /// - GetPlayerDataAsync: pobiera dane gracza z API
    /// - SavePlayerDataAsync: zapisuje dane gracza (TODO)
    /// - UpdatePlayerResourcesAsync: aktualizuje zasoby gracza (TODO)
    /// </summary>
    public static class GameAPI
    {
        private static readonly string BASE_URL = "http://127.0.0.1:8000/api/v1/games/zawomons";
        private static string authToken;
        
        public static void SetAuthToken(string token)
        {
            authToken = token;
        }
        
        public static string GetAuthToken()
        {
            return authToken;
        }

        public static async Task<PlayerData> GetPlayerDataAsync()
        {
            if (string.IsNullOrEmpty(authToken))
            {
                Debug.LogError("Brak tokena autoryzacji!");
                return null;
            }

            string url = BASE_URL + "/player-data/";
            
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("Authorization", "Bearer " + authToken);
                
                var operation = request.SendWebRequest();
                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string jsonResponse = request.downloadHandler.text;
                    Debug.Log("Player Data Response: " + jsonResponse);
                    
                    try
                    {
                        PlayerDataResponse apiResponse = JsonUtility.FromJson<PlayerDataResponse>(jsonResponse);
                        return ConvertToPlayerData(apiResponse);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError("Błąd parsowania JSON: " + e.Message);
                        return null;
                    }
                }
                else
                {
                    Debug.LogError($"Error getting player data: {request.error}, Code: {request.responseCode}");
                    return null;
                }
            }
        }

        private static PlayerData ConvertToPlayerData(PlayerDataResponse apiResponse)
        {
            PlayerData playerData = new PlayerData();
            playerData.id = apiResponse.id;
            playerData.username = apiResponse.username;
            playerData.name = apiResponse.name;
            playerData.gold = apiResponse.gold;
            playerData.wood = apiResponse.wood;
            playerData.stone = apiResponse.stone;
            playerData.gems = apiResponse.gems;
            playerData.lastPlayed = apiResponse.last_played;
            playerData.createdAt = apiResponse.created_at;

            // Convert creatures
            playerData.creatures = new List<Creature>();
            foreach (var creatureResponse in apiResponse.creatures)
            {
                Creature creature = new Creature();
                creature.id = creatureResponse.id;
                creature.name = creatureResponse.name;
                creature.mainElement = ParseElement(creatureResponse.main_element);
                creature.secondaryElement = ParseElement(creatureResponse.secondary_element);
                creature.colorString = creatureResponse.color;
                
                // Konwertuj string koloru na Color (można to usprawnić później)
                if (ColorUtility.TryParseHtmlString(creatureResponse.color, out Color parsedColor))
                {
                    creature.color = parsedColor;
                }
                else
                {
                    creature.color = Color.white; // Domyślny kolor
                }
                
                creature.experience = creatureResponse.experience;
                creature.maxHP = creatureResponse.max_hp;
                creature.currentHP = creatureResponse.current_hp;
                creature.maxEnergy = creatureResponse.max_energy;
                creature.currentEnergy = creatureResponse.current_energy;
                creature.damage = creatureResponse.damage;
                creature.initiative = creatureResponse.initiative;
                
                playerData.creatures.Add(creature);
            }

            return playerData;
        }

        private static CreatureElement ParseElement(string elementString)
        {
            if (string.IsNullOrEmpty(elementString))
                return CreatureElement.None;
                
            if (System.Enum.TryParse<CreatureElement>(elementString, true, out CreatureElement element))
                return element;
            
            return CreatureElement.None;
        }

        // Placeholder metoda do zapisywania danych gracza w przyszłości
        public static async Task<bool> SavePlayerDataAsync(PlayerData playerData)
        {
            // TODO: Implementacja zapisywania danych do API
            Debug.Log("SavePlayerDataAsync - TODO: Implement API call");
            await Task.Yield(); // Placeholder
            return true;
        }

        // Placeholder metoda do aktualizacji zasobów gracza
        public static async Task<bool> UpdatePlayerResourcesAsync(int gold, int wood, int stone, int gems)
        {
            // TODO: Implementacja aktualizacji zasobów przez API
            Debug.Log($"UpdatePlayerResourcesAsync - TODO: Implement API call for Gold:{gold}, Wood:{wood}, Stone:{stone}, Gems:{gems}");
            await Task.Yield(); // Placeholder
            return true;
        }
    }
} 