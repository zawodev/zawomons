using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;
using Systems.Battle.Models;
using Systems.Creatures.Models;

namespace Systems.Battle.UI
{
    public class BattleSelectionUI : MonoBehaviour
    {
        [Header("Selection Panel")]
        public Transform teamASelectionContainer;
        public Transform teamBSelectionContainer;
        public GameObject creatureRowPrefab; // Prefab for creature + spells row
        
        [Header("Visibility Controls")]
        public GameObject teamAVisibilityToggle;
        public GameObject teamBVisibilityToggle;
        
        [Header("Team Ready System")]
        public Image teamABackground; // Background indicator for Team A ready button
        public Image teamAProgress;   // Radial fill progress for Team A hold E
        public Image teamBBackground; // Background indicator for Team B ready button  
        public Image teamBProgress;   // Radial fill progress for Team B hold O
        public float holdDuration = 1.0f; // How long to hold E/O
        
        [Header("Team Ready Colors")]
        public Color teamReadyColor = Color.green;      // When team is ready/committed
        public Color teamNotReadyColor = Color.gray;    // When team is not ready
        public Color teamSelectedColor = Color.yellow;  // When ready button is selected (hover)
        
        // Selection state
        private BattleState battleState;
        private int teamASelectedCreature = 0;
        private int teamASelectedSpell = 0;
        private int teamASelectedTarget = 0;
        private int teamBSelectedCreature = 0;
        private int teamBSelectedSpell = 0;
        private int teamBSelectedTarget = 0;
        private bool teamAVisible = true;
        private bool teamBVisible = true;
        
        // Control modes
        private enum ControlMode { Navigation, Selection }
        private ControlMode teamAMode = ControlMode.Navigation;
        private ControlMode teamBMode = ControlMode.Navigation;
        
        // Battle state
        private bool isBattleStarted = false;
        
        // Current selection context
        private BattleParticipant teamACurrentParticipant = null;
        private BattleParticipant teamBCurrentParticipant = null;
        private bool teamATargetMoved = false;
        private bool teamBTargetMoved = false;
        
        // Hold E/O system for Ready button
        private float teamAHoldTime = 0f;
        private float teamBHoldTime = 0f;
        private bool teamAIsHolding = false;
        private bool teamBIsHolding = false;
        
        // UI rows for creatures and their spells
        private List<CreatureSelectionRow> teamARows = new List<CreatureSelectionRow>();
        private List<CreatureSelectionRow> teamBRows = new List<CreatureSelectionRow>();
        
        // Events
        public System.Action<BattleParticipant, Spell, BattleParticipant> OnSpellSelected; // caster, spell, target
        public System.Action<bool, bool> OnVisibilityChanged; // teamAVisible, teamBVisible
        public System.Action<BattleParticipant, bool> OnTargetIndicatorChanged; // target, show
        
        public void Initialize(BattleState state)
        {
            battleState = state;
            ResetSelectionState();
            BuildUI();
            UpdateVisibility();
        }
        
        private void ResetSelectionState()
        {
            teamASelectedCreature = 0;
            teamASelectedSpell = 0;
            teamASelectedTarget = 0;
            teamBSelectedCreature = 0;
            teamBSelectedSpell = 0;
            teamBSelectedTarget = 0;
            teamAVisible = true;
            teamBVisible = true;
            teamAMode = ControlMode.Navigation;
            teamBMode = ControlMode.Navigation;
            teamACurrentParticipant = null;
            teamBCurrentParticipant = null;
            teamATargetMoved = false;
            teamBTargetMoved = false;
            
            // Reset hold system
            teamAHoldTime = 0f;
            teamBHoldTime = 0f;
            teamAIsHolding = false;
            teamBIsHolding = false;
        }
        
        private void BuildUI()
        {
            ClearUI();
            
            if (battleState == null) return;
            
            // Build Team A UI
            BuildTeamUI(battleState.teamA, teamASelectionContainer, teamARows);
            
            // Build Team B UI  
            BuildTeamUI(battleState.teamB, teamBSelectionContainer, teamBRows);
        }
        
