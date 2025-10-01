using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using Systems.Battle.Models;
using Systems.Creatures.Models;

namespace Systems.Battle.UI
{
    public class CreatureSelectionRow : MonoBehaviour
    {
        [Header("UI Components")]
        public Image creatureIcon;
        public TextMeshProUGUI creatureNameText;
        public TextMeshProUGUI creatureHPText;
        public Transform spellsContainer;
        public GameObject spellSlotPrefab;
        
        [Header("Indicators")]
        public Image selectIndicator; // Shows if this row is currently selected (WS navigation)
        public Image readyIndicator; // Shows if this creature has confirmed move
        
        [Header("Visual States")]
        public Color normalColor = Color.white;
        public Color selectedColor = Color.yellow;
        public Color confirmedColor = Color.green;
        public Color deadColor = Color.gray;
        
        [Header("Indicator Colors")]
        public Color selectIndicatorActiveColor = Color.yellow;
        public Color selectIndicatorInactiveColor = Color.gray;
        public Color readyIndicatorReadyColor = Color.green;
        public Color readyIndicatorSelectionColor = new Color(1f, 0.5f, 0f); // Orange
        public Color readyIndicatorInactiveColor = Color.gray;
        
        // Internal state
        private BattleParticipant participant;
        private List<GameObject> spellSlots = new List<GameObject>();
        private bool isSelected = false;
        private bool isInSelectionMode = false; // True when actively in Selection control mode
        private int selectedSpellIndex = 0;
        
        public void Initialize(BattleParticipant battleParticipant)
        {
            participant = battleParticipant;
            BuildUI();
        }
        
        private void BuildUI()
        {
            if (participant?.creature == null) return;
            
            // Set creature info
            if (creatureNameText != null)
                creatureNameText.text = participant.creature.name;
                
            if (creatureHPText != null)
                creatureHPText.text = $"{participant.currentHP}/{participant.creature.maxHP}";
                
            if (creatureIcon != null)
            {
                // For now use creature color, later replace with actual icon/sprite
                creatureIcon.color = participant.creature.color;
            }
            
            // Build spell slots
            BuildSpellSlots();
        }
        
        private void BuildSpellSlots()
        {
            ClearSpellSlots();
            
            if (spellsContainer == null || spellSlotPrefab == null) return;
            
            foreach (var spell in participant.creature.spells)
            {
                GameObject slotObj = Instantiate(spellSlotPrefab, spellsContainer);
                SpellSlot spellSlot = slotObj.GetComponent<SpellSlot>();
                
                if (spellSlot != null)
                {
                    spellSlot.Initialize(spell);
                    spellSlots.Add(slotObj);
                }
            }
        }
        
        private void ClearSpellSlots()
        {
            foreach (var slot in spellSlots)
            {
                if (slot != null)
                    Destroy(slot);
            }
            spellSlots.Clear();
        }
        
        public void SetSelected(bool selected, int spellIndex = 0, bool inSelectionMode = false)
        {
            isSelected = selected;
            isInSelectionMode = inSelectionMode;
            selectedSpellIndex = spellIndex;
            UpdateVisualState();
        }
        
        public void SetSelectIndicator(bool active)
        {
            if (selectIndicator != null)
            {
                selectIndicator.color = active ? selectIndicatorActiveColor : selectIndicatorInactiveColor;
            }
        }
        
        public enum ReadyState { NotReady, InSelection, Ready }
        
        public void SetReadyIndicator(ReadyState state)
        {
            if (readyIndicator != null)
            {
                switch (state)
                {
                    case ReadyState.NotReady:
                        readyIndicator.color = readyIndicatorInactiveColor;
                        break;
                    case ReadyState.InSelection:
                        readyIndicator.color = readyIndicatorSelectionColor;
                        break;
                    case ReadyState.Ready:
                        readyIndicator.color = readyIndicatorReadyColor;
                        break;
                }
            }
        }
        
        private void UpdateVisualState()
        {
            Color targetColor = normalColor;
            
            if (!participant.IsAlive)
            {
                targetColor = deadColor;
            }
            else if (participant.hasConfirmedMove)
            {
                targetColor = confirmedColor;
            }
            else if (isSelected)
            {
                targetColor = selectedColor;
            }
            
            // Apply color to creature name
            if (creatureNameText != null)
                creatureNameText.color = targetColor;
                
            // Update indicators
            SetSelectIndicator(isSelected && participant.IsAlive);
            
            // Determine ready state - Orange (InSelection) has priority over Green (Ready)
            ReadyState readyState = ReadyState.NotReady;
            if (participant.IsAlive)
            {
                if (isSelected && isInSelectionMode)
                {
                    readyState = ReadyState.InSelection; // Orange - highest priority when actively editing
                }
                else if (participant.hasConfirmedMove)
                {
                    readyState = ReadyState.Ready; // Green - only when not actively editing
                }
            }
            SetReadyIndicator(readyState);
                
            // Update spell slots selection
            for (int i = 0; i < spellSlots.Count; i++)
            {
                SpellSlot spellSlot = spellSlots[i].GetComponent<SpellSlot>();
                if (spellSlot != null)
                {
                    // Show spell selection when selected AND in Selection mode (even if ready)
                    bool isSpellSelected = isSelected && i == selectedSpellIndex && isInSelectionMode;
                    spellSlot.SetSelected(isSpellSelected);
                }
            }
            
            // Update HP text color
            if (creatureHPText != null)
            {
                float healthPercentage = (float)participant.currentHP / participant.creature.maxHP;
                if (healthPercentage > 0.6f)
                    creatureHPText.color = Color.green;
                else if (healthPercentage > 0.3f)
                    creatureHPText.color = Color.yellow;
                else
                    creatureHPText.color = Color.red;
            }
        }
        
        public void RefreshData()
        {
            if (participant?.creature == null) return;
            
            // Update HP display
            if (creatureHPText != null)
                creatureHPText.text = $"{participant.currentHP}/{participant.creature.maxHP}";
                
            UpdateVisualState();
        }
        
        public BattleParticipant GetParticipant() => participant;
    }
}