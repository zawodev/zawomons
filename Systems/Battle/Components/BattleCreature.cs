using UnityEngine;
using Systems.Battle.Models;
using Systems.Creatures.Models;

namespace Systems.Battle.Components
{
    [RequireComponent(typeof(Animator))]
    public class BattleCreature : MonoBehaviour
    {
        [Header("Visual Components")]
        public SpriteRenderer spriteRenderer;
        public Transform targetIndicator; // green arrow above creature when targeted
        
        [Header("Animation Settings")]
        public float attackAnimationDuration = 1f;
        public float buffAnimationDuration = 0.8f;
        
        // Internal state
        private Animator animator;
        private BattleParticipant participant;
        private bool isAnimating = false;
        
        // Animation triggers
        private static readonly int AttackTrigger = Animator.StringToHash("BattleAttackTrigger");
        private static readonly int BuffTrigger = Animator.StringToHash("BattleBuffTrigger");
        private static readonly int HurtTrigger = Animator.StringToHash("BattleHurtTrigger");
        private static readonly int IdleState = Animator.StringToHash("Idle");
        
        // Events
        public System.Action<BattleCreature> OnAnimationComplete;
        
        void Awake()
        {
            animator = GetComponent<Animator>();
            if (targetIndicator != null)
                targetIndicator.gameObject.SetActive(false);
        }
        
        public void Initialize(BattleParticipant battleParticipant)
        {
            participant = battleParticipant;
            
            // Set visual representation
            if (spriteRenderer != null && participant?.creature != null)
            {
                // For now, use creature color. Later replace with actual sprites
                spriteRenderer.color = participant.creature.color;
            }
            
            // Start in idle state
            // SetIdleAnimation();
        }
        
        public void SetIdleAnimation()
        {
            if (animator != null && !isAnimating)
            {
                animator.Play(IdleState, 0, 0);
            }
        }
        
        public void PlayAttackAnimation()
        {
            if (animator != null && !isAnimating)
            {
                isAnimating = true;
                animator.SetTrigger(AttackTrigger);
                
                // Check if animations should be accelerated
                float duration = GetEffectiveAnimationDuration(attackAnimationDuration);
                
                // Auto-return to idle after duration
                Invoke(nameof(ReturnToIdle), duration);
            }
        }
        
        public void PlayBuffAnimation()
        {
            if (animator != null && !isAnimating)
            {
                isAnimating = true;
                animator.SetTrigger(BuffTrigger);
                
                // Check if animations should be accelerated
                float duration = GetEffectiveAnimationDuration(buffAnimationDuration);
                
                // Auto-return to idle after duration
                Invoke(nameof(ReturnToIdle), duration);
            }
        }
        
        public void PlayHurtAnimation()
        {
            if (animator != null)
            {
                // Hurt animation can interrupt other animations
                isAnimating = true;
                animator.SetTrigger(HurtTrigger);
                
                // Check if animations should be accelerated
                float duration = GetEffectiveAnimationDuration(0.5f); // Default hurt duration
                
                // Auto-return to idle after duration
                CancelInvoke(nameof(ReturnToIdle)); // Cancel any pending idle return
                Invoke(nameof(ReturnToIdle), duration);
            }
        }
        
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
        
        private void ReturnToIdle()
        {
            isAnimating = false;
            //SetIdleAnimation();
            OnAnimationComplete?.Invoke(this);
        }
        
        public void ShowTargetIndicator(bool show)
        {
            if (targetIndicator != null)
            {
                targetIndicator.gameObject.SetActive(show);
                
                // Set high sorting order to be above all creatures
                var indicatorRenderer = targetIndicator.GetComponent<SpriteRenderer>();
                if (indicatorRenderer != null)
                {
                    indicatorRenderer.sortingOrder = 304;
                }
            }
        }
        
        public void UpdateHealthVisuals()
        {
            if (participant == null) return;
            
            // Update visual based on health status
            float healthPercentage = (float)participant.currentHP / participant.creature.maxHP;
            
            if (spriteRenderer != null)
            {
                // Darken sprite based on damage taken
                Color baseColor = participant.creature.color;
                spriteRenderer.color = Color.Lerp(Color.gray, baseColor, healthPercentage);
                
                // Hide if dead
                spriteRenderer.enabled = participant.IsAlive;
            }
        }
        
        public BattleParticipant GetParticipant() => participant;
        
        public bool IsAnimating() => isAnimating;
        
        // Called by animation events (if using animation events instead of timers)
        public void OnAttackAnimationComplete()
        {
            ReturnToIdle();
        }
        
        public void OnBuffAnimationComplete()
        {
            ReturnToIdle();
        }
    }
}