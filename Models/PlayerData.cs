using System.Collections.Generic;

namespace Models {
    [System.Serializable]
    public class PlayerData {
        public string name;
        public List<Creature> creatures = new List<Creature>();

        public void AddCreature(Creature creature) {
            creatures.Add(creature);
        }

        public void RemoveCreature(Creature creature) {
            creatures.Remove(creature);
        }
    }
}