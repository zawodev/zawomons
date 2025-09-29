using UnityEngine;
using UnityEditor;
using Systems.Map.Core;

[CustomEditor(typeof(MapSystem))]
public class MapSystemEditor : Editor
{
    private MapSystem mapSystem;
    
    public override void OnInspectorGUI()
    {
        mapSystem = (MapSystem)target;
        
        DrawDefaultInspector();
        
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Map Generation Tools", EditorStyles.boldLabel);
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Generate Map", GUILayout.Height(30)))
        {
            mapSystem.GenerateMap();
        }
        
        if (GUILayout.Button("Random Seed", GUILayout.Height(30)))
        {
            mapSystem.RandomizeSeed();
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.BeginHorizontal();
        
        if (GUILayout.Button("Reset Map", GUILayout.Height(25)))
        {
            mapSystem.ClearMap();
        }
        
        EditorGUILayout.EndHorizontal();
        
        EditorGUILayout.Space();
        
        // Show current map statistics
        if (Application.isPlaying)
        {
            EditorGUILayout.LabelField("Map Statistics", EditorStyles.boldLabel);
            EditorGUILayout.LabelField($"Seed: {mapSystem.seed}");
            EditorGUILayout.LabelField($"Map Size: {mapSystem.mapWidth} x {mapSystem.mapHeight}");
            EditorGUILayout.LabelField($"Hex Size: {mapSystem.hexSize}");
            EditorGUILayout.LabelField($"Pointy Top: {mapSystem.pointyTop}");
            
            var visibleTiles = mapSystem.GetVisibleTiles();
            EditorGUILayout.LabelField($"Visible Tiles: {visibleTiles.Count}");
        }
        
        // Preview seed generation
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("Preview Tools", EditorStyles.boldLabel);
        
        if (GUILayout.Button("Preview New Random Seed"))
        {
            int newSeed = Random.Range(0, 999999);
            EditorGUILayout.HelpBox($"New seed would be: {newSeed}", MessageType.Info);
        }
    }
}
