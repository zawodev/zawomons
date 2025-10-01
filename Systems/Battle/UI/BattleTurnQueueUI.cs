using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;
using Systems.Battle.Models;
using Systems.Creatures.Models;

namespace Systems.Battle.UI
{
    public class BattleTurnQueueUI : MonoBehaviour
    {
        [Header("Queue UI")]
        public Transform queueContainer;
        public GameObject turnSlotPrefab;
        public ScrollRect scrollRect;
        
        [Header("Animation Settings")]
        public float slideAnimationDuration = 0.5f;
        public AnimationCurve slideAnimationCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
        
        private float GetEffectiveAnimationDuration(float baseDuration)
        {
            // Try to find BattleSystem to check acceleration flag
            var battleSystem = FindFirstObjectByType<Systems.Battle.Core.BattleSystem>();
            if (battleSystem != null && battleSystem.IsAcceleratingAnimations)
            {
                return 0.01f; // Very fast duration when accelerated
            }
            return baseDuration; // Normal duration
        }
        
        [Header("Current Turn Indicator")]
        public GameObject currentTurnIndicator;
        public Color currentTurnColor = Color.yellow;
        public Color upcomingTurnColor = Color.white;
        public Color completedTurnColor = Color.gray;
        
        // Internal state
        private List<TurnSlot> turnSlots = new List<TurnSlot>();
        private List<BattleParticipant> currentQueue = new List<BattleParticipant>();
        private int currentTurnIndex = 0;
        private bool isAnimating = false;
        
        // Events
        public System.Action OnQueueAnimationComplete;
        
        public void InitializeQueue(BattleState battleState)
        {
            if (battleState == null) return;
            
            BuildTurnQueue(battleState);
            UpdateQueueVisuals();
        }
        
        private void BuildTurnQueue(BattleState battleState)
        {
            // Get all alive participants with confirmed moves
            var allParticipants = battleState.teamA.Concat(battleState.teamB)
                .Where(p => p.IsAlive && p.selectedSpell != null && p.hasConfirmedMove)
                .ToList();
            
            // Sort by initiative (same as BattleSystem logic)
            currentQueue = allParticipants
                .OrderByDescending(p => p.TotalInitiative)
                .ThenByDescending(p => p.creature.level)
                .ThenByDescending(p => p.creature.experience)
                .ThenBy(p => p.creature.name)
                .ToList();
            
            currentTurnIndex = 0;
            CreateQueueSlots();
        }
        
        private void CreateQueueSlots()
        {
            ClearQueue();
            
            if (queueContainer == null || turnSlotPrefab == null) return;
            
            for (int i = 0; i < currentQueue.Count; i++)
            {
                var participant = currentQueue[i];
                GameObject slotObj = Instantiate(turnSlotPrefab, queueContainer);
                TurnSlot turnSlot = slotObj.GetComponent<TurnSlot>();
                
                if (turnSlot != null)
                {
                    turnSlot.Initialize(participant, i);
                    turnSlots.Add(turnSlot);
                }
            }
        }
        
        private void ClearQueue()
        {
            foreach (var slot in turnSlots)
            {
                if (slot != null && slot.gameObject != null)
                    Destroy(slot.gameObject);
            }
            turnSlots.Clear();
        }
        
        public void UpdateQueueVisuals()
        {
            for (int i = 0; i < turnSlots.Count; i++)
            {
                if (turnSlots[i] != null)
                {
                    TurnSlotState state = TurnSlotState.Upcoming;
                    
                    if (i < currentTurnIndex)
                        state = TurnSlotState.Completed;
                    else if (i == currentTurnIndex)
                        state = TurnSlotState.Current;
                    
                    turnSlots[i].SetState(state);
                }
            }
            
            // Position current turn indicator
            if (currentTurnIndicator != null && currentTurnIndex < turnSlots.Count)
            {
                var currentSlot = turnSlots[currentTurnIndex];
                if (currentSlot != null)
                {
                    currentTurnIndicator.transform.position = currentSlot.transform.position;
                    currentTurnIndicator.SetActive(true);
                }
            }
        }
        
