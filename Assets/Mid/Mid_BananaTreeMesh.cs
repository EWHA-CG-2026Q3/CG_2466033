using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Mid_BananaTreeMesh : MonoBehaviour
{
    [Header("Banana Plant")]
    public float height = 4.5f;
    public float radius = 0.35f;
    public int leafCount = 9;
    public float leafLength = 2.7f;
    public float leafWidth = 0.65f;

    [Header("Materials")]
    public Material stemMaterial;
    public Material leafMaterial;

    void OnEnable()
    {
        BuildBanana();
    }

    [ContextMenu("Rebuild Banana")]
    void BuildBanana()
    {
        if (height <= 0f || radius <= 0f ||
            leafLength <= 0f || leafWidth <= 0f)
            return;

        Transform old = transform.Find("GeneratedBanana");

        if (old != null)
        {
            old.gameObject.SetActive(false);
            DestroyImmediate(old.gameObject);
        }

        MeshFilter filter = GetComponent<MeshFilter>();
        Mesh previous = filter.sharedMesh;

        if (previous != null && previous.name == "BananaTrunk")
        {
            filter.sharedMesh = null;
            DestroyImmediate(previous);
        }

        CreateTrunk();

        GameObject root = new GameObject("GeneratedBanana");
        root.transform.SetParent(transform, false);

        int count = Mathf.Clamp(leafCount, 3, 14);

        for (int i = 0; i < count; i++)
        {
            float u = i / (float)(count - 1);
            float angle = i * 137.5f * Mathf.Deg2Rad;

            Vector3 direction = new Vector3(
                Mathf.Cos(angle), 0f, Mathf.Sin(angle)
            );

            float y = height * (0.38f + 0.56f * u);

            Vector3 start = new Vector3(
                direction.x * radius * 0.75f,
                y,
                direction.z * radius * 0.75f
            );

            Vector3 leafStart = start
                + direction * 0.45f
                + Vector3.up * 0.85f;

            CreateLeafSheath(root.transform, start, leafStart);

            CreateLeaf(
                root.transform,
                leafStart,
                direction,
                leafLength * Mathf.Lerp(1f, 0.75f, u),
                leafWidth * Mathf.Lerp(1f, 0.8f, u)
            );
        }
    }

    void CreateTrunk()
    {
        int sides = 16;
        int rings = 32;
        int row = sides + 1;

        Vector3[] vertices =
            new Vector3[(rings + 1) * row + 2];

        int[] triangles =
            new int[rings * sides * 6 + sides * 6];

        for (int j = 0; j <= rings; j++)
        {
            float u = j / (float)rings;
            float y = height * u;

            float taper = Mathf.Lerp(1.25f, 0.65f, u);
            float groove = 0.025f * Mathf.Sin(u * Mathf.PI * 12f);

            float tip = Mathf.Clamp01((1f - u) * 8f);
            float r = radius * (taper + groove) * tip;

            for (int i = 0; i <= sides; i++)
            {
                float angle = i * Mathf.PI * 2f / sides;
                int index = j * row + i;

                float ridge = 1f +
                    0.025f * Mathf.Cos(angle * 5f + u * 2f);

                vertices[index] = new Vector3(
                    Mathf.Cos(angle) * r * ridge + 0.08f * u * u,
                    y,
                    Mathf.Sin(angle) * r * ridge
                );
            }
        }

        int bottom = (rings + 1) * row;
        int top = bottom + 1;

        vertices[bottom] = Vector3.zero;
        vertices[top] = new Vector3(0.08f, height, 0f);

        int t = 0;

        for (int j = 0; j < rings; j++)
        {
            for (int i = 0; i < sides; i++)
            {
                int a = j * row + i;
                int b = a + row;

                triangles[t++] = a;
                triangles[t++] = b;
                triangles[t++] = a + 1;

                triangles[t++] = a + 1;
                triangles[t++] = b;
                triangles[t++] = b + 1;
            }
        }

        int topRow = rings * row;

        for (int i = 0; i < sides; i++)
        {
            triangles[t++] = bottom;
            triangles[t++] = i + 1;
            triangles[t++] = i;

            triangles[t++] = top;
            triangles[t++] = topRow + i;
            triangles[t++] = topRow + i + 1;
        }

        Mesh mesh = new Mesh();
        mesh.name = "BananaTrunk";
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().sharedMesh = mesh;

        if (stemMaterial != null)
            GetComponent<MeshRenderer>().sharedMaterial = stemMaterial;
    }

    void CreateLeafSheath(
        Transform parent,
        Vector3 start,
        Vector3 end)
    {
        int steps = 12;
        Vector3[] vertices = new Vector3[(steps + 1) * 3];
        int[] triangles = new int[steps * 24];

        Vector3 direction = end - start;
        Vector3 side = Vector3.Cross(Vector3.up, direction);

        if (side.sqrMagnitude < 0.000001f)
            side = Vector3.right;

        side.Normalize();

        for (int i = 0; i <= steps; i++)
        {
            float u = i / (float)steps;

            Vector3 center = Vector3.Lerp(start, end, u);
            center.y += 0.10f * Mathf.Sin(Mathf.PI * u);

            float w = Mathf.Lerp(radius * 0.7f, 0.035f, u);

            vertices[i * 3] = center - side * w;
            vertices[i * 3 + 1] = center + Vector3.up * 0.04f;
            vertices[i * 3 + 2] = center + side * w;
        }

        FillTriangles(triangles, steps);
        CreateMeshObject(parent, "LeafSheath",
            vertices, triangles, stemMaterial);
    }

    void CreateLeaf(
        Transform parent,
        Vector3 start,
        Vector3 direction,
        float length,
        float width)
    {
        int steps = 24;
        Vector3[] vertices = new Vector3[(steps + 1) * 3];
        int[] triangles = new int[steps * 24];

        Vector3 side =
            Vector3.Cross(Vector3.up, direction).normalized;

        for (int i = 0; i <= steps; i++)
        {
            float u = i / (float)steps;

            // 음수의 소수 제곱으로 인한 NaN 방지
            float s = Mathf.Max(0f, Mathf.Sin(Mathf.PI * u));
            float w = width * Mathf.Pow(s, 0.8f);

            Vector3 center = start + direction * length * u;

            center.y += 0.55f * Mathf.Sin(Mathf.PI * u);
            center.y -= 0.9f * u * u;

            float fold = 0.12f * s;

            vertices[i * 3] =
                center - side * w - Vector3.up * fold;

            vertices[i * 3 + 1] =
                center + Vector3.up * 0.04f;

            vertices[i * 3 + 2] =
                center + side * w - Vector3.up * fold;
        }

        FillTriangles(triangles, steps);
        CreateMeshObject(parent, "BananaLeaf",
            vertices, triangles, leafMaterial);
    }

    void FillTriangles(int[] triangles, int steps)
    {
        int t = 0;

        for (int i = 0; i < steps; i++)
        {
            int a = i * 3;
            int b = a + 3;

            int[] face = new int[]
            {
                a, b, a + 1,
                a + 1, b, b + 1,
                a + 1, b + 1, a + 2,
                a + 2, b + 1, b + 2
            };

            for (int j = 0; j < face.Length; j += 3)
            {
                triangles[t++] = face[j];
                triangles[t++] = face[j + 1];
                triangles[t++] = face[j + 2];

                triangles[t++] = face[j + 2];
                triangles[t++] = face[j + 1];
                triangles[t++] = face[j];
            }
        }
    }

    void CreateMeshObject(
        Transform parent,
        string objectName,
        Vector3[] vertices,
        int[] triangles,
        Material material)
    {
        GameObject obj = new GameObject(objectName);
        obj.transform.SetParent(parent, false);

        MeshFilter filter = obj.AddComponent<MeshFilter>();
        MeshRenderer renderer = obj.AddComponent<MeshRenderer>();

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        filter.sharedMesh = mesh;

        if (material != null)
            renderer.sharedMaterial = material;
    }
}