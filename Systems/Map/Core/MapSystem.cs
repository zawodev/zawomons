using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using Systems.Map.Models;
using Systems.Map.UI;
using Systems.Map.Utilities;

namespace Systems.Map.Core {
    public class MapSystem : MonoBehaviour
{
    [Header("Map Generation Settings")]
    public int mapWidth = 50;
    public int mapHeight = 50;
    public int seed = 42;
    public float hexSize = 1f;
    public bool pointyTop = true;
    
    [Header("Biome Settings")]
    [SerializeField] private List<BiomeData> availableBiomes = new List<BiomeData>();
    
    [Header("Prefabs")]
    public GameObject hexTilePrefab;
    
    [Header("Optimization")]
    public bool enableCulling = true;
    public float cullingDistance = 20f;
    
    [Header("Visual Settings")]
    public Transform mapParent;
    public Transform buildingsParent;
    public Transform creaturesParent;
    
    // Private data
    private Dictionary<Vector2Int, HexTile> hexTiles = new Dictionary<Vector2Int, HexTile>();
    private Dictionary<Vector2Int, HexTileData> mapData = new Dictionary<Vector2Int, HexTileData>();
    private System.Random randomGenerator;
    private Camera mainCamera;
    private CameraController cameraController;
    
    // Culling
    private List<HexTile> visibleTiles = new List<HexTile>();
    private List<HexTile> allTiles = new List<HexTile>();
    
    // Selection
    private HexTile currentSelectedTile = null;
    
    // Events
    public System.Action<MapSystem> OnMapGenerated;
    public System.Action<HexTile> OnTileClicked;
    public System.Action<HexTile> OnTileHovered;
    
    // API Mock Data
    private List<BuildingData> mockBuildings = new List<BuildingData>();
    private List<CreatureMapData> mockCreatures = new List<CreatureMapData>();
    
    private void Awake()
    {
        if (mapParent == null)
        {
            GameObject mapContainer = new GameObject("Map Container");
            mapContainer.transform.SetParent(transform);
            mapContainer.transform.position = new Vector3(0, 0, 100); // Z = 100 for proper sorting
            mapParent = mapContainer.transform;
        }
        
        if (buildingsParent == null)
        {
            GameObject buildingsContainer = new GameObject("Buildings Container");
            buildingsContainer.transform.SetParent(transform);
            buildingsContainer.transform.position = new Vector3(0, 0, 100); // Z = 100 for proper sorting
            buildingsParent = buildingsContainer.transform;
        }
        
        if (creaturesParent == null)
        {
            GameObject creaturesContainer = new GameObject("Creatures Container");
            creaturesContainer.transform.SetParent(transform);
            creaturesContainer.transform.position = new Vector3(0, 0, 100); // Z = 100 for proper sorting
            creaturesParent = creaturesContainer.transform;
        }
        
        mainCamera = Camera.main;
        cameraController = FindFirstObjectByType<CameraController>();
        
        LoadBiomeData();
    }
    
    private void Start()
    {
        GenerateMap();
        LoadAPIData();
    }
    
    private void Update()
    {
        if (enableCulling)
        {
            UpdateCulling();
        }
    }
    
    [ContextMenu("Generate New Map")]
    public void GenerateMap()
    {
        ClearExistingMap();
        
        randomGenerator = new System.Random(seed);
        
        // Generate map data first
        GenerateMapData();
        
        // Create physical tiles
        CreatePhysicalTiles();
        
        // Setup camera boundaries
        SetupCameraBoundaries();
        
        OnMapGenerated?.Invoke(this);
    }
    
    [ContextMenu("Randomize Seed")]
    public void RandomizeSeed()
    {
        seed = Random.Range(0, 999999);
        GenerateMap();
    }
    
    private void LoadBiomeData()
    {
        availableBiomes.Clear();
        BiomeData[] biomes = Resources.LoadAll<BiomeData>("Biomes");
        availableBiomes.AddRange(biomes);
        
        if (availableBiomes.Count == 0)
        {
            Debug.LogWarning("No biome data found in Resources/Biomes folder! Creating default biome.");
            CreateDefaultBiome();
        }
    }
    
    private void CreateDefaultBiome()
    {
        // This would normally be created as a ScriptableObject asset
        // For now, we'll create it dynamically
        BiomeData defaultBiome = ScriptableObject.CreateInstance<BiomeData>();
        defaultBiome.biomeName = "Default Grassland";
        defaultBiome.biomeColor = Color.green;
        defaultBiome.rarity = 0.7f;
        defaultBiome.minClusterSize = 3;
        defaultBiome.maxClusterSize = 8;
        defaultBiome.movementSpeedModifier = 1f;
        defaultBiome.isWalkable = true;
        defaultBiome.canBuildOn = true;
        
        availableBiomes.Add(defaultBiome);
    }
    