        private void BuildTeamUI(List<BattleParticipant> team, Transform container, List<CreatureSelectionRow> rowsList)
        {
            if (container == null || creatureRowPrefab == null) return;
            
            var aliveMembers = team.FindAll(p => p.IsAlive);
            
            foreach (var participant in aliveMembers)
            {
                GameObject rowObj = Instantiate(creatureRowPrefab, container);
                CreatureSelectionRow row = rowObj.GetComponent<CreatureSelectionRow>();
                
                if (row != null)
                {
                    row.Initialize(participant);
                    rowsList.Add(row);
                }
            }
        }
        
        private void ClearUI()
        {
            // Clear Team A
            foreach (var row in teamARows)
            {
                if (row != null && row.gameObject != null)
                    Destroy(row.gameObject);
            }
            teamARows.Clear();
            
            // Clear Team B
            foreach (var row in teamBRows)
            {
                if (row != null && row.gameObject != null)
                    Destroy(row.gameObject);
            }
            teamBRows.Clear();
        }
        
        public void HandleInput()
        {
            if (battleState?.phase != BattlePhase.Selection) return;
            if (isBattleStarted) return; // Block input when battle has started
            
            var keyboard = UnityEngine.InputSystem.Keyboard.current;
            if (keyboard == null) return;
            
            // Team A controls (always available for local player)
            if (battleState.mode == BattleMode.Local || battleState.mode == BattleMode.Online)
            {
                HandleTeamInput(
                    keyboard.wKey.wasPressedThisFrame, keyboard.sKey.wasPressedThisFrame,
                    keyboard.aKey.wasPressedThisFrame, keyboard.dKey.wasPressedThisFrame,
                    keyboard.qKey.wasPressedThisFrame, keyboard.eKey.wasPressedThisFrame,
                    keyboard.eKey.isPressed, // Hold E for team ready
                    true, // isTeamA
                    battleState.teamA, ref teamASelectedCreature, ref teamASelectedSpell, 
                    ref teamASelectedTarget, ref teamAVisible, ref teamAMode, 
                    ref teamACurrentParticipant, teamARows
                );
            }
            
            // Team B controls (only in local mode)
            if (battleState.mode == BattleMode.Local)
            {
                HandleTeamInput(
                    keyboard.iKey.wasPressedThisFrame, keyboard.kKey.wasPressedThisFrame,
                    keyboard.jKey.wasPressedThisFrame, keyboard.lKey.wasPressedThisFrame,
                    keyboard.uKey.wasPressedThisFrame, keyboard.oKey.wasPressedThisFrame,
                    keyboard.oKey.isPressed, // Hold O for team ready
                    false, // isTeamA
                    battleState.teamB, ref teamBSelectedCreature, ref teamBSelectedSpell,
                    ref teamBSelectedTarget, ref teamBVisible, ref teamBMode, 
                    ref teamBCurrentParticipant, teamBRows
                );
            }
            
            // Update hold progress indicators
            UpdateHoldProgress();
        }
        
