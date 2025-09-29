using UnityEngine;
using UnityEditor;
using Systems.Map.Core;

public static class HexTilePrefabCreator
{
    [MenuItem("Tools/Map System/Create Complete Hex Tile Prefab")]
    public static void CreateCompleteHexTilePrefab()
    {
        // Create main hex tile object
        GameObject hexTileObject = new GameObject("HexTile_Prefab");
        
        // Add required components
        MeshFilter meshFilter = hexTileObject.AddComponent<MeshFilter>();
        MeshRenderer meshRenderer = hexTileObject.AddComponent<MeshRenderer>();
        MeshCollider meshCollider = hexTileObject.AddComponent<MeshCollider>();
        
        // Add HexTile script
        HexTile hexTile = hexTileObject.AddComponent<HexTile>();
        
        // Create and assign materials
        CreateAndAssignMaterials(hexTile, meshRenderer);
        
        // Generate initial mesh
        hexTile.GenerateMesh();
        
        // Create prefab folder if it doesn't exist
        string prefabFolder = "Assets/Prefabs/Map";
        if (!AssetDatabase.IsValidFolder(prefabFolder))
        {
            if (!AssetDatabase.IsValidFolder("Assets/Prefabs"))
            {
                AssetDatabase.CreateFolder("Assets", "Prefabs");
            }
            AssetDatabase.CreateFolder("Assets/Prefabs", "Map");
        }
        
        // Save as prefab
        string prefabPath = $"{prefabFolder}/HexTile_Prefab.prefab";
        GameObject prefab = PrefabUtility.SaveAsPrefabAsset(hexTileObject, prefabPath);
        
        // Clean up the scene object
        Object.DestroyImmediate(hexTileObject);
        
        // Select the prefab asset
        Selection.activeObject = prefab;
        
        // Highlight in project window
        EditorGUIUtility.PingObject(prefab);
        
        Debug.Log($"Created complete HexTile prefab at: {prefabPath}");
    }
    
    [MenuItem("Tools/Map System/Create Default Biomes")]
    public static void CreateDefaultBiomesWrapper()
    {
        BiomeCreator.CreateDefaultBiomes();
        Debug.Log("Created default biomes in Assets/Resources/Biomes/");
    }
    
    [MenuItem("Tools/Map System/Create Default Materials")]
    public static void CreateDefaultMaterials()
    {
        CreateHexTileMaterials();
        Debug.Log("Created default HexTile materials in Assets/Materials/Map/");
    }

    private static void CreateAndAssignMaterials(HexTile hexTile, MeshRenderer meshRenderer)
    {
        var materials = CreateHexTileMaterials();
        
        // Assign materials to HexTile (only base and outline needed now)
        hexTile.baseMaterial = materials.baseMaterial;
        hexTile.outlineMaterial = materials.outlineMaterial;
        
        // Apply base material to renderer (use sharedMaterial in edit mode)
        meshRenderer.sharedMaterial = materials.baseMaterial;
    }
    
    public static (Material baseMaterial, Material outlineMaterial) CreateHexTileMaterials()
    {
        // Create materials folder if needed
        string materialFolder = "Assets/Materials/Map";
        if (!AssetDatabase.IsValidFolder(materialFolder))
        {
            if (!AssetDatabase.IsValidFolder("Assets/Materials"))
            {
                AssetDatabase.CreateFolder("Assets", "Materials");
            }
            AssetDatabase.CreateFolder("Assets/Materials", "Map");
        }
        
        // Check if materials already exist, if not create them
        Material baseMaterial = AssetDatabase.LoadAssetAtPath<Material>($"{materialFolder}/HexTile_Base.mat");
        if (baseMaterial == null)
        {
            baseMaterial = new Material(Shader.Find("Unlit/Color"));
            baseMaterial.color = new Color(0.4f, 0.8f, 0.4f); // Nice green
            baseMaterial.name = "HexTile_Base";
            AssetDatabase.CreateAsset(baseMaterial, $"{materialFolder}/HexTile_Base.mat");
        }
        
        Material outlineMaterial = AssetDatabase.LoadAssetAtPath<Material>($"{materialFolder}/HexTile_Outline.mat");
        if (outlineMaterial == null)
        {
            outlineMaterial = new Material(Shader.Find("Unlit/Color"));
            outlineMaterial.color = Color.white;
            outlineMaterial.name = "HexTile_Outline";
            AssetDatabase.CreateAsset(outlineMaterial, $"{materialFolder}/HexTile_Outline.mat");
        }
        
        // Save assets
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();
        
        return (baseMaterial, outlineMaterial);
    }
}
