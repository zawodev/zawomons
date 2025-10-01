using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using Systems.Battle.Models;
using Systems.Battle.Core;
using Systems.Battle.Components;
using Systems.Creatures.Models;

namespace Systems.Battle.UI
{
    public class BattleMoveSelectionUI : MonoBehaviour
    {
        [Header("New UI Systems")]
        public BattleSelectionUI selectionUI;
        public BattleTurnQueueUI turnQueueUI;
        public BattleArena battleArena;
        
        [Header("Legacy UI Components")]
        public TextMeshProUGUI turnInfoText;
        public Button nextTurnButton;
        
        [Header("Battle System Reference")]
        public Systems.Battle.Core.BattleSystem battleSystem;
        
        [Header("Network")]
        public Systems.Battle.Network.IBattleNetworkHandler networkHandler;
        
        // References
        private BattleState battleState;
        
        // Target selection system
        private Dictionary<BattleParticipant, BattleParticipant> selectedTargets = new Dictionary<BattleParticipant, BattleParticipant>();
        
        // Auto-trigger protection
        private bool battleStartTriggered = false;
        
        // Events
        public System.Action OnBothTeamsReady;
        
        void Start()
        {
            if (nextTurnButton != null)
                nextTurnButton.onClick.AddListener(() => {
                    HandleNextTurnButton();
                });
                
            SetupEventHandlers();
        }
        
        private void HandleNextTurnButton()
        {
            if (battleSystem != null && battleSystem.IsPlayingAnimations)
            {
                // Accelerate current animations
                Debug.Log("[BattleMoveSelectionUI] NextTurn button clicked during animations - accelerating animations");
                battleSystem.AccelerateCurrentAnimations();
            }
            else if (battleSystem != null && battleSystem.GetBattleState() != null && battleSystem.GetBattleState().phase == Systems.Battle.Models.BattlePhase.Selection)
            {
                // Only process turn if we're in Selection phase
                Debug.Log("[BattleMoveSelectionUI] NextTurn button clicked during Selection phase - starting battle");
                OnBothTeamsReady?.Invoke();
            }
            else
            {
                Debug.Log("[BattleMoveSelectionUI] NextTurn button clicked but battle is not in Selection phase or no animations playing - ignoring");
            }
        }
        
        private void SetupEventHandlers()
        {
            if (selectionUI != null)
            {
                selectionUI.OnSpellSelected += OnSpellSelected;
                selectionUI.OnVisibilityChanged += OnVisibilityChanged;
                selectionUI.OnTargetIndicatorChanged += OnTargetIndicatorChanged;
            }
            
            if (turnQueueUI != null)
            {
                turnQueueUI.OnQueueAnimationComplete += OnQueueAnimationComplete;
            }
            
            if (battleArena != null)
            {
                battleArena.OnCreatureAnimationComplete += OnCreatureAnimationComplete;
            }
        }
        
        public void Initialize(BattleState state)
        {
            battleState = state;
            
            // Initialize all subsystems
            if (selectionUI != null)
                selectionUI.Initialize(state);
                
            if (battleArena != null)
                battleArena.SetupArena(state);
                
            selectedTargets.Clear();
            
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
            if (selectionUI != null)
                selectionUI.HandleInput();
        }
        
        private void OnSpellSelected(BattleParticipant caster, Spell spell, BattleParticipant target)
        {
            // Store target selection if applicable
            if (target != null)
            {
                selectedTargets[caster] = target;
            }
            
            // Update arena to show target indicators
            if (battleArena != null && target != null)
            {
                battleArena.ShowTargetIndicator(target, true);
            }
            
            // Check if all teams are ready and update turn queue
            if (battleState.AreBothTeamsReady)
            {
                InitializeTurnQueue();
            }
        }
        
        private void OnVisibilityChanged(bool teamAVisible, bool teamBVisible)
        {
            // Handle visibility changes if needed
            // For now, the BattleSelectionUI handles this internally
        }
        
        private void OnQueueAnimationComplete()
        {
            // Turn queue animation finished
            // This can trigger turn processing
        }
        
        private void OnCreatureAnimationComplete(BattleCreature creature)
        {
            // Creature animation finished
            // Can be used to chain animations or proceed to next action
        }
        
        private void OnTargetIndicatorChanged(BattleParticipant target, bool show)
        {
            if (battleArena != null)
            {
                if (show && target != null)
                {
                    // Clear all indicators first, then show on target
                    battleArena.ShowTargetIndicator(target, true);
                }
                else
                {
                    // Clear all target indicators
                    foreach (var participant in battleState.teamA.Concat(battleState.teamB))
                    {
                        battleArena.ShowTargetIndicator(participant, false);
                    }
                }
            }
        }
        
