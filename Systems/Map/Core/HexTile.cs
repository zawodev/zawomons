using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using Systems.Map.Models;
using Systems.Map.Utilities;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Systems.Map.Core {
    [ExecuteInEditMode]
    [RequireComponent(typeof(MeshFilter), typeof(MeshRenderer), typeof(MeshCollider))]
    public class HexTile : MonoBehaviour
{
    [Header("Basic Settings")]
    public float hexSize = 1f;
    public bool pointyTop = true;
    
    [Header("Materials")]
    public Material baseMaterial;
    public Material outlineMaterial;
    
    [Header("Base State")]
    [Range(0.5f, 2f)]
    public float baseBrightnessMultiplier = 1f;
    public bool baseShowOutline = false;
    public Color baseTintColor = Color.white;
    [Range(0f, 1f)]
    public float baseColorTint = 0f;
    public Color baseOutlineColor = Color.white;
    [Range(0.02f, 0.2f)]
    public float baseOutlineThickness = 0.05f;
    public OutlinePosition baseOutlinePosition = OutlinePosition.Outside;
    
    [Header("Hover State")]
    [Range(0.5f, 2f)]
    public float hoverBrightnessMultiplier = 1.2f;
    public bool hoverShowOutline = true;
    public Color hoverTintColor = Color.white;
    [Range(0f, 1f)]
    public float hoverColorTint = 0f;
    public Color hoverOutlineColor = Color.white;
    [Range(0.02f, 0.2f)]
    public float hoverOutlineThickness = 0.03f;
    public OutlinePosition hoverOutlinePosition = OutlinePosition.Outside;
    
    [Header("Selected State")]
    [Range(0.5f, 2f)]
    public float selectedBrightnessMultiplier = 1f;
    public bool selectedShowOutline = true;
    public Color selectedTintColor = Color.yellow;
    [Range(0f, 1f)]
    public float selectedColorTint = 0.3f;
    public Color selectedOutlineColor = Color.yellow;
    [Range(0.02f, 0.2f)]
    public float selectedOutlineThickness = 0.05f;
    public OutlinePosition selectedOutlinePosition = OutlinePosition.Outside;
    
    public enum OutlinePosition
    {
        Inside,   // Outline goes inward from hex edge
        Center,   // Outline is centered on hex edge (half in, half out)
        Outside   // Outline goes outward from hex edge
    }
    
    public enum TileState
    {
        Base,
        Hovered,
        Selected
    }
    
    [Header("Debug")]
    [SerializeField]
    private TileState currentState = TileState.Base;
    
    [Header("Data")]
    public HexTileData tileData;
    
    // Components
    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;
    private GameObject outlineObject;
    private Material originalMaterial;
    
    // Events
    public System.Action<HexTile> OnTileClicked;
    public System.Action<HexTile> OnTileHoverEnter;
    public System.Action<HexTile> OnTileHoverExit;
    
    // State
    private bool isInitialized = false;
    
    // Mouse interaction
    private Vector3 mouseDownPosition;
    private float clickThreshold = 5f; // Minimum distance to consider it a drag
    
    private void Awake()
    {
        InitializeComponents();
        // Always generate mesh on Awake to ensure it's ready
        GenerateMesh();
    }
    
    private void Start()
    {
        if (!isInitialized)
        {
            GenerateMesh();
        }
    }
    
    private void OnEnable()
    {
        // Auto-regenerate mesh when object is enabled (fixes prefab mode issues)
#if UNITY_EDITOR
        EditorApplication.delayCall += () => {
            if (this != null && gameObject.activeInHierarchy)
            {
                // Clean up any ghost outlines first
                CleanupGhostOutlines();
                
                // Check if we need to generate mesh
                Mesh currentMesh = Application.isPlaying ? 
                    (meshFilter != null ? meshFilter.mesh : null) : 
                    (meshFilter != null ? meshFilter.sharedMesh : null);
                
                // Generate mesh if missing or invalid
                if (meshFilter != null && currentMesh == null)
                {
                    GenerateMesh();
                }
            }
        };
#else
        if (meshFilter != null && meshFilter.sharedMesh == null)
        {
            GenerateMesh();
        }
#endif
    }
    
    private void InitializeComponents()
    {
        if (isInitialized) return;
        
        meshFilter = GetComponent<MeshFilter>();
        meshRenderer = GetComponent<MeshRenderer>();
        meshCollider = GetComponent<MeshCollider>();
        
        // Ensure map is behind UI
        meshRenderer.sortingOrder = -1;
        
        if (meshRenderer.sharedMaterial != null)
        {
            originalMaterial = meshRenderer.sharedMaterial;
        }
        
        isInitialized = true;
    }    public void Initialize(HexTileData data)
    {
        tileData = data;
        InitializeComponents();
        
        ApplyBiomeVisuals();
        GenerateMesh();
        
        SetState(TileState.Base);
    }
    
    private void ApplyBiomeVisuals()
    {
        if (tileData.biomeData == null) return;
        
        // Apply biome color
        Material currentMaterial = Application.isPlaying ? meshRenderer.material : meshRenderer.sharedMaterial;
        if (currentMaterial != null)
        {
            MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
            propertyBlock.SetColor("_Color", tileData.biomeData.biomeColor);
            meshRenderer.SetPropertyBlock(propertyBlock);
        }
        
        // Apply biome material if available
        if (tileData.biomeData.tileMaterial != null)
        {
            if (Application.isPlaying)
            {
                meshRenderer.material = new Material(tileData.biomeData.tileMaterial);
            }
            else
            {
                meshRenderer.sharedMaterial = tileData.biomeData.tileMaterial;
            }
            originalMaterial = tileData.biomeData.tileMaterial;
            
            // Re-apply biome color after material change
            MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
            propertyBlock.SetColor("_Color", tileData.biomeData.biomeColor);
            meshRenderer.SetPropertyBlock(propertyBlock);
        }
        
        // Store base material reference
        if (baseMaterial == null && meshRenderer.sharedMaterial != null)
        {
            baseMaterial = meshRenderer.sharedMaterial;
        }
        
        // Apply current state visuals
        ApplyStateVisuals(currentState);
    }
    
    public void GenerateMesh()
    {
        if (meshFilter == null) InitializeComponents();
        
        // Clean up any existing mesh to avoid memory leaks
        Mesh currentMesh = Application.isPlaying ? meshFilter.mesh : meshFilter.sharedMesh;
        if (currentMesh != null)
        {
            if (Application.isPlaying)
                Destroy(currentMesh);
            else
                DestroyImmediate(currentMesh);
        }
        
        Mesh hexMesh = new Mesh();
        hexMesh.name = tileData?.gridPosition != null 
            ? $"HexTile_{tileData.gridPosition.x}_{tileData.gridPosition.y}" 
            : "HexTile_Prefab";
        
        Vector3[] vertices = HexMath.GenerateHexMesh(hexSize, pointyTop);
        int[] triangles = HexMath.GenerateHexTriangles();
        Vector2[] uv = new Vector2[vertices.Length];
        
        // Generate UV coordinates (for XY plane)
        for (int i = 0; i < vertices.Length; i++)
        {
            uv[i] = new Vector2((vertices[i].x / hexSize + 1) * 0.5f, (vertices[i].y / hexSize + 1) * 0.5f);
        }
        
        hexMesh.vertices = vertices;
        hexMesh.triangles = triangles;
        hexMesh.uv = uv;
        hexMesh.RecalculateNormals();
        
        // Use sharedMesh in edit mode to avoid leaks, mesh in play mode for performance
        if (Application.isPlaying)
        {
            meshFilter.mesh = hexMesh;
        }
        else
        {
            meshFilter.sharedMesh = hexMesh;
        }
        meshCollider.sharedMesh = hexMesh;
        
        // Update outline after mesh generation
        UpdateOutline();
    }
    
    public void SetState(TileState newState)
    {
        currentState = newState;
        
        switch (currentState)
        {
            case TileState.Base:
                ApplyStateVisuals(TileState.Base);
                if (tileData != null)
                {
                    tileData.isHovered = false;
                    tileData.isSelected = false;
                }
                break;
                
            case TileState.Hovered:
                ApplyStateVisuals(TileState.Hovered);
                if (tileData != null)
                {
                    tileData.isHovered = true;
                    tileData.isSelected = false;
                }
                break;
                
            case TileState.Selected:
                ApplyStateVisuals(TileState.Selected);
                if (tileData != null)
                {
                    tileData.isHovered = false;
                    tileData.isSelected = true;
                }
                break;
        }
    }
    
    public TileState GetCurrentState()
    {
        return currentState;
    }
    
    public void SetHover(bool hovered)
    {
        if (hovered)
        {
            SetState(TileState.Hovered);
            OnTileHoverEnter?.Invoke(this);
        }
        else if (tileData != null && !tileData.isSelected)
        {
            SetState(TileState.Base);
            OnTileHoverExit?.Invoke(this);
        }
    }
    
    public void SetSelected(bool selected)
    {
        if (selected)
        {
            SetState(TileState.Selected);
        }
        else if (tileData != null && !tileData.isHovered)
        {
            SetState(TileState.Base);
        }
    }
    
    private void ApplyStateVisuals(TileState state)
    {
        // Get base color (from biome or material)
        Color baseColor = Color.green; // Default color
        
        if (tileData?.biomeData != null)
        {
            baseColor = tileData.biomeData.biomeColor;
        }
        else if (baseMaterial != null)
        {
            baseColor = baseMaterial.color;
        }
        
        // Get state-specific settings
        float brightness;
        bool showOutline;
        Color tintColor;
        float colorTint;
        Color outlineColor;
        float outlineThickness;
        OutlinePosition outlinePosition;
        
        switch (state)
        {
            case TileState.Hovered:
                brightness = hoverBrightnessMultiplier;
                showOutline = hoverShowOutline;
                tintColor = hoverTintColor;
                colorTint = hoverColorTint;
                outlineColor = hoverOutlineColor;
                outlineThickness = hoverOutlineThickness;
                outlinePosition = hoverOutlinePosition;
                break;
                
            case TileState.Selected:
                brightness = selectedBrightnessMultiplier;
                showOutline = selectedShowOutline;
                tintColor = selectedTintColor;
                colorTint = selectedColorTint;
                outlineColor = selectedOutlineColor;
                outlineThickness = selectedOutlineThickness;
                outlinePosition = selectedOutlinePosition;
                break;
                
            case TileState.Base:
            default:
                brightness = baseBrightnessMultiplier;
                showOutline = baseShowOutline;
                tintColor = baseTintColor;
                colorTint = baseColorTint;
                outlineColor = baseOutlineColor;
                outlineThickness = baseOutlineThickness;
                outlinePosition = baseOutlinePosition;
                break;
        }
        
        // Apply brightness and tint
        Color finalColor = Color.Lerp(baseColor * brightness, tintColor, colorTint);
        MaterialPropertyBlock propertyBlock = new MaterialPropertyBlock();
        propertyBlock.SetColor("_Color", finalColor);
        meshRenderer.SetPropertyBlock(propertyBlock);
        
        // Handle outline
        UpdateOutlineForState(showOutline, outlineColor, outlineThickness, outlinePosition);
    }
    
    private void UpdateOutlineForState(bool show, Color color, float thickness, OutlinePosition position)
    {
        if (outlineObject == null && show)
        {
            CreateOutlineObject();
        }
        
        if (outlineObject != null)
        {
            // Używamy SetActive zamiast usuwania/tworzenia obiektu
            outlineObject.SetActive(show);
            
            if (show)
            {
                // Update outline geometry with current settings
                UpdateOutlineGeometry(thickness, position);
                
                // Update outline color
                MeshRenderer outlineRenderer = outlineObject.GetComponent<MeshRenderer>();
                if (outlineRenderer != null)
                {
                    outlineRenderer.material.color = color;
                }
            }
        }
    }
    
    public void SetHighlighted(bool highlighted)
    {
        if (tileData != null)
        {
            tileData.isHighlighted = highlighted;
            UpdateOutline();
        }
    }
    
    private void UpdateOutline()
    {
        // This will use current state settings
        ApplyStateVisuals(currentState);
    }
    
    private void CreateOutlineObject()
    {
        // Don't create if already exists
        if (outlineObject != null) return;
        
        outlineObject = new GameObject("Hex Outline");
        outlineObject.transform.SetParent(transform);
        outlineObject.transform.localPosition = new Vector3(0, 0, -0.01f); // Slightly in front for 2D
        outlineObject.transform.localRotation = Quaternion.identity;
        outlineObject.transform.localScale = Vector3.one;
        
        MeshFilter outlineMeshFilter = outlineObject.AddComponent<MeshFilter>();
        MeshRenderer outlineMeshRenderer = outlineObject.AddComponent<MeshRenderer>();
        
        // Create default outline material if none provided
        if (outlineMaterial == null)
        {
            outlineMaterial = new Material(Shader.Find("Unlit/Color"));
            outlineMaterial.color = Color.white;
        }
        
        // Create instance of material for this hex to avoid shared material issues
        outlineMeshRenderer.material = new Material(outlineMaterial);
        
        // Ensure outline is behind UI
        outlineMeshRenderer.sortingOrder = -1;

        UpdateOutlineGeometry(baseOutlineThickness, baseOutlinePosition);
    }
    
    private void CleanupGhostOutlines()
    {
        // Find and remove any orphaned outline objects
        Transform[] children = GetComponentsInChildren<Transform>();
        bool outlineExists = false;
        for (int i = children.Length - 1; i >= 0; i--)
        {
            if (children[i] != transform && children[i].name.Contains("Hex Outline"))
            {
                // Check if this is our current outline object
                if (outlineObject != null && children[i].gameObject == outlineObject)
                {
                    outlineExists = true;
                    continue; // Don't destroy our current outline
                }
                
                if (Application.isPlaying)
                    Destroy(children[i].gameObject);
                else
                    DestroyImmediate(children[i].gameObject);
            }
        }
        
        // Only set to null if no valid outline exists
        if (!outlineExists)
        {
            outlineObject = null;
        }
    }
    
    private void UpdateOutlineGeometry(float thickness, OutlinePosition position)
    {
        if (outlineObject == null) return;
        
        MeshFilter outlineMeshFilter = outlineObject.GetComponent<MeshFilter>();
        if (outlineMeshFilter == null) return;
        
        // Calculate outline sizes based on position preference
        float innerSize, outerSize;
        
        switch (position)
        {
            case OutlinePosition.Inside:
                outerSize = hexSize;
                innerSize = hexSize - thickness;
                break;
                
            case OutlinePosition.Center:
                outerSize = hexSize + (thickness * 0.5f);
                innerSize = hexSize - (thickness * 0.5f);
                break;
                
            case OutlinePosition.Outside:
            default:
                outerSize = hexSize + thickness;
                innerSize = hexSize;
                break;
        }
        
        // Generate outline geometry (ring shape)
        Vector3[] outerVertices = HexMath.GenerateHexMesh(outerSize, pointyTop);
        Vector3[] innerVertices = HexMath.GenerateHexMesh(innerSize, pointyTop);
        
        // Combine inner and outer vertices for ring mesh
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        
        // Add outer vertices (skip center vertex)
        for (int i = 1; i < outerVertices.Length; i++)
        {
            vertices.Add(outerVertices[i]);
        }
        
        // Add inner vertices (skip center vertex)
        for (int i = 1; i < innerVertices.Length; i++)
        {
            vertices.Add(innerVertices[i]);
        }
        
        // Create triangles for the ring (with same winding order as main hex)
        for (int i = 0; i < 6; i++)
        {
            int outerCurrent = i;
            int outerNext = (i + 1) % 6;
            int innerCurrent = i + 6;
            int innerNext = ((i + 1) % 6) + 6;
            
            // First triangle (same winding as main hex)
            triangles.Add(outerCurrent);
            triangles.Add(innerNext);
            triangles.Add(outerNext);
            
            // Second triangle (same winding as main hex)
            triangles.Add(outerCurrent);
            triangles.Add(innerCurrent);
            triangles.Add(innerNext);
        }
        
        Mesh outlineMesh = new Mesh();
        outlineMesh.vertices = vertices.ToArray();
        outlineMesh.triangles = triangles.ToArray();
        outlineMesh.RecalculateNormals();
        outlineMesh.name = "Hex Outline Mesh";
        
        // Use sharedMesh in edit mode to avoid leaks, mesh in play mode for performance
        if (Application.isPlaying)
        {
            outlineMeshFilter.mesh = outlineMesh;
        }
        else
        {
            outlineMeshFilter.sharedMesh = outlineMesh;
        }
    }
    
    // Public API methods
    public void SetOccupyingObject(GameObject obj, string type)
    {
        if (tileData != null)
        {
            tileData.isOccupied = obj != null;
            // Add more logic as needed for different object types
        }
    }
    
    public void ClearOccupyingObject()
    {
        if (tileData != null)
        {
            tileData.isOccupied = false;
        }
    }
    
    // Mouse interaction
    private void OnMouseDown()
    {
        mouseDownPosition = Input.mousePosition;
    }
    
    private void OnMouseUp()
    {
        // Sprawdź czy to było kliknięcie (mała odległość ruchu myszki)
        float dragDistance = Vector3.Distance(Input.mousePosition, mouseDownPosition);
        if (dragDistance < clickThreshold)
        {
            // Sprawdź czy pointer nie jest nad UI
            if (UnityEngine.EventSystems.EventSystem.current != null && 
                UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject())
            {
                return;
            }
            
            OnTileClicked?.Invoke(this);
        }
    }
    
    private void OnMouseEnter()
    {
        // Jeśli hex jest selected, nie zmieniaj na hover
        if (tileData != null && tileData.isSelected)
        {
            return;
        }
        SetHover(true);
    }
    
    private void OnMouseExit()
    {
        // Jeśli hex jest selected, nie zmieniaj z hover
        if (tileData != null && tileData.isSelected)
        {
            return;
        }
        SetHover(false);
    }
    
    public Vector2Int GetGridPosition()
    {
        return tileData?.gridPosition ?? Vector2Int.zero;
    }
    
    public BiomeData GetBiome()
    {
        return tileData?.biomeData;
    }
    
#if UNITY_EDITOR
    // Menu item to create a default hex tile
    [UnityEditor.MenuItem("GameObject/3D Object/Hex Tile", false, 10)]
    static void CreateHexTile()
    {
        GameObject hexTileObject = new GameObject("Hex Tile");
        
        // Add required components
        hexTileObject.AddComponent<MeshFilter>();
        hexTileObject.AddComponent<MeshRenderer>();
        hexTileObject.AddComponent<MeshCollider>();
        
        // Add HexTile script
        HexTile hexTile = hexTileObject.AddComponent<HexTile>();
        
        // Create default materials if not available
        if (hexTile.baseMaterial == null)
        {
            hexTile.baseMaterial = new Material(Shader.Find("Unlit/Color"));
            hexTile.baseMaterial.color = new Color(0.4f, 0.8f, 0.4f);
        }
        
        if (hexTile.outlineMaterial == null)
        {
            hexTile.outlineMaterial = new Material(Shader.Find("Unlit/Color"));
            hexTile.outlineMaterial.color = Color.white;
        }
        
        // Apply base material to renderer
        hexTileObject.GetComponent<MeshRenderer>().sharedMaterial = hexTile.baseMaterial;
        
        // Generate initial mesh
        hexTile.GenerateMesh();
        
        // Select the created object
        UnityEditor.Selection.activeGameObject = hexTileObject;
        
        // Position at scene view center
        if (UnityEditor.SceneView.lastActiveSceneView != null)
        {
            hexTileObject.transform.position = UnityEditor.SceneView.lastActiveSceneView.pivot;
        }
    }
    
    // Reset method called when component is added or reset in inspector
    void Reset()
    {
        InitializeComponents();
        GenerateMesh();
    }
#endif
}
}