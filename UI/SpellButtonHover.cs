using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Models;
using System.Collections.Generic;

namespace UI {
    public class SpellButtonHover : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler {
        public Spell spell;
        public Zawomon zawomon;
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
            if (zawomon.Spells.Exists(s => s.Name == spell.Name))
                reasons.Add("Zawomon już zna ten spell");
            if (zawomon.LearningSpells.Exists(ls => ls.SpellName == spell.Name))
                reasons.Add("Zawomon już uczy się tego spella");
            if (spell.RequiredClass != null && spell.RequiredClass != zawomon.MainClass && spell.RequiredClass != zawomon.SecondaryClass)
                reasons.Add($"Wymagana klasa: {spell.RequiredClass}");
            if (zawomon.Level < spell.RequiredLevel)
                reasons.Add($"Wymagany poziom: {spell.RequiredLevel}");
            if (playerGold < 10)
                reasons.Add("Za mało golda (10)");

            bool canLearn = reasons.Count == 0 && spell.RequiresLearning;

            if (canLearn) {
                return $"<b>{spell.Name}</b>\n" +
                       $"Typ: {spell.Type}\n" +
                       $"Moc: {spell.Power}\n" +
                       $"Koszt: 10 gold\n" +
                       $"Czas nauki: {spell.LearnTimeSeconds}s\n" +
                       $"Opis: {spell.Description}";
            }
            else {
                return $"<b>{spell.Name}</b>\n" +
                       $"Typ: {spell.Type}\n" +
                       $"Moc: {spell.Power}\n" +
                       $"Opis: {spell.Description}\n\n" +
                       $"<color=red>Nie można się nauczyć:</color>\n" +
                       string.Join("\n", reasons);
            }
        }
    }
} 