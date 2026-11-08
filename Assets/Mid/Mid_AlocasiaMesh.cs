using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Mid_AlocasiaMesh : MonoBehaviour
{
    [Header("Plant Shape")]
    [Range(3, 12)] public int leafCount = 7;
    public float plantHeight = 3.2f;
    public float leafLength = 1.8f;
    public float leafWidth = 1.15f;
    public float stemRadius = 0.075f;

    [Header("Materials")]
    public Material stemMaterial;
    public Material leafMaterial;

    void OnEnable()
    {
        BuildPlant();
    }

    [ContextMenu("Rebuild Alocasia")]
    void BuildPlant()
    {
        if (plantHeight <= 0f || leafLength <= 0f ||
            leafWidth <= 0f || stemRadius <= 0f)
            return;

        Transform old = transform.Find("GeneratedAlocasia");

        if (old != null)
        {
            old.gameObject.SetActive(false);
            DestroyImmediate(old.gameObject);
        }

        GameObject root = new GameObject("GeneratedAlocasia");
        root.transform.SetParent(transform, false);

        int count = Mathf.Clamp(leafCount, 3, 12);

        for (int i = 0; i < count; i++)
        {
            float u = i / (float)Mathf.Max(1, count - 1);
            float angle = i * 137.5f * Mathf.Deg2Rad;

            Vector3 direction = new Vector3(
                Mathf.Cos(angle), 0f, Mathf.Sin(angle)
            );

            float stemHeight = plantHeight *
                Mathf.Lerp(0.55f, 1f, u);

            Vector3 start = direction * (0.05f + 0.06f * u);
            start.y = 0.04f;

            Vector3 end = direction *
                Mathf.Lerp(0.75f, 0.25f, u);
            end.y = stemHeight;

            CreateStem(root.transform, start, end, direction);

            float length = leafLength *
                Mathf.Lerp(0.8f, 1.1f, u);

            float width = leafWidth *
                Mathf.Lerp(0.85f, 1.1f, u);

            CreateLeaf(root.transform, end, direction, length, width);
        }
    }

    void CreateStem(
        Transform parent,
        Vector3 start,
        Vector3 end,
        Vector3 direction)
    {
        int rings = 16;
        int sides = 12;
        int row = sides + 1;

        Vector3[] vertices =
            new Vector3[(rings + 1) * row + 2];

        int[] triangles =
            new int[rings * sides * 6 + sides * 6];

        for (int j = 0; j <= rings; j++)
        {
            float u = j / (float)rings;

            Vector3 center = Vector3.Lerp(start, end, u);
            center += direction *
                (0.15f * Mathf.Sin(Mathf.PI * u));

            Vector3 tangent =
                (end - start) +
                direction * (0.15f * Mathf.PI *
                Mathf.Cos(Mathf.PI * u));

            tangent.Normalize();

            if (tangent.sqrMagnitude < 0.000001f)
                tangent = Vector3.up;

            Vector3 reference =
                Mathf.Abs(Vector3.Dot(tangent, Vector3.up)) > 0.95f
                ? Vector3.forward : Vector3.up;

            Vector3 right =
                Vector3.Cross(tangent, reference).normalized;

            Vector3 forward =
                Vector3.Cross(right, tangent).normalized;

            float r = stemRadius *
                Mathf.Lerp(1.35f, 0.65f, u);

            for (int k = 0; k <= sides; k++)
            {
                float a = k * Mathf.PI * 2f / sides;
                int index = j * row + k;

                vertices[index] = center +
                    (right * Mathf.Cos(a) +
                     forward * Mathf.Sin(a)) * r;
            }
        }

        int bottomCenter = (rings + 1) * row;
        int topCenter = bottomCenter + 1;

        vertices[bottomCenter] = start;
        vertices[topCenter] = end;

        int t = 0;

        for (int j = 0; j < rings; j++)
        {
            for (int k = 0; k < sides; k++)
            {
                int a = j * row + k;
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

        for (int k = 0; k < sides; k++)
        {
            triangles[t++] = bottomCenter;
            triangles[t++] = k;
            triangles[t++] = k + 1;

            triangles[t++] = topCenter;
            triangles[t++] = topRow + k + 1;
            triangles[t++] = topRow + k;
        }

        CreateMeshObject(
            parent, "AlocasiaStem",
            vertices, triangles, stemMaterial
        );
    }

    void CreateLeaf(
        Transform parent,
        Vector3 start,
        Vector3 direction,
        float length,
        float width)
    {
        int steps = 24;
        int row = 5;

        Vector3[] vertices =
            new Vector3[(steps + 1) * row];

        int[] triangles =
            new int[steps * (row - 1) * 12];

        Vector3 side =
            Vector3.Cross(Vector3.up, direction).normalized;

        for (int i = 0; i <= steps; i++)
        {
            float u = i / (float)steps;

            float s = Mathf.Max(0f, Mathf.Sin(Mathf.PI * u));

            float heart = Mathf.Pow(s, 0.65f) *
                (0.8f + 0.35f * u);

            float w = width * heart;

            Vector3 center =
                start + direction * (length * u);

            center.y += 0.38f * Mathf.Sin(Mathf.PI * u);
            center.y -= 0.65f * u * u;

            for (int k = 0; k < row; k++)
            {
                float v = (k / 4f) * 2f - 1f;

                Vector3 point = center + side * (v * w);

                point.y -= 0.22f * v * v * s;

                point.y +=
                    0.045f * Mathf.Sin(u * Mathf.PI * 3f) *
                    (1f - Mathf.Abs(v));

                vertices[i * row + k] = point;
            }
        }

        int t = 0;

        for (int i = 0; i < steps; i++)
        {
            for (int k = 0; k < row - 1; k++)
            {
                int a = i * row + k;
                int b = a + row;

                triangles[t++] = a;
                triangles[t++] = b;
                triangles[t++] = a + 1;

                triangles[t++] = a + 1;
                triangles[t++] = b;
                triangles[t++] = b + 1;

                triangles[t++] = a + 1;
                triangles[t++] = b;
                triangles[t++] = a;

                triangles[t++] = b + 1;
                triangles[t++] = b;
                triangles[t++] = a + 1;
            }
        }

        CreateMeshObject(
            parent, "AlocasiaLeaf",
            vertices, triangles, leafMaterial
        );
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
        mesh.name = objectName;
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        filter.sharedMesh = mesh;

        if (material != null)
            renderer.sharedMaterial = material;
    }
}