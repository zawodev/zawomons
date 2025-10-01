using UnityEngine;
using System.Collections;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Text;

// Simple WebSocket using Unity's built-in classes
namespace Systems.Notifications
{
    public class SimpleNotificationManager : MonoBehaviour
    {
        public static SimpleNotificationManager Instance;
        
        [Header("Settings")]
        public string wsUrl = "ws://localhost:8000/ws/invitations/";
        
        // Real WebSocket client
        private UnityWebSocketClient wsClient;
        private string authToken;
        private string currentUsername;
        
        // Events
        public System.Action<string> OnInvitationReceived; // from username
        public System.Action<string> OnInvitationAccepted; // from username
        public System.Action<string> OnInvitationDeclined; // from username
        
        void Awake()
        {
            if (Instance == null)
            {
                Instance = this;
                DontDestroyOnLoad(gameObject);
                
                // Create WebSocket client
                GameObject wsGO = new GameObject("WebSocketClient");
                wsGO.transform.SetParent(transform);
                wsClient = wsGO.AddComponent<UnityWebSocketClient>();
                wsClient.wsUrl = wsUrl;
                
                // Subscribe to events
                wsClient.OnMessageReceived += ProcessMessage;
                wsClient.OnConnected += () => Debug.Log("[SimpleNotifications] WebSocket connected");
                wsClient.OnDisconnected += () => Debug.Log("[SimpleNotifications] WebSocket disconnected");
            }
            else
            {
                Destroy(gameObject);
            }
        }
        
        void Start()
        {
            // Get auth info and start connection
            authToken = PlayerPrefs.GetString("auth_token", "");
            currentUsername = PlayerPrefs.GetString("username", "");
            
            if (!string.IsNullOrEmpty(authToken))
            {
                ConnectWebSocket();
            }
        }
        
        // Real WebSocket connection
        public void ConnectWebSocket()
        {
            if (wsClient != null && !wsClient.IsConnected)
            {
                wsClient.Connect(authToken);
                Debug.Log("[SimpleNotifications] Connecting to WebSocket...");
            }
        }
        
        public void DisconnectWebSocket()
        {
            if (wsClient != null)
            {
                wsClient.Disconnect();
            }
        }
        

        
        private void ProcessMessage(string messageJson)
        {
            try
            {
                var message = JsonConvert.DeserializeObject<Dictionary<string, object>>(messageJson);
                string type = message["type"].ToString();
                
                switch (type)
                {
                    case "invitation_received":
                        string fromUser = message["from_username"].ToString();
                        OnInvitationReceived?.Invoke(fromUser);
                        break;
                        
                    case "invitation_accepted":
                        string acceptedBy = message["from_username"].ToString();
                        OnInvitationAccepted?.Invoke(acceptedBy);
                        break;
                        
                    case "invitation_declined":
                        string declinedBy = message["from_username"].ToString();
                        OnInvitationDeclined?.Invoke(declinedBy);
                        break;
                }
            }
            catch (System.Exception e)
            {
                Debug.LogError($"[SimpleNotifications] Error processing message: {e.Message}");
            }
        }
        
        public void SendInvitation(string targetUsername)
        {
            StartCoroutine(SendMessage("send_invitation", new { target_username = targetUsername }));
        }
        
        public void AcceptInvitation(string fromUsername)
        {
            StartCoroutine(SendMessage("accept_invitation", new { from_username = fromUsername }));
        }
        
        public void DeclineInvitation(string fromUsername)
        {
            StartCoroutine(SendMessage("decline_invitation", new { from_username = fromUsername }));
        }
        
        // Send message through real WebSocket
        private IEnumerator SendMessage(string messageType, object data)
        {
            var message = new { 
                type = messageType, 
                from_username = currentUsername,
                data = data 
            };
            string json = JsonConvert.SerializeObject(message);
            
            Debug.Log($"[SimpleNotifications] Sending WebSocket message: {json}");
            
            if (wsClient != null && wsClient.IsConnected)
            {
                wsClient.SendMessage(json);
            }
            else
            {
                Debug.LogWarning("[SimpleNotifications] WebSocket not connected");
            }
            
            yield return null;
        }

    }
}