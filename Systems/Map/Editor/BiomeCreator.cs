using UnityEngine;
using UnityEditor;
using Systems.Map.Models;

// This script helps create biome assets programmatically
public static class BiomeCreator
{
    // Moved to HexTilePrefabCreator to avoid duplication
    // [MenuItem("Tools/Create Default Biomes")]
    public static void CreateDefaultBiomes()
    {
        CreateGrasslandBiome();
        CreateForestBiome();
        CreateMountainBiome();
        CreateWaterBiome();
        CreateDesertBiome();
        
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
    }
    
    private static void CreateGrasslandBiome()
    {
        BiomeData biome = ScriptableObject.CreateInstance<BiomeData>();
        biome.biomeName = "Grassland";
        biome.biomeColor = new Color(0.4f, 0.8f, 0.4f);
        biome.rarity = 0.2f; // Very common
        biome.minClusterSize = 4;
        biome.maxClusterSize = 12;
        biome.movementSpeedModifier = 1.2f; // Fast movement
        biome.isWalkable = true;
        biome.canBuildOn = true;
        biome.elevationLevel = 0;
        biome.description = "Open grasslands perfect for settlements and fast travel.";
        
        AssetDatabase.CreateAsset(biome, "Assets/Resources/Biomes/Grassland.asset");
    }
    
    private static void CreateForestBiome()
    {
        BiomeData biome = ScriptableObject.CreateInstance<BiomeData>();
        biome.biomeName = "Forest";
        biome.biomeColor = new Color(0.2f, 0.6f, 0.2f);
        biome.rarity = 0.4f; // Common
        biome.minClusterSize = 3;
        biome.maxClusterSize = 8;
        biome.movementSpeedModifier = 0.8f; // Slower movement
        biome.isWalkable = true;
        biome.canBuildOn = false; // Can't build in dense forest
        biome.elevationLevel = 0;
        biome.description = "Dense forests that provide resources but slow down movement.";
        
        AssetDatabase.CreateAsset(biome, "Assets/Resources/Biomes/Forest.asset");
    }
    
    private static void CreateMountainBiome()
    {
        BiomeData biome = ScriptableObject.CreateInstance<BiomeData>();
        biome.biomeName = "Mountain";
        biome.biomeColor = new Color(0.6f, 0.6f, 0.7f);
        biome.rarity = 0.7f; // Uncommon
        biome.minClusterSize = 2;
        biome.maxClusterSize = 6;
        biome.movementSpeedModifier = 0.5f; // Very slow movement
        biome.isWalkable = true;
        biome.canBuildOn = false;
        biome.elevationLevel = 2;
        biome.description = "High mountains that are difficult to traverse.";
        
        AssetDatabase.CreateAsset(biome, "Assets/Resources/Biomes/Mountain.asset");
    }
    
    private static void CreateWaterBiome()
    {
        BiomeData biome = ScriptableObject.CreateInstance<BiomeData>();
        biome.biomeName = "Water";
        biome.biomeColor = new Color(0.2f, 0.4f, 0.8f);
        biome.rarity = 0.6f; // Uncommon
        biome.minClusterSize = 3;
        biome.maxClusterSize = 10;
        biome.movementSpeedModifier = 0.3f; // Very slow without boats
        biome.isWalkable = false; // Need special movement
        biome.canBuildOn = false;
        biome.elevationLevel = -1;
        biome.description = "Water bodies that require special means of transportation.";
        
        AssetDatabase.CreateAsset(biome, "Assets/Resources/Biomes/Water.asset");
    }
    
    private static void CreateDesertBiome()
    {
        BiomeData biome = ScriptableObject.CreateInstance<BiomeData>();
        biome.biomeName = "Desert";
        biome.biomeColor = new Color(0.9f, 0.8f, 0.4f);
        biome.rarity = 0.8f; // Rare
        biome.minClusterSize = 5;
        biome.maxClusterSize = 15;
        biome.movementSpeedModifier = 0.7f; // Slower due to sand
        biome.isWalkable = true;
        biome.canBuildOn = true;
        biome.elevationLevel = 0;
        biome.description = "Harsh desert lands with difficult conditions.";
        
        AssetDatabase.CreateAsset(biome, "Assets/Resources/Biomes/Desert.asset");
    }
}
