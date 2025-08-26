using System;

namespace Systems {
    // Klasy odpowiedzi API - używane przez GameAPI i testy
    [System.Serializable]
    public class PlayerDataResponse {
        public int id;
        public string username;
        public string name;
        public int gold;
        public int wood;
        public int stone;
        public int gems;
        public CreatureDataResponse[] creatures;
        public string last_played;
        public string created_at;
    }

    [System.Serializable]
    public class CreatureDataResponse {
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
    }
}
