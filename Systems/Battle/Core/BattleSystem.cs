
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Systems.Battle.Models;
using Systems.Battle.UI;
using Systems.Creatures.Models;

namespace Systems.Battle.Core
{
    public class BattleSystem : MonoBehaviour
    {
        [Header("UI References")]
        public TeamSelectionUI teamSelectionUI;
        public BattleMoveSelectionUI moveSelectionUI;
        public BattleResultsUI battleResultsUI;
        public GameObject teamSelectionPanel;
        public GameObject battlePanel;
        public GameObject battleResultsPanel;
        
        [Header("Arena")]
        public BattleArena battleArena;
        
        [Header("Animation Settings")]
        [Tooltip("Delay in seconds between each participant's turn in combat")]
        public float turnSlotDelay = 3.0f;
        
        private BattleState battleState;
        private bool accelerateAnimations = false;
        private bool isPlayingAnimations = false;
        
        [Header("Animation Settings")]
        public float normalAnimationSpeed = 1f;
        public float acceleratedAnimationSpeed = 0.01f;
        
        // Events
        public System.Action<string> OnBattleFinished;

        void Awake()
        {
            if (teamSelectionPanel != null) teamSelectionPanel.SetActive(false);
            if (battlePanel != null) battlePanel.SetActive(false);
            if (battleResultsPanel != null) battleResultsPanel.SetActive(false);
        }

        void Start()
        {
            Initialize();
        }
        
        public void AccelerateCurrentAnimations()
        {
            accelerateAnimations = true;
            Debug.Log("[BattleSystem] Accelerate animations requested - speeding up to fast mode");
            
            // Update all current animation speeds
            UpdateAnimationSpeeds();
        }
        
        private void UpdateAnimationSpeeds()
        {
            Debug.Log($"[BattleSystem] Updating animation speeds - accelerated: {accelerateAnimations}");
            // Note: Individual animation speed updates will be handled by each component
            // when they check the accelerateAnimations flag during their animations
        }
        
        public bool IsPlayingAnimations => isPlayingAnimations;
        public bool IsAcceleratingAnimations => accelerateAnimations;
        
        public void SetBattleConfiguration(Systems.Battle.Models.BattleMode mode, Systems.Battle.Models.BattleType battleType)
        {
            Debug.Log($"[BattleSystem] Battle configuration set - Mode: {mode}, Type: {battleType}");
            
            if (battleState != null)
            {
                battleState.mode = mode;
                battleState.battleType = battleType;
            }
            
            // Configure battle mode
            Debug.Log($"[BattleSystem] Battle mode set to: {mode}, Type: {battleType}");
        }
        
        void Initialize()
        {
            // Set default battle configuration if not set
            if (battleState == null || (battleState.mode == Systems.Battle.Models.BattleMode.Local && battleState.battleType == Systems.Battle.Models.BattleType.FriendlyMatch))
            {
                SetBattleConfiguration(Systems.Battle.Models.BattleMode.Local, Systems.Battle.Models.BattleType.FriendlyMatch);
            }
            
            // Setup UI event handlers
            if (teamSelectionUI != null)
            {
                teamSelectionUI.OnTeamsSelected += StartBattle;
            }
            
            if (moveSelectionUI != null)
            {
                Debug.Log("[BattleSystem] Connecting ProcessTurn to OnBothTeamsReady event");
                moveSelectionUI.OnBothTeamsReady += ProcessTurn;
            }
            else
            {
                Debug.LogWarning("[BattleSystem] moveSelectionUI is null, cannot connect ProcessTurn event");
            }
            
            if (battleResultsUI != null)
            {
                battleResultsUI.OnRematchClicked += HandleRematch;
                battleResultsUI.OnExitBattleClicked += HandleExitBattle;
            }
            
            // Network events will be handled by online battle components when needed
        }
        
        void ShowTeamSelection()
        {
            if (teamSelectionPanel != null) teamSelectionPanel.SetActive(true);
            if (battlePanel != null) battlePanel.SetActive(false);
            if (battleResultsPanel != null) battleResultsPanel.SetActive(false);
        }
        