        private void UpdateHoldProgress()
        {
            // Update Team A hold progress
            if (teamAIsHolding)
            {
                teamAHoldTime += Time.deltaTime;
                float progress = Mathf.Clamp01(teamAHoldTime / holdDuration);
                
                if (teamAProgress != null)
                {
                    teamAProgress.fillAmount = progress;
                    // Gradient from gray to green
                    teamAProgress.color = Color.Lerp(teamNotReadyColor, teamReadyColor, progress);
                }
                
                // Complete hold
                if (progress >= 1.0f && !battleState.teamAManualReady)
                {
                    battleState.teamAManualReady = true;
                    teamAIsHolding = false;
                    Debug.Log("[BattleSelectionUI] Team A ready for battle!");
                    
                    // Auto-confirm "skip turn" for creatures without selected spells
                    AutoConfirmSkipTurns(battleState.teamA);
                }
            }
            else if (teamAProgress != null)
            {
                // Not holding - keep progress if already ready, otherwise reset
                if (battleState.teamAManualReady)
                {
                    teamAProgress.fillAmount = 1.0f;
                    teamAProgress.color = teamReadyColor;
                }
                else
                {
                    teamAProgress.fillAmount = 0f;
                    teamAProgress.color = teamNotReadyColor;
                }
            }
            
            // Update Team B hold progress
            if (teamBIsHolding)
            {
                teamBHoldTime += Time.deltaTime;
                float progress = Mathf.Clamp01(teamBHoldTime / holdDuration);
                
                if (teamBProgress != null)
                {
                    teamBProgress.fillAmount = progress;
                    // Gradient from gray to green
                    teamBProgress.color = Color.Lerp(teamNotReadyColor, teamReadyColor, progress);
                }
                
                // Complete hold
                if (progress >= 1.0f && !battleState.teamBManualReady)
                {
                    battleState.teamBManualReady = true;
                    teamBIsHolding = false;
                    Debug.Log("[BattleSelectionUI] Team B ready for battle!");
                    
                    // Auto-confirm "skip turn" for creatures without selected spells
                    AutoConfirmSkipTurns(battleState.teamB);
                }
            }
            else if (teamBProgress != null)
            {
                // Not holding - keep progress if already ready, otherwise reset
                if (battleState.teamBManualReady)
                {
                    teamBProgress.fillAmount = 1.0f;
                    teamBProgress.color = teamReadyColor;
                }
                else
                {
                    teamBProgress.fillAmount = 0f;
                    teamBProgress.color = teamNotReadyColor;
                }
            }
            
            // Update background ready indicators
            if (teamABackground != null)
            {
                bool isTeamASelected = (teamASelectedCreature == -1);
                
                // Background logic: ONLY Yellow if selected, Gray otherwise (NO green)
                if (isTeamASelected)
                    teamABackground.color = teamSelectedColor;
                else
                    teamABackground.color = teamNotReadyColor;
            }
            
            if (teamBBackground != null)
            {
                bool isTeamBSelected = (teamBSelectedCreature == -1);
                
                // Background logic: ONLY Yellow if selected, Gray otherwise (NO green)
                if (isTeamBSelected)
                    teamBBackground.color = teamSelectedColor;
                else
                    teamBBackground.color = teamNotReadyColor;
            }
            
            // Check if both teams ready and trigger battle
            if (battleState.AreBothTeamsReady && !isBattleStarted)
            {
                Debug.Log("[BattleSelectionUI] Both teams committed - starting battle!");
                StartBattleSequence();
            }
        }
        
