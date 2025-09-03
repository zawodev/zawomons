using UnityEngine;

[System.Serializable]
public class HexTileData
{
    [Header("Position Data")]
    public Vector2Int gridPosition;
    public Vector3 worldPosition;
    
    [Header("Biome Information")]
    public BiomeData biomeData;
    public int biomeVariant; // Dla różnych wariantów tego samego biomu
    
    [Header("Occupancy")]
    public bool isOccupied;
    public GameObject occupyingObject; // Budynek, gracz itp.
    public string occupyingObjectType;
    
    [Header("State")]
    public bool isSelected;
    public bool isHighlighted;
    public bool isHovered;
    
    [Header("Game Properties")]
    public float movementCost;
    public bool canMoveTo;
    public bool canBuildOn;
    
    public HexTileData(Vector2Int gridPos, Vector3 worldPos, BiomeData biome)
    {
        gridPosition = gridPos;
        worldPosition = worldPos;
        biomeData = biome;
        biomeVariant = 0;
        isOccupied = false;
        occupyingObject = null;
        occupyingObjectType = "";
        isSelected = false;
        isHighlighted = false;
        isHovered = false;
        movementCost = biome ? biome.movementSpeedModifier : 1f;
        canMoveTo = biome ? biome.isWalkable : true;
        canBuildOn = biome ? biome.canBuildOn : true;
    }
    
    public void SetOccupyingObject(GameObject obj, string type)
    {
        occupyingObject = obj;
        occupyingObjectType = type;
        isOccupied = obj != null;
    }
    
    public void ClearOccupyingObject()
    {
        occupyingObject = null;
        occupyingObjectType = "";
        isOccupied = false;
    }
}
