using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Systems.Battle.Models;
using Systems.Battle.Components;

namespace Systems.Battle.Core
{
    public class BattleArena : MonoBehaviour
    {
        [Header("Arena Layout")]
        public Transform teamASpawnArea;
        public Transform teamBSpawnArea;
        public GameObject creaturePrefab; // Prefab with BattleCreature component
        
        [Header("Spawn Settings")]
        public float creatureSpacing = 2f;
        public Vector3 spawnOffset = Vector3.zero;
        
        // Internal state
        private List<BattleCreature> teamACreatures = new List<BattleCreature>();
        private List<BattleCreature> teamBCreatures = new List<BattleCreature>();
        private Dictionary<BattleParticipant, BattleCreature> participantToCreature = new Dictionary<BattleParticipant, BattleCreature>();
        
        // Events for animation coordination
        public System.Action<BattleCreature> OnCreatureAnimationComplete;
        
        public void SetupArena(BattleState battleState)
        {
            ClearArena();
            
            // Setup team spawn areas scaling
            if (teamASpawnArea != null)
            {
                // Team A faces right (normal scale)
                teamASpawnArea.localScale = new Vector3(1, 1, 1);
            }
            
            if (teamBSpawnArea != null)
            {
                // Team B faces left (flip X by negative scale)
                teamBSpawnArea.localScale = new Vector3(-1, 1, 1);
            }
            
            // Spawn Team A creatures (no individual flip needed)
            SpawnTeam(battleState.teamA, teamASpawnArea, teamACreatures);
            
            // Spawn Team B creatures (no individual flip needed - parent is flipped)
            SpawnTeam(battleState.teamB, teamBSpawnArea, teamBCreatures);
        }
        
        private void SpawnTeam(List<BattleParticipant> team, Transform spawnArea, List<BattleCreature> creatureList)
        {
            if (spawnArea == null || creaturePrefab == null) return;
            
            for (int i = 0; i < team.Count; i++)
            {
                var participant = team[i];
                
                // Calculate spawn position
                Vector3 spawnPos = spawnArea.position + spawnOffset;
                spawnPos.y += i * creatureSpacing; // Vertical spacing
                
                // Spawn creature
                GameObject creatureObj = Instantiate(creaturePrefab, spawnPos, Quaternion.identity, spawnArea);
                BattleCreature battleCreature = creatureObj.GetComponent<BattleCreature>();
                
                if (battleCreature != null)
                {
                    // Initialize creature
                    battleCreature.Initialize(participant);
                    battleCreature.OnAnimationComplete += OnCreatureAnimationComplete;
                    
                    // Set sorting order based on Y position (lower Y = higher order = in front)
                    if (battleCreature.spriteRenderer != null)
                    {
                        // Note: Sprite flipping is now handled by parent Transform scale
                        // Set sorting order: 302 for bottom, 301 for middle, 300 for top
                        battleCreature.spriteRenderer.sortingOrder = 302 - i;
                    }
                    
                    // Store references
                    creatureList.Add(battleCreature);
                    participantToCreature[participant] = battleCreature;
                }
            }
        }
        
        public void ClearArena()
        {
            // Clear existing creatures
            foreach (var creature in teamACreatures.Concat(teamBCreatures))
            {
                if (creature != null)
                {
                    creature.OnAnimationComplete -= OnCreatureAnimationComplete;
                    if (creature.gameObject != null)
                        Destroy(creature.gameObject);
                }
            }
            
            teamACreatures.Clear();
            teamBCreatures.Clear();
            participantToCreature.Clear();
        }
        
        public BattleCreature GetCreatureForParticipant(BattleParticipant participant)
        {
            participantToCreature.TryGetValue(participant, out BattleCreature creature);
            return creature;
        }
        
        public void UpdateAllHealthVisuals()
        {
            foreach (var creature in teamACreatures.Concat(teamBCreatures))
            {
                if (creature != null)
                    creature.UpdateHealthVisuals();
            }
        }
        
        public void ShowTargetIndicator(BattleParticipant target, bool show)
        {
            var creature = GetCreatureForParticipant(target);
            if (creature != null)
                creature.ShowTargetIndicator(show);
        }
        
        public void PlayAttackAnimation(BattleParticipant attacker)
        {
            var creature = GetCreatureForParticipant(attacker);
            if (creature != null)
                creature.PlayAttackAnimation();
        }
        
        public void PlayBuffAnimation(BattleParticipant buffer)
        {
            var creature = GetCreatureForParticipant(buffer);
            if (creature != null)
                creature.PlayBuffAnimation();
        }
        
        public bool IsAnyCreatureAnimating()
        {
            return teamACreatures.Concat(teamBCreatures).Any(c => c != null && c.IsAnimating());
        }
        
        public List<BattleCreature> GetAllCreatures()
        {
            return teamACreatures.Concat(teamBCreatures).ToList();
        }
        
        public List<BattleCreature> GetTeamACreatures() => teamACreatures;
        public List<BattleCreature> GetTeamBCreatures() => teamBCreatures;
        
        void OnDestroy()
        {
            ClearArena();
        }
    }
}