using UnityEngine;

public class TerrainGenerator : MonoBehaviour
{
    // Public variables to customize the terrain in the Inspector
    public int terrain_size = 85;
    public float scale = 10.0f;
    public float[] noiseFrequencies = { 2f, 0.5f, 0.25f };
    public float[] noiseAmplitudes = { 1f, 0.5f, 0.25f };
    public int numberOfTrees = 10;
    public GameObject prefabTree;
    public Material waterMaterial;
    public GameObject prefabHotBalloon;

    private MeshFilter meshFilter;
    private MeshRenderer meshRenderer;
    private MeshCollider meshCollider;
    private GameObject[,] terrain_matrix = new GameObject[3, 3];
    int terrain_x = 0;
    int terrain_z = 0;

    void Start()
    {
        create_new_terrain();
    }

    void Update() {
        Vector3 cam_pos = Camera.main.transform.position;
        // Debug.LogFormat ("x z: {0} {1}", terrain_x, terrain_z);
        if (cam_pos.z > (terrain_z + 1) * terrain_size) {
            terrain_z++;
            for (int x = 0; x < 3; x++) {
                GameObject terrain = terrain_matrix[0, x];
                Destroy(terrain);
            }
            for (int z = 1; z < 3; z++) {
                for (int x = 0; x < 3; x++) {
                    terrain_matrix[z - 1, x] = terrain_matrix[z, x];
                }
            }
            for (int x = 0; x < 3; x++) {
                terrain_matrix[2, x] = null;
            }
        } else if (cam_pos.z < terrain_z * terrain_size) {
            terrain_z--;
            for (int x = 0; x < 3; x++) {
                Destroy(terrain_matrix[2, x]);
            }
            for (int z = 1; z >= 0; z--) {
                for (int x = 0; x < 3; x++) {
                    terrain_matrix[z + 1, x] = terrain_matrix[z, x];
                }
            }
            for (int x = 0; x < 3; x++) {
                terrain_matrix[0, x] = null;
            }
        } else if (cam_pos.x > (terrain_x + 1) * terrain_size) {
            terrain_x++;
            for (int z = 0; z < 3; z++) {
                GameObject terrain = terrain_matrix[z, 0];
                Destroy(terrain);
            }
            for (int z = 0; z < 3; z++) {
                for (int x = 1; x < 3; x++) {
                    terrain_matrix[z, x - 1] = terrain_matrix[z, x];
                }
            }
            for (int z = 0; z < 3; z++) {
                terrain_matrix[z, 2] = null;
            }
        }
        else if (cam_pos.x < terrain_x * terrain_size) {
            terrain_x--;
            for (int z = 0; z < 3; z++) {
                Destroy(terrain_matrix[z, 2]);
            }
            for (int z = 0; z < 3; z++) {
                for (int x = 1; x >= 0; x--) {
                    terrain_matrix[z, x + 1] = terrain_matrix[z, x];
                }
            }
            for (int z = 0; z < 3; z++) {
                terrain_matrix[z, 0] = null;
            }
        }
        if (!terrain_matrix[1, 1]) create_new_terrain();
    }

    void create_new_terrain()
    {
        // Set up the GameObject and its components
        GameObject terrainObject = new GameObject("Terrain Mesh");
        meshFilter = terrainObject.AddComponent<MeshFilter>();
        meshRenderer = terrainObject.AddComponent<MeshRenderer>();
        meshCollider = terrainObject.AddComponent<MeshCollider>();

        // Generate the terrain and assign the mesh
        Mesh terrainMesh = GenerateTerrain();
        meshFilter.mesh = terrainMesh;
        meshCollider.sharedMesh = terrainMesh;

        terrainObject.transform.position = new Vector3 (terrain_size * terrain_x + 1, 0.0f, terrain_size * terrain_z + 1);
        terrain_matrix[1, 1] = terrainObject;

        SpawnTrees(terrainObject);
        SpawnRockPiles(terrainObject, 3, 5);
        SpawnWater(terrainObject);
        SpawnHotBalloon(terrainObject);
    }

