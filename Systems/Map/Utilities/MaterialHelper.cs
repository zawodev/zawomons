using UnityEngine;

namespace Systems.Map.Utilities {
    // Helper script to create materials for hex tiles
    public static class MaterialHelper
{
    public static Material CreateHexMaterial(Color baseColor, string name = "HexMaterial")
    {
        Material material = new Material(Shader.Find("Standard"));
        material.name = name;
        material.color = baseColor;
        material.SetFloat("_Metallic", 0.0f);
        material.SetFloat("_Glossiness", 0.3f);
        
        return material;
    }
    
    public static Material CreateOutlineMaterial(Color outlineColor, string name = "OutlineMaterial")
    {
        // For outline, we'll use an unlit shader
        Material material = new Material(Shader.Find("Unlit/Color"));
        material.name = name;
        material.color = outlineColor;
        
        return material;
    }
}
}
