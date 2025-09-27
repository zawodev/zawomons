using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Systems;
using Systems.Battle;
using Systems.API;

namespace UI {
    public class FriendsPanel : MonoBehaviour {
        [Header("Panel References")]
        public GameObject panelRoot;
        public Button closeButton;
        
        [Header("View Toggle")]
        public Button friendsViewButton;
        public Button allPlayersViewButton;
        public TMP_Text viewModeText;
        public Button refreshButton;
        
        [Header("Friends List")]
        public Transform friendsContentParent;
        public GameObject friendItemPrefab;
        public ScrollRect friendsScrollView;
        
        [Header("Battle Options")]
        public Button offlineBattleButton;
        
        [Header("Battle System Reference")]
        public BattleSystem battleSystem;
        
        // Data from API
        private PlayerSummaryResponse[] allPlayersData;
        private PlayerSummaryResponse[] friendsData;
        private bool isShowingFriends = true;
        
        // Refresh cooldown
        private float lastRefreshTime = 0f;
        private const float REFRESH_COOLDOWN = 10f; // 10 seconds cooldown
        private bool isRefreshing = false;
        
        // Legacy mock data class - keep for compatibility
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
            LoadDataFromAPI();
        }
        
        private void Update() {
            // Update refresh button state continuously during cooldown
            if (refreshButton != null && lastRefreshTime > 0) {
                UpdateRefreshButton();
            }
        }
        
        private void InitializeUI() {
            // Close button
            if (closeButton != null) {
                closeButton.onClick.AddListener(HidePanel);
            }
            
            // View toggle buttons
            if (friendsViewButton != null) {
                friendsViewButton.onClick.AddListener(() => SwitchToView(true));
            }
            
            if (allPlayersViewButton != null) {
                allPlayersViewButton.onClick.AddListener(() => SwitchToView(false));
            }
            
            // Refresh button
            if (refreshButton != null) {
                refreshButton.onClick.AddListener(RefreshData);
            }
            
            // Offline battle button
            if (offlineBattleButton != null) {
                offlineBattleButton.onClick.AddListener(StartOfflineBattle);
            }
            
            // Initialize view state
            UpdateViewToggleButtons();
            
            // Initialize as hidden
            if (panelRoot != null) {
                panelRoot.SetActive(false);
            }
        }
        
        private async void LoadDataFromAPI() {
            // Load both friends and all players data once on start
            isRefreshing = true;
            UpdateRefreshButton();
            
            try {
                Debug.Log("Loading friends and players data from API...");
                
                // Load friends data
                friendsData = await GameAPI.Players.GetMyFriendsAsync();
                if (friendsData == null) {
                    Debug.LogWarning("Failed to load friends data from API");
                    friendsData = new PlayerSummaryResponse[0];
                }
                
                // Load all players data
                allPlayersData = await GameAPI.Players.GetAllPlayersAsync();
                if (allPlayersData == null) {
                    Debug.LogWarning("Failed to load players data from API");
                    allPlayersData = new PlayerSummaryResponse[0];
                }
                
                Debug.Log($"Loaded {friendsData.Length} friends and {allPlayersData.Length} players");
                
                // Update display if panel is currently visible
                if (panelRoot != null && panelRoot.activeSelf) {
                    UpdateDisplayedList();
                }
            }
            catch (System.Exception e) {
                Debug.LogError($"Error loading data from API: {e.Message}");
                // Fallback to empty arrays
                friendsData = new PlayerSummaryResponse[0];
                allPlayersData = new PlayerSummaryResponse[0];
            }
            finally {
                isRefreshing = false;
                UpdateRefreshButton();
            }
        }
        
        private void RefreshData() {
            // Check cooldown
            float timeSinceLastRefresh = Time.time - lastRefreshTime;
            if (timeSinceLastRefresh < REFRESH_COOLDOWN) {
                float remainingTime = REFRESH_COOLDOWN - timeSinceLastRefresh;
                Debug.Log($"Refresh on cooldown. Try again in {remainingTime:F1} seconds.");
                return;
            }
            
            if (isRefreshing) {
                Debug.Log("Already refreshing data...");
                return;
            }
            
            Debug.Log("Refreshing data from API...");
            lastRefreshTime = Time.time;
            LoadDataFromAPI();
            UpdateRefreshButton();
        }
        
        private void UpdateRefreshButton() {
            if (refreshButton == null) return;
            
            float timeSinceLastRefresh = Time.time - lastRefreshTime;
            bool canRefresh = timeSinceLastRefresh >= REFRESH_COOLDOWN && !isRefreshing;
            
            refreshButton.interactable = canRefresh;
            
            // Update button text to show cooldown
            TMP_Text buttonText = refreshButton.GetComponentInChildren<TMP_Text>();
            if (buttonText != null) {
                if (!canRefresh && !isRefreshing) {
                    float remainingTime = REFRESH_COOLDOWN - timeSinceLastRefresh;
                    buttonText.text = $"Refresh ({remainingTime:F0}s)";
                } else if (isRefreshing) {
                    buttonText.text = "Refreshing...";
                } else {
                    buttonText.text = "Refresh";
                }
            }
        }
        