        private void HandleTeamInput(bool upPressed, bool downPressed, bool leftPressed, bool rightPressed,
                                   bool qPressed, bool ePressed, bool eHeld, bool isTeamA,
                                   List<BattleParticipant> team, ref int selectedCreature, ref int selectedSpell,
                                   ref int selectedTarget, ref bool teamVisible, ref ControlMode mode,
                                   ref BattleParticipant currentParticipant, List<CreatureSelectionRow> rows)
        {
            var aliveMembers = team.FindAll(p => p.IsAlive);
            if (aliveMembers.Count == 0) return;
            
            // Navigation now includes Ready button at -1 and creatures 0 to aliveMembers.Count-1
            int totalNavigationItems = aliveMembers.Count + 1; // +1 for Ready button
            
            // Ensure selected creature is valid (-1 = Ready button, 0+ = creatures)
            if (selectedCreature >= aliveMembers.Count)
                selectedCreature = -1; // Ready button
            
            BattleParticipant selectedParticipant = null;
            if (selectedCreature >= 0)
                selectedParticipant = aliveMembers[selectedCreature];
            
            if (mode == ControlMode.Navigation)
            {
                // Navigation mode: W/S = creature, A/D = spell, Q = toggle visibility, E = confirm creature
                
                // Navigation (W/S or I/K) - includes Ready button at -1
                if (upPressed)
                {
                    selectedCreature--;
                    if (selectedCreature < -1)
                        selectedCreature = aliveMembers.Count - 1; // Wrap to last creature
                    selectedSpell = 0;
                    
                    // Reset target moved flag when changing selection
                    if (team == battleState.teamA)
                        teamATargetMoved = false;
                    else
                        teamBTargetMoved = false;
                }
                else if (downPressed)
                {
                    selectedCreature++;
                    if (selectedCreature >= aliveMembers.Count)
                        selectedCreature = -1; // Wrap to Ready button
                    selectedSpell = 0;
                    
                    // Reset target moved flag when changing selection
                    if (team == battleState.teamA)
                        teamATargetMoved = false;
                    else
                        teamBTargetMoved = false;
                }
                
                // Spell selection (A/D or J/L) - only for creatures, not Ready button
                // BLOCKED if team is manually ready
                if (selectedCreature >= 0 && selectedParticipant != null && !IsTeamManualReady(isTeamA))
                {
                    if (leftPressed && selectedParticipant.creature.spells.Count > 0)
                    {
                        selectedSpell = (selectedSpell - 1 + selectedParticipant.creature.spells.Count) % selectedParticipant.creature.spells.Count;
                    }
                    else if (rightPressed && selectedParticipant.creature.spells.Count > 0)
                    {
                        selectedSpell = (selectedSpell + 1) % selectedParticipant.creature.spells.Count;
                    }
                }
                
                // Visibility toggle (Q/U)
                if (qPressed)
                {
                    teamVisible = !teamVisible;
                    UpdateVisibility();
                }
                
                // Enter selection mode (E/O) - confirm creature or edit confirmed move
                // Only works for creatures, not Ready button
                // BLOCKED if team is manually ready
                if (ePressed && selectedCreature >= 0 && selectedParticipant != null && selectedParticipant.creature.spells.Count > 0 && !IsTeamManualReady(isTeamA))
                {
                    mode = ControlMode.Selection;
                    currentParticipant = selectedParticipant;
                    
                    // Reset target moved flag when entering Selection mode
                    if (team == battleState.teamA)
                        teamATargetMoved = false;
                    else
                        teamBTargetMoved = false;
                    
                    // If creature already has confirmed move, set current spell selection to that spell
                    if (selectedParticipant.hasConfirmedMove && selectedParticipant.selectedSpell != null)
                    {
                        for (int i = 0; i < selectedParticipant.creature.spells.Count; i++)
                        {
                            if (selectedParticipant.creature.spells[i] == selectedParticipant.selectedSpell)
                            {
                                selectedSpell = i;
                                break;
                            }
                        }
                        
                        // Restore previous target selection if exists
                        if (selectedParticipant.selectedTarget != null)
                        {
                            var spellForTargeting = selectedParticipant.selectedSpell;
                            var potentialTargets = GetPotentialTargets(selectedParticipant, spellForTargeting);
                            var previousTarget = selectedParticipant.selectedTarget;
                            
                            for (int i = 0; i < potentialTargets.Count; i++)
                            {
                                if (potentialTargets[i] == previousTarget)
                                {
                                    selectedTarget = i;
                                    break;
                                }
                            }
                        }
                        else
                        {
                            selectedTarget = 0; // Default to first target
                        }
                    }
                    else
                    {
                        selectedTarget = 0; // Reset target selection for new moves
                    }
                    
                    // Show initial target indicator for target[0] if spell needs targeting
                    var currentSpellObj = currentParticipant.creature.spells[selectedSpell];
                    if (NeedsTargetSelection(currentSpellObj))
                    {
                        var potentialTargets = GetPotentialTargets(currentParticipant, currentSpellObj);
                        if (potentialTargets.Count > 0)
                        {
                            // Force show target indicator immediately for target[0]
                            if (team == battleState.teamA)
                                teamATargetMoved = true;
                            else
                                teamBTargetMoved = true;
                            UpdateTargetIndicators(potentialTargets, selectedTarget);
                        }
                    }
                }
                
                // Ready button logic (when selectedCreature == -1)
                if (selectedCreature == -1)
                {
                    // Handle hold E/O for Ready button
                    if (eHeld)
                    {
                        if (isTeamA)
                        {
                            if (!teamAIsHolding)
                            {
                                teamAIsHolding = true;
                                teamAHoldTime = 0f;
                            }
                        }
                        else
                        {
                            if (!teamBIsHolding)
                            {
                                teamBIsHolding = true;
                                teamBHoldTime = 0f;
                            }
                        }
                    }
                    else
                    {
                        // Released E/O - reset hold
                        if (isTeamA)
                        {
                            teamAIsHolding = false;
                            teamAHoldTime = 0f;
                        }
                        else
                        {
                            teamBIsHolding = false;
                            teamBHoldTime = 0f;
                        }
                    }
                }
            }
            else if (mode == ControlMode.Selection)
            {
                // Selection mode: W/S = target, A/D = spell, Q = cancel, E = confirm spell
                
                if (currentParticipant != null && currentParticipant.creature.spells.Count > 0)
                {
                    var selectedSpellObj = currentParticipant.creature.spells[selectedSpell];
                    
                    // Target selection (W/S or I/K) - only for spells that need targeting
                    // BLOCKED if team is manually ready
                    if ((upPressed || downPressed) && !IsTeamManualReady(isTeamA))
                    {
                        if (NeedsTargetSelection(selectedSpellObj))
                        {
                            var potentialTargets = GetPotentialTargets(currentParticipant, selectedSpellObj);
                            if (potentialTargets.Count > 0)
                            {
                                // Reversed: UP increases target index, DOWN decreases
                                if (upPressed)
                                    selectedTarget = (selectedTarget + 1) % potentialTargets.Count;
                                else
                                    selectedTarget = (selectedTarget - 1 + potentialTargets.Count) % potentialTargets.Count;
                                
                                // Mark that user moved target
                                if (team == battleState.teamA)
                                    teamATargetMoved = true;
                                else
                                    teamBTargetMoved = true;
                                
                                // Only show target indicators if user has moved
                                if ((team == battleState.teamA && teamATargetMoved) || 
                                    (team == battleState.teamB && teamBTargetMoved))
                                {
                                    UpdateTargetIndicators(potentialTargets, selectedTarget);
                                }
                            }
                        }
                    }
                    
                    // Spell selection (A/D or J/L) - still available in selection mode
                    // BLOCKED if team is manually ready
                    if (leftPressed && !IsTeamManualReady(isTeamA))
                    {
                        int oldSpell = selectedSpell;
                        selectedSpell = (selectedSpell - 1 + currentParticipant.creature.spells.Count) % currentParticipant.creature.spells.Count;
                        HandleSpellChange(currentParticipant, oldSpell, selectedSpell, ref selectedTarget, team);
                    }
                    else if (rightPressed && !IsTeamManualReady(isTeamA))
                    {
                        int oldSpell = selectedSpell;
                        selectedSpell = (selectedSpell + 1) % currentParticipant.creature.spells.Count;
                        HandleSpellChange(currentParticipant, oldSpell, selectedSpell, ref selectedTarget, team);
                    }
                    
                    // Cancel selection mode (Q/U)
                    if (qPressed)
                    {
                        mode = ControlMode.Navigation;
                        currentParticipant = null;
                        ClearTargetIndicators();
                    }
                    
                    // Confirm spell (E/O)
                    // BLOCKED if team is manually ready
                    if (ePressed && !IsTeamManualReady(isTeamA))
                    {
                        BattleParticipant targetParticipant = null;
                        
                        if (NeedsTargetSelection(selectedSpellObj))
                        {
                            var potentialTargets = GetPotentialTargets(currentParticipant, selectedSpellObj);
                            if (potentialTargets.Count > 0 && selectedTarget < potentialTargets.Count)
                            {
                                targetParticipant = potentialTargets[selectedTarget];
                            }
                        }
                        
                        ConfirmSpellSelection(currentParticipant, selectedSpellObj, targetParticipant);
                        mode = ControlMode.Navigation;
                        currentParticipant = null;
                        ClearTargetIndicators();
                    }
                }
            }
        }
        
