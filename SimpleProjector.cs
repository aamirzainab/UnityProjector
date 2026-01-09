using UnityEngine;

public class SimpleProjector : MonoBehaviour
{
    [Header("Projection Settings")]
    public Material projectorMaterial;
    public Texture2D projectionTexture;
    public Light spotLight;

    [Header("Projection Parameters")]
    public float fieldOfView = 33f;
    public float aspectRatio = 1f;
    public float nearClip = 0.1f;
    public float farClip = 10f;

    [Header("Aspect Ratio Settings")]
    public bool autoCalculateAspectRatio = true; // Automatically use texture aspect ratio

    [Header("Auto-Test Settings")]
    public bool autoTestAspectRatio = false;
    public bool autoTestFieldOfView = false;
    public float testSpeed = 0.5f;

    [Header("Debug Settings")]
    public bool showDebugInfo = true; // Toggle debug logging
    public bool showDebugEveryFrame = false; // Show every frame or just once

    private float timer = 0f;
    private bool hasLoggedOnce = false;
    private float lastLoggedFOV = -1f;

    void Start()
    {
        if (projectorMaterial == null)
        {
            Debug.LogError("Projector Material is not assigned!");
            return;
        }

        if (projectionTexture == null)
        {
            Debug.LogError("Projection Texture is not assigned!");
            return;
        }

        // Auto-calculate aspect ratio from texture
        if (autoCalculateAspectRatio && projectionTexture != null)
        {
            aspectRatio = (float)projectionTexture.width / (float)projectionTexture.height;
            Debug.Log($"<color=cyan>Auto-calculated Aspect Ratio: {aspectRatio:F3} (Texture: {projectionTexture.width}x{projectionTexture.height})</color>");
        }

        PrintDebugInfo();
        GameObject wall = GameObject.Find("ProjectionWall");
        if (wall != null)
        {
            LogTargetWallInfo(wall);
        }
        else
        {
            Debug.LogWarning("Could not find 'ProjectionWall' - check the name!");
        }
    }

void LateUpdate()
{
    if (projectorMaterial == null || projectionTexture == null)
        return;

    // AUTO-TEST: Cycle through Field of View
    if (autoTestFieldOfView)
    {
        timer += Time.deltaTime * testSpeed;
        fieldOfView = Mathf.PingPong(timer * 30f, 90f) + 30f;
    }
    // AUTO-TEST: Cycle through aspect ratios
    else if (autoTestAspectRatio)
    {
        timer += Time.deltaTime * testSpeed;
        aspectRatio = Mathf.PingPong(timer, 2.0f) + 0.5f;
    }
    // Auto-calculate aspect ratio from texture if enabled
    else if (autoCalculateAspectRatio && projectionTexture != null)
    {
        aspectRatio = (float)projectionTexture.width / (float)projectionTexture.height;
    }

    // Set the projection texture
    projectorMaterial.SetTexture("_MainTex", projectionTexture);

    // Build projection matrix
    Matrix4x4 projectionMatrix = Matrix4x4.Perspective(
        fieldOfView,
        aspectRatio,
        nearClip,
        farClip
    );

    // DEBUG: Log when FOV changes significantly
    if (Mathf.Abs(fieldOfView - lastLoggedFOV) > 1f)
    {
        Debug.Log($"<color=yellow>━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━</color>");
        Debug.Log($"<color=yellow>FOV Changed to: {fieldOfView:F2}°</color>");
        Debug.Log($"<color=cyan>Projection Matrix m00: {projectionMatrix.m00:F4}</color>");
        Debug.Log($"<color=cyan>Projection Matrix m11: {projectionMatrix.m11:F4}</color>");
        Debug.Log($"<color=yellow>━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━━</color>");
        lastLoggedFOV = fieldOfView;
    }

    // Build view matrix (world to projector space)
    Matrix4x4 viewMatrix = transform.worldToLocalMatrix;

    // Combine them
    Matrix4x4 finalMatrix = projectionMatrix * viewMatrix;

    // Send to shader
    projectorMaterial.SetMatrix("_ProjectorMatrix", finalMatrix);
    projectorMaterial.SetVector("_ProjectorPos", transform.position);
    projectorMaterial.SetFloat("_Brightness", projectorMaterial.GetFloat("_Brightness"));
    projectorMaterial.SetFloat("_FalloffPower", projectorMaterial.GetFloat("_FalloffPower"));

    // Debug output
    if (showDebugInfo && (showDebugEveryFrame || !hasLoggedOnce))
    {
        PrintDebugInfo();
        hasLoggedOnce = true;
    }
}

