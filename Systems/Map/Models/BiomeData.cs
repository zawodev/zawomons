using UnityEngine;

namespace Systems.Map.Models {
    [CreateAssetMenu(fileName = "New Biome", menuName = "Map/Biome Data")]
    public class BiomeData : ScriptableObject
    {
        [Header("Basic Information")]
        public string biomeName;
        public Color biomeColor = Color.white;
        public Texture2D biomeTexture;
        
        [Header("Generation Parameters")]
        [Range(0f, 1f)]
        public float rarity = 0.5f; // Im wyższa wartość, tym rzadszy biom
        
        [Header("Size Constraints")]
        public int minClusterSize = 1;
        public int maxClusterSize = 5;
        
        [Header("Movement Properties")]
        [Range(0.1f, 2f)]
        public float movementSpeedModifier = 1f;
        
        [Header("Visual Properties")]
        public Material tileMaterial;
        public GameObject[] decorationPrefabs;
        
        [Header("Special Properties")]
        public bool isWalkable = true;
        public bool canBuildOn = true;
        public int elevationLevel = 0;
        
        [TextArea(3, 5)]
        public string description;
    }
}