        private void HandleSpellChange(BattleParticipant participant, int oldSpellIndex, int newSpellIndex, ref int selectedTarget, List<BattleParticipant> team)
        {
            var oldSpell = participant.creature.spells[oldSpellIndex];
            var newSpell = participant.creature.spells[newSpellIndex];
            
            bool oldNeedsTarget = NeedsTargetSelection(oldSpell);
            bool newNeedsTarget = NeedsTargetSelection(newSpell);
            
            // If new spell doesn't need target, reset
            if (!newNeedsTarget)
            {
                selectedTarget = 0;
            }
            // If both spells need target, try to preserve target if valid
            else if (oldNeedsTarget && newNeedsTarget)
            {
                var oldTargets = GetPotentialTargets(participant, oldSpell);
                var newTargets = GetPotentialTargets(participant, newSpell);
                
                // Check if current target is still valid for new spell
                if (selectedTarget < oldTargets.Count && selectedTarget < newTargets.Count)
                {
                    var currentTarget = oldTargets[selectedTarget];
                    
                    // Check if this target is also valid for new spell
                    bool targetStillValid = newTargets.Contains(currentTarget);
                    
                    if (targetStillValid)
                    {
                        // Find new index of same target
                        for (int i = 0; i < newTargets.Count; i++)
                        {
                            if (newTargets[i] == currentTarget)
                            {
                                selectedTarget = i;
                                return; // Target preserved
                            }
                        }
                    }
                }
                
                // Target not valid anymore, reset
                selectedTarget = 0;
            }
            else
            {
                // Old didn't need target, new does - start fresh
                selectedTarget = 0;
            }
            
            // Reset target moved flag when changing spell
            if (team == battleState.teamA)
                teamATargetMoved = false;
            else
                teamBTargetMoved = false;
        }
        
