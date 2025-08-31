using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Systems.Battle.UI
{
    /// <summary>
    /// Component for creature result item prefab used in BattleResultsUI
    /// This script should be attached to the prefab that represents a single creature's battle result
    /// 
    /// Expected hierarchy structure for the prefab:
    /// CreatureResultItem (GameObject with this script)
    /// ├── CreatureImage (Image)
    /// ├── CreatureName (TextMeshPro)
    /// ├── Status (TextMeshPro)
    /// ├── HPBar (Slider)
    /// └── HPText (TextMeshPro)
    /// </summary>
    public class CreatureResultItem : MonoBehaviour
    {
        [Header("UI Components")]
        public Image creatureImage;
        public TMP_Text creatureNameText;
        public TMP_Text statusText;
        public Slider hpBar;
        public TMP_Text hpText;
        
        public void UpdateDisplay(string creatureName, bool isAlive, int currentHP, int maxHP, Color elementColor)
        {
            // Set creature name
            if (creatureNameText != null)
            {
                creatureNameText.text = creatureName;
            }
            
            // Set status
            if (statusText != null)
            {
                statusText.text = isAlive ? "✅ Alive" : "💀 Defeated";
                statusText.color = isAlive ? Color.green : Color.red;
            }
            
            // Set HP bar
            if (hpBar != null)
            {
                hpBar.maxValue = maxHP;
                hpBar.value = currentHP;
            }
            
            // Set HP text
            if (hpText != null)
            {
                hpText.text = $"{currentHP}/{maxHP}";
            }
            
            // Set creature image color (placeholder until sprites are available)
            if (creatureImage != null)
            {
                creatureImage.color = elementColor;
            }
        }
    }
}
