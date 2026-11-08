using UnityEngine;
using System.Collections.Generic;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Mid_PalmTreeMesh : MonoBehaviour
{
    [Header("Palm Tree")]
    public float trunkHeight = 6f;
    public float trunkRadius = 0.28f;
    [Range(5, 14)] public int leafCount = 9;
    public float leafLength = 3.2f;
    public float leafSpread = 1.15f;

    [Header("Materials")]
    public Material trunkMaterial;
    public Material leafMaterial;

    void OnEnable()
    {
        BuildPalm();
    }

    [ContextMenu("Rebuild Palm")]
    void BuildPalm()
    {
        if (trunkHeight <= 0f || trunkRadius <= 0f ||
            leafLength <= 0f || leafSpread <= 0f)
            return;

        Transform old = transform.Find("GeneratedPalm");

        if (old != null)
        {
            old.gameObject.SetActive(false);
            DestroyImmediate(old.gameObject);
        }

        MeshFilter filter = GetComponent<MeshFilter>();
        Mesh previous = filter.sharedMesh;
        filter.sharedMesh = null;

        if (previous != null && previous.name == "PalmTrunk")
            DestroyImmediate(previous);

        CreateTrunk();

        GameObject root = new GameObject("GeneratedPalm");
        root.transform.SetParent(transform, false);

        Vector3 crown = TrunkCenter(1f);


        for (int layer = 0; layer < 3; layer++)
        {
            float layerHeight = layer * 0.3f;
            float lengthScale = 1f + layer * 0.2f;
            float liftOffset = layer * 0.4f;

            for (int i = 0; i < leafCount; i++)
            {
                float angle =
                    (i + layer * 0.33f) *
                    Mathf.PI * 2f / leafCount;

                Vector3 direction = new Vector3(
                    Mathf.Cos(angle), 0f, Mathf.Sin(angle)
                );

                float variation =
                    0.88f + 0.12f * Mathf.Sin(i * 2.4f);

                float lift =
                    ((i % 3 == 0) ? 0.65f :
                    (i % 3 == 1) ? 0.35f : 0.10f)
                    - liftOffset;

                Vector3 layerStart =
                    crown - Vector3.up * layerHeight;

                CreateFrond(
                    root.transform,
                    layerStart,
                    direction,
                    leafLength * variation * lengthScale,
                    leafSpread,
                    lift,
                    i
                );
            }
        }
    }

    Vector3 TrunkCenter(float u)
    {
        return new Vector3(
            0.35f * u * u,
            trunkHeight * u,
            0.18f * Mathf.Sin(u * Mathf.PI)
        );
    }

    void CreateTrunk()
    {
        int rings = 48;
        int sides = 16;
        int row = sides + 1;

        Vector3[] vertices =
            new Vector3[(rings + 1) * row + 2];

        int[] triangles =
            new int[rings * sides * 6 + sides * 6];

        for (int j = 0; j <= rings; j++)
        {
            float u = j / (float)rings;
            Vector3 center = TrunkCenter(u);

            float taper = Mathf.Lerp(1.25f, 0.65f, u);

            // 야자수 줄기의 가로 마디
            float ring = 0.035f *
                Mathf.Cos(u * Mathf.PI * 34f);

            float r = trunkRadius * (taper + ring);

            for (int k = 0; k <= sides; k++)
            {
                float a = k * Mathf.PI * 2f / sides;

                vertices[j * row + k] = center +
                    new Vector3(
                        Mathf.Cos(a) * r,
                        0f,
                        Mathf.Sin(a) * r
                    );
            }
        }

        int bottom = (rings + 1) * row;
        int top = bottom + 1;

        vertices[bottom] = TrunkCenter(0f);
        vertices[top] = TrunkCenter(1f);

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
            triangles[t++] = bottom;
            triangles[t++] = k + 1;
            triangles[t++] = k;

            triangles[t++] = top;
            triangles[t++] = topRow + k;
            triangles[t++] = topRow + k + 1;
        }

        Mesh mesh = new Mesh();
        mesh.name = "PalmTrunk";
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        GetComponent<MeshFilter>().sharedMesh = mesh;

        if (trunkMaterial != null)
            GetComponent<MeshRenderer>().sharedMaterial = trunkMaterial;
    }

    Vector3 FrondCenter(
        Vector3 start,
        Vector3 direction,
        float length,
        float lift,
        float u)
    {
        Vector3 p = start + direction * (length * u);

        p.y += Mathf.Max(0f, lift) * Mathf.Sin(Mathf.PI * u);

        float droop = Mathf.Lerp(
            2.2f, 0.8f,
            Mathf.Clamp01(lift / 0.65f)
        );

        p.y -= droop * u * u;

        return p;
    }

    void CreateFrond(
        Transform parent,
        Vector3 start,
        Vector3 direction,
        float length,
        float spread,
        float lift,
        int index)
    {
        int segments = 24;
        int leafletsPerSide = 18;

        Vector3 side =
            Vector3.Cross(Vector3.up, direction).normalized;

        List<Vector3> stemVertices = new List<Vector3>();
        List<int> stemTriangles = new List<int>();

        for (int i = 0; i <= segments; i++)
        {
            float u = i / (float)segments;
            Vector3 center =
                FrondCenter(start, direction, length, lift, u);

            float w = Mathf.Lerp(0.045f, 0.008f, u);

            stemVertices.Add(center - side * w);
            stemVertices.Add(center + side * w);
        }

        for (int i = 0; i < segments; i++)
        {
            int a = i * 2;
            int b = a + 2;

            AddDoubleSidedQuad(stemTriangles,
                a, b, a + 1, b + 1);
        }

        CreateMeshObject(
            parent, "PalmLeafRachis",
            stemVertices, stemTriangles, trunkMaterial
        );

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();

        for (int sideIndex = 0; sideIndex < 2; sideIndex++)
        {
            float sign = sideIndex == 0 ? -1f : 1f;

            for (int i = 0; i < leafletsPerSide; i++)
            {
                float u = 0.07f +
                    i / (float)(leafletsPerSide - 1) * 0.86f;

                Vector3 basePoint =
                    FrondCenter(start, direction, length, lift, u);

                float profile = Mathf.Max(
                    0f, Mathf.Sin(Mathf.PI * u)
                );

                float leafletLength =
                    spread * Mathf.Pow(profile, 0.8f);

                float forwardShift =
                    0.18f + 0.15f * u;

                Vector3 tip = basePoint
                    + side * sign * leafletLength
                    + direction * forwardShift;

                tip.y -= 0.18f + 0.16f * u;

                Vector3 nextBase =
                    FrondCenter(
                        start, direction, length, lift,
                        Mathf.Min(1f, u + 0.032f)
                    );

                int a = vertices.Count;

                vertices.Add(basePoint);
                vertices.Add(nextBase);
                vertices.Add(tip);

                triangles.Add(a);
                triangles.Add(a + 1);
                triangles.Add(a + 2);

                triangles.Add(a + 2);
                triangles.Add(a + 1);
                triangles.Add(a);
            }
        }

        CreateMeshObject(
            parent, "PalmLeaflets",
            vertices, triangles, leafMaterial
        );
    }

    void AddDoubleSidedQuad(
        List<int> triangles,
        int a, int b, int c, int d)
    {
        triangles.Add(a);
        triangles.Add(b);
        triangles.Add(c);

        triangles.Add(c);
        triangles.Add(b);
        triangles.Add(d);

        triangles.Add(c);
        triangles.Add(b);
        triangles.Add(a);

        triangles.Add(d);
        triangles.Add(b);
        triangles.Add(c);
    }

    void CreateMeshObject(
        Transform parent,
        string objectName,
        List<Vector3> vertices,
        List<int> triangles,
        Material material)
    {
        GameObject obj = new GameObject(objectName);
        obj.transform.SetParent(parent, false);

        MeshFilter filter = obj.AddComponent<MeshFilter>();
        MeshRenderer renderer = obj.AddComponent<MeshRenderer>();

        Mesh mesh = new Mesh();
        mesh.name = objectName;
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        filter.sharedMesh = mesh;

        if (material != null)
            renderer.sharedMaterial = material;
    }
}