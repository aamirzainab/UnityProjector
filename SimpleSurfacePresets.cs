using UnityEngine;

[System.Serializable]
public class SurfacePreset
{
    public string name;
    public Material baseMaterial; // The actual material to use (grass, concrete, etc.)
    public Color surfaceColor;
    public float reflectance;
    public float roughness;
    public float specular;
    public float specularPower;

    public SurfacePreset(string name, Material material, float reflectance, float roughness, float specular, float specularPower)
    {
        this.name = name;
        this.baseMaterial = material;
        this.surfaceColor = material != null ? material.color : Color.white;
        this.reflectance = reflectance;
        this.roughness = roughness;
        this.specular = specular;
        this.specularPower = specularPower;
    }
}

public class SimpleSurfacePresets : MonoBehaviour
{
    [Header("Target Components")]
    public Material targetMaterial; // For projector properties (kept for compatibility)
    public MeshRenderer meshRenderer; // The mesh renderer to swap materials on

    [Header("Base Materials - Assign These!")]
    public Material grassMaterial;
    public Material concreteMaterial;
    public Material woodMaterial;
    public Material tileMaterial;
    public Material carpetMaterial;
    public Material metalMaterial;

    [Header("Preset Selection")]
    public int presetIndex = 0;

    [Header("Auto-Test Settings")]
    public bool autoTestSurfaces = false; // Automatically cycle through surfaces
    public float testSpeed = 2.0f; // Seconds between surface changes
    public bool pingPongMode = true; // Go back and forth, or loop

    [Header("Debug Settings")]
    public bool showDebugInfo = true; // Toggle debug logging
    public bool showDebugEveryFrame = false; // Show every frame or just on change
    public bool showDetailedLogs = true; // Show detailed surface properties

    private float timer = 0f;
    private int direction = 1; // For ping-pong mode
    private int lastLoggedIndex = -1;

    // Define some basic surface types
    private SurfacePreset[] presets;

    void Start()
    {
        // Build presets from assigned materials
        presets = new SurfacePreset[]
        {
            // Name, Material, Reflectance, Roughness, Specular, SpecularPower
            new SurfacePreset("Grass", grassMaterial, 0.15f, 0.95f, 0.02f, 4f),
            new SurfacePreset("Concrete", concreteMaterial, 0.35f, 0.75f, 0.08f, 12f),
            new SurfacePreset("Wood Floor", woodMaterial, 0.45f, 0.4f, 0.3f, 32f),
            new SurfacePreset("Glossy Tile", tileMaterial, 0.75f, 0.1f, 0.8f, 96f),
            new SurfacePreset("Dark Carpet", carpetMaterial, 0.2f, 0.98f, 0.01f, 2f),
            new SurfacePreset("Polished Metal", metalMaterial, 0.9f, 0.15f, 1.5f, 96f),
        };

        // Try to auto-find MeshRenderer if not assigned
        if (meshRenderer == null)
        {
            meshRenderer = GetComponent<MeshRenderer>();
        }

        if (meshRenderer == null)
        {
            Debug.LogError("SimpleSurfacePresets: No MeshRenderer found! Please assign one.");
            return;
        }

        ApplyPreset(presetIndex);
        PrintStartupInfo();
    }

    void Update()
    {
        if (targetMaterial == null) return;

        // AUTO-TEST: Cycle through surfaces
        if (autoTestSurfaces)
        {
            timer += Time.deltaTime;

            if (timer >= testSpeed)
            {
                timer = 0f;

                if (pingPongMode)
                {
                    // Ping-pong through presets
                    presetIndex += direction;

                    if (presetIndex >= presets.Length - 1)
                    {
                        presetIndex = presets.Length - 1;
                        direction = -1;
                    }
                    else if (presetIndex <= 0)
                    {
                        presetIndex = 0;
                        direction = 1;
                    }
                }
                else
                {
                    // Loop through presets
                    presetIndex = (presetIndex + 1) % presets.Length;
                }

                ApplyPreset(presetIndex);
            }
        }

        // Debug output
        if (showDebugInfo && (showDebugEveryFrame || presetIndex != lastLoggedIndex))
        {
            if (presetIndex != lastLoggedIndex)
            {
                PrintSurfaceInfo();
                lastLoggedIndex = presetIndex;
            }
        }
    }

