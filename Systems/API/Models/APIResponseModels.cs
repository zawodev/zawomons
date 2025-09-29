using System;

namespace Systems.API.Models
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
        public int experience;
        public bool can_claim_creature;
        public CreatureDataResponse[] creatures;
        public CityDataResponse[] cities;
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

    // REQUEST MODELS - dla wysyłania danych do API
    [System.Serializable]
    public class UpdateCreatureRequest
    {
        public string name;
    }

    // PUBLIC SPELL DATA - Response model (for /spells/ endpoint)
    [System.Serializable]
    public class PublicSpellDataResponse
    {
        public int spell_id;
        public string name;
        public string description;
        public string spell_img;
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

    // GENERIC ARRAY WRAPPER for Unity JsonUtility
    [System.Serializable]
    public class ArrayWrapper<T>
    {
        public T[] items;
    }

    // CITY DATA - Response models
    [System.Serializable]
    public class CityDataResponse
    {
        public int id;
        public string name;
        public float pos_x;
        public float pos_y;
        public int level;
        public string created_at;
    }

    // TASKS DATA - Response models
    [System.Serializable]
    public class TasksResponse
    {
        public CreatureTaskResponse[] creature_tasks;
        public SpellLearningTaskResponse[] spell_learning_tasks;
        public TravelTaskResponse[] travel_tasks;
    }

    [System.Serializable]
    public class CreatureTaskResponse
    {
        public int creature_id;
        public string task_type;
        public string start_time;
        public string end_time;
        public float progress;
    }

    [System.Serializable]
    public class SpellLearningTaskResponse
    {
        public int creature_id;
        public int spell_id;
        public string start_time;
        public string end_time;
        public float progress;
    }

    [System.Serializable]
    public class TravelTaskResponse
    {
        public int creature_id;
        public float start_x;
        public float start_y;
        public float destination_x;
        public float destination_y;
        public string start_time;
        public string end_time;
        public float progress;
    }

    // CREATURE PROGRESS - Response models
    [System.Serializable]
    public class CreatureProgressResponse
    {
        public float spell_learning_progress;
        public float travel_progress;
    }
}