    private void GenerateMapData()
    {
        mapData.Clear();
        
        // First pass: generate base biome layout using noise
        for (int x = -mapWidth / 2; x < mapWidth / 2; x++)
        {
            for (int y = -mapHeight / 2; y < mapHeight / 2; y++)
            {
                Vector2Int gridPos = new Vector2Int(x, y);
                Vector3 worldPos = HexMath.HexToWorldPosition(gridPos, hexSize, pointyTop);
                
                BiomeData selectedBiome = SelectBiomeForPosition(gridPos);
                HexTileData tileData = new HexTileData(gridPos, worldPos, selectedBiome);
                
                mapData[gridPos] = tileData;
            }
        }
        
        // Second pass: apply biome clustering
        ApplyBiomeClustering();
    }
    
    private BiomeData SelectBiomeForPosition(Vector2Int position)
    {
        if (availableBiomes.Count == 0) return null;
        
        // Use deterministic noise based on position and seed
        float noiseValue = Mathf.PerlinNoise(
            (position.x + seed * 0.1f) * 0.1f,
            (position.y + seed * 0.1f) * 0.1f
        );
        
        // Add some variation with a different frequency
        noiseValue += Mathf.PerlinNoise(
            (position.x + seed * 0.3f) * 0.05f,
            (position.y + seed * 0.3f) * 0.05f
        ) * 0.5f;
        
        // Normalize
        noiseValue = Mathf.Clamp01(noiseValue / 1.5f);
        
        // Select biome based on rarity and noise
        var sortedBiomes = availableBiomes.OrderBy(b => b.rarity).ToList();
        
        float accumulatedWeight = 0f;
        foreach (var biome in sortedBiomes)
        {
            accumulatedWeight += (1f - biome.rarity) + 0.1f; // Ensure minimum weight
            if (noiseValue <= accumulatedWeight / sortedBiomes.Count)
            {
                return biome;
            }
        }
        
        return sortedBiomes.Last(); // Fallback to rarest biome
    }
    
    private void ApplyBiomeClustering()
    {
        // This creates more natural biome clusters
        var processedPositions = new HashSet<Vector2Int>();
        var clustersToProcess = new Queue<Vector2Int>();
        
        foreach (var kvp in mapData.ToList())
        {
            if (processedPositions.Contains(kvp.Key)) continue;
            
            BiomeData biome = kvp.Value.biomeData;
            if (biome == null) continue;
            
            int clusterSize = randomGenerator.Next(biome.minClusterSize, biome.maxClusterSize + 1);
            
            clustersToProcess.Clear();
            clustersToProcess.Enqueue(kvp.Key);
            processedPositions.Add(kvp.Key);
            
            int tilesInCluster = 1;
            
            while (clustersToProcess.Count > 0 && tilesInCluster < clusterSize)
            {
                Vector2Int currentPos = clustersToProcess.Dequeue();
                var neighbors = HexMath.GetHexNeighbors(currentPos);
                
                foreach (var neighborPos in neighbors)
                {
                    if (processedPositions.Contains(neighborPos) || !mapData.ContainsKey(neighborPos))
                        continue;
                    
                    float spreadChance = 1f - ((float)tilesInCluster / clusterSize);
                    if (randomGenerator.NextDouble() < spreadChance)
                    {
                        mapData[neighborPos].biomeData = biome;
                        clustersToProcess.Enqueue(neighborPos);
                        processedPositions.Add(neighborPos);
                        tilesInCluster++;
                        
                        if (tilesInCluster >= clusterSize) break;
                    }
                }
            }
        }
    }
    