        void UpdateUI()
        {
            if (battleState == null) return;
            
            // Update selection UI
            if (selectionUI != null)
                selectionUI.UpdateUI();
            
            // Update arena visuals
            if (battleArena != null)
                battleArena.UpdateAllHealthVisuals();
            
            // Update turn info text (legacy)
            if (turnInfoText != null)
            {
                string status = GetBattleStatusText();
                turnInfoText.text = $"Turn: {battleState.currentTurn + 1}\\n{status}";
            }
            
            // Check for automatic battle start when both teams manual ready + all confirmed
            bool bothReady = battleState.AreBothTeamsReady;
            //Debug.Log($"[BattleMoveSelectionUI] UpdateDisplay - Both teams ready: {bothReady}, Team A ready: {battleState.IsTeamAReady}, Team B ready: {battleState.IsTeamBReady}");
            //Debug.Log($"[BattleMoveSelectionUI] Manual ready states - Team A: {battleState.teamAManualReady}, Team B: {battleState.teamBManualReady}");
            
            // Auto-trigger battle when both teams are manually ready AND all creatures have confirmed moves
            if (bothReady && battleState.phase == BattlePhase.Selection && !battleStartTriggered)
            {
                Debug.Log("[BattleMoveSelectionUI] Both teams ready - auto-triggering OnBothTeamsReady");
                battleStartTriggered = true;
                OnBothTeamsReady?.Invoke();
            }
            
            // Update next turn button (for manual fallback)
            if (nextTurnButton != null)
            {
                nextTurnButton.interactable = bothReady;
            }
        }
        
        private string GetBattleStatusText()
        {
            // Use manual ready states instead of hasConfirmedMove states
            if (battleState.teamAManualReady && battleState.teamBManualReady)
                return "Both teams ready! Battle starting...";
            else if (battleState.teamAManualReady)
                return "Team A ready. Waiting for Team B to hold O...";
            else if (battleState.teamBManualReady)
                return "Team B ready. Waiting for Team A to hold E...";
            else
                return "Select moves for your Zawomons. Hold E (Team A) or O (Team B) when ready.";
        }
        
        private void InitializeTurnQueue()
        {
            if (turnQueueUI != null && battleState != null)
            {
                turnQueueUI.InitializeQueue(battleState);
            }
            
            // Don't auto-trigger battle start - let manual ready system handle it
            Debug.Log("[BattleMoveSelectionUI] InitializeTurnQueue - queue initialized, waiting for manual ready");
        }
        
        public void ResetTurn()
        {
            if (battleState == null) return;
            
            // Reset all move confirmations
            foreach (var participant in battleState.teamA)
                participant.ResetMoveSelection();
            
            foreach (var participant in battleState.teamB)
                participant.ResetMoveSelection();
            
            // Reset manual ready states
            battleState.teamAManualReady = false;
            battleState.teamBManualReady = false;
            
            // Reset battle start trigger
            battleStartTriggered = false;
            
            // Clear target selections
            selectedTargets.Clear();
            
            // Clear target indicators in arena
            if (battleArena != null)
            {
                foreach (var participant in battleState.teamA.Concat(battleState.teamB))
                {
                    battleArena.ShowTargetIndicator(participant, false);
                }
            }
            
            // Reset turn queue
            if (turnQueueUI != null)
                turnQueueUI.ResetQueue();
            
            // Reset BattleSelectionUI ready states
            if (selectionUI != null)
            {
                selectionUI.Initialize(battleState);
                // Reset visual ready indicators
                selectionUI.ResetReadyStates();
            }
        }
        
        public void SetInteractable(bool interactable)
        {
            // Disable/enable next turn button
            if (nextTurnButton != null)
            {
                nextTurnButton.interactable = interactable && battleState?.AreBothTeamsReady == true;
            }
        }
        
        // Public methods for BattleSystem integration
        public void PlayAttackAnimation(BattleParticipant attacker)
        {
            if (battleArena != null)
                battleArena.PlayAttackAnimation(attacker);
        }
        
        public void PlayBuffAnimation(BattleParticipant buffer)
        {
            if (battleArena != null)
                battleArena.PlayBuffAnimation(buffer);
        }
        
        public void AdvanceTurnQueue()
        {
            if (turnQueueUI != null)
                turnQueueUI.AdvanceQueue();
        }
        
        public BattleParticipant GetCurrentTurnParticipant()
        {
            return turnQueueUI?.GetCurrentParticipant();
        }
        
        public bool IsTurnQueueComplete()
        {
            return turnQueueUI?.IsQueueComplete() ?? true;
        }
        
        public BattleParticipant GetSelectedTarget(BattleParticipant caster)
        {
            selectedTargets.TryGetValue(caster, out BattleParticipant target);
            return target;
        }
        
        public bool IsAnyAnimationPlaying()
        {
            return battleArena?.IsAnyCreatureAnimating() ?? false;
        }
        
        void OnDestroy()
        {
            // Clean up event handlers
            if (selectionUI != null)
            {
                selectionUI.OnSpellSelected -= OnSpellSelected;
                selectionUI.OnVisibilityChanged -= OnVisibilityChanged;
                selectionUI.OnTargetIndicatorChanged -= OnTargetIndicatorChanged;
            }
            
            if (turnQueueUI != null)
            {
                turnQueueUI.OnQueueAnimationComplete -= OnQueueAnimationComplete;
            }
            
            if (battleArena != null)
            {
                battleArena.OnCreatureAnimationComplete -= OnCreatureAnimationComplete;
            }
        }
    }
}
