using UnityEngine;
using UnityEngine.Rendering;

public static class TowerVisualHelper
{
    public static Material CreateTransparentMaterial(Color color)
    {
        Material mat = new Material(FindUnlitShader());
        ApplyColor(mat, color);
        ApplyTransparency(mat);
        return mat;
    }

    public static Material CreateEmissiveMaterial(Color color)
    {
        Material mat = CreateTransparentMaterial(color);
        if (mat.HasProperty("_EmissionColor"))
            mat.SetColor("_EmissionColor", color * 1.8f);
        return mat;
    }

    private static Shader FindUnlitShader() =>
        Shader.Find("Universal Render Pipeline/Unlit")
        ?? Shader.Find("Unlit/Color")
        ?? Shader.Find("Standard");

    private static void ApplyColor(Material mat, Color color)
    {
        mat.color = color;
        if (mat.HasProperty("_BaseColor")) mat.SetColor("_BaseColor", color);
    }

    private static void ApplyTransparency(Material mat)
    {
        if (mat.HasProperty("_Surface")) mat.SetFloat("_Surface", 1f);
        if (mat.HasProperty("_Blend")) mat.SetFloat("_Blend", 0f);
        if (mat.HasProperty("_SrcBlend")) mat.SetFloat("_SrcBlend", (float)BlendMode.SrcAlpha);
        if (mat.HasProperty("_DstBlend")) mat.SetFloat("_DstBlend", (float)BlendMode.OneMinusSrcAlpha);
        if (mat.HasProperty("_ZWrite")) mat.SetFloat("_ZWrite", 0f);
        mat.renderQueue = 3000;
    }
}
