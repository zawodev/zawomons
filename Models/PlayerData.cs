using System.Collections.Generic;

namespace Models {
    [System.Serializable]
    public class PlayerData {
        public string Name;
        public List<Zawomon> Zawomons = new List<Zawomon>();

        public void AddZawomon(Zawomon zawomon) {
            Zawomons.Add(zawomon);
        }

        public void RemoveZawomon(Zawomon zawomon) {
            Zawomons.Remove(zawomon);
        }
    }
}