        private bool NeedsTargetSelection(Spell spell)
        {
            // Check if any effect targets Enemy or Ally (need specific target)
            foreach (var effect in spell.effects)
            {
                if (effect.targetType == SpellTargetType.Enemy || effect.targetType == SpellTargetType.Ally)
                    return true;
            }
            return false;
        }
        

        
        private List<BattleParticipant> GetPotentialTargets(BattleParticipant caster, Spell spell)
        {
            var targets = new List<BattleParticipant>();
            
            bool isCasterInTeamA = battleState.teamA.Contains(caster);
            var ownTeam = isCasterInTeamA ? battleState.teamA : battleState.teamB;
            var enemyTeam = isCasterInTeamA ? battleState.teamB : battleState.teamA;
            
            foreach (var effect in spell.effects)
            {
                switch (effect.targetType)
                {
                    case SpellTargetType.Enemy:
                        targets.AddRange(enemyTeam.Where(p => p.IsAlive));
                        break;
                    case SpellTargetType.Ally:
                        targets.AddRange(ownTeam.Where(p => p.IsAlive && p != caster));
                        break;
                }
            }
            
            return targets.Distinct().ToList();
        }
        
        private void UpdateTargetIndicators(List<BattleParticipant> targets, int selectedIndex)
        {
            // Clear all indicators first
            ClearTargetIndicators();
            
            // Show indicator on selected target
            if (targets.Count > 0 && selectedIndex < targets.Count)
            {
                var selectedTarget = targets[selectedIndex];
                // Notify external system (BattleMoveSelectionUI will handle BattleArena communication)
                OnTargetIndicatorChanged?.Invoke(selectedTarget, true);
            }
        }
        
        private void ClearTargetIndicators()
        {
            // Notify external system to clear all target indicators
            OnTargetIndicatorChanged?.Invoke(null, false);
        }
        
        private void ConfirmSpellSelection(BattleParticipant caster, Spell spell, BattleParticipant target)
        {
            caster.selectedSpell = spell;
            caster.selectedTarget = target; // Remember selected target for editing
            caster.hasConfirmedMove = true;
            
            ClearTargetIndicators();
            
            OnSpellSelected?.Invoke(caster, spell, target);
            
            // Move to next unconfirmed creature
            MoveToNextUnconfirmedCreature(caster);
        }
        
        private void MoveToNextUnconfirmedCreature(BattleParticipant confirmedParticipant)
        {
            bool isCasterInTeamA = battleState.teamA.Contains(confirmedParticipant);
            var team = isCasterInTeamA ? battleState.teamA : battleState.teamB;
            var aliveMembers = team.FindAll(p => p.IsAlive);
            
            ref int selectedCreature = ref (isCasterInTeamA ? ref teamASelectedCreature : ref teamBSelectedCreature);
            ref int selectedSpell = ref (isCasterInTeamA ? ref teamASelectedSpell : ref teamBSelectedSpell);
            
            for (int i = 0; i < aliveMembers.Count; i++)
            {
                int nextIdx = (selectedCreature + i + 1) % aliveMembers.Count;
                if (!aliveMembers[nextIdx].hasConfirmedMove)
                {
                    selectedCreature = nextIdx;
                    selectedSpell = 0;
                    break;
                }
            }
        }
        
        private void UpdateVisibility()
        {
            if (teamASelectionContainer != null)
                teamASelectionContainer.gameObject.SetActive(teamAVisible);
                
            if (teamBSelectionContainer != null)
                teamBSelectionContainer.gameObject.SetActive(teamBVisible);
                
            if (teamAVisibilityToggle != null)
                teamAVisibilityToggle.SetActive(!teamAVisible);
                
            if (teamBVisibilityToggle != null)
                teamBVisibilityToggle.SetActive(!teamBVisible);
                
            OnVisibilityChanged?.Invoke(teamAVisible, teamBVisible);
        }
        
        public void UpdateUI()
        {
            if (battleState == null) return;
            
            UpdateRowStates();
            
            // Check for auto-start when both teams are ready
            if (!isBattleStarted && battleState.AreBothTeamsReady)
            {
                StartBattleSequence();
            }
        }
        
        private void StartBattleSequence()
        {
            isBattleStarted = true;
            
            // Trigger battle start event - let BattleMoveSelectionUI handle queue initialization
            OnSpellSelected?.Invoke(null, null, null); // Signal battle start with null parameters
        }
        
        private void UpdateRowStates()
        {
            // Update visual states of all creature rows
            for (int i = 0; i < teamARows.Count; i++)
            {
                if (teamARows[i] != null)
                {
                    // Refresh HP data first
                    teamARows[i].RefreshData();
                    
                    bool isSelected = (i == teamASelectedCreature && teamAVisible);
                    bool inSelectionMode = (teamAMode == ControlMode.Selection && teamACurrentParticipant != null);
                    teamARows[i].SetSelected(isSelected, teamASelectedSpell, inSelectionMode && isSelected);
                }
            }
            
            for (int i = 0; i < teamBRows.Count; i++)
            {
                if (teamBRows[i] != null)
                {
                    // Refresh HP data first
                    teamBRows[i].RefreshData();
                    
                    bool isSelected = (i == teamBSelectedCreature && teamBVisible);
                    bool inSelectionMode = (teamBMode == ControlMode.Selection && teamBCurrentParticipant != null);
                    teamBRows[i].SetSelected(isSelected, teamBSelectedSpell, inSelectionMode && isSelected);
                }
            }
            
            // Update team ready indicators
            UpdateTeamReadyIndicators();
        }
        
        private void UpdateTeamReadyIndicators()
        {
            // This function is now handled by UpdateHoldProgress()
            // Team ready indicators are updated there with the new naming scheme
        }
        
        public void ResetReadyStates()
        {
            // Reset battle started flag - CRITICAL for input unfreeze!
            isBattleStarted = false;
            
            // Reset team ready visual states
            if (teamABackground != null)
                teamABackground.color = teamNotReadyColor;
            if (teamAProgress != null)
                teamAProgress.fillAmount = 0f;
                
            if (teamBBackground != null)
                teamBBackground.color = teamNotReadyColor;
            if (teamBProgress != null)
                teamBProgress.fillAmount = 0f;
                
            // Reset selection states to navigation mode
            teamAMode = ControlMode.Navigation;
            teamBMode = ControlMode.Navigation;
            teamASelectedCreature = 0;
            teamBSelectedCreature = 0;
        }
        
        private void AutoConfirmSkipTurns(List<BattleParticipant> team)
        {
            foreach (var participant in team)
            {
                // If creature is alive but has no selected spell, mark as "confirmed skip turn"
                if (participant.IsAlive && participant.selectedSpell == null && !participant.hasConfirmedMove)
                {
                    participant.hasConfirmedMove = true;
                    Debug.Log($"[BattleSelectionUI] Auto-confirmed skip turn for {participant.creature.name}");
                }
            }
        }
        
        private bool IsTeamManualReady(bool isTeamA)
        {
            return isTeamA ? battleState.teamAManualReady : battleState.teamBManualReady;
        }
        
        void OnDestroy()
        {
            ClearUI();
        }
    }
}