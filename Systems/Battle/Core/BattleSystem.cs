
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
        
        private BattleState battleState;
        
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
        
        void Initialize()
        {
            // Setup UI event handlers
            if (teamSelectionUI != null)
            {
                teamSelectionUI.OnTeamsSelected += StartBattle;
            }
            
            if (moveSelectionUI != null)
            {
                moveSelectionUI.OnBothTeamsReady += ProcessTurn;
            }
            
            if (battleResultsUI != null)
            {
                battleResultsUI.OnRematchClicked += HandleRematch;
                battleResultsUI.OnExitBattleClicked += HandleExitBattle;
            }
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
            
            // Initialize move selection UI
            if (moveSelectionUI != null)
            {
                moveSelectionUI.Initialize(battleState);
            }
            
            Debug.Log("Battle started! Players can now select their moves.");
        }
        
        public void ProcessTurn()
        {
            if (battleState == null || battleState.phase != BattlePhase.Selection)
            {
                Debug.LogWarning("Cannot process turn: invalid battle state");
                return;
            }
            
            Debug.Log($"Processing turn {battleState.currentTurn + 1}");
            
            // Change to combat phase
            battleState.phase = BattlePhase.Combat;
            
            // Execute all moves based on initiative
            ExecuteCombatRound();
            
            // Check for battle end
            if (CheckBattleEnd())
            {
                EndBattle();
                return;
            }
            
            // Prepare for next turn
            battleState.currentTurn++;
            battleState.phase = BattlePhase.Selection;
            
            // Reset move selection UI for next turn
            if (moveSelectionUI != null)
            {
                moveSelectionUI.ResetTurn();
            }
            
            Debug.Log($"Turn {battleState.currentTurn} complete. Starting turn {battleState.currentTurn + 1}");
        }
        
        void ExecuteCombatRound()
        {
            // Collect all participants with selected spells
            var allParticipants = battleState.teamA.Concat(battleState.teamB)
                .Where(p => p.IsAlive && p.selectedSpell != null)
                .ToList();
            
            // Sort by initiative (higher first), then by level (higher first), then by experience and name for deterministic ordering
            allParticipants = allParticipants
                .OrderByDescending(p => p.TotalInitiative)
                .ThenByDescending(p => p.creature.level)
                .ThenByDescending(p => p.creature.experience)
                .ThenBy(p => p.creature.name)
                .ToList();
            
            Debug.Log($"Combat order: {string.Join(", ", allParticipants.Select(p => p.creature.name))}");
            
            // Execute each participant's move
            foreach (var participant in allParticipants)
            {
                if (participant.IsAlive && participant.selectedSpell != null)
                {
                    ExecuteSpell(participant, participant.selectedSpell);
                }
            }
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
        }
        
        List<BattleParticipant> GetSpellTargets(BattleParticipant caster, SpellEffect effect)
        {
            var targets = new List<BattleParticipant>();
            
            // Determine teams
            bool isCasterInTeamA = battleState.teamA.Contains(caster);
            var ownTeam = isCasterInTeamA ? battleState.teamA : battleState.teamB;
            var enemyTeam = isCasterInTeamA ? battleState.teamB : battleState.teamA;
            
            switch (effect.targetType)
            {
                case SpellTargetType.Enemy:
                    var firstEnemy = enemyTeam.FirstOrDefault(p => p.IsAlive);
                    if (firstEnemy != null) targets.Add(firstEnemy);
                    break;
                    
                case SpellTargetType.AllEnemies:
                    targets.AddRange(enemyTeam.Where(p => p.IsAlive));
                    break;
                    
                case SpellTargetType.Ally:
                    var firstAlly = ownTeam.FirstOrDefault(p => p.IsAlive && p != caster);
                    if (firstAlly != null) targets.Add(firstAlly);
                    break;
                    
                case SpellTargetType.AllAllies:
                    targets.AddRange(ownTeam.Where(p => p.IsAlive && p != caster));
                    break;
                    
                case SpellTargetType.Self:
                    targets.Add(caster);
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
                    break;
                    
                case SpellEffectType.Heal:
                    int healing = effect.power;
                    target.currentHP = Mathf.Min(target.creature.maxHP, target.currentHP + healing);
                    Debug.Log($"{caster.creature.name} heals {target.creature.name} for {healing} HP ({target.currentHP}/{target.creature.maxHP} HP)");
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