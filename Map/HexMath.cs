using UnityEngine;
using System.Collections.Generic;

public static class HexMath
{
    public static readonly Vector3[] HexDirections = {
        new Vector3(1f, 0f, 0f),
        new Vector3(0.5f, 0f, Mathf.Sqrt(3f) * 0.5f),
        new Vector3(-0.5f, 0f, Mathf.Sqrt(3f) * 0.5f),
        new Vector3(-1f, 0f, 0f),
        new Vector3(-0.5f, 0f, -Mathf.Sqrt(3f) * 0.5f),
        new Vector3(0.5f, 0f, -Mathf.Sqrt(3f) * 0.5f)
    };
    
    public static readonly Vector2Int[] HexDirectionsGrid = {
        new Vector2Int(1, 0),
        new Vector2Int(0, 1),
        new Vector2Int(-1, 1),
        new Vector2Int(-1, 0),
        new Vector2Int(0, -1),
        new Vector2Int(1, -1)
    };
    
    public static Vector3 HexToWorldPosition(Vector2Int hexCoord, float hexSize, bool pointyTop = true)
    {
        float x, y;
        
        if (pointyTop)
        {
            // For pointy-top hexes: every other row offset by half hex width
            x = hexSize * Mathf.Sqrt(3f) * (hexCoord.x + (hexCoord.y % 2) * 0.5f);
            y = hexSize * 1.5f * hexCoord.y;
        }
        else
        {
            // For flat-top hexes: every other column offset by half hex height
            x = hexSize * 1.5f * hexCoord.x;
            y = hexSize * Mathf.Sqrt(3f) * (hexCoord.y + (hexCoord.x % 2) * 0.5f);
        }
        
        return new Vector3(x, y, 0); // Z=0 for 2D top-down view
    }
    
    public static Vector2Int WorldToHexPosition(Vector3 worldPos, float hexSize, bool pointyTop = true)
    {
        float x, y;
        
        if (pointyTop)
        {
            x = (Mathf.Sqrt(3f) / 3f * worldPos.x - 1f / 3f * worldPos.y) / hexSize;
            y = (2f / 3f * worldPos.y) / hexSize;
        }
        else
        {
            x = (2f / 3f * worldPos.x) / hexSize;
            y = (-1f / 3f * worldPos.x + Mathf.Sqrt(3f) / 3f * worldPos.y) / hexSize;
        }
        
        return HexRound(x, y);
    }
    
    public static Vector2Int HexRound(float x, float y)
    {
        float z = -x - y;
        
        int rx = Mathf.RoundToInt(x);
        int ry = Mathf.RoundToInt(y);
        int rz = Mathf.RoundToInt(z);
        
        float xDiff = Mathf.Abs(rx - x);
        float yDiff = Mathf.Abs(ry - y);
        float zDiff = Mathf.Abs(rz - z);
        
        if (xDiff > yDiff && xDiff > zDiff)
        {
            rx = -ry - rz;
        }
        else if (yDiff > zDiff)
        {
            ry = -rx - rz;
        }
        
        return new Vector2Int(rx, ry);
    }
    
    public static int HexDistance(Vector2Int a, Vector2Int b)
    {
        return (Mathf.Abs(a.x - b.x) + Mathf.Abs(a.x + a.y - b.x - b.y) + Mathf.Abs(a.y - b.y)) / 2;
    }
    
    public static List<Vector2Int> GetHexNeighbors(Vector2Int center)
    {
        List<Vector2Int> neighbors = new List<Vector2Int>();
        
        foreach (Vector2Int direction in HexDirectionsGrid)
        {
            neighbors.Add(center + direction);
        }
        
        return neighbors;
    }
    
    public static List<Vector2Int> GetHexesInRange(Vector2Int center, int range)
    {
        List<Vector2Int> results = new List<Vector2Int>();
        
        for (int x = -range; x <= range; x++)
        {
            int yMin = Mathf.Max(-range, -x - range);
            int yMax = Mathf.Min(range, -x + range);
            
            for (int y = yMin; y <= yMax; y++)
            {
                results.Add(center + new Vector2Int(x, y));
            }
        }
        
        return results;
    }
    
    public static Vector3[] GenerateHexMesh(float size, bool pointyTop = true)
    {
        Vector3[] vertices = new Vector3[7]; // Center + 6 vertices
        vertices[0] = Vector3.zero; // Center
        
        float angle = pointyTop ? 30f : 0f;
        
        for (int i = 0; i < 6; i++)
        {
            float angleRad = (angle + i * 60f) * Mathf.Deg2Rad;
            vertices[i + 1] = new Vector3(
                Mathf.Cos(angleRad) * size,
                Mathf.Sin(angleRad) * size, // Y axis for 2D top-down
                0f // Z = 0 for 2D
            );
        }
        
        return vertices;
    }
    
    public static int[] GenerateHexTriangles()
    {
        int[] triangles = new int[18]; // 6 triangles * 3 vertices each
        
        for (int i = 0; i < 6; i++)
        {
            triangles[i * 3] = 0; // Center vertex
            triangles[i * 3 + 1] = (i + 1) % 6 + 1; // Flip winding order for correct orientation
            triangles[i * 3 + 2] = i + 1;
        }
        
        return triangles;
    }
}