        void ShowBattleUI()
        {
            if (teamSelectionPanel != null) teamSelectionPanel.SetActive(false);
            if (battlePanel != null) battlePanel.SetActive(true);
            if (battleResultsPanel != null) battleResultsPanel.SetActive(false);
        }
        
        public void StartBattle(List<Creature> teamA, List<Creature> teamB)
        {
            StartBattle(teamA, teamB, BattleMode.Local);
        }
        
        public void StartBattle(List<Creature> teamA, List<Creature> teamB, BattleMode mode)
        {
            Debug.Log($"Starting battle: Team A ({teamA.Count}) vs Team B ({teamB.Count})");
            
            // Initialize battle state
            battleState = new BattleState
            {
                teamA = teamA.Select(creature => new BattleParticipant(creature)).ToList(),
                teamB = teamB.Select(creature => new BattleParticipant(creature)).ToList(),
                mode = mode,
                phase = BattlePhase.Selection,
                currentTurn = 0
            };
            
            // Switch to battle UI
            ShowBattleUI();
            
            // Setup battle arena
            if (battleArena != null)
            {
                battleArena.SetupArena(battleState);
            }
            
            // Initialize move selection UI
            if (moveSelectionUI != null)
            {
                moveSelectionUI.Initialize(battleState);
            }
            
            Debug.Log("Battle started! Players can now select their moves.");
        }
        
        public void ProcessTurn()
        {
            Debug.Log("[BattleSystem] ProcessTurn called - starting coroutine");
            StartCoroutine(ProcessTurnCoroutine());
        }
        
        System.Collections.IEnumerator ProcessTurnCoroutine()
        {
            Debug.Log("[BattleSystem] ProcessTurnCoroutine started");
            
            if (battleState == null || battleState.phase != BattlePhase.Selection)
            {
                Debug.LogWarning($"Cannot process turn: invalid battle state. BattleState null: {battleState == null}, Phase: {battleState?.phase}");
                yield break;
            }
            
            Debug.Log($"[BattleSystem] Processing turn {battleState.currentTurn + 1}");
            
            // Change to combat phase
            battleState.phase = BattlePhase.Combat;
            Debug.Log("[BattleSystem] Changed to Combat phase, starting ExecuteCombatRound");
            
            // Execute all moves based on initiative (wait for animations)
            yield return ExecuteCombatRoundCoroutine();
            
            Debug.Log("[BattleSystem] Combat round finished, checking battle end");
            
            // Check for battle end
            if (CheckBattleEnd())
            {
                EndBattle();
                yield break;
            }
            
            // Prepare for next turn
            battleState.currentTurn++;
            battleState.phase = BattlePhase.Selection;
            
            Debug.Log($"[BattleSystem] Preparing next turn - current turn: {battleState.currentTurn}");
            
            // Reset move selection UI for next turn
            if (moveSelectionUI != null)
            {
                moveSelectionUI.ResetTurn();
                
                // Ensure UI is properly refreshed for new turn
                if (moveSelectionUI.selectionUI != null)
                {
                    moveSelectionUI.selectionUI.UpdateUI();
                }
            }
            
            Debug.Log($"[BattleSystem] Turn {battleState.currentTurn} complete. Starting turn {battleState.currentTurn + 1}");
        }
        

        
        System.Collections.IEnumerator ExecuteCombatRoundCoroutine()
        {
            Debug.Log("[BattleSystem] ExecuteCombatRoundCoroutine started");
            
            // Collect all participants with selected spells
            var allParticipants = battleState.teamA.Concat(battleState.teamB).ToList();
            Debug.Log($"[BattleSystem] Total participants: {allParticipants.Count}");
            
            foreach (var p in allParticipants)
            {
                Debug.Log($"[BattleSystem] Participant {p.creature.name}: IsAlive={p.IsAlive}, selectedSpell={(p.selectedSpell?.name ?? "null")}, hasConfirmedMove={p.hasConfirmedMove}");
            }
            
            // Filter to only participants with spells (same logic as TurnQueue)
            allParticipants = allParticipants
                .Where(p => p.IsAlive && p.selectedSpell != null && p.hasConfirmedMove)
                .ToList();
                
            Debug.Log($"[BattleSystem] Found {allParticipants.Count} participants with spells for animation");
            
            // Sort by initiative (higher first), then by level (higher first), then by experience and name for deterministic ordering
            allParticipants = allParticipants
                .OrderByDescending(p => p.TotalInitiative)
                .ThenByDescending(p => p.creature.level)
                .ThenByDescending(p => p.creature.experience)
                .ThenBy(p => p.creature.name)
                .ToList();
            
            Debug.Log($"[BattleSystem] Combat order: {string.Join(", ", allParticipants.Select(p => p.creature.name))}");
            
            // Set animation flags
            isPlayingAnimations = true;
            accelerateAnimations = false;
            
            // Disable UI during animation phase
            if (moveSelectionUI != null)
            {
                moveSelectionUI.SetInteractable(false);
            }
            
            // Execute each participant's move with animations (all have spells now)
            for (int i = 0; i < allParticipants.Count; i++)
            {
                // Check if acceleration requested 
                if (accelerateAnimations)
                {
                    Debug.Log($"[BattleSystem] Animations accelerated - continuing with fast animations");
                    UpdateAnimationSpeeds(); // Make sure speeds are updated
                }
                
                var participant = allParticipants[i];
                Debug.Log($"[BattleSystem] Executing turn {i + 1}/{allParticipants.Count}: {participant.creature.name} casts {participant.selectedSpell.name}");
                
                yield return ExecuteSpellWithAnimation(participant, participant.selectedSpell);
                
                // Advance turn queue after each participant
                if (moveSelectionUI != null && moveSelectionUI.turnQueueUI != null)
                {
                    moveSelectionUI.turnQueueUI.AdvanceQueue();
                    Debug.Log($"[BattleSystem] Advanced turn queue after {participant.creature.name}'s turn");
                }
                
                // Add configurable delay between turns (except after the last one)
                if (i < allParticipants.Count - 1 && turnSlotDelay > 0)
                {
                    float delayTime = accelerateAnimations ? acceleratedAnimationSpeed : turnSlotDelay;
                    Debug.Log($"[BattleSystem] Waiting {delayTime} seconds before next turn...");
                    yield return new WaitForSeconds(delayTime);
                }
            }
            
            // Clear animation flags
            isPlayingAnimations = false;
            accelerateAnimations = false;
            
            // Reset animation speeds to normal
            UpdateAnimationSpeeds();
            
            // Re-enable UI after animations complete
            if (moveSelectionUI != null)
            {
                moveSelectionUI.SetInteractable(true);
            }
        }
        
