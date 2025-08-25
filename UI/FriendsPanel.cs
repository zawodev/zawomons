using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Systems;
using Systems.Battle;

namespace UI {
    public class FriendsPanel : MonoBehaviour {
        [Header("Panel References")]
        public GameObject panelRoot;
        public Button closeButton;
        
        [Header("Friends List")]
        public Transform friendsContentParent;
        public GameObject friendItemPrefab;
        public ScrollRect friendsScrollView;
        
        [Header("Battle Options")]
        public Button offlineBattleButton;
        
        [Header("Battle System Reference")]
        public BattleSystem battleSystem;
        
        // Mock friends data - in the future this will come from API
        private List<FriendData> mockFriends = new List<FriendData>();
        
        [System.Serializable]
        public class FriendData {
            public string username;
            public string avatarUrl; // For future use
            public bool isOnline;
            
            public FriendData(string name, bool online) {
                username = name;
                isOnline = online;
                avatarUrl = ""; // Mock for now
            }
        }
        
        private void Awake() {
            InitializeUI();
            InitializeMockData();
        }
        
        private void InitializeUI() {
            // Close button
            if (closeButton != null) {
                closeButton.onClick.AddListener(HidePanel);
            }
            
            // Offline battle button
            if (offlineBattleButton != null) {
                offlineBattleButton.onClick.AddListener(StartOfflineBattle);
            }
            
            // Initialize as hidden
            if (panelRoot != null) {
                panelRoot.SetActive(false);
            }
        }
        
        private void InitializeMockData() {
            // Create mock friends data - in the future this will be loaded from API
            mockFriends = new List<FriendData> {
                new FriendData("PlayerOne", true),
                new FriendData("DragonMaster", false),
                new FriendData("WaterWizard", true),
                new FriendData("FireKing", true),
                new FriendData("StoneCrusher", false),
                new FriendData("NatureLover", true),
                new FriendData("MagicScholar", false),
                new FriendData("DarkSorcerer", true)
            };
        }
        
        public void ShowPanel() {
            if (panelRoot != null) {
                panelRoot.SetActive(true);
                LoadFriendsList();
            }
        }
        
        public void HidePanel() {
            if (panelRoot != null) {
                panelRoot.SetActive(false);
            }
        }
        
        private void LoadFriendsList() {
            // In the future, this will call API to get friends list
            // For now, use mock data
            UpdateFriendsDisplay();
        }
        
        private void UpdateFriendsDisplay() {
            // Clear existing friend items
            if (friendsContentParent != null) {
                foreach (Transform child in friendsContentParent) {
                    Destroy(child.gameObject);
                }
            }
            
            // Create friend items
            if (friendItemPrefab != null && friendsContentParent != null) {
                foreach (var friend in mockFriends) {
                    GameObject friendItem = Instantiate(friendItemPrefab, friendsContentParent);
                    SetupFriendItem(friendItem, friend);
                }
            }
        }
        
        private void SetupFriendItem(GameObject friendItem, FriendData friend) {
            // Find UI components in the friend item prefab
            TMP_Text usernameText = friendItem.transform.Find("UsernameText")?.GetComponent<TMP_Text>();
            Image statusIndicator = friendItem.transform.Find("StatusIndicator")?.GetComponent<Image>();
            Image avatarImage = friendItem.transform.Find("AvatarImage")?.GetComponent<Image>();
            Button challengeButton = friendItem.transform.Find("ChallengeButton")?.GetComponent<Button>();
            
            // Set friend data
            if (usernameText != null) {
                usernameText.text = friend.username;
            }
            
            // Set online status indicator color
            if (statusIndicator != null) {
                statusIndicator.color = friend.isOnline ? Color.green : Color.gray;
            }
            
            // Set mock avatar color (random color based on username)
            if (avatarImage != null) {
                avatarImage.color = GenerateColorFromString(friend.username);
            }
            
            // Setup challenge button
            if (challengeButton != null) {
                TMP_Text buttonText = challengeButton.GetComponentInChildren<TMP_Text>();
                if (buttonText != null) {
                    buttonText.text = friend.isOnline ? "Challenge" : "Offline";
                }
                
                challengeButton.interactable = friend.isOnline;
                challengeButton.onClick.AddListener(() => ChallengeFriend(friend));
            }
        }
        
        private Color GenerateColorFromString(string text) {
            // Generate a consistent color based on string hash
            int hash = text.GetHashCode();
            Random.State oldState = Random.state;
            Random.InitState(hash);
            Color color = new Color(
                Random.Range(0.3f, 1f),
                Random.Range(0.3f, 1f),
                Random.Range(0.3f, 1f),
                1f
            );
            Random.state = oldState;
            return color;
        }
        
        private void ChallengeFriend(FriendData friend) {
            Debug.Log($"Challenging friend: {friend.username} to an online sparring match");
            
            // Mock: Start online battle (without consequences)
            // In the future, this will send a challenge request through API
            StartOnlineSparringMatch(friend);
        }
        
        private void StartOnlineSparringMatch(FriendData friend) {
            Debug.Log($"Starting online sparring match against {friend.username}");
            
            if (battleSystem != null) {
                // Hide this panel
                HidePanel();
                
                // Mock: Start battle in online mode but without consequences
                // For now, we'll use the same team selection as offline
                if (battleSystem.teamSelectionPanel != null) {
                    battleSystem.teamSelectionPanel.SetActive(true);
                }
            } else {
                Debug.LogWarning("BattleSystem reference not set in FriendsPanel!");
            }
        }
        
        private void StartOfflineBattle() {
            Debug.Log("Starting offline battle (local multiplayer)");
            
            if (battleSystem != null) {
                // Hide this panel
                HidePanel();
                
                // Start local battle
                if (battleSystem.teamSelectionPanel != null) {
                    battleSystem.teamSelectionPanel.SetActive(true);
                }
            } else {
                Debug.LogWarning("BattleSystem reference not set in FriendsPanel!");
            }
        }
        
        // Future methods for API integration
        private void LoadFriendsFromAPI() {
            // TODO: Implement API call to get friends list
            // var friends = await GameAPI.GetFriendsListAsync();
            // UpdateFriendsDisplay(friends);
        }
        
        private void SendChallengeRequest(string friendUsername) {
            // TODO: Implement API call to send challenge request
            // var success = await GameAPI.SendChallengeAsync(friendUsername);
            // if (success) { /* Handle success */ }
        }
        
        private void OnDestroy() {
            // Clean up event listeners
            if (closeButton != null) {
                closeButton.onClick.RemoveAllListeners();
            }
            if (offlineBattleButton != null) {
                offlineBattleButton.onClick.RemoveAllListeners();
            }
        }
    }
}
