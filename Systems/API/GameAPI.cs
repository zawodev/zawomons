using System.Threading.Tasks;
using System.Text;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using Models;

namespace Systems.API {
    /// <summary>
    /// GameAPI - statyczna klasa do komunikacji z backendem API
    /// 
    /// Odpowiedzialności:
    /// - Wykonywanie HTTP requestów do API
    /// - Zarządzanie tokenem autoryzacji
    /// 
    /// Metody:
    /// - GetPlayerDataAsync: pobiera dane gracza z API
    /// - SetSingleResourceAsync: aktualizuje pojedynczy zasób gracza
    /// - AddCreatureAsync: dodaje nowego stworka do gracza
    /// - GetCreatureAsync: pobiera dane konkretnego stworka
    /// - UpdateCreatureAsync: aktualizuje dane stworka
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

            string url = BASE_URL + "/player-data-get/";
            
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
                        return APIDataConverter.ConvertToPlayerData(apiResponse);
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

        public static async Task<bool> SetSingleResourceAsync(string resourceType, int newValue)
        {
            if (string.IsNullOrEmpty(authToken))
            {
                Debug.LogError("Brak tokena autoryzacji!");
                return false;
            }

            string url = BASE_URL + "/set-single-resource/";
            
            var resourceData = new UpdateSingleResourceRequest {
                resource_type = resourceType.ToLower(),
                value = newValue
            };
            
            string jsonData = JsonUtility.ToJson(resourceData);
            
            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                
                request.SetRequestHeader("Authorization", "Bearer " + authToken);
                request.SetRequestHeader("Content-Type", "application/json");
                
                var operation = request.SendWebRequest();
                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log($"Zasób {resourceType} zaktualizowany pomyślnie na: {newValue}");
                    return true;
                }
                else
                {
                    Debug.LogError($"Błąd aktualizacji zasobu {resourceType}: {request.error}, Code: {request.responseCode}");
                    Debug.LogError($"Response: {request.downloadHandler.text}");
                    return false;
                }
            }
        }

        public static async Task<bool> AddCreatureAsync(Creature creature)
        {
            if (string.IsNullOrEmpty(authToken))
            {
                Debug.LogError("Brak tokena autoryzacji!");
                return false;
            }

            string url = BASE_URL + "/creature-add/";

            Debug.Log($"Adding creature: {creature.name} (element: {creature.mainElement})");
            
            // Konwertuj Creature na format API
            var creatureData = new CreateCreatureRequest
            {
                name = creature.name,
                main_element = creature.mainElement.ToString().ToLower(),
                secondary_element = creature.secondaryElement.HasValue ? creature.secondaryElement.Value.ToString().ToLower() : "",
                color = "#" + ColorUtility.ToHtmlStringRGB(creature.color),
                experience = creature.experience,
                max_hp = creature.maxHP,
                current_hp = creature.currentHP,
                max_energy = creature.maxEnergy,
                current_energy = creature.currentEnergy,
                damage = creature.damage,
                initiative = creature.initiative,
                spells = APIDataConverter.ConvertCreatureSpellsToApiFormat(creature)
            };
            
            string jsonData = JsonUtility.ToJson(creatureData);
            
            Debug.Log("Add Creature JSON: " + jsonData);
            
            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();

                request.SetRequestHeader("Authorization", "Bearer " + authToken);
                request.SetRequestHeader("Content-Type", "application/json");

                var operation = request.SendWebRequest();
                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("Creature dodany pomyślnie");
                    Debug.Log("Response: " + request.downloadHandler.text);
                    return true;
                }
                else
                {
                    Debug.LogError($"Błąd dodawania creature: {request.error}, Code: {request.responseCode}");
                    Debug.LogError($"Response: {request.downloadHandler.text}");
                    return false;
                }
            }
        }

        public static async Task<Creature> GetCreatureAsync(int creatureId)
        {
            if (string.IsNullOrEmpty(authToken))
            {
                Debug.LogError("Brak tokena autoryzacji!");
                return null;
            }

            string url = BASE_URL + $"/creature-get/{creatureId}/";
            
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
                    Debug.Log("Creature Data Response: " + jsonResponse);
                    
                    try
                    {
                        CreatureDataResponse apiResponse = JsonUtility.FromJson<CreatureDataResponse>(jsonResponse);
                        return APIDataConverter.ConvertToCreature(apiResponse);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError("Błąd parsowania JSON: " + e.Message);
                        return null;
                    }
                }
                else
                {
                    Debug.LogError($"Error getting creature: {request.error}, Code: {request.responseCode}");
                    return null;
                }
            }
        }

        public static async Task<bool> UpdateCreatureAsync(int creatureId, Creature creature)
        {
            if (string.IsNullOrEmpty(authToken))
            {
                Debug.LogError("Brak tokena autoryzacji!");
                return false;
            }

            string url = BASE_URL + "/creature-set/";

            Debug.Log($"Updating creature {creatureId}: {creature.name}");
            
            // Konwertuj Creature na format API
            var updateData = new UpdateCreatureRequest
            {
                name = creature.name,
                main_element = creature.mainElement.ToString().ToLower(),
                secondary_element = creature.secondaryElement.HasValue ? creature.secondaryElement.Value.ToString().ToLower() : "",
                color = "#" + ColorUtility.ToHtmlStringRGB(creature.color),
                experience = creature.experience,
                max_hp = creature.maxHP,
                current_hp = creature.currentHP,
                max_energy = creature.maxEnergy,
                current_energy = creature.currentEnergy,
                damage = creature.damage,
                initiative = creature.initiative,
                spells = APIDataConverter.ConvertCreatureSpellsToApiFormat(creature)
            };
            
            string jsonData = JsonUtility.ToJson(updateData);
            
            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                
                request.SetRequestHeader("Authorization", "Bearer " + authToken);
                request.SetRequestHeader("Content-Type", "application/json");
                
                var operation = request.SendWebRequest();
                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log($"Creature {creatureId} zaktualizowany pomyślnie");
                    return true;
                }
                else
                {
                    Debug.LogError($"Błąd aktualizacji creature {creatureId}: {request.error}, Code: {request.responseCode}");
                    Debug.LogError($"Response: {request.downloadHandler.text}");
                    return false;
                }
            }
        }

        // SOCIAL API METHODS

        /// <summary>
        /// Pobiera listę wszystkich graczy z API
        /// </summary>
        public static async Task<PlayerSummaryResponse[]> GetAllPlayersAsync()
        {
            if (string.IsNullOrEmpty(authToken))
            {
                Debug.LogError("Brak tokena autoryzacji!");
                return null;
            }

            string url = BASE_URL + "/players/";
            
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("Authorization", "Bearer " + authToken);
                
                await request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string jsonResponse = request.downloadHandler.text;
                    Debug.Log($"Pobrano listę graczy: {jsonResponse}");
                    
                    try
                    {
                        // Unity JsonUtility nie obsługuje bezpośrednio tablic, więc owijamy w wrapper
                        string wrappedJson = "{\"items\":" + jsonResponse + "}";
                        var wrapper = JsonUtility.FromJson<PlayerSummaryResponseWrapper>(wrappedJson);
                        return wrapper.items;
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"Błąd parsowania listy graczy: {e.Message}");
                        return null;
                    }
                }
                else
                {
                    Debug.LogError($"Błąd pobierania listy graczy: {request.error}, Code: {request.responseCode}");
                    Debug.LogError($"Response: {request.downloadHandler.text}");
                    return null;
                }
            }
        }

        /// <summary>
        /// Pobiera listę znajomych z API
        /// </summary>
        public static async Task<PlayerSummaryResponse[]> GetFriendsAsync()
        {
            if (string.IsNullOrEmpty(authToken))
            {
                Debug.LogError("Brak tokena autoryzacji!");
                return null;
            }

            string url = BASE_URL + "/friends/";
            
            using (UnityWebRequest request = UnityWebRequest.Get(url))
            {
                request.SetRequestHeader("Authorization", "Bearer " + authToken);
                
                await request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string jsonResponse = request.downloadHandler.text;
                    Debug.Log($"Pobrano listę znajomych: {jsonResponse}");
                    
                    try
                    {
                        // Unity JsonUtility nie obsługuje bezpośrednio tablic, więc owijamy w wrapper
                        string wrappedJson = "{\"items\":" + jsonResponse + "}";
                        var wrapper = JsonUtility.FromJson<PlayerSummaryResponseWrapper>(wrappedJson);
                        return wrapper.items;
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"Błąd parsowania listy znajomych: {e.Message}");
                        return null;
                    }
                }
                else
                {
                    Debug.LogError($"Błąd pobierania listy znajomych: {request.error}, Code: {request.responseCode}");
                    Debug.LogError($"Response: {request.downloadHandler.text}");
                    return null;
                }
            }
        }
    }
} 