using System.Threading.Tasks;
using System.Text;
using System.Linq;
using UnityEngine;
using UnityEngine.Networking;
using Systems.API.Models;
using Systems.API.Utilities;
using Systems.Creatures.Models;
using Shared.Models;

namespace Systems.API.Core {
    /// <summary>
    /// GameAPI - statyczna klasa do komunikacji z backendem API
    /// 
    /// Nowa architektura RESTful z zagnieżdżonymi klasami:
    /// - Players: endpointy graczy (/players/)
    /// - Creatures: endpointy stworków (/players/me/creatures/)
    /// - Cities: endpointy miast (/players/me/cities/)
    /// - Tasks: endpointy zadań (/players/me/tasks/)
    /// - Public: publiczne endpointy (/creatures/, /cities/, /spells/)
    /// </summary>
    public static class GameAPI
    {
        private static readonly string BASE_URL = "http://127.0.0.1:8000/api/v1/zawomons";
        private static string authToken;
        
        public static void SetAuthToken(string token)
        {
            authToken = token;
        }
        
        public static string GetAuthToken()
        {
            return authToken;
        }

        /// <summary>
        /// Wysyła żądanie GET i zwraca odpowiedź jako tablicę obiektów typu T
        /// </summary>
        private static async Task<T[]> SendGetRequestArrayAsync<T>(string url) where T : class
        {
            if (string.IsNullOrEmpty(authToken))
            {
                Debug.LogError("Brak tokena autoryzacji!");
                return null;
            }

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
                    Debug.Log($"API Array Response from {url}: {jsonResponse}");
                    
                    try
                    {
                        // Unity JsonUtility nie obsługuje bezpośrednio tablic, więc owijamy w wrapper
                        string wrappedJson = "{\"items\":" + jsonResponse + "}";
                        var wrapper = JsonUtility.FromJson<ArrayWrapper<T>>(wrappedJson);
                        return wrapper.items;
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"Błąd parsowania JSON Array z {url}: {e.Message}");
                        return null;
                    }
                }
                else
                {
                    Debug.LogError($"Error from {url}: {request.error}, Code: {request.responseCode}");
                    Debug.LogError($"Response: {request.downloadHandler.text}");
                    return null;
                }
            }
        }

        /// <summary>
        /// Wysyła żądanie GET i zwraca odpowiedź jako obiekt typu T
        /// </summary>
        private static async Task<T> SendGetRequestAsync<T>(string url) where T : class
        {
            if (string.IsNullOrEmpty(authToken))
            {
                Debug.LogError("Brak tokena autoryzacji!");
                return null;
            }

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
                    Debug.Log($"API Response from {url}: {jsonResponse}");
                    
                    try
                    {
                        return JsonUtility.FromJson<T>(jsonResponse);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"Błąd parsowania JSON z {url}: {e.Message}");
                        return null;
                    }
                }
                else
                {
                    Debug.LogError($"Error from {url}: {request.error}, Code: {request.responseCode}");
                    Debug.LogError($"Response: {request.downloadHandler.text}");
                    return null;
                }
            }
        }

        private static async Task<bool> SendPostRequestAsync(string url, object data = null)
        {
            if (string.IsNullOrEmpty(authToken))
            {
                Debug.LogError("Brak tokena autoryzacji!");
                return false;
            }

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                if (data != null)
                {
                    string jsonData = JsonUtility.ToJson(data);
                    byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                    request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                    request.SetRequestHeader("Content-Type", "application/json");
                }
                
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Authorization", "Bearer " + authToken);
                
                var operation = request.SendWebRequest();
                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                if (request.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log($"POST Success to {url}");
                    Debug.Log($"Response: {request.downloadHandler.text}");
                    return true;
                }
                else
                {
                    Debug.LogError($"POST Error to {url}: {request.error}, Code: {request.responseCode}");
                    Debug.LogError($"Response: {request.downloadHandler.text}");
                    return false;
                }
            }
        }

        private static async Task<T> SendPostRequestAsync<T>(string url, object data = null) where T : class
        {
            if (string.IsNullOrEmpty(authToken))
            {
                Debug.LogError("Brak tokena autoryzacji!");
                return null;
            }

            using (UnityWebRequest request = new UnityWebRequest(url, "POST"))
            {
                if (data != null)
                {
                    string jsonData = JsonUtility.ToJson(data);
                    byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonData);
                    request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                    request.SetRequestHeader("Content-Type", "application/json");
                }
                
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Authorization", "Bearer " + authToken);
                
                var operation = request.SendWebRequest();
                while (!operation.isDone)
                {
                    await Task.Yield();
                }

                if (request.result == UnityWebRequest.Result.Success)
                {
                    string jsonResponse = request.downloadHandler.text;
                    Debug.Log($"POST Success to {url}: {jsonResponse}");
                    
                    try
                    {
                        return JsonUtility.FromJson<T>(jsonResponse);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"Błąd parsowania JSON z {url}: {e.Message}");
                        return null;
                    }
                }
                else
                {
                    Debug.LogError($"POST Error to {url}: {request.error}, Code: {request.responseCode}");
                    Debug.LogError($"Response: {request.downloadHandler.text}");
                    return null;
                }
            }
        }

        private static async Task<bool> SendPutRequestAsync(string url, object data)
        {
            if (string.IsNullOrEmpty(authToken))
            {
                Debug.LogError("Brak tokena autoryzacji!");
                return false;
            }

            using (UnityWebRequest request = new UnityWebRequest(url, "PUT"))
            {
                string jsonData = JsonUtility.ToJson(data);
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
                    Debug.Log($"PUT Success to {url}");
                    Debug.Log($"Response: {request.downloadHandler.text}");
                    return true;
                }
                else
                {
                    Debug.LogError($"PUT Error to {url}: {request.error}, Code: {request.responseCode}");
                    Debug.LogError($"Response: {request.downloadHandler.text}");
                    return false;
                }
            }
        }

        private static async Task<T> SendPutRequestAsync<T>(string url, object data) where T : class
        {
            if (string.IsNullOrEmpty(authToken))
            {
                Debug.LogError("Brak tokena autoryzacji!");
                return null;
            }

            using (UnityWebRequest request = new UnityWebRequest(url, "PUT"))
            {
                string jsonData = JsonUtility.ToJson(data);
                Debug.Log($"PUT Request to {url} with data: {jsonData}");
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
                    string jsonResponse = request.downloadHandler.text;
                    Debug.Log($"PUT Success to {url}: {jsonResponse}");
                    
                    try
                    {
                        return JsonUtility.FromJson<T>(jsonResponse);
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"Błąd parsowania JSON z {url}: {e.Message}");
                        return null;
                    }
                }
                else
                {
                    Debug.LogError($"PUT Error to {url}: {request.error}, Code: {request.responseCode}");
                    Debug.LogError($"Response: {request.downloadHandler.text}");
                    return null;
                }
            }
        }

        /// <summary>
        /// Endpointy związane z graczami (/players/)
        /// </summary>
        public static class Players
        {
            /// <summary>
            /// GET /zawomons/players/me/ - pobiera wszystkie dane gracza
            /// </summary>
            public static async Task<PlayerData> GetMyDataAsync()
            {
                string url = BASE_URL + "/players/me/";
                var response = await SendGetRequestAsync<PlayerDataResponse>(url);
                return response != null ? APIDataConverter.ConvertToPlayerData(response) : null;
            }

            /// <summary>
            /// GET /zawomons/players/ - global player list
            /// </summary>
            public static async Task<PlayerSummaryResponse[]> GetAllPlayersAsync()
            {
                string url = BASE_URL + "/players/";
                return await SendGetRequestArrayAsync<PlayerSummaryResponse>(url);
            }

            /// <summary>
            /// GET /zawomons/players/me/friends/ - friend list
            /// </summary>
            public static async Task<PlayerSummaryResponse[]> GetMyFriendsAsync()
            {
                string url = BASE_URL + "/players/me/friends/";
                return await SendGetRequestArrayAsync<PlayerSummaryResponse>(url);
            }

            /// <summary>
            /// GET /zawomons/players/{id}/ - pobranie danych innego gracza
            /// </summary>
            public static async Task<PlayerSummaryResponse> GetPlayerByIdAsync(int playerId)
            {
                string url = BASE_URL + $"/players/{playerId}/";
                return await SendGetRequestAsync<PlayerSummaryResponse>(url);
            }

            /// <summary>
            /// GET /zawomons/players/{id}/creatures/ - lista stworków innego gracza
            /// </summary>
            public static async Task<CreatureDataResponse[]> GetPlayerCreaturesAsync(int playerId)
            {
                string url = BASE_URL + $"/players/{playerId}/creatures/";
                return await SendGetRequestArrayAsync<CreatureDataResponse>(url);
            }
        }

        /// <summary>
        /// Endpointy związane ze stworkami (/players/me/creatures/)
        /// </summary>
        public static class Creatures
        {
            /// <summary>
            /// GET /zawomons/players/me/creatures/ - lista stworków gracza
            /// </summary>
            public static async Task<CreatureDataResponse[]> GetMyCreaturesAsync()
            {
                string url = BASE_URL + "/players/me/creatures/";
                return await SendGetRequestArrayAsync<CreatureDataResponse>(url);
            }

            /// <summary>
            /// GET /zawomons/players/me/creatures/{id}/ - pobiera dane danego stworka gracza
            /// </summary>
            public static async Task<Creature> GetMyCreatureAsync(int creatureId)
            {
                string url = BASE_URL + $"/players/me/creatures/{creatureId}/";
                var response = await SendGetRequestAsync<CreatureDataResponse>(url);
                return response != null ? APIDataConverter.ConvertToCreature(response) : null;
            }

            /// <summary>
            /// GET /zawomons/players/me/creatures/{id}/progress/ - pobiera tylko progress taska stworka
            /// </summary>
            public static async Task<CreatureProgressResponse> GetMyCreatureProgressAsync(int creatureId)
            {
                string url = BASE_URL + $"/players/me/creatures/{creatureId}/progress/";
                return await SendGetRequestAsync<CreatureProgressResponse>(url);
            }

            /// <summary>
            /// PUT /zawomons/players/me/creatures/{id}/ - aktualizuje stworka (np. nazwę)
            /// </summary>
            public static async Task<Creature> UpdateMyCreatureAsync(int creatureId, string newName)
            {
                string url = BASE_URL + $"/players/me/creatures/{creatureId}/";
                var data = new UpdateCreatureRequest { name = newName };
                var response = await SendPutRequestAsync<CreatureDataResponse>(url, data);
                return response != null ? APIDataConverter.ConvertToCreature(response) : null;
            }

            /// <summary>
            /// POST /zawomons/players/me/creatures/claim/ - claim darmowego stworka (co 4h)
            /// </summary>
            public static async Task<Creature> ClaimFreeCreatureAsync()
            {
                string url = BASE_URL + "/players/me/creatures/claim/";
                var response = await SendPostRequestAsync<CreatureDataResponse>(url);
                return response != null ? APIDataConverter.ConvertToCreature(response) : null;
            }

            /// <summary>
            /// POST /zawomons/players/me/creatures/{id}/spells/learn/ - zaczyna naukę spella
            /// </summary>
            public static async Task<bool> LearnSpellAsync(int creatureId, int spellId)
            {
                string url = BASE_URL + $"/players/me/creatures/{creatureId}/spells/learn/";
                var data = new { spell_id = spellId };
                return await SendPostRequestAsync(url, data);
            }

            /// <summary>
            /// POST /zawomons/players/me/creatures/{id}/travel/start/ - zaczyna podróż
            /// </summary>
            public static async Task<bool> StartTravelAsync(int creatureId, float destinationX, float destinationY)
            {
                string url = BASE_URL + $"/players/me/creatures/{creatureId}/travel/start/";
                var data = new { destination_x = destinationX, destination_y = destinationY };
                return await SendPostRequestAsync(url, data);
            }
        }

        /// <summary>
        /// Endpointy związane z miastami (/players/me/cities/)
        /// </summary>
        public static class Cities
        {
            /// <summary>
            /// GET /zawomons/players/me/cities/ - lista miast gracza
            /// </summary>
            public static async Task<CityDataResponse[]> GetMyCitiesAsync()
            {
                string url = BASE_URL + "/players/me/cities/";
                return await SendGetRequestArrayAsync<CityDataResponse>(url);
            }

            /// <summary>
            /// GET /zawomons/players/me/cities/{id}/ - pobiera dane danego miasta gracza
            /// </summary>
            public static async Task<CityDataResponse> GetMyCityAsync(int cityId)
            {
                string url = BASE_URL + $"/players/me/cities/{cityId}/";
                return await SendGetRequestAsync<CityDataResponse>(url);
            }

            /// <summary>
            /// POST /zawomons/players/me/cities/build/ - buduje nowe miasto
            /// </summary>
            public static async Task<bool> BuildCityAsync(string cityName, float posX, float posY)
            {
                string url = BASE_URL + "/players/me/cities/build/";
                var data = new { name = cityName, pos_x = posX, pos_y = posY };
                return await SendPostRequestAsync(url, data);
            }
        }

        /// <summary>
        /// Endpointy związane z zadaniami (/players/me/tasks/)
        /// </summary>
        public static class Tasks
        {
            /// <summary>
            /// GET /zawomons/players/me/tasks/ - all ongoing tasks for the player
            /// </summary>
            public static async Task<TasksResponse> GetMyTasksAsync()
            {
                string url = BASE_URL + "/players/me/tasks/";
                return await SendGetRequestAsync<TasksResponse>(url);
            }
        }

        /// <summary>
        /// Publiczne endpointy (read-only)
        /// </summary>
        public static class Public
        {
            /// <summary>
            /// GET /zawomons/creatures/ - lista wszystkich stworków
            /// </summary>
            public static async Task<CreatureDataResponse[]> GetAllCreaturesAsync()
            {
                string url = BASE_URL + "/creatures/";
                return await SendGetRequestArrayAsync<CreatureDataResponse>(url);
            }

            /// <summary>
            /// GET /zawomons/creatures/{id}/ - pobiera dane dowolnego stworka
            /// </summary>
            public static async Task<CreatureDataResponse> GetCreatureAsync(int creatureId)
            {
                string url = BASE_URL + $"/creatures/{creatureId}/";
                return await SendGetRequestAsync<CreatureDataResponse>(url);
            }

            /// <summary>
            /// GET /zawomons/cities/ - lista wszystkich miast
            /// </summary>
            public static async Task<CityDataResponse[]> GetAllCitiesAsync()
            {
                string url = BASE_URL + "/cities/";
                return await SendGetRequestArrayAsync<CityDataResponse>(url);
            }

            /// <summary>
            /// GET /zawomons/cities/{id}/ - pobiera dane dowolnego miasta
            /// </summary>
            public static async Task<CityDataResponse> GetCityAsync(int cityId)
            {
                string url = BASE_URL + $"/cities/{cityId}/";
                return await SendGetRequestAsync<CityDataResponse>(url);
            }

            /// <summary>
            /// GET /zawomons/spells/ - lista wszystkich spelli w grze
            /// </summary>
            public static async Task<PublicSpellDataResponse[]> GetAllSpellsAsync()
            {
                string url = BASE_URL + "/spells/";
                return await SendGetRequestArrayAsync<PublicSpellDataResponse>(url);
            }
        }
    }
} 