        System.Collections.IEnumerator ExecuteSpellWithAnimation(BattleParticipant caster, Spell spell)
        {
            Debug.Log($"{caster.creature.name} casts {spell.name}");
            
            // Find caster's BattleCreature component in the arena
            if (battleArena != null)
            {
                var casterCreature = battleArena.GetCreatureForParticipant(caster);
                if (casterCreature != null)
                {
                    // Determine animation type based on spell
                    bool isAttackSpell = spell.effects.Any(e => e.effectType == SpellEffectType.Damage);
                    
                    if (isAttackSpell)
                        casterCreature.PlayAttackAnimation();
                    else
                        casterCreature.PlayBuffAnimation();
                        
                    // Wait for animation to complete
                    bool animationComplete = false;
                    System.Action<Systems.Battle.Components.BattleCreature> onComplete = (creature) => animationComplete = true;
                    casterCreature.OnAnimationComplete += onComplete;
                    
                    // Wait for animation
                    yield return new WaitUntil(() => animationComplete);
                    
                    casterCreature.OnAnimationComplete -= onComplete;
                }
            }
            
            // Execute spell logic after animation
            ExecuteSpell(caster, spell);
        }
        
        void ExecuteSpell(BattleParticipant caster, Spell spell)
        {
            Debug.Log($"{caster.creature.name} casts {spell.name}");
            
            // Wykonaj każdy efekt spella
            foreach (var effect in spell.effects)
            {
                var targets = GetSpellTargets(caster, effect);
                
                foreach (var target in targets)
                {
                    ApplySpellEffect(caster, target, effect);
                }
            }
            
            // Note: Turn queue advance is now handled in main combat loop
        }
        
