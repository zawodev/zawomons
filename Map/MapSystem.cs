using UnityEngine;
using System.Collections.Generic;
using System.Linq;

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
    
    // Private data
    private Dictionary<Vector2Int, HexTile> hexTiles = new Dictionary<Vector2Int, HexTile>();
    private Dictionary<Vector2Int, HexTileData> mapData = new Dictionary<Vector2Int, HexTileData>();
    private System.Random randomGenerator;
    private Camera mainCamera;
    private CameraController cameraController;
    
    // Culling
    private List<HexTile> visibleTiles = new List<HexTile>();
    private List<HexTile> allTiles = new List<HexTile>();
    
    // Events
    public System.Action<MapSystem> OnMapGenerated;
    public System.Action<HexTile> OnTileClicked;
    public System.Action<HexTile> OnTileHoverEnter;
    public System.Action<HexTile> OnTileHoverExit;
    
    // Selection system
    private HexTile selectedTile = null;
    
    // API Mock Data
    private List<BuildingData> mockBuildings = new List<BuildingData>();
    private List<CreatureMapData> mockCreatures = new List<CreatureMapData>();
    
    private void Awake()
    {
        if (mapParent == null)
        {
            GameObject mapContainer = new GameObject("Map Container");
            mapContainer.transform.SetParent(transform);
            mapParent = mapContainer.transform;
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
            
            HexTile hexTile = tileObject.GetComponent<HexTile>();
            if (hexTile == null)
            {
                hexTile = tileObject.AddComponent<HexTile>();
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
            float distance = Vector3.Distance(cameraPos, tile.transform.position);
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
        // System selekcji - tylko jeden kafelek może być wybrany na raz
        if (selectedTile != null && selectedTile != tile)
        {
            selectedTile.SetState(HexTile.TileState.Base);
        }
        
        selectedTile = tile;
        tile.SetState(HexTile.TileState.Selected);
        
        // Debug info o kliknięciem kafelku
        Debug.Log($"Tile clicked: Grid Position {tile.GetGridPosition()}, World Position {tile.transform.position}");
        if (tile.tileData?.biomeData != null)
        {
            Debug.Log($"Biome: {tile.tileData.biomeData.biomeName}");
        }
        
        // Sprawdź czy na kafelku są budynki lub creatures
        LogTileOccupants(tile);
        
        OnTileClicked?.Invoke(tile);
    }
    
    private void LogTileOccupants(HexTile tile)
    {
        Vector2Int gridPos = tile.GetGridPosition();
        
        // Sprawdź budynki
        var building = mockBuildings.Find(b => b.position == gridPos);
        if (building != null)
        {
            Debug.Log($"Building found: ID {building.id}, Type: {building.buildingType}, Owner: Player {building.ownerPlayerId}");
        }
        
        // Sprawdź creatures
        var creature = mockCreatures.Find(c => c.position == gridPos);
        if (creature != null)
        {
            Debug.Log($"Creature found: ID {creature.creatureId}, Name: {creature.creatureName}, Player owned: {creature.isOwnedByPlayer}");
        }
        
        if (building == null && creature == null)
        {
            Debug.Log("Tile is empty - no buildings or creatures");
        }
        
        OnTileClicked?.Invoke(tile);
    }
    
    private void HandleTileHovered(HexTile tile)
    {
        OnTileHoverEnter?.Invoke(tile);
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
                buildingObj.transform.position = tile.transform.position + Vector3.up * 0.25f; // Slightly above tile
                buildingObj.transform.localScale = Vector3.one * 0.5f;
                buildingObj.GetComponent<Renderer>().material.color = Color.gray;
                
                // WAŻNE: Usuwamy Collider żeby nie blokował raycastów do kafelków
                Collider buildingCollider = buildingObj.GetComponent<Collider>();
                if (buildingCollider != null)
                {
                    DestroyImmediate(buildingCollider);
                }
                
                // Ustaw jako child kafelka
                buildingObj.transform.SetParent(tile.transform);
                
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
                creatureObj.transform.position = tile.transform.position + Vector3.up * 0.15f; // Slightly above tile
                creatureObj.transform.localScale = Vector3.one * 0.3f;
                
                // Different color based on ownership
                Color creatureColor = creature.isOwnedByPlayer ? Color.blue : Color.red;
                creatureObj.GetComponent<Renderer>().material.color = creatureColor;
                
                // WAŻNE: Usuwamy Collider żeby nie blokował raycastów do kafelków
                Collider creatureCollider = creatureObj.GetComponent<Collider>();
                if (creatureCollider != null)
                {
                    DestroyImmediate(creatureCollider);
                }
                
                // Ustaw jako child kafelka
                creatureObj.transform.SetParent(tile.transform);
                
                tile.SetOccupyingObject(creatureObj, "Creature");
            }
        }
    }
    
    // Public API
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