    private void CreatePhysicalTiles()
    {
        hexTiles.Clear();
        allTiles.Clear();
        
        foreach (var kvp in mapData)
        {
            Vector2Int gridPos = kvp.Key;
            HexTileData tileData = kvp.Value;
            
            GameObject tileObject = Instantiate(hexTilePrefab, tileData.worldPosition, Quaternion.identity, mapParent);
            tileObject.name = $"HexTile_{gridPos.x}_{gridPos.y}";
            
            // Ensure proper Z position for sorting
            Vector3 pos = tileObject.transform.position;
            tileObject.transform.position = new Vector3(pos.x, pos.y, 100);
            
            HexTile hexTile = tileObject.GetComponent<HexTile>();
            if (hexTile == null)
            {
                hexTile = tileObject.AddComponent<HexTile>();
            }
            
            // Set sorting for hex tiles
            Renderer tileRenderer = tileObject.GetComponent<Renderer>();
            if (tileRenderer != null)
            {
                tileRenderer.sortingLayerName = "Default";
                tileRenderer.sortingOrder = -3; // Hex tiles should be behind buildings/creatures
            }
            
            hexTile.hexSize = hexSize;
            hexTile.pointyTop = pointyTop;
            hexTile.Initialize(tileData);
            
            // Subscribe to tile events
            hexTile.OnTileClicked += HandleTileClicked;
            hexTile.OnTileHoverEnter += HandleTileHovered;
            
            hexTiles[gridPos] = hexTile;
            allTiles.Add(hexTile);
        }
    }
    
    private void SetupCameraBoundaries()
    {
        if (cameraController == null) return;
        
        // Calculate map bounds
        float halfWidth = (mapWidth * hexSize * Mathf.Sqrt(3f)) / 2f;
        float halfHeight = (mapHeight * hexSize * 1.5f) / 2f;
        
        Vector2 minBounds = new Vector2(-halfWidth, -halfHeight);
        Vector2 maxBounds = new Vector2(halfWidth, halfHeight);
        
        cameraController.SetMapBoundaries(minBounds, maxBounds);
    }
    
    private void UpdateCulling()
    {
        if (mainCamera == null) return;
        
        Vector3 cameraPos = mainCamera.transform.position;
        visibleTiles.Clear();
        
        foreach (var tile in allTiles)
        {
            // Use 2D distance for culling (ignore Z difference)
            Vector2 cameraPos2D = new Vector2(cameraPos.x, cameraPos.y);
            Vector2 tilePos2D = new Vector2(tile.transform.position.x, tile.transform.position.y);
            float distance = Vector2.Distance(cameraPos2D, tilePos2D);
            bool shouldBeVisible = distance <= cullingDistance;
            
            if (tile.gameObject.activeSelf != shouldBeVisible)
            {
                tile.gameObject.SetActive(shouldBeVisible);
            }
            
            if (shouldBeVisible)
            {
                visibleTiles.Add(tile);
            }
        }
    }
    
    private void HandleTileClicked(HexTile tile)
    {
        // Odznacz poprzedni zaznaczony hex
        if (currentSelectedTile != null && currentSelectedTile != tile)
        {
            currentSelectedTile.SetSelected(false);
        }
        
        // Zaznacz nowy hex
        if (tile != currentSelectedTile)
        {
            tile.SetSelected(true);
            currentSelectedTile = tile;
            Debug.Log($"Kliknięto w hex: {tile.tileData.gridPosition}");
        }
        else
        {
            // Jeśli kliknięto w ten sam hex, odznacz go
            tile.SetSelected(false);
            currentSelectedTile = null;
        }
        
        OnTileClicked?.Invoke(tile);
    }
    
    private void HandleTileHovered(HexTile tile)
    {
        OnTileHovered?.Invoke(tile);
    }
    
    private void ClearExistingMap()
    {
        if (mapParent != null)
        {
            for (int i = mapParent.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(mapParent.GetChild(i).gameObject);
            }
        }
        
        if (buildingsParent != null)
        {
            for (int i = buildingsParent.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(buildingsParent.GetChild(i).gameObject);
            }
        }
        
        if (creaturesParent != null)
        {
            for (int i = creaturesParent.childCount - 1; i >= 0; i--)
            {
                DestroyImmediate(creaturesParent.GetChild(i).gameObject);
            }
        }
        
        hexTiles.Clear();
        mapData.Clear();
        allTiles.Clear();
        visibleTiles.Clear();
    }
    
    // Public method to clear the map (for editor)
    public void ClearMap()
    {
        ClearExistingMap();
        Debug.Log("Map cleared successfully!");
    }
    
    // API Integration (Mock for now)
    private void LoadAPIData()
    {
        // Mock building data
        LoadBuildingData();
        
        // Mock creature data  
        LoadCreatureData();
        
        PlaceBuildingsAndCreatures();
    }
    
    private void LoadBuildingData()
    {
        // TODO: Replace with actual API call
        mockBuildings.Clear();
        
        // Add some mock buildings
        for (int i = 0; i < 5; i++)
        {
            BuildingData building = new BuildingData
            {
                id = i,
                position = new Vector2Int(
                    randomGenerator.Next(-mapWidth / 4, mapWidth / 4),
                    randomGenerator.Next(-mapHeight / 4, mapHeight / 4)
                ),
                buildingType = "City",
                ownerPlayerId = i % 3
            };
            
            mockBuildings.Add(building);
        }
    }
    
