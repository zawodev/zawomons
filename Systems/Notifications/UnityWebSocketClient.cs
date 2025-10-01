using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine.Networking;

namespace Systems.Notifications
{
    /// <summary>
    /// Real WebSocket connection using Unity's networking
    /// This replaces the simulated version
    /// </summary>
    public class UnityWebSocketClient : MonoBehaviour
    {
        public string wsUrl = "ws://localhost:8000/ws/invitations/";
        
        // Since Unity doesn't have native WebSocket, we'll use HTTP long polling
        // that simulates WebSocket behavior
        private bool isConnected = false;
        private string authToken;
        private Coroutine connectionCoroutine;
        
        // Events
        public System.Action<string> OnMessageReceived;
        public System.Action OnConnected;
        public System.Action OnDisconnected;
        
        public bool IsConnected => isConnected;
        
        public void Connect(string token)
        {
            authToken = token;
            
            if (connectionCoroutine != null)
                StopCoroutine(connectionCoroutine);
            
            connectionCoroutine = StartCoroutine(ConnectionLoop());
        }
        
        public void Disconnect()
        {
            isConnected = false;
            
            if (connectionCoroutine != null)
            {
                StopCoroutine(connectionCoroutine);
                connectionCoroutine = null;
            }
            
            OnDisconnected?.Invoke();
        }
        
        public void SendMessage(string messageJson)
        {
            if (!isConnected)
            {
                Debug.LogWarning("[WebSocketClient] Cannot send message - not connected");
                return;
            }
            
            StartCoroutine(SendMessageCoroutine(messageJson));
        }
        
        private IEnumerator ConnectionLoop()
        {
            // Try to connect
            yield return StartCoroutine(EstablishConnection());
            
            if (isConnected)
            {
                OnConnected?.Invoke();
                
                // Start message polling loop
                while (isConnected)
                {
                    yield return StartCoroutine(PollForMessages());
                    yield return new WaitForSeconds(1f); // Poll every second
                }
                
                OnDisconnected?.Invoke();
            }
        }
        
        private IEnumerator EstablishConnection()
        {
            // Convert WebSocket URL to HTTP for connection test
            string httpUrl = wsUrl.Replace("ws://", "http://").Replace("wss://", "https://");
            httpUrl = httpUrl.Replace("/ws/invitations/", "/api/ws/connect/");
            
            using (UnityWebRequest request = UnityWebRequest.Get(httpUrl))
            {
                if (!string.IsNullOrEmpty(authToken))
                {
                    request.SetRequestHeader("Authorization", $"Bearer {authToken}");
                }
                
                yield return request.SendWebRequest();
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    isConnected = true;
                    Debug.Log("[WebSocketClient] Connected successfully");
                }
                else
                {
                    Debug.LogError($"[WebSocketClient] Connection failed: {request.error}");
                    isConnected = false;
                }
            }
        }
        
        private IEnumerator PollForMessages()
        {
            string httpUrl = wsUrl.Replace("ws://", "http://").Replace("wss://", "https://");
            httpUrl = httpUrl.Replace("/ws/invitations/", "/api/ws/poll/");
            
            using (UnityWebRequest request = UnityWebRequest.Get(httpUrl))
            {
                if (!string.IsNullOrEmpty(authToken))
                {
                    request.SetRequestHeader("Authorization", $"Bearer {authToken}");
                }
                
                request.timeout = 30; // Long polling timeout
                
                yield return request.SendWebRequest();
                
                if (request.result == UnityWebRequest.Result.Success)
                {
                    string response = request.downloadHandler.text;
                    if (!string.IsNullOrEmpty(response) && response != "null")
                    {
                        OnMessageReceived?.Invoke(response);
                    }
                }
                else if (request.result != UnityWebRequest.Result.ConnectionError)
                {
                    // Only disconnect on non-timeout errors
                    Debug.LogWarning($"[WebSocketClient] Poll error: {request.error}");
                    isConnected = false;
                }
            }
        }
        
        private IEnumerator SendMessageCoroutine(string messageJson)
        {
            string httpUrl = wsUrl.Replace("ws://", "http://").Replace("wss://", "https://");
            httpUrl = httpUrl.Replace("/ws/invitations/", "/api/ws/send/");
            
            byte[] bodyRaw = System.Text.Encoding.UTF8.GetBytes(messageJson);
            
            using (UnityWebRequest request = new UnityWebRequest(httpUrl, "POST"))
            {
                request.uploadHandler = new UploadHandlerRaw(bodyRaw);
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader("Content-Type", "application/json");
                
                if (!string.IsNullOrEmpty(authToken))
                {
                    request.SetRequestHeader("Authorization", $"Bearer {authToken}");
                }
                
                yield return request.SendWebRequest();
                
                if (request.result != UnityWebRequest.Result.Success)
                {
                    Debug.LogError($"[WebSocketClient] Send failed: {request.error}");
                }
            }
        }
    }
}