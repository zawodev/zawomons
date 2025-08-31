using System;

namespace Systems.API
{
    // PLAYER DATA - Response models
    [System.Serializable]
    public class PlayerDataResponse
    {
        public int id;
        public string username;
        public int gold;
        public int wood;
        public int stone;
        public int gems;
        public bool can_claim_start_creature;
        public CreatureDataResponse[] creatures;
        public string last_played;
        public string created_at;
    }

    // CREATURE DATA - Response models
    [System.Serializable]
    public class CreatureDataResponse
    {
        public int id;
        public string name;
        public string main_element;
        public string secondary_element;
        public string color;
        public int experience;
        public int max_hp;
        public int current_hp;
        public int max_energy;
        public int current_energy;
        public int damage;
        public int initiative;
        public SpellDataResponse[] spells;
    }

    [System.Serializable]
    public class SpellDataResponse
    {
        public int spell_id;
        public string start_time;
        public string end_time;
        public bool is_learned;
    }

    // RESOURCES - Request models
    [System.Serializable]
    public class UpdateSingleResourceRequest
    {
        public string resource_type;
        public int value;
    }
    
    // CREATURE - Request models
    [System.Serializable]
    public class CreateCreatureRequest
    {
        public string name;
        public string main_element;
        public string secondary_element;
        public string color;
        public int experience;
        public int max_hp;
        public int current_hp;
        public int max_energy;
        public int current_energy;
        public int damage;
        public int initiative;
        public SpellRequestData[] spells;
    }

    [System.Serializable]
    public class SpellRequestData
    {
        public int spell_id;
        public string start_time;
        public string end_time;
        public bool is_learned;
    }

    [System.Serializable]
    public class UpdateCreatureRequest
    {
        public string name;
        public string main_element;
        public string secondary_element;
        public string color;
        public int experience;
        public int max_hp;
        public int current_hp;
        public int max_energy;
        public int current_energy;
        public int damage;
        public int initiative;
        public SpellRequestData[] spells;
    }

    // SOCIAL DATA - Response models
    [System.Serializable]
    public class PlayerSummaryResponse
    {
        public string username;
        public int experience;
        public int creature_count;
        public bool is_online;
    }

    [System.Serializable]
    public class PlayerSummaryResponseWrapper
    {
        public PlayerSummaryResponse[] items;
    }
}
