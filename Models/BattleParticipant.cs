using UnityEngine;

namespace Models
{
    [System.Serializable]
    public class BattleParticipant
    {
        public Zawomon zawomon;
        public Spell selectedSpell;
        public bool isVisible = true;
        public int currentHP;
        public int initiativeBonus = 0;
        public bool hasConfirmedMove = false;
        
        public BattleParticipant(Zawomon zawomon)
        {
            this.zawomon = zawomon;
            this.currentHP = zawomon.MaxHP;
        }
        
        public bool IsAlive => currentHP > 0;
        
        public int TotalInitiative => zawomon.Initiative + initiativeBonus;
        
        public void ResetMoveSelection()
        {
            selectedSpell = null;
            hasConfirmedMove = false;
        }
    }
}