    Mesh GenerateTerrain()
    {
        Mesh mesh = new Mesh();
        Texture2D texture = new Texture2D (terrain_size + 1, terrain_size + 1);
		Color[] colors = new Color[(terrain_size + 1) * (terrain_size + 1)];

        int numVertices = (terrain_size + 1) * (terrain_size + 1);
        int numTriangles = terrain_size * terrain_size * 6;

        Vector3[] vertices = new Vector3[numVertices];
        int[] triangles = new int[numTriangles];
        Vector2[] uvs = new Vector2[numVertices];

        float xOffset = 600f;
        float zOffset = 600f;

        int triangleIndex = 0;
        for (int z = 0; z <= terrain_size; z++)
        {
            for (int x = 0; x <= terrain_size; x++)
            {
                int vertexIndex = x + z * (terrain_size + 1);

                float y = PerlinNoiseY(terrain_x * terrain_size + x, terrain_z * terrain_size + z, xOffset, zOffset);
                if (y > 8.0f) {
                    colors[vertexIndex] = new Color (0.9f, 0.9f, 0.9f, 1.0f);
                } else if (y > 4.5f) {
                    colors[vertexIndex] = new Color (0.0f, 0.75f, 0.0f, 1.0f);
                } else if (y > 1.5f) {
                    colors[vertexIndex] = new Color (0.0f, 1.0f, 0.0f, 1.0f);
                } else if (y > 1.2f) {
                    colors[vertexIndex] = new Color (0.82f, 0.67f, 0.42f, 1.0f);
                } else {
                    colors[vertexIndex] = new Color (0.957f, 0.847f, 0.576f, 1.0f);
                }
                // colors[vertexIndex] = new Color (0.0f, 0.75f, 0.0f, 1.0f);

                vertices[vertexIndex] = new Vector3(x, y, z);
                uvs[vertexIndex] = new Vector2((x + 0.5f) / (terrain_size + 1), (z + 0.5f) / (terrain_size + 1));

                // Create triangles
                if (x < terrain_size && z < terrain_size)
                {
                    int topLeft = vertexIndex;
                    int topRight = vertexIndex + 1;
                    int bottomLeft = vertexIndex + terrain_size + 1;
                    int bottomRight = vertexIndex + terrain_size + 2;

                    triangles[triangleIndex++] = topLeft;
                    triangles[triangleIndex++] = bottomLeft;
                    triangles[triangleIndex++] = topRight;

                    triangles[triangleIndex++] = topRight;
                    triangles[triangleIndex++] = bottomLeft;
                    triangles[triangleIndex++] = bottomRight;
                }
            }
        }

        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.uv = uvs;

		texture.SetPixels(colors);
		texture.Apply();
        meshRenderer.material.mainTexture = texture;

        // mesh.RecalculateNormals();
        // Debug.LogFormat ("x: {0}", mesh.normals[23 * 86 + 46]);
        // mesh.RecalculateBounds();

        // seamless action: calculate the average normal of mesh
        Vector3[] normals = new Vector3[numVertices];
        for (int x = 0; x <= terrain_size; x++) {
            for (int z = 0; z <= terrain_size; z++) {
                int vertexIndex = x + z * (terrain_size + 1);
                // we need to find 7 vertex to determine the normal
                float y00 = PerlinNoiseY(terrain_x * terrain_size + x - 1, terrain_z * terrain_size + z - 1, xOffset, zOffset);
                float y01 = PerlinNoiseY(terrain_x * terrain_size + x, terrain_z * terrain_size + z - 1, xOffset, zOffset);
                float y10 = PerlinNoiseY(terrain_x * terrain_size + x - 1, terrain_z * terrain_size + z, xOffset, zOffset);
                float y11 = PerlinNoiseY(terrain_x * terrain_size + x, terrain_z * terrain_size + z, xOffset, zOffset);
                float y12 = PerlinNoiseY(terrain_x * terrain_size + x + 1, terrain_z * terrain_size + z, xOffset, zOffset);
                float y21 = PerlinNoiseY(terrain_x * terrain_size + x, terrain_z * terrain_size + z + 1, xOffset, zOffset);
                float y22 = PerlinNoiseY(terrain_x * terrain_size + x + 1, terrain_z * terrain_size + z + 1, xOffset, zOffset);

                Vector3 p00 = new Vector3(terrain_x * terrain_size + x - 1, y00, terrain_z * terrain_size + z - 1);
                Vector3 p01 = new Vector3(terrain_x * terrain_size + x, y01, terrain_z * terrain_size + z - 1);
                Vector3 p10 = new Vector3(terrain_x * terrain_size + x - 1, y10, terrain_z * terrain_size + z);
                Vector3 p11 = new Vector3(terrain_x * terrain_size + x, y11, terrain_z * terrain_size + z);
                Vector3 p12 = new Vector3(terrain_x * terrain_size + x + 1, y12, terrain_z * terrain_size + z);
                Vector3 p21 = new Vector3(terrain_x * terrain_size + x, y21, terrain_z * terrain_size + z + 1);
                Vector3 p22 = new Vector3(terrain_x * terrain_size + x + 1, y22, terrain_z * terrain_size + z + 1);

                Vector3 n1 = TriNormal(p11, p00, p10);
                Vector3 n2 = TriNormal(p11, p01, p00);
                Vector3 n3 = TriNormal(p11, p12, p01);
                Vector3 n4 = TriNormal(p11, p10, p21);
                Vector3 n5 = TriNormal(p11, p21, p22);
                Vector3 n6 = TriNormal(p11, p22, p12);

                Vector3 normal = n1 + n2 + n3 + n4 + n5 + n6;
                normals[vertexIndex] = normal.normalized;
            }
        }
        // Debug.LogFormat ("x: {0}", normals[0 * 86 + 46]);
        // Debug.LogFormat ("x: {0}", normals[85 * 86 + 46]);
        mesh.normals = normals;
        return mesh;
    }