    public void ApplyPreset(int index)
    {
        if (presets == null || presets.Length == 0)
        {
            Debug.LogError("SimpleSurfacePresets: Presets not initialized!");
            return;
        }

        if (index < 0 || index >= presets.Length)
        {
            Debug.LogWarning($"SimpleSurfacePresets: Invalid preset index {index}! Valid range: 0-{presets.Length - 1}");
            return;
        }

        presetIndex = index;
        SurfacePreset preset = presets[index];

        // SWAP THE BASE MATERIAL on MeshRenderer
        if (meshRenderer != null && preset.baseMaterial != null)
        {
            meshRenderer.material = preset.baseMaterial;
            Debug.Log($"<color=green>Swapped to material: {preset.baseMaterial.name}</color>");
        }
        else if (preset.baseMaterial == null)
        {
            Debug.LogWarning($"Preset '{preset.name}' has no material assigned!");
        }

        // Also update projector material properties if targetMaterial is assigned
        // (for projection overlay - we'll add this later if needed)
        if (targetMaterial != null)
        {
            targetMaterial.SetFloat("_Reflectance", preset.reflectance);
            targetMaterial.SetFloat("_Roughness", preset.roughness);
            targetMaterial.SetFloat("_Specular", preset.specular);
            targetMaterial.SetFloat("_SpecularPower", preset.specularPower);
        }

        // Log the change
        if (showDebugInfo)
        {
            PrintSurfaceInfo();
        }
    }

    void PrintStartupInfo()
    {
        Debug.Log("╔════════════════════════════════════════════════════╗");
        Debug.Log("║  <color=green><b>SURFACE PRESETS INITIALIZED</b></color>");
        Debug.Log("╠════════════════════════════════════════════════════╣");
        Debug.Log($"║  Total Presets: {presets.Length}");
        Debug.Log($"║  Auto-Test: {(autoTestSurfaces ? "ON" : "OFF")}");
        if (autoTestSurfaces)
        {
            Debug.Log($"║  Test Speed: {testSpeed:F1}s per surface");
            Debug.Log($"║  Mode: {(pingPongMode ? "Ping-Pong" : "Loop")}");
        }
        Debug.Log("╠════════════════════════════════════════════════════╣");
        Debug.Log("║  Available Surfaces:");
        for (int i = 0; i < presets.Length; i++)
        {
            Debug.Log($"║    [{i}] {presets[i].name}");
        }
        Debug.Log("╚════════════════════════════════════════════════════╝");
    }

