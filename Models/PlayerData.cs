using System.Collections.Generic;

namespace Models
{
    [System.Serializable]
    public class PlayerData
    {
        public int id;
        public string username;
        public string name;
        public List<Creature> creatures = new List<Creature>();
        public int gold;
        public int wood;
        public int stone;
        public int gems;
        public string lastPlayed;
        public string createdAt;

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