    void PrintDebugInfo()
    {
        Debug.Log("════════════════════════════════════════════════");
        Debug.Log("<color=yellow><b>PROJECTOR DEBUG INFO</b></color>");
        Debug.Log("════════════════════════════════════════════════");

        // Projector Transform
        Debug.Log("<color=cyan><b>PROJECTOR TRANSFORM:</b></color>");
        Debug.Log($"  Position: {transform.position}");
        Debug.Log($"  Rotation: {transform.rotation.eulerAngles}");
        Debug.Log($"  Forward Direction: {transform.forward}");

        // Texture Info
        if (projectionTexture != null)
        {
            Debug.Log("<color=cyan><b>TEXTURE INFO:</b></color>");
            Debug.Log($"  Texture Name: {projectionTexture.name}");
            Debug.Log($"  Dimensions: {projectionTexture.width} x {projectionTexture.height}");
            Debug.Log($"  Texture Aspect Ratio: {(float)projectionTexture.width / projectionTexture.height:F3}");
        }

        // Projection Parameters
        Debug.Log("<color=cyan><b>PROJECTION PARAMETERS:</b></color>");
        Debug.Log($"  Field of View: {fieldOfView:F2}°");
        Debug.Log($"  Aspect Ratio (used): {aspectRatio:F3}");
        Debug.Log($"  Near Clip: {nearClip}");
        Debug.Log($"  Far Clip: {farClip}");
        Debug.Log($"  Auto-Calculate Aspect: {autoCalculateAspectRatio}");

        // Material Info
        if (projectorMaterial != null)
        {
            Debug.Log("<color=cyan><b>MATERIAL SETTINGS:</b></color>");
            Debug.Log($"  Brightness: {projectorMaterial.GetFloat("_Brightness")}");
            Debug.Log($"  Falloff Power: {projectorMaterial.GetFloat("_FalloffPower")}");

            Color surfaceColor = projectorMaterial.GetColor("_SurfaceColor");
            Debug.Log($"  Surface Color: R={surfaceColor.r:F2}, G={surfaceColor.g:F2}, B={surfaceColor.b:F2}");
        }

        Debug.Log("════════════════════════════════════════════════");
    }

    // Method to find and log target wall info
    public void LogTargetWallInfo(GameObject wall)
    {
        if (wall == null)
        {
            Debug.LogWarning("No wall GameObject provided!");
            return;
        }

        Debug.Log("<color=green><b>TARGET WALL INFO:</b></color>");
        Debug.Log($"  Wall Name: {wall.name}");
        Debug.Log($"  Position: {wall.transform.position}");
        Debug.Log($"  Scale: {wall.transform.localScale}");
        Debug.Log($"  Wall Aspect Ratio: {wall.transform.localScale.x / wall.transform.localScale.y:F3}");

        // Calculate distance
        float distance = Vector3.Distance(transform.position, wall.transform.position);
        Debug.Log($"  Distance from Projector: {distance:F2} units");

        // Calculate angle
        Vector3 directionToWall = (wall.transform.position - transform.position).normalized;
        float angle = Vector3.Angle(transform.forward, directionToWall);
        Debug.Log($"  Angle from Projector Forward: {angle:F2}°");
    }
}