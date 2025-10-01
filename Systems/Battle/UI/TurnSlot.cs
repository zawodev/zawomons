using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Systems.Battle.Models;

namespace Systems.Battle.UI
{
    public class TurnSlot : MonoBehaviour
    {
        [Header("UI Components")]
        public Image creatureIcon;
        public Image spellIcon;
        public TextMeshProUGUI creatureNameText;
        public TextMeshProUGUI spellNameText;
        public Image backgroundImage;
        public CanvasGroup canvasGroup;
        
        [Header("Visual States")]
        public Color upcomingBackgroundColor = new Color(0.3f, 0.3f, 0.3f, 0.8f);
        public Color currentBackgroundColor = new Color(0.8f, 0.8f, 0.2f, 0.9f);
        public Color completedBackgroundColor = new Color(0.1f, 0.1f, 0.1f, 0.6f);
        
        [Header("Animation")]
        public float pulseSpeed = 2f;
        public float pulseIntensity = 0.3f;
        
        // Internal state
        private BattleParticipant participant;
        private TurnSlotState currentState = TurnSlotState.Upcoming;
        private int queueIndex;
        private bool isPulsing = false;
        
        public void Initialize(BattleParticipant battleParticipant, int index)
        {
            participant = battleParticipant;
            queueIndex = index;
            BuildUI();
        }
        
        private void BuildUI()
        {
            if (participant?.creature == null) return;
            
            // Set creature info
            if (creatureNameText != null)
                creatureNameText.text = participant.creature.name;
                
            if (creatureIcon != null)
            {
                // For now use creature color, later replace with actual icon
                creatureIcon.color = participant.creature.color;
            }
            
            // Set spell info
            if (participant.selectedSpell != null)
            {
                if (spellNameText != null)
                    spellNameText.text = participant.selectedSpell.name;
                    
                if (spellIcon != null)
                {
                    // Color based on spell effect type
                    spellIcon.color = GetSpellColor(participant.selectedSpell);
                }
            }
        }
        
        private Color GetSpellColor(Systems.Creatures.Models.Spell spell)
        {
            if (spell?.effects == null || spell.effects.Count == 0)
                return Color.gray;
                
            var primaryEffect = spell.effects[0];
            
            switch (primaryEffect.effectType)
            {
                case Systems.Creatures.Models.SpellEffectType.Damage:
                    return Color.red;
                case Systems.Creatures.Models.SpellEffectType.Heal:
                    return Color.green;
                case Systems.Creatures.Models.SpellEffectType.BuffInitiative:
                    return Color.blue;
                case Systems.Creatures.Models.SpellEffectType.BuffDamage:
                    return Color.magenta;
                default:
                    return Color.white;
            }
        }
        
        public void SetState(TurnSlotState state)
        {
            currentState = state;
            UpdateVisualState();
        }
        
        private void UpdateVisualState()
        {
            Color backgroundColor = upcomingBackgroundColor;
            isPulsing = false;
            
            switch (currentState)
            {
                case TurnSlotState.Upcoming:
                    backgroundColor = upcomingBackgroundColor;
                    break;
                    
                case TurnSlotState.Current:
                    backgroundColor = currentBackgroundColor;
                    isPulsing = true;
                    break;
                    
                case TurnSlotState.Completed:
                    backgroundColor = completedBackgroundColor;
                    break;
            }
            
            if (backgroundImage != null)
                backgroundImage.color = backgroundColor;
                
            // Update text colors based on state
            Color textColor = currentState == TurnSlotState.Completed ? Color.gray : Color.white;
            if (creatureNameText != null)
                creatureNameText.color = textColor;
            if (spellNameText != null)
                spellNameText.color = textColor;
        }
        
        void Update()
        {
            if (isPulsing && backgroundImage != null)
            {
                // Pulse effect for current turn
                float pulse = Mathf.Sin(Time.time * pulseSpeed) * pulseIntensity;
                Color baseColor = currentBackgroundColor;
                backgroundImage.color = new Color(
                    baseColor.r + pulse,
                    baseColor.g + pulse,
                    baseColor.b + pulse,
                    baseColor.a
                );
            }
        }
        
        public void SetAlpha(float alpha)
        {
            if (canvasGroup != null)
                canvasGroup.alpha = alpha;
        }
        
        public BattleParticipant GetParticipant() => participant;
        public int GetQueueIndex() => queueIndex;
    }
}