using UnityEngine;

namespace Models {
    public static class ZawomonGenerator {
        private static string[] namePrefixes = {"Za", "Wo", "Mon", "Ka", "Lu", "Pi", "Ra", "Su"};
        private static string[] nameSuffixes = {"mon", "gon", "rus", "lek", "tor", "nix", "zar"};

        public static Zawomon GenerateRandomZawomon(int level = 1) {
            ZawomonClass zawomonClass = (ZawomonClass)Random.Range(0, System.Enum.GetValues(typeof(ZawomonClass)).Length);
            string name = namePrefixes[Random.Range(0, namePrefixes.Length)] + nameSuffixes[Random.Range(0, nameSuffixes.Length)];
            Color color = GetColorForClass(zawomonClass);
            int maxHP = Random.Range(30, 51) + level * 5;
            int dmg = Random.Range(5, 11) + level * 2;
            Zawomon zawomon = new Zawomon {
                Name = name,
                MainClass = zawomonClass,
                SecondaryClass = null,
                Color = color,
                Level = level,
                MaxHP = maxHP,
                CurrentHP = maxHP,
                Damage = dmg,
            };
            return zawomon;
        }

        public static Color GetColorForClass(ZawomonClass zawomonClass) {
            switch (zawomonClass) {
                case ZawomonClass.Fire:
                    return new Color(1f, 0.3f, 0.1f);
                case ZawomonClass.Water:
                    return new Color(0.2f, 0.5f, 1f);
                case ZawomonClass.Ice:
                    return new Color(0.6f, 0.9f, 1f);
                case ZawomonClass.Stone:
                    return new Color(0.5f, 0.4f, 0.3f);
                case ZawomonClass.Nature:
                    return new Color(0.2f, 0.8f, 0.2f);
                case ZawomonClass.Magic:
                    return new Color(0.7f, 0.2f, 1f);
                case ZawomonClass.DarkMagic:
                    return new Color(0.2f, 0.1f, 0.3f);
                default:
                    return Color.white;
            }
        }
    }
}