        private void SwitchToView(bool showFriends) {
            isShowingFriends = showFriends;
            UpdateViewToggleButtons();
            UpdateDisplayedList();
        }
        
        private void UpdateViewToggleButtons() {
            // Update button interactability
            if (friendsViewButton != null) {
                friendsViewButton.interactable = !isShowingFriends;
            }
            
            if (allPlayersViewButton != null) {
                allPlayersViewButton.interactable = isShowingFriends;
            }
            
            // Update refresh button
            UpdateRefreshButton();
            
            // Update view mode text
            if (viewModeText != null) {
                viewModeText.text = isShowingFriends ? "Friends" : "All Players";
            }
        }
        
        public void ShowPanel() {
            if (panelRoot != null) {
                panelRoot.SetActive(true);
                UpdateDisplayedList();
            }
        }
        
        public void HidePanel() {
            if (panelRoot != null) {
                panelRoot.SetActive(false);
            }
        }
        
        private void LoadFriendsList() {
            // Legacy method - now redirects to UpdateDisplayedList
            UpdateDisplayedList();
        }
        
        private void UpdateDisplayedList() {
            // Clear existing items
            if (friendsContentParent != null) {
                foreach (Transform child in friendsContentParent) {
                    Destroy(child.gameObject);
                }
            }
            
            // Get current data to display
            PlayerSummaryResponse[] currentData = isShowingFriends ? friendsData : allPlayersData;
            
            if (currentData == null) {
                Debug.LogWarning("No data to display - API data not loaded yet");
                return;
            }
            
            // Create items for current view
            if (friendItemPrefab != null && friendsContentParent != null) {
                foreach (var playerData in currentData) {
                    GameObject friendItem = Instantiate(friendItemPrefab, friendsContentParent);
                    SetupPlayerItem(friendItem, playerData);
                }
            }
        }
        
        private void SetupPlayerItem(GameObject friendItem, PlayerSummaryResponse playerData) {
            // Find UI components in the friend item prefab
            TMP_Text usernameText = friendItem.transform.Find("UsernameText")?.GetComponent<TMP_Text>();
            Image statusIndicator = friendItem.transform.Find("StatusIndicator")?.GetComponent<Image>();
            Image avatarImage = friendItem.transform.Find("AvatarImage")?.GetComponent<Image>();
            Button challengeButton = friendItem.transform.Find("ChallengeButton")?.GetComponent<Button>();
            
            // Additional UI components for player info
            TMP_Text experienceText = friendItem.transform.Find("ExperienceText")?.GetComponent<TMP_Text>();
            TMP_Text creatureCountText = friendItem.transform.Find("CreatureCountText")?.GetComponent<TMP_Text>();
            
            // Set player data
            if (usernameText != null) {
                usernameText.text = playerData.username;
            }
            
            // Set online status indicator color
            if (statusIndicator != null) {
                statusIndicator.color = playerData.is_online ? Color.green : Color.gray;
            }
            
            // Set mock avatar color (random color based on username)
            if (avatarImage != null) {
                avatarImage.color = GenerateColorFromString(playerData.username);
            }
            
            // Set additional info
            if (experienceText != null) {
                experienceText.text = $"XP: {playerData.experience}";
            }
            
            if (creatureCountText != null) {
                creatureCountText.text = $"Creatures: {playerData.creature_count}";
            }
            
            // Setup challenge button
            if (challengeButton != null) {
                TMP_Text buttonText = challengeButton.GetComponentInChildren<TMP_Text>();
                if (buttonText != null) {
                    buttonText.text = playerData.is_online ? "Challenge" : "Offline";
                }
                
                challengeButton.interactable = playerData.is_online;
                challengeButton.onClick.AddListener(() => ChallengePlayer(playerData));
            }
        }
        
        // Legacy method for compatibility
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
            StartOnlineSparringMatch(friend.username);
        }
        
        private void ChallengePlayer(PlayerSummaryResponse playerData) {
            Debug.Log($"Challenging player: {playerData.username} to an online sparring match");
            
            // Mock: Start online battle (without consequences)
            // In the future, this will send a challenge request through API
            StartOnlineSparringMatch(playerData.username);
        }
        
        private void StartOnlineSparringMatch(string playerUsername) {
            Debug.Log($"Starting online sparring match against {playerUsername}");
            
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
        
        // API integration methods - now implemented
        private void SendChallengeRequest(string playerUsername) {
            // TODO: Implement API call to send challenge request in the future
            // var success = await GameAPI.SendChallengeAsync(playerUsername);
            // if (success) { /* Handle success */ }
            Debug.Log($"Challenge request sent to {playerUsername} (mock implementation)");
        }
        
        private void OnDestroy() {
            // Clean up event listeners
            if (closeButton != null) {
                closeButton.onClick.RemoveAllListeners();
            }
            if (offlineBattleButton != null) {
                offlineBattleButton.onClick.RemoveAllListeners();
            }
            if (friendsViewButton != null) {
                friendsViewButton.onClick.RemoveAllListeners();
            }
            if (allPlayersViewButton != null) {
                allPlayersViewButton.onClick.RemoveAllListeners();
            }
            if (refreshButton != null) {
                refreshButton.onClick.RemoveAllListeners();
            }
        }
    }
}
