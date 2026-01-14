using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
[ExecuteInEditMode]
public class DeformableSurface : MonoBehaviour
{
    [Header("Mesh Settings")]
    public int width = 20; // Number of quads wide
    public int height = 20; // Number of quads tall
    public float size = 10f; // Total size in world units

    [Header("Deformation Settings")]
    public float deformationHeight = 1.0f; // How much to deform (0 = flat)
    public float noiseScale = 0.3f; // Perlin noise scale (smaller = smoother)
    public Vector2 noiseOffset = Vector2.zero; // Offset for variation

    [Header("Auto-Update")]
    public bool autoUpdate = false; // Update mesh in real-time
    public bool animateDeformation = false; // Animate the deformation
    public float animationSpeed = 1.0f;

    private Mesh mesh;
    private Vector3[] baseVertices; // Original flat vertices
    private Vector3[] vertices; // Current deformed vertices
    private float animationTime = 0f;

    void Start()
    {
        GenerateMesh();
    }

    void Update()
    {
        if (animateDeformation)
        {
            animationTime += Time.deltaTime * animationSpeed;
            UpdateDeformation();
        }
        else if (autoUpdate)
        {
            UpdateDeformation();
        }
    }

    [ContextMenu("Generate Mesh")]
    public void GenerateMesh()
    {
        mesh = new Mesh();
        mesh.name = "Deformable Surface";
        GetComponent<MeshFilter>().mesh = mesh;

        CreateMeshData();
        UpdateDeformation();

        Debug.Log($"<color=green>Generated mesh: {width}x{height} = {vertices.Length} vertices</color>");
    }

    void CreateMeshData()
    {
        // Calculate vertices
        int vertexCount = (width + 1) * (height + 1);
        baseVertices = new Vector3[vertexCount];
        vertices = new Vector3[vertexCount];
        Vector2[] uvs = new Vector2[vertexCount];

        float cellSizeX = size / width;
        float cellSizeY = size / height;

        // Create grid of vertices
        for (int y = 0, i = 0; y <= height; y++)
        {
            for (int x = 0; x <= width; x++, i++)
            {
                float xPos = x * cellSizeX - size * 0.5f;
                float yPos = y * cellSizeY - size * 0.5f;

                baseVertices[i] = new Vector3(xPos, 0, yPos);
                vertices[i] = baseVertices[i];

                // UVs for texture mapping
                uvs[i] = new Vector2((float)x / width, (float)y / height);
            }
        }

        // Create triangles
        int triangleCount = width * height * 6;
        int[] triangles = new int[triangleCount];

        int vert = 0;
        int tris = 0;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // First triangle
                triangles[tris + 0] = vert + 0;
                triangles[tris + 1] = vert + width + 1;
                triangles[tris + 2] = vert + 1;

                // Second triangle
                triangles[tris + 3] = vert + 1;
                triangles[tris + 4] = vert + width + 1;
                triangles[tris + 5] = vert + width + 2;

                vert++;
                tris += 6;
            }
            vert++;
        }

        mesh.Clear();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;
        mesh.RecalculateNormals();
    }

    void UpdateDeformation()
    {
        if (mesh == null || baseVertices == null) return;

        float time = animateDeformation ? animationTime : 0f;

        // Deform each vertex
        for (int i = 0; i < vertices.Length; i++)
        {
            Vector3 basePos = baseVertices[i];

            // Calculate Perlin noise
            float noiseX = (basePos.x * noiseScale) + noiseOffset.x + time;
            float noiseZ = (basePos.z * noiseScale) + noiseOffset.y + time;
            float noiseValue = Mathf.PerlinNoise(noiseX, noiseZ);

            // Apply deformation to Y axis
            float height = (noiseValue - 0.5f) * 2f * deformationHeight;
            vertices[i] = new Vector3(basePos.x, height, basePos.z);
        }

        mesh.vertices = vertices;
        mesh.RecalculateNormals();
    }

    void OnValidate()
    {
        // Clamp values to reasonable ranges
        width = Mathf.Max(2, width);
        height = Mathf.Max(2, height);
        size = Mathf.Max(0.1f, size);
        noiseScale = Mathf.Max(0.01f, noiseScale);
        UpdateDeformation();
    }

    void OnDrawGizmos()
    {
        // Draw bounds
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireCube(transform.position, new Vector3(size, 0.1f, size));
    }
}