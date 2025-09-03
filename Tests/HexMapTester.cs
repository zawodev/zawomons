using UnityEngine;

public class HexMapTester : MonoBehaviour
{
    [Header("Test Settings")]
    public MapSystem mapSystem;
    public CameraController cameraController;
    
    [Header("Test Controls")]
    [Space]
    [Button("Test Hex Math")]
    public bool testHexMath;
    
    [Button("Test Map Generation")]
    public bool testMapGeneration;
    
    [Button("Test Camera Controls")]
    public bool testCameraControls;
    
    [Button("Test Biome Loading")]
    public bool testBiomeLoading;
    
    private void Start()
    {
        Debug.Log("HexMapTester initialized. Use the inspector buttons to run tests.");
    }
    
    private void Update()
    {
        // Test keyboard shortcuts
        if (Input.GetKeyDown(KeyCode.G))
        {
            TestMapGeneration();
        }
        
        if (Input.GetKeyDown(KeyCode.R))
        {
            TestRandomSeed();
        }
        
        if (Input.GetKeyDown(KeyCode.H))
        {
            TestHexMath();
        }
        
        if (Input.GetKeyDown(KeyCode.B))
        {
            TestBiomeLoading();
        }
    }
    
    public void TestHexMath()
    {
        Debug.Log("=== HEX MATH TEST ===");
        
        // Test coordinate conversion
        Vector2Int gridPos = new Vector2Int(3, 4);
        Vector3 worldPos = HexMath.HexToWorldPosition(gridPos, 1f, true);
        Vector2Int backToGrid = HexMath.WorldToHexPosition(worldPos, 1f, true);
        
        Debug.Log($"Grid: {gridPos} -> World: {worldPos} -> Grid: {backToGrid}");
        Debug.Log($"Conversion accurate: {gridPos == backToGrid}");
        
        // Test distance calculation
        Vector2Int pos1 = new Vector2Int(0, 0);
        Vector2Int pos2 = new Vector2Int(3, 4);
        int distance = HexMath.HexDistance(pos1, pos2);
        Debug.Log($"Distance from {pos1} to {pos2}: {distance}");
        
        // Test neighbors
        var neighbors = HexMath.GetHexNeighbors(Vector2Int.zero);
        Debug.Log($"Neighbors of (0,0): {string.Join(", ", neighbors)}");
        
        // Test range
        var hexesInRange = HexMath.GetHexesInRange(Vector2Int.zero, 2);
        Debug.Log($"Hexes in range 2 of (0,0): {hexesInRange.Count} tiles");
    }
    
    public void TestMapGeneration()
    {
        Debug.Log("=== MAP GENERATION TEST ===");
        
        if (mapSystem == null)
        {
            mapSystem = FindFirstObjectByType<MapSystem>();
        }
        
        if (mapSystem != null)
        {
            Debug.Log($"Generating map with seed: {mapSystem.seed}");
            Debug.Log($"Map size: {mapSystem.mapWidth}x{mapSystem.mapHeight}");
            
            mapSystem.GenerateMap();
            Debug.Log("Map generation completed!");
        }
        else
        {
            Debug.LogError("MapSystem not found! Please assign it in the inspector or add it to the scene.");
        }
    }
    
    public void TestRandomSeed()
    {
        Debug.Log("=== RANDOM SEED TEST ===");
        
        if (mapSystem == null)
        {
            mapSystem = FindFirstObjectByType<MapSystem>();
        }
        
        if (mapSystem != null)
        {
            int oldSeed = mapSystem.seed;
            mapSystem.RandomizeSeed();
            Debug.Log($"Seed changed from {oldSeed} to {mapSystem.seed}");
        }
        else
        {
            Debug.LogError("MapSystem not found!");
        }
    }
    
    public void TestCameraControls()
    {
        Debug.Log("=== CAMERA CONTROLS TEST ===");
        
        if (cameraController == null)
        {
            cameraController = FindFirstObjectByType<CameraController>();
        }
        
        if (cameraController != null)
        {
            Debug.Log("Camera Controller found!");
            Debug.Log($"Pan Speed: {cameraController.panSpeed}");
            Debug.Log($"Zoom Range: {cameraController.minZoom} - {cameraController.maxZoom}");
            Debug.Log($"Boundaries Enabled: {cameraController.useBoundaries}");
            
            // Test focus on center
            cameraController.FocusOnPosition(Vector3.zero, 10f);
            Debug.Log("Focused camera on center with zoom 10");
        }
        else
        {
            Debug.LogError("CameraController not found! Please assign it in the inspector or add it to the scene.");
        }
    }
    
    public void TestBiomeLoading()
    {
        Debug.Log("=== BIOME LOADING TEST ===");
        
        BiomeData[] biomes = Resources.LoadAll<BiomeData>("Biomes");
        Debug.Log($"Found {biomes.Length} biomes in Resources/Biomes:");
        
        foreach (var biome in biomes)
        {
            Debug.Log($"- {biome.biomeName} (rarity: {biome.rarity}, color: {biome.biomeColor})");
        }
        
        if (biomes.Length == 0)
        {
            Debug.LogWarning("No biomes found! Use Tools -> Create Default Biomes to create some test biomes.");
        }
    }
    
    private void OnValidate()
    {
        // Auto-find components if not assigned
        if (mapSystem == null)
        {
            mapSystem = FindFirstObjectByType<MapSystem>();
        }
        
        if (cameraController == null)
        {
            cameraController = FindFirstObjectByType<CameraController>();
        }
    }
}

// Custom attribute for inspector buttons (simple implementation)
public class ButtonAttribute : PropertyAttribute
{
    public string MethodName;
    
    public ButtonAttribute(string methodName)
    {
        MethodName = methodName;
    }
}