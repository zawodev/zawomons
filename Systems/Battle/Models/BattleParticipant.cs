using UnityEngine;
using Systems.Creatures.Models;

namespace Systems.Battle.Models {
    [System.Serializable]
    public class BattleParticipant {
        public Creature creature;
        public Spell selectedSpell;
        public int currentHP;
        public int initiativeBonus = 0;
        public bool hasConfirmedMove = false;
        public BattleParticipant selectedTarget; // Remember last selected target
        
        public BattleParticipant(Creature creature) {
            this.creature = creature;
            this.currentHP = creature.maxHP;
        }
        
        public bool IsAlive => currentHP > 0;
        
        public int TotalInitiative => creature.initiative + initiativeBonus;
        
        public void ResetMoveSelection() {
            selectedSpell = null;
            selectedTarget = null;
            hasConfirmedMove = false;
        }
    }
}
