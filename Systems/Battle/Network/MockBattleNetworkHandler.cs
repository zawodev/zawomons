using System;
using System.Collections;
using UnityEngine;
using Systems.Battle.Models;
using Systems.Creatures.Models;

namespace Systems.Battle.Network
{
    public class MockBattleNetworkHandler : MonoBehaviour, IBattleNetworkHandler
    {
        [Header("Mock Settings")]
        public bool simulateNetworkDelay = true;
        public float networkDelayMin = 0.1f;
        public float networkDelayMax = 0.3f;
        
        private bool isConnected = false;
        private bool offlineMode = false;
        
        // Events
        public event Action<BattleNetworkMessage> OnMessageReceived;
        public event Action OnConnected;
        public event Action OnDisconnected;
        
        // Properties
        public bool IsConnected => isConnected;
        
        void Start()
        {
            // Auto-connect in mock mode
            if (Application.isPlaying)
            {
                StartCoroutine(MockConnect());
            }
        }
        
        public void Connect()
        {
            if (!isConnected)
            {
                StartCoroutine(MockConnect());
            }
        }
        
        public void Disconnect()
        {
            isConnected = false;
            OnDisconnected?.Invoke();
            Debug.Log("[MockBattleNetwork] Disconnected from mock server");
        }
        
        public void EnableOfflineMode(bool enabled)
        {
            offlineMode = enabled;
            Debug.Log($"[MockBattleNetwork] Offline mode: {enabled}");
        }
        
        public void SendBattleStart(BattleStartRequest request)
        {
            Debug.Log($"[MockBattleNetwork] Battle Start: {request.battleId}, Mode: {request.mode}, Type: {request.battleType}");
            
            if (offlineMode)
            {
                Debug.Log("[MockBattleNetwork] Offline mode - handling locally");
                return;
            }
            
            if (simulateNetworkDelay)
            {
                StartCoroutine(MockSendWithDelay(() => {
                    // Mock server response - battle accepted
                    var response = new BattleNetworkMessage
                    {
                        messageType = "BattleStarted",
                        payload = JsonUtility.ToJson(new { battleId = request.battleId, status = "ready" }),
                        timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                    };
                    OnMessageReceived?.Invoke(response);
                }));
            }
        }
        
        public void SendMoveSelection(MoveSelectionRequest request)
        {
            Debug.Log($"[MockBattleNetwork] Move Selection: Creature {request.creatureId} casts {request.spellId}");
            
            if (offlineMode) return;
            
            if (simulateNetworkDelay)
            {
                StartCoroutine(MockSendWithDelay(() => {
                    // Mock server response - move acknowledged
                    var response = new BattleNetworkMessage
                    {
                        messageType = "MoveAcknowledged",
                        payload = JsonUtility.ToJson(new { battleId = request.battleId, turnNumber = request.turnNumber }),
                        timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                    };
                    OnMessageReceived?.Invoke(response);
                }));
            }
        }
        
        public void SendTurnComplete(TurnCompleteRequest request)
        {
            Debug.Log($"[MockBattleNetwork] Turn Complete: {request.battleId}, Turn {request.turnNumber}");
            
            if (offlineMode) return;
            
            if (simulateNetworkDelay)
            {
                StartCoroutine(MockSendWithDelay(() => {
                    // Mock server response - turn results validated
                    var response = new BattleNetworkMessage
                    {
                        messageType = "TurnValidated",
                        payload = JsonUtility.ToJson(new { 
                            battleId = request.battleId, 
                            turnNumber = request.turnNumber,
                            validated = true 
                        }),
                        timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
                    };
                    OnMessageReceived?.Invoke(response);
                }));
            }
        }
        
        private IEnumerator MockConnect()
        {
            Debug.Log("[MockBattleNetwork] Connecting to mock server...");
            yield return new WaitForSeconds(UnityEngine.Random.Range(0.5f, 1.5f));
            
            isConnected = true;
            OnConnected?.Invoke();
            Debug.Log("[MockBattleNetwork] Connected to mock server");
        }
        
        private IEnumerator MockSendWithDelay(Action callback)
        {
            float delay = UnityEngine.Random.Range(networkDelayMin, networkDelayMax);
            yield return new WaitForSeconds(delay);
            callback?.Invoke();
        }
        
        void OnDestroy()
        {
            Disconnect();
        }
    }
}