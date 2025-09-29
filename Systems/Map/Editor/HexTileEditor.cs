using UnityEngine;
using UnityEditor;
using Systems.Map.Core;
using Systems.Map.Models;

[CustomEditor(typeof(HexTile))]
public class HexTileEditor : Editor
{
    private HexTile hexTile;
    
    public override void OnInspectorGUI()
    {
        hexTile = (HexTile)target;
        
        EditorGUI.BeginChangeCheck();
        DrawDefaultInspector();
        
        bool valuesChanged = EditorGUI.EndChangeCheck();
        
        if (valuesChanged)
        {
            // Automatically regenerate mesh when values change
            hexTile.GenerateMesh();
            EditorUtility.SetDirty(hexTile);
        }
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Test Tools", EditorStyles.boldLabel);
        
        // Show current state
        EditorGUI.BeginDisabledGroup(true);
        EditorGUILayout.EnumPopup("Current State", hexTile.GetCurrentState());
        EditorGUI.EndDisabledGroup();
        
        EditorGUILayout.Space();
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Test Base"))
        {
            hexTile.SetState(HexTile.TileState.Base);
            EditorUtility.SetDirty(hexTile);
        }
        
        if (GUILayout.Button("Test Hover"))
        {
            hexTile.SetState(HexTile.TileState.Hovered);
            EditorUtility.SetDirty(hexTile);
        }
        
        if (GUILayout.Button("Test Selected"))
        {
            hexTile.SetState(HexTile.TileState.Selected);
            EditorUtility.SetDirty(hexTile);
        }
        
        EditorGUILayout.EndHorizontal();
        
        // Apply Base Material button
        EditorGUILayout.Space();
        if (GUILayout.Button("Apply Base Material", GUILayout.Height(25)))
        {
            if (hexTile.baseMaterial != null)
            {
                hexTile.GetComponent<MeshRenderer>().sharedMaterial = hexTile.baseMaterial;
                hexTile.SetState(HexTile.TileState.Base);
                EditorUtility.SetDirty(hexTile);
            }
        }
        
        // Show tile information if available
        if (hexTile.tileData != null)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Tile Information", EditorStyles.boldLabel);
            
            EditorGUI.BeginDisabledGroup(true);
            EditorGUILayout.Vector2IntField("Grid Position", hexTile.tileData.gridPosition);
            EditorGUILayout.Vector3Field("World Position", hexTile.tileData.worldPosition);
            
            if (hexTile.tileData.biomeData != null)
            {
                EditorGUILayout.ObjectField("Biome", hexTile.tileData.biomeData, typeof(BiomeData), false);
                EditorGUILayout.ColorField("Biome Color", hexTile.tileData.biomeData.biomeColor);
            }
            
            EditorGUILayout.Toggle("Is Selected", hexTile.tileData.isSelected);
            EditorGUILayout.Toggle("Is Hovered", hexTile.tileData.isHovered);
            EditorGUI.EndDisabledGroup();
        }
        else
        {
            EditorGUILayout.Space();
            EditorGUILayout.HelpBox("This is a prefab or uninitialized tile. Use Tools → Map System → Create Default Materials to create materials.", MessageType.Info);
        }
    }
}