    private void LoadCreatureData()
    {
        // TODO: Replace with actual API call  
        mockCreatures.Clear();
        
        for (int i = 0; i < 3; i++)
        {
            CreatureMapData creature = new CreatureMapData
            {
                creatureId = i,
                position = new Vector2Int(
                    randomGenerator.Next(-mapWidth / 6, mapWidth / 6),
                    randomGenerator.Next(-mapHeight / 6, mapHeight / 6)
                ),
                creatureName = $"Creature_{i}",
                isOwnedByPlayer = i < 2 // First 2 creatures belong to player
            };
            
            mockCreatures.Add(creature);
        }
    }
    
    private void PlaceBuildingsAndCreatures()
    {
        // Place buildings
        foreach (var building in mockBuildings)
        {
            if (hexTiles.TryGetValue(building.position, out HexTile tile))
            {
                // Create a simple building representation
                GameObject buildingObj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                buildingObj.name = $"Building_{building.id}";
                buildingObj.transform.SetParent(buildingsParent);
                buildingObj.transform.localScale = Vector3.one * 0.5f;
                
                // Position on the tile with correct Z for sorting
                Vector3 tilePos = tile.transform.position;
                buildingObj.transform.position = new Vector3(tilePos.x, tilePos.y, 100);
                
                Renderer buildingRenderer = buildingObj.GetComponent<Renderer>();
                buildingRenderer.material.color = Color.gray;
                buildingRenderer.sortingLayerName = "Default";
                buildingRenderer.sortingOrder = -2;
                
                // WAŻNE: Usuwamy Collider żeby nie blokował raycastów do kafelków
                Collider buildingCollider = buildingObj.GetComponent<Collider>();
                if (buildingCollider != null)
                {
                    DestroyImmediate(buildingCollider);
                }
                
                tile.SetOccupyingObject(buildingObj, "Building");
            }
        }
        
        // Place creatures
        foreach (var creature in mockCreatures)
        {
            if (hexTiles.TryGetValue(creature.position, out HexTile tile))
            {
                // Create a simple creature representation
                GameObject creatureObj = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                creatureObj.name = $"Creature_{creature.creatureId}";
                creatureObj.transform.SetParent(creaturesParent);
                creatureObj.transform.localScale = Vector3.one * 0.3f;
                
                // Position on the tile with correct Z for sorting
                Vector3 tilePos = tile.transform.position;
                creatureObj.transform.position = new Vector3(tilePos.x, tilePos.y, 100);
                
                // Different color based on ownership
                Color creatureColor = creature.isOwnedByPlayer ? Color.blue : Color.red;
                Renderer creatureRenderer = creatureObj.GetComponent<Renderer>();
                creatureRenderer.material.color = creatureColor;
                creatureRenderer.sortingLayerName = "Default";
                creatureRenderer.sortingOrder = -1;
                
                // WAŻNE: Usuwamy Collider żeby nie blokował raycastów do kafelków
                Collider creatureCollider = creatureObj.GetComponent<Collider>();
                if (creatureCollider != null)
                {
                    DestroyImmediate(creatureCollider);
                }
                
                tile.SetOccupyingObject(creatureObj, "Creature");
            }
        }
    }    // Public API
    public HexTile GetTile(Vector2Int gridPosition)
    {
        hexTiles.TryGetValue(gridPosition, out HexTile tile);
        return tile;
    }
    
    public HexTileData GetTileData(Vector2Int gridPosition)
    {
        mapData.TryGetValue(gridPosition, out HexTileData data);
        return data;
    }
    
    public List<HexTile> GetTilesInRange(Vector2Int center, int range)
    {
        var positions = HexMath.GetHexesInRange(center, range);
        var tiles = new List<HexTile>();
        
        foreach (var pos in positions)
        {
            if (hexTiles.TryGetValue(pos, out HexTile tile))
            {
                tiles.Add(tile);
            }
        }
        
        return tiles;
    }
    
    public List<HexTile> GetVisibleTiles()
    {
        return new List<HexTile>(visibleTiles);
    }
}

// Helper classes for API data
[System.Serializable]
public class BuildingData
{
    public int id;
    public Vector2Int position;
    public string buildingType;
    public int ownerPlayerId;
}

[System.Serializable]
public class CreatureMapData
{
    public int creatureId;
    public Vector2Int position;  
    public string creatureName;
    public bool isOwnedByPlayer; // True if belongs to current player, false for other players/NPCs
}
}