    float PerlinNoiseY(int x, int z, float xOffset, float zOffset)
    {
        float totalHeight = 0f;

        for (int i = 0; i < noiseFrequencies.Length; i++)
        {
            float freq = noiseFrequencies[i];
            float amp = noiseAmplitudes[i];

            float perlinX = ((float)x / terrain_size) * scale * amp + xOffset;
            float perlinZ = ((float)z / terrain_size) * scale * amp + zOffset;

            float perlinValue = Mathf.PerlinNoise(perlinX, perlinZ);
            totalHeight += (perlinValue * freq) * 1.8f - 0.5f;
        }

        return totalHeight * 3f;
    }

    void SpawnTrees(GameObject terrainObject)
    {
        int spawned = 0;
        int attempts = 0;
        int maxAttempts = numberOfTrees * 10;
        Mesh mesh = meshFilter.mesh;
        Vector3[] vertices = mesh.vertices;

        while (spawned < numberOfTrees && attempts < maxAttempts)
        {
            attempts++;

            float x = Random.Range(0, terrain_size + 1);
            float z = Random.Range(0, terrain_size + 1);
            int vertexIndex = (int)x + (int)z * (terrain_size + 1);

            if (vertices[vertexIndex].y < 2.0f && vertices[vertexIndex].y > 0.0f) {
                Vector3 treePosition = new Vector3(x + terrain_x * terrain_size, vertices[vertexIndex].y, z + terrain_z * terrain_size);
                GameObject tree = Instantiate(prefabTree, treePosition, Quaternion.identity);
                tree.transform.SetParent(terrainObject.transform);
                spawned++;
            }
        }
    }

    void SpawnRockPiles(GameObject terrainObject, int numberOfPiles = 2, int rocksPerPile = 20)
    {
        // Debug.Log("rock created");
        for (int i = 0; i < numberOfPiles; i++)
        {
            float x = Random.Range(terrain_x * terrain_size, (terrain_x + 1) * terrain_size);
            float z = Random.Range(terrain_z * terrain_size, (terrain_z + 1) * terrain_size);
            Vector3 origin = new Vector3(x, 10f, z);

            // Debug.LogFormat ("x, z: {0} {1}", x, z);
            for (int j = 0; j < rocksPerPile; j++)
            {
                Vector3 spawnPos = origin + new Vector3(
                    Random.Range(-1f, 1f),
                    Random.Range(5f, 15f),
                    Random.Range(-1f, 1f)
                );

                GameObject rock = GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                rock.transform.SetParent(terrainObject.transform);
                rock.name = "Rock";

                rock.transform.localScale = new Vector3(
                    Random.Range(0.4f, 1.0f),
                    Random.Range(0.2f, 0.5f),
                    Random.Range(0.4f, 1.0f)
                );

                rock.transform.position = spawnPos;
                rock.transform.rotation = Random.rotation;

                Rigidbody rb = rock.AddComponent<Rigidbody>();
                rb.mass = 0.5f;
                Renderer rend = rock.GetComponent<Renderer>();
                rend.material.color = Color.gray;
            }
        }
    }

    void SpawnWater(GameObject terrainObject)
    {
        float waterHeight = 0.5f;

        GameObject water = GameObject.CreatePrimitive(PrimitiveType.Plane);
        water.name = "Water";

        float scale = terrain_size / 10.0f;
        water.transform.localScale = new Vector3(scale, 1, scale);

        float x = terrain_x * terrain_size + terrain_size / 2f;
        float z = terrain_z * terrain_size + terrain_size / 2f;
        water.transform.position = new Vector3(x, waterHeight, z);
        water.transform.SetParent(terrainObject.transform);

        if (waterMaterial != null)
        {
            Renderer renderer = water.GetComponent<Renderer>();
            renderer.material = waterMaterial;
        }
    }

    Vector3 TriNormal(Vector3 v1, Vector3 v2, Vector3 v3)
    {
        Vector3 edge1 = v2 - v1;
        Vector3 edge2 = v3 - v1;
        return Vector3.Cross(edge1, edge2).normalized;
    }

    void SpawnHotBalloon(GameObject terrainObject)
    {
        Texture2D texture = Resources.Load<Texture2D>("balloon");

        float x = terrain_x * terrain_size + terrain_size / 2f;
        float z = terrain_z * terrain_size + terrain_size / 2f;
        Vector3 position = new Vector3(x, 10, z);
        GameObject balloon = Instantiate(prefabHotBalloon, position, Quaternion.identity);

        balloon.transform.SetParent(terrainObject.transform);
        Renderer renderer = balloon.GetComponentInChildren<Renderer>();
        if (renderer != null)
        {
            renderer.material.mainTexture = texture;
        }
    }
}