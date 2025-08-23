using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Models;
using System.Collections.Generic;

namespace UI {
    public class SpellButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
        public Spell spell;
        public Creature zawomon;
        public int playerGold;
        
        [Header("UI References")]
        public Image iconImage; // Referencja do ikony spella

        public void OnPointerEnter(PointerEventData eventData) {
            string tooltipText = GenerateTooltipText();
            GlobalTooltip.Instance.ShowTooltip(tooltipText);
        }

        public void OnPointerExit(PointerEventData eventData) {
            GlobalTooltip.Instance.HideTooltip();
        }

        public void SetIconColor(Color color) {
            if (iconImage != null) {
                iconImage.color = color;
                Debug.Log($"Ustawiono kolor ikony na: {color}");
            }
            else {
                Debug.LogWarning("iconImage jest null! Sprawdź czy jest podpięty w prefabie przycisku.");
            }
        }
        private string GenerateTooltipText() {
            if (spell == null) return "Błąd: brak danych spella";

            List<string> reasons = new List<string>();
            if (zawomon.spells.Exists(s => s.name == spell.name))
                reasons.Add("Zawomon już zna ten spell");
            if (zawomon.learningSpells.Exists(ls => ls.spellName == spell.name))
                reasons.Add("Zawomon już uczy się tego spella");
            if (spell.requiredClass != null && spell.requiredClass != zawomon.mainElement && spell.requiredClass != zawomon.secondaryElement)
                reasons.Add($"Wymagana klasa: {spell.requiredClass}");
            if (zawomon.level < spell.requiredLevel)
                reasons.Add($"Wymagany poziom: {spell.requiredLevel}");
            if (playerGold < 10)
                reasons.Add("Za mało golda (10)");

            bool canLearn = reasons.Count == 0 && spell.requiresLearning;

            if (canLearn) {
                return $"<b>{spell.name}</b>\n" +
                       $"Typ: {spell.type}\n" +
                       $"Moc: {spell.power}\n" +
                       $"Koszt: 10 gold\n" +
                       $"Czas nauki: {spell.learnTimeSeconds}s\n" +
                       $"Opis: {spell.description}";
            }
            else {
                return $"<b>{spell.name}</b>\n" +
                       $"Typ: {spell.type}\n" +
                       $"Moc: {spell.power}\n" +
                       $"Opis: {spell.description}\n\n" +
                       $"<color=red>Nie można się nauczyć:</color>\n" +
                       string.Join("\n", reasons);
            }
        }
    }
} 