    void PrintSurfaceInfo()
    {
        SurfacePreset preset = presets[presetIndex];

        Debug.Log("┌────────────────────────────────────────────────────┐");
        Debug.Log($"│ <color=cyan><b>SURFACE CHANGED: {preset.name}</b></color>");
        Debug.Log($"│ <color=yellow>Index: {presetIndex}/{presets.Length - 1}</color>");
        Debug.Log("├────────────────────────────────────────────────────┤");

        if (showDetailedLogs)
        {
            if (preset.baseMaterial != null)
            {
                Debug.Log("│ <color=green><b>BASE MATERIAL:</b></color>");
                Debug.Log($"│   Material: {preset.baseMaterial.name}");
                Debug.Log($"│   Shader: {preset.baseMaterial.shader.name}");
            }
            else
            {
                Debug.Log("│ <color=gray>No base material assigned</color>");
            }

            Debug.Log("│ <color=green><b>COLOR:</b></color>");
            Debug.Log($"│   RGB: ({preset.surfaceColor.r:F3}, {preset.surfaceColor.g:F3}, {preset.surfaceColor.b:F3})");
            Debug.Log($"│   Hex: #{ColorUtility.ToHtmlStringRGB(preset.surfaceColor)}");

            Debug.Log("│ <color=green><b>LIGHT INTERACTION:</b></color>");
            Debug.Log($"│   Reflectance: {preset.reflectance:F2} ({GetReflectanceDescription(preset.reflectance)})");
            Debug.Log($"│   Absorption: {(1f - preset.reflectance):F2}");

            Debug.Log("│ <color=green><b>SURFACE FINISH:</b></color>");
            Debug.Log($"│   Roughness: {preset.roughness:F2} ({GetRoughnessDescription(preset.roughness)})");
            Debug.Log($"│   Smoothness: {(1f - preset.roughness):F2}");

            Debug.Log("│ <color=green><b>SPECULAR:</b></color>");
            Debug.Log($"│   Intensity: {preset.specular:F2} ({GetSpecularDescription(preset.specular)})");
            Debug.Log($"│   Sharpness: {preset.specularPower:F0} ({GetSharpnessDescription(preset.specularPower)})");

            Debug.Log("│ <color=green><b>EXPECTED APPEARANCE:</b></color>");
            Debug.Log($"│   Projection Brightness: {GetBrightnessEstimate(preset)}");
            Debug.Log($"│   Highlight Visibility: {GetHighlightEstimate(preset)}");
        }
        else
        {
            Debug.Log($"│ Reflectance: {preset.reflectance:F2} | Roughness: {preset.roughness:F2}");
            Debug.Log($"│ Specular: {preset.specular:F2} | Power: {preset.specularPower:F0}");
        }

        Debug.Log("└────────────────────────────────────────────────────┘");
    }

    // Helper descriptions for better understanding
    string GetReflectanceDescription(float value)
    {
        if (value > 0.8f) return "Very High - Excellent projection";
        if (value > 0.6f) return "High - Good projection";
        if (value > 0.4f) return "Medium - Moderate projection";
        if (value > 0.2f) return "Low - Dim projection";
        return "Very Low - Poor projection";
    }

    string GetRoughnessDescription(float value)
    {
        if (value > 0.9f) return "Very Rough - Matte, diffuse";
        if (value > 0.7f) return "Rough - Mostly matte";
        if (value > 0.5f) return "Semi-Rough - Satin finish";
        if (value > 0.3f) return "Semi-Smooth - Some gloss";
        if (value > 0.1f) return "Smooth - Glossy";
        return "Mirror-like - Highly reflective";
    }

    string GetSpecularDescription(float value)
    {
        if (value > 1.5f) return "Very Strong - Mirror-like";
        if (value > 0.8f) return "Strong - Glossy";
        if (value > 0.3f) return "Moderate - Semi-gloss";
        if (value > 0.1f) return "Weak - Subtle";
        return "None - Matte";
    }

    string GetSharpnessDescription(float value)
    {
        if (value > 80f) return "Very Sharp - Tight highlights";
        if (value > 40f) return "Sharp - Defined highlights";
        if (value > 20f) return "Moderate - Soft highlights";
        if (value > 10f) return "Soft - Diffuse highlights";
        return "Very Soft - Barely visible";
    }

    string GetBrightnessEstimate(SurfacePreset preset)
    {
        float brightness = preset.reflectance * (1f - preset.roughness * 0.3f);
        if (brightness > 0.7f) return "★★★★★ Very Bright";
        if (brightness > 0.5f) return "★★★★☆ Bright";
        if (brightness > 0.3f) return "★★★☆☆ Moderate";
        if (brightness > 0.15f) return "★★☆☆☆ Dim";
        return "★☆☆☆☆ Very Dim";
    }

    string GetHighlightEstimate(SurfacePreset preset)
    {
        float highlight = preset.specular * (1f - preset.roughness);
        if (highlight > 1.0f) return "Very Strong";
        if (highlight > 0.5f) return "Strong";
        if (highlight > 0.2f) return "Moderate";
        if (highlight > 0.05f) return "Weak";
        return "None";
    }

    public string[] GetPresetNames()
    {
        string[] names = new string[presets.Length];
        for (int i = 0; i < presets.Length; i++)
        {
            names[i] = presets[i].name;
        }
        return names;
    }
}
