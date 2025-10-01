using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Systems.Battle.Core;

namespace UI.Social
{
    public class SimpleInvitationPanel : MonoBehaviour
    {
        [Header("UI References")]
        public GameObject panelRoot;
        public TMP_Text messageText;
        public Button acceptButton;
        public Button declineButton;
        
        [Header("Battle System")]
        public BattleSystem battleSystem;
        
        private string currentInviterUsername;
        
        void Awake()
        {
            if (acceptButton != null)
                acceptButton.onClick.AddListener(OnAccept);
                
            if (declineButton != null)
                declineButton.onClick.AddListener(OnDecline);
            
            // Hide panel initially
            if (panelRoot != null)
                panelRoot.SetActive(false);
            
            // Subscribe to notifications
            if (Systems.Notifications.SimpleNotificationManager.Instance != null)
            {
                Systems.Notifications.SimpleNotificationManager.Instance.OnInvitationReceived += ShowInvitation;
            }
        }
        
        private void ShowInvitation(string fromUsername)
        {
            currentInviterUsername = fromUsername;
            
            if (messageText != null)
                messageText.text = $"{fromUsername} invites you to a friendly battle!";
            
            if (panelRoot != null)
                panelRoot.SetActive(true);
            
            Debug.Log($"[InvitationPanel] Received invitation from {fromUsername}");
        }
        
        private void OnAccept()
        {
            if (!string.IsNullOrEmpty(currentInviterUsername))
            {
                // Send acceptance
                Systems.Notifications.SimpleNotificationManager.Instance?.AcceptInvitation(currentInviterUsername);
                
                // Start online battle
                StartOnlineBattle();
                
                // Hide panel
                HidePanel();
            }
        }
        
        private void OnDecline()
        {
            if (!string.IsNullOrEmpty(currentInviterUsername))
            {
                // Send decline
                Systems.Notifications.SimpleNotificationManager.Instance?.DeclineInvitation(currentInviterUsername);
                
                // Hide panel
                HidePanel();
            }
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
                
                Debug.Log("[InvitationPanel] Started online battle setup");
            }
        }
        
        private void HidePanel()
        {
            if (panelRoot != null)
                panelRoot.SetActive(false);
            
            currentInviterUsername = "";
        }
        
        void OnDestroy()
        {
            if (acceptButton != null)
                acceptButton.onClick.RemoveAllListeners();
                
            if (declineButton != null)
                declineButton.onClick.RemoveAllListeners();
        }
    }
}