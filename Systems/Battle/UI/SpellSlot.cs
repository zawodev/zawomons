using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Systems.Creatures.Models;

namespace Systems.Battle.UI
{
    public class SpellSlot : MonoBehaviour
    {
        [Header("UI Components")]
        public Image spellIcon;
        public TextMeshProUGUI spellNameText;
        public Image backgroundImage;
        
        [Header("Visual States")]
        public Color normalBackgroundColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        public Color selectedBackgroundColor = new Color(0.8f, 0.2f, 0.2f, 0.9f);
        public Color normalTextColor = Color.white;
        public Color selectedTextColor = Color.white;
        
        // Internal state
        private Spell spell;
        private bool isSelected = false;
        
        public void Initialize(Spell spellData)
        {
            spell = spellData;
            BuildUI();
        }
        
        private void BuildUI()
        {
            if (spell == null) return;
            
            // Set spell name
            if (spellNameText != null)
            {
                spellNameText.text = spell.name;
            }
            
            // Set spell icon (placeholder for now)
            if (spellIcon != null)
            {
                // For now, use a color based on spell effects
                Color iconColor = GetSpellColor();
                spellIcon.color = iconColor;
            }
            
            UpdateVisualState();
        }
        
        private Color GetSpellColor()
        {
            if (spell?.effects == null || spell.effects.Count == 0)
                return Color.gray;
                
            // Use color based on primary effect type
            var primaryEffect = spell.effects[0];
            
            switch (primaryEffect.effectType)
            {
                case SpellEffectType.Damage:
                    return Color.red;
                case SpellEffectType.Heal:
                    return Color.green;
                case SpellEffectType.BuffInitiative:
                    return Color.blue;
                case SpellEffectType.BuffDamage:
                    return Color.magenta;
                default:
                    return Color.white;
            }
        }
        
        public void SetSelected(bool selected)
        {
            isSelected = selected;
            UpdateVisualState();
        }
        
        private void UpdateVisualState()
        {
            // Update background color
            if (backgroundImage != null)
            {
                backgroundImage.color = isSelected ? selectedBackgroundColor : normalBackgroundColor;
            }
            
            // Update text color
            if (spellNameText != null)
            {
                spellNameText.color = isSelected ? selectedTextColor : normalTextColor;
            }
            
            // Optional: Scale effect when selected
            if (isSelected)
            {
                transform.localScale = Vector3.one * 1.1f;
            }
            else
            {
                transform.localScale = Vector3.one;
            }
        }
        
        public Spell GetSpell() => spell;
    }
}