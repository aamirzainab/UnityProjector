using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class ShaderPropertyResetter : MonoBehaviour
{
    private Renderer rend;

    void Start()
    {
        rend = GetComponent<Renderer>();
    }

    public void ResetShaderToDefaults()
    {
        if (rend == null)
            rend = GetComponent<Renderer>();

        if (rend == null || rend.sharedMaterial == null)
        {
            Debug.LogWarning("No renderer or material found!");
            return;
        }

        Material mat = rend.sharedMaterial;

        // Projection Properties
        mat.SetFloat("_Brightness", 5f);
        mat.SetFloat("_FalloffPower", 0.1f);

        // Projector Realism
        mat.SetFloat("_BlackLevel", 0.08f);
        mat.SetFloat("_CenterHotspotIntensity", 0.15f);
        mat.SetFloat("_CenterHotspotSize", 1.0f);
        mat.SetColor("_ColorTemperature", new Color(0.98f, 0.99f, 1.0f, 1.0f));

        // Base Material
        // mat.SetColor("_BaseColorTint", Color.white);
        // mat.SetFloat("_BumpScale", 1.0f);
        // mat.SetFloat("_HeightScale", 0.5f);
        // mat.SetFloat("_AOStrength", 1.0f);
        // mat.SetFloat("_MetallicStrength", 0.0f);

        // Surface Material
        // mat.SetColor("_SurfaceAlbedo", new Color(0.8f, 0.8f, 0.8f, 1f));
        // mat.SetFloat("_Reflectance", 0.7f);
        // mat.SetFloat("_Roughness", 0.5f);
        // mat.SetFloat("_Specular", 0.1f);
        // mat.SetFloat("_SpecularPower", 32f);
        // mat.SetFloat("_ParallaxScale", 0.5f);

        Debug.Log("<color=green>Shader properties reset to defaults!</color>");
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(ShaderPropertyResetter))]
public class ShaderPropertyResetterEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        ShaderPropertyResetter resetter = (ShaderPropertyResetter)target;

        GUILayout.Space(10);

        // Big, prominent button
        GUI.backgroundColor = Color.cyan;
        if (GUILayout.Button("Reset Shader to Defaults", GUILayout.Height(40)))
        {
            resetter.ResetShaderToDefaults();
        }
        GUI.backgroundColor = Color.white;

        GUILayout.Space(5);
        EditorGUILayout.HelpBox("This will reset all shader properties to their default values as defined in the shader.", MessageType.Info);
    }
}
#endif