        List<BattleParticipant> GetSpellTargets(BattleParticipant caster, SpellEffect effect)
        {
            var targets = new List<BattleParticipant>();
            
            // Determine teams
            bool isCasterInTeamA = battleState.teamA.Contains(caster);
            var ownTeam = isCasterInTeamA ? battleState.teamA : battleState.teamB;
            var enemyTeam = isCasterInTeamA ? battleState.teamB : battleState.teamA;
            
            // For single target spells, use player's selected target if available
            if (effect.targetType == SpellTargetType.Enemy || effect.targetType == SpellTargetType.Ally)
            {
                if (caster.selectedTarget != null && caster.selectedTarget.IsAlive)
                {
                    // Validate that selected target matches spell type
                    bool isSelectedTargetEnemy = enemyTeam.Contains(caster.selectedTarget);
                    bool isSelectedTargetAlly = ownTeam.Contains(caster.selectedTarget) && caster.selectedTarget != caster;
                    
                    if ((effect.targetType == SpellTargetType.Enemy && isSelectedTargetEnemy) ||
                        (effect.targetType == SpellTargetType.Ally && isSelectedTargetAlly))
                    {
                        targets.Add(caster.selectedTarget);
                        Debug.Log($"[BattleSystem] Using player selected target: {caster.creature.name} -> {caster.selectedTarget.creature.name}");
                        return targets;
                    }
                }
            }
            
            // Fallback to default targeting logic
            switch (effect.targetType)
            {
                case SpellTargetType.Enemy:
                    var firstEnemy = enemyTeam.FirstOrDefault(p => p.IsAlive);
                    if (firstEnemy != null) targets.Add(firstEnemy);
                    Debug.Log($"[BattleSystem] Using default enemy target: {caster.creature.name} -> {firstEnemy?.creature.name ?? "none"}");
                    break;
                    
                case SpellTargetType.AllEnemies:
                    targets.AddRange(enemyTeam.Where(p => p.IsAlive));
                    Debug.Log($"[BattleSystem] Targeting all enemies: {string.Join(", ", targets.Select(t => t.creature.name))}");
                    break;
                    
                case SpellTargetType.Ally:
                    var firstAlly = ownTeam.FirstOrDefault(p => p.IsAlive && p != caster);
                    if (firstAlly != null) targets.Add(firstAlly);
                    Debug.Log($"[BattleSystem] Using default ally target: {caster.creature.name} -> {firstAlly?.creature.name ?? "none"}");
                    break;
                    
                case SpellTargetType.AllAllies:
                    targets.AddRange(ownTeam.Where(p => p.IsAlive && p != caster));
                    Debug.Log($"[BattleSystem] Targeting all allies: {string.Join(", ", targets.Select(t => t.creature.name))}");
                    break;
                    
                case SpellTargetType.Self:
                    targets.Add(caster);
                    Debug.Log($"[BattleSystem] Self-targeting: {caster.creature.name}");
                    break;
            }
            
            return targets;
        }
        
        void ApplySpellEffect(BattleParticipant caster, BattleParticipant target, SpellEffect effect)
        {
            switch (effect.effectType)
            {
                case SpellEffectType.Damage:
                    int damage = effect.power;
                    target.currentHP = Mathf.Max(0, target.currentHP - damage);
                    Debug.Log($"{caster.creature.name} deals {damage} damage to {target.creature.name} ({target.currentHP}/{target.creature.maxHP} HP remaining)");
                    
                    // Play hurt animation on the target
                    if (battleArena != null)
                    {
                        var targetCreature = battleArena.GetCreatureForParticipant(target);
                        if (targetCreature != null)
                        {
                            targetCreature.PlayHurtAnimation();
                            Debug.Log($"[BattleSystem] Playing hurt animation on {target.creature.name}");
                        }
                    }
                    
                    // Update UI immediately after damage
                    RefreshBattleUI();
                    break;
                    
                case SpellEffectType.Heal:
                    int healing = effect.power;
                    target.currentHP = Mathf.Min(target.creature.maxHP, target.currentHP + healing);
                    Debug.Log($"{caster.creature.name} heals {target.creature.name} for {healing} HP ({target.currentHP}/{target.creature.maxHP} HP)");
                    
                    // Update UI immediately after healing
                    RefreshBattleUI();
                    break;
                    
                case SpellEffectType.BuffInitiative:
                    target.initiativeBonus += effect.power;
                    Debug.Log($"{caster.creature.name} gives {target.creature.name} +{effect.power} initiative bonus");
                    break;
                    
                case SpellEffectType.BuffDamage:
                    target.creature.damage += effect.power;
                    Debug.Log($"{caster.creature.name} gives {target.creature.name} +{effect.power} damage bonus");
                    break;
            }
        }
        
        bool CheckBattleEnd()
        {
            bool teamAAlive = battleState.IsTeamAAlive;
            bool teamBAlive = battleState.IsTeamBAlive;
            
            if (!teamAAlive && !teamBAlive)
            {
                battleState.winner = "Draw"; // Should not happen due to initiative order
                return true;
            }
            else if (!teamAAlive)
            {
                battleState.winner = "Team B";
                return true;
            }
            else if (!teamBAlive)
            {
                battleState.winner = "Team A";
                return true;
            }
            
            return false;
        }
        
        void EndBattle()
        {
            battleState.phase = BattlePhase.Finished;
            
            Debug.Log($"Battle finished! Winner: {battleState.winner}");
            
            OnBattleFinished?.Invoke(battleState.winner);
            
            // Disable interaction with move selection UI but keep it visible
            if (moveSelectionUI != null)
            {
                moveSelectionUI.SetInteractable(false);
            }
            
            // Show battle results panel
            if (battleResultsPanel != null)
            {
                battleResultsPanel.SetActive(true);
            }
            
            // Show battle results UI
            if (battleResultsUI != null)
            {
                battleResultsUI.ShowResults(battleState);
            }
            else
            {
                Debug.LogWarning("BattleResultsUI is not assigned! Falling back to auto-return.");
                // Fallback: return to team selection after delay
                Invoke(nameof(ReturnToTeamSelection), 3f);
            }
        }
        
        void ReturnToTeamSelection()
        {
            battleState = null;
            
            if (teamSelectionUI != null)
            {
                teamSelectionUI.ResetSelection();
            }
            
            ShowTeamSelection();
        }
        
        // Public methods for external access
        public BattleState GetBattleState() => battleState;
        
        public bool IsBattleActive() => battleState != null && battleState.phase != BattlePhase.Finished;
        
        public void ResetBattleSystem()
        {
            Debug.Log("Resetting battle system");
            
            battleState = null;
            
            if (teamSelectionUI != null)
            {
                teamSelectionUI.ResetSelection();
            }
            
            if (moveSelectionUI != null && moveSelectionUI.gameObject.activeInHierarchy)
            {
                moveSelectionUI.ResetTurn();
                moveSelectionUI.SetInteractable(true); // Re-enable interactions
            }
            
            if (battleResultsPanel != null)
            {
                battleResultsPanel.SetActive(false);
            }
            
            ShowTeamSelection();
        }
        
        // Event handlers for Battle Results UI
        void HandleRematch()
        {
            Debug.Log("Rematch requested");
            
            // Hide results panel
            if (battleResultsPanel != null)
            {
                battleResultsPanel.SetActive(false);
            }
            
            // Re-enable move selection UI
            if (moveSelectionUI != null)
            {
                moveSelectionUI.SetInteractable(true);
            }
            
            // Reset battle state but keep teams
            if (teamSelectionUI != null)
            {
                teamSelectionUI.ResetSelection();
            }
            
            // Return to team selection for new battle
            ShowTeamSelection();
            battleState = null;
        }
        
        void RefreshBattleUI()
        {
            // Refresh UI components to show updated HP values
            if (moveSelectionUI != null && moveSelectionUI.selectionUI != null)
            {
                moveSelectionUI.selectionUI.UpdateUI();
            }
        }
        
        void HandleExitBattle()
        {
            // Reset battle system
            ResetBattleSystem();

            Debug.Log("Exiting battle completely");
            
            // Hide all battle UI panels
            if (battleResultsPanel != null)
            {
                battleResultsPanel.SetActive(false);
            }
            
            if (teamSelectionPanel != null)
            {
                teamSelectionPanel.SetActive(false);
            }
            
            if (battlePanel != null)
            {
                battlePanel.SetActive(false);
            }
            
            // You might want to return to main menu or world map here
            // For now, we'll just reset everything
        }
        

        

        



    }
}