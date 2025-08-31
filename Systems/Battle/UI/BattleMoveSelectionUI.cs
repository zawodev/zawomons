using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Models;

namespace Systems.Battle.UI
{
    public class BattleMoveSelectionUI : MonoBehaviour
    {
        [Header("UI Components")]
        public TextMeshProUGUI teamAInfoText;
        public TextMeshProUGUI teamBInfoText;
        public TextMeshProUGUI turnInfoText;
        public Button nextTurnButton;
        public GameObject teamAPanel;
        public GameObject teamBPanel;
        
        [Header("Visibility Toggles")]
        public GameObject teamAVisibilityIndicator;
        public GameObject teamBVisibilityIndicator;
        
        // Player Selection State
        private int teamASelectedZawomon = 0;
        private int teamBSelectedZawomon = 0;
        private int teamASelectedSpell = 0;
        private int teamBSelectedSpell = 0;
        private bool teamAVisible = true;
        private bool teamBVisible = true;
        
        // References
        private BattleState battleState;
        
        // Events
        public System.Action OnBothTeamsReady;
        
        void Start()
        {
            if (nextTurnButton != null)
                nextTurnButton.onClick.AddListener(() => OnBothTeamsReady?.Invoke());
        }
        
        public void Initialize(BattleState state)
        {
            battleState = state;
            teamASelectedZawomon = 0;
            teamBSelectedZawomon = 0;
            teamASelectedSpell = 0;
            teamBSelectedSpell = 0;
            teamAVisible = true;
            teamBVisible = true;
            
            UpdateUI();
        }
        
        void Update()
        {
            if (battleState == null || battleState.phase != BattlePhase.Selection) return;
            
            HandleInput();
            UpdateUI();
        }
        
        void HandleInput()
        {
            if (UnityEngine.InputSystem.Keyboard.current == null) return;
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            
            // Team A (Red) Controls: WASDQE
            HandleTeamInput(
                keyboard.wKey.wasPressedThisFrame, keyboard.sKey.wasPressedThisFrame,
                keyboard.aKey.wasPressedThisFrame, keyboard.dKey.wasPressedThisFrame,
                keyboard.qKey.wasPressedThisFrame, keyboard.eKey.wasPressedThisFrame,
                battleState.teamA, ref teamASelectedZawomon, ref teamASelectedSpell, ref teamAVisible
            );
            
            // Team B (Blue) Controls: IJKLUO
            HandleTeamInput(
                keyboard.iKey.wasPressedThisFrame, keyboard.kKey.wasPressedThisFrame,
                keyboard.jKey.wasPressedThisFrame, keyboard.lKey.wasPressedThisFrame,
                keyboard.uKey.wasPressedThisFrame, keyboard.oKey.wasPressedThisFrame,
                battleState.teamB, ref teamBSelectedZawomon, ref teamBSelectedSpell, ref teamBVisible
            );
        }
        
        void HandleTeamInput(bool upPressed, bool downPressed, bool leftPressed, bool rightPressed, 
                           bool toggleVisibilityPressed, bool confirmPressed,
                           List<BattleParticipant> team, ref int selectedZawomon, ref int selectedSpell, ref bool teamVisible)
        {
            if (team.Count == 0) return;
            
            // Filter alive team members
            var aliveMembers = team.FindAll(p => p.IsAlive);
            if (aliveMembers.Count == 0) return;
            
            // Ensure selected zawomon is alive
            if (selectedZawomon >= aliveMembers.Count)
                selectedZawomon = 0;
            
            var currentParticipant = aliveMembers[selectedZawomon];
            
            // Zawomon selection (W/S or I/K)
            if (upPressed)
            {
                selectedZawomon = (selectedZawomon - 1 + aliveMembers.Count) % aliveMembers.Count;
                selectedSpell = 0; // Reset spell selection
            }
            else if (downPressed)
            {
                selectedZawomon = (selectedZawomon + 1) % aliveMembers.Count;
                selectedSpell = 0; // Reset spell selection
            }
            
            // Spell selection (A/D or J/L)
            if (leftPressed && currentParticipant.creature.spells.Count > 0)
            {
                selectedSpell = (selectedSpell - 1 + currentParticipant.creature.spells.Count) % currentParticipant.creature.spells.Count;
            }
            else if (rightPressed && currentParticipant.creature.spells.Count > 0)
            {
                selectedSpell = (selectedSpell + 1) % currentParticipant.creature.spells.Count;
            }
            
            // Visibility toggle (Q/U) - only in local mode
            if (toggleVisibilityPressed && battleState.mode == BattleMode.Local)
            {
                teamVisible = !teamVisible;
            }
            
            // Confirm move (E/O)
            if (confirmPressed && currentParticipant.creature.spells.Count > 0)
            {
                currentParticipant.selectedSpell = currentParticipant.creature.spells[selectedSpell];
                currentParticipant.hasConfirmedMove = true;
                
                // Move to next unconfirmed zawomon
                for (int i = 0; i < aliveMembers.Count; i++)
                {
                    int nextIdx = (selectedZawomon + i + 1) % aliveMembers.Count;
                    if (!aliveMembers[nextIdx].hasConfirmedMove)
                    {
                        selectedZawomon = nextIdx;
                        selectedSpell = 0;
                        break;
                    }
                }
            }
        }
        
