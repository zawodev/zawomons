using System.Collections.Generic;
using Systems.Creatures.Models;

namespace Shared.Models
{
    [System.Serializable]
    public class PlayerData
    {
        public int id;
        public string username;
        public List<Creature> creatures = new List<Creature>();
        public int gold;
        public int wood;
        public int stone;
        public int gems;
        public string lastPlayed;
        public string createdAt;
        public bool can_claim_start_creature;

        public void AddCreature(Creature creature)
        {
            creatures.Add(creature);
        }

        public void RemoveCreature(Creature creature)
        {
            creatures.Remove(creature);
        }
    }
}