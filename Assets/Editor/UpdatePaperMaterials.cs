using UnityEditor;
using UnityEngine;

public class UpdatePaperMaterials : EditorWindow
{
    [MenuItem("Tools/Update Paper Materials")]
    public static void UpdateMaterials()
    {
        string[] guids = AssetDatabase.FindAssets("t:Material");
        int updatedCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            Material mat = AssetDatabase.LoadAssetAtPath<Material>(path);

            if (mat != null && mat.shader != null && mat.shader.name == "UI/Paper Rough")
            {
                // Edge Settings
                mat.SetFloat("_EdgeCutoff", 0.035f);
                mat.SetFloat("_EdgeSoftness", 0.005f);
                mat.SetFloat("_EdgeNoiseScale", 45f);
                mat.SetFloat("_EdgeNoiseStrength", 0.025f);
                mat.SetFloat("_TearNoiseScale", 12f);
                mat.SetFloat("_TearNoiseStrength", 0.08f);

                // Burn and Rim
                mat.SetColor("_BurnColor", new Color(0.25f, 0.12f, 0.05f, 1.0f));
                mat.SetFloat("_BurnWidth", 0.12f);
                mat.SetFloat("_BurnStrength", 0.85f);

                mat.SetColor("_RimColor", new Color(0.9f, 0.82f, 0.65f, 1.0f));
                mat.SetFloat("_RimWidth", 0.006f);

                // Details
                mat.SetFloat("_GrainStrength", 0.06f);
                
                // Shadow
                mat.SetColor("_ShadowColor", new Color(0f, 0f, 0f, 0.5f));
                mat.SetVector("_ShadowOffset", new Vector4(0.015f, -0.015f, 0, 0));
                mat.SetFloat("_ShadowSoftness", 0.015f);

                EditorUtility.SetDirty(mat);
                updatedCount++;
            }
        }

        AssetDatabase.SaveAssets();
        Debug.Log($"Updated {updatedCount} Paper Rough materials!");
    }
}