        void UpdateUI()
        {
            if (battleState == null) return;
            
            // Update team panels visibility
            if (teamAPanel != null) teamAPanel.SetActive(teamAVisible);
            if (teamBPanel != null) teamBPanel.SetActive(teamBVisible);
            
            // Update visibility indicators
            if (teamAVisibilityIndicator != null) 
                teamAVisibilityIndicator.SetActive(!teamAVisible);
            if (teamBVisibilityIndicator != null) 
                teamBVisibilityIndicator.SetActive(!teamBVisible);
            
            // Update team info texts
            UpdateTeamInfoText(teamAInfoText, battleState.teamA, teamASelectedZawomon, teamASelectedSpell, teamAVisible, "Team A (Red)");
            UpdateTeamInfoText(teamBInfoText, battleState.teamB, teamBSelectedZawomon, teamBSelectedSpell, teamBVisible, "Team B (Blue)");
            
            // Update turn info
            if (turnInfoText != null)
            {
                string status = "";
                if (battleState.IsTeamAReady && battleState.IsTeamBReady)
                    status = "Both teams ready! Click Next Turn or wait for resolution.";
                else if (battleState.IsTeamAReady)
                    status = "Team A ready. Waiting for Team B...";
                else if (battleState.IsTeamBReady)
                    status = "Team B ready. Waiting for Team A...";
                else
                    status = "Select moves for your Zawomons. E/O to confirm, Q/U to toggle visibility.";
                
                turnInfoText.text = $"Turn: {battleState.currentTurn + 1}\n{status}";
            }
            
            // Update next turn button
            if (nextTurnButton != null)
                nextTurnButton.interactable = battleState.AreBothTeamsReady;
        }
        
        void UpdateTeamInfoText(TextMeshProUGUI textComponent, List<BattleParticipant> team, 
                              int selectedZawomon, int selectedSpell, bool isVisible, string teamName)
        {
            if (textComponent == null) return;
            
            if (!isVisible)
            {
                textComponent.text = $"{teamName}\n[HIDDEN]";
                return;
            }
            
            var aliveMembers = team.FindAll(p => p.IsAlive);
            if (aliveMembers.Count == 0)
            {
                textComponent.text = $"{teamName}\nAll defeated!";
                return;
            }
            
            string text = $"{teamName}\n\n";
            
            for (int i = 0; i < aliveMembers.Count; i++)
            {
                var participant = aliveMembers[i];
                string color = "white";
                
                if (i == selectedZawomon)
                    color = participant.hasConfirmedMove ? "green" : "yellow";
                else if (participant.hasConfirmedMove)
                    color = "grey";
                
                text += $"<color={color}>{participant.creature.name} ({participant.currentHP}/{participant.creature.maxHP} HP)";
                
                if (participant.hasConfirmedMove && participant.selectedSpell != null)
                    text += $" - {participant.selectedSpell.name}";
                
                text += "</color>\n";
            }
            
            // Show spells for selected zawomon
            if (selectedZawomon < aliveMembers.Count)
            {
                var current = aliveMembers[selectedZawomon];
                if (!current.hasConfirmedMove && current.creature.spells.Count > 0)
                {
                    text += "\nSpells:\n";
                    for (int i = 0; i < current.creature.spells.Count; i++)
                    {
                        string spellColor = i == selectedSpell ? "red" : "grey";
                        text += $"<color={spellColor}>{current.creature.spells[i].name}</color>\n";
                    }
                }
            }
            
            textComponent.text = text;
        }
        
        public void ResetTurn()
        {
            if (battleState == null) return;
            
            // Reset all move confirmations
            foreach (var participant in battleState.teamA)
                participant.ResetMoveSelection();
            
            foreach (var participant in battleState.teamB)
                participant.ResetMoveSelection();
            
            // Reset selections to first alive member
            teamASelectedZawomon = 0;
            teamBSelectedZawomon = 0;
            teamASelectedSpell = 0;
            teamBSelectedSpell = 0;
        }
        
        public void SetInteractable(bool interactable)
        {
            // Disable/enable next turn button
            if (nextTurnButton != null)
            {
                nextTurnButton.interactable = interactable;
            }
            
            // You can add more interactive elements here if needed
            // For example: spell selection buttons, creature selection buttons, etc.
        }
    }
}
