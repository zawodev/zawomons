using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Systems.Battle.Core;

namespace UI.Social
{
    public class SimpleWaitingPanel : MonoBehaviour
    {
        [Header("UI References")]
        public GameObject panelRoot;
        public TMP_Text messageText;
        public Button cancelButton;
        
        [Header("Battle System")]
        public BattleSystem battleSystem;
        
        private string waitingForUsername;
        
        void Awake()
        {
            if (cancelButton != null)
                cancelButton.onClick.AddListener(OnCancel);
            
            // Hide panel initially
            if (panelRoot != null)
                panelRoot.SetActive(false);
            
            // Subscribe to notifications
            if (Systems.Notifications.SimpleNotificationManager.Instance != null)
            {
                Systems.Notifications.SimpleNotificationManager.Instance.OnInvitationAccepted += OnInvitationAccepted;
                Systems.Notifications.SimpleNotificationManager.Instance.OnInvitationDeclined += OnInvitationDeclined;
            }
        }
        
        public void ShowWaitingFor(string targetUsername)
        {
            waitingForUsername = targetUsername;
            
            if (messageText != null)
                messageText.text = $"Waiting for {targetUsername} to respond...";
            
            if (panelRoot != null)
                panelRoot.SetActive(true);
            
            Debug.Log($"[WaitingPanel] Waiting for response from {targetUsername}");
        }
        
        private void OnInvitationAccepted(string fromUsername)
        {
            if (fromUsername == waitingForUsername)
            {
                // Start online battle
                StartOnlineBattle();
                
                // Hide panel
                HidePanel();
                
                Debug.Log($"[WaitingPanel] {fromUsername} accepted invitation");
            }
        }
        
        private void OnInvitationDeclined(string fromUsername)
        {
            if (fromUsername == waitingForUsername)
            {
                // Show decline message briefly
                if (messageText != null)
                    messageText.text = $"{fromUsername} declined the invitation";
                
                // Hide panel after delay
                Invoke(nameof(HidePanel), 2f);
                
                Debug.Log($"[WaitingPanel] {fromUsername} declined invitation");
            }
        }
        
        private void OnCancel()
        {
            // TODO: Send cancel message to server
            HidePanel();
        }
        
        private void StartOnlineBattle()
        {
            if (battleSystem != null)
            {
                // Set battle mode to Online
                battleSystem.SetBattleConfiguration(
                    Systems.Battle.Models.BattleMode.Online,
                    Systems.Battle.Models.BattleType.FriendlyMatch
                );
                
                // Show team selection
                if (battleSystem.teamSelectionPanel != null)
                {
                    battleSystem.teamSelectionPanel.SetActive(true);
                }
                
                Debug.Log("[WaitingPanel] Started online battle setup");
            }
        }
        
        private void HidePanel()
        {
            if (panelRoot != null)
                panelRoot.SetActive(false);
            
            waitingForUsername = "";
        }
        
        void OnDestroy()
        {
            if (cancelButton != null)
                cancelButton.onClick.RemoveAllListeners();
        }
    }
}