        public void AdvanceQueue()
        {
            if (isAnimating) return;
            
            currentTurnIndex++;
            
            if (currentTurnIndex < turnSlots.Count)
            {
                AnimateQueueAdvance();
            }
            else
            {
                // Queue completed - hide indicator
                if (currentTurnIndicator != null)
                    currentTurnIndicator.SetActive(false);
                    
                OnQueueAnimationComplete?.Invoke();
            }
        }
        
        private void AnimateQueueAdvance()
        {
            isAnimating = true;
            
            // Animate removal of completed turn
            if (currentTurnIndex > 0 && turnSlots.Count > currentTurnIndex - 1)
            {
                var completedSlot = turnSlots[currentTurnIndex - 1];
                if (completedSlot != null)
                {
                    // Slide out animation
                    StartCoroutine(AnimateSlideOut(completedSlot));
                }
            }
            
            UpdateQueueVisuals();
            
            // Auto-scroll to current turn
            ScrollToCurrentTurn();
            
            // Mark animation complete after duration
            float effectiveDuration = GetEffectiveAnimationDuration(slideAnimationDuration);
            Invoke(nameof(CompleteAnimation), effectiveDuration);
        }
        
        private System.Collections.IEnumerator AnimateSlideOut(TurnSlot slot)
        {
            Vector3 startPos = slot.transform.localPosition;
            Vector3 endPos = startPos + Vector3.left * 200f; // Slide left off screen
            
            float effectiveDuration = GetEffectiveAnimationDuration(slideAnimationDuration);
            float elapsed = 0f;
            while (elapsed < effectiveDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / effectiveDuration;
                float curveValue = slideAnimationCurve.Evaluate(progress);
                
                slot.transform.localPosition = Vector3.Lerp(startPos, endPos, curveValue);
                slot.SetAlpha(1f - curveValue); // Fade out
                
                yield return null;
            }
            
            // Hide the slot completely
            slot.gameObject.SetActive(false);
        }
        
        private void ScrollToCurrentTurn()
        {
            if (scrollRect == null || currentTurnIndex >= turnSlots.Count) return;
            
            var currentSlot = turnSlots[currentTurnIndex];
            if (currentSlot != null)
            {
                // Calculate target scroll position to center current turn
                float targetScrollPos = (float)currentTurnIndex / (turnSlots.Count - 1);
                targetScrollPos = Mathf.Clamp01(targetScrollPos);
                
                // Animate scroll position
                StartCoroutine(AnimateScrollPosition(scrollRect.horizontalNormalizedPosition, targetScrollPos));
            }
        }
        
        private System.Collections.IEnumerator AnimateScrollPosition(float startPos, float endPos)
        {
            float effectiveDuration = GetEffectiveAnimationDuration(slideAnimationDuration * 0.5f); // Faster scroll animation
            float elapsed = 0f;
            while (elapsed < effectiveDuration)
            {
                elapsed += Time.deltaTime;
                float progress = elapsed / effectiveDuration;
                
                scrollRect.horizontalNormalizedPosition = Mathf.Lerp(startPos, endPos, progress);
                
                yield return null;
            }
            
            scrollRect.horizontalNormalizedPosition = endPos;
        }
        
        private void CompleteAnimation()
        {
            isAnimating = false;
        }
        
        public BattleParticipant GetCurrentParticipant()
        {
            if (currentTurnIndex < currentQueue.Count)
                return currentQueue[currentTurnIndex];
            return null;
        }
        
        public bool IsQueueComplete()
        {
            return currentTurnIndex >= currentQueue.Count;
        }
        
        public void ResetQueue()
        {
            currentTurnIndex = 0;
            isAnimating = false;
            
            if (currentTurnIndicator != null)
                currentTurnIndicator.SetActive(false);
                
            ClearQueue();
        }
        
        void OnDestroy()
        {
            ClearQueue();
        }
    }
    
    public enum TurnSlotState
    {
        Upcoming,
        Current,
        Completed
    }
}