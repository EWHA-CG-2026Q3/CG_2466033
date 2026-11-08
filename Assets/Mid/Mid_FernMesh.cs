using UnityEngine;
using System.Collections.Generic;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Mid_FernMesh : MonoBehaviour
{
    [Header("Fern Shape")]
    [Range(5, 40)] public int frondCount = 30;
    public float plantHeight = 1.6f;
    public float frondLength = 2.0f;
    public float frondWidth = 0.45f;
    public float stemRadius = 0.018f;

    [Header("Materials")]
    public Material stemMaterial;
    public Material leafMaterial;

    void OnEnable()
    {
        BuildFern();
    }

    [ContextMenu("Rebuild Fern")]
    void BuildFern()
    {
        if (plantHeight <= 0f || frondLength <= 0f ||
            frondWidth <= 0f || stemRadius <= 0f)
            return;

        Transform old = transform.Find("GeneratedFern");

        if (old != null)
        {
            old.gameObject.SetActive(false);
            DestroyImmediate(old.gameObject);
        }

        GameObject root = new GameObject("GeneratedFern");
        root.transform.SetParent(transform, false);

        int count = Mathf.Clamp(frondCount, 5, 20);

        for (int i = 0; i < count; i++)
        {
            float u = i / (float)count;
            float angle = u * Mathf.PI * 2f;

            Vector3 direction = new Vector3(
                Mathf.Cos(angle), 0f, Mathf.Sin(angle)
            );

            float variation = 0.85f +
                0.15f * Mathf.Sin(i * 2.7f);

            float lift = (i % 3 == 0) ? 1.0f :
                         (i % 3 == 1) ? 0.75f : 0.55f;

            CreateFrond(
                root.transform,
                direction,
                frondLength * variation,
                frondWidth,
                plantHeight * lift,
                i
            );
        }
    }

    Vector3 FrondCenter(
        Vector3 direction,
        float length,
        float height,
        float u)
    {
        Vector3 p = direction * (length * u);

        // 처음에는 위로 올라갔다가 끝으로 갈수록 처짐
        p.y = height * Mathf.Sin(u * Mathf.PI * 0.75f)
            - 0.65f * u * u;

        return p;
    }

    void CreateFrond(
        Transform parent,
        Vector3 direction,
        float length,
        float width,
        float height,
        int index)
    {
        int segments = 32;
        int leafletsPerSide = 24;

        Vector3 side =
            Vector3.Cross(Vector3.up, direction).normalized;

        // 1. 중앙 줄기 Mesh
        List<Vector3> stemVertices = new List<Vector3>();
        List<int> stemTriangles = new List<int>();

        int sides = 8;
        int rings = segments;
        int row = sides + 1;

        for (int j = 0; j <= rings; j++)
        {
            float u = j / (float)rings;

            Vector3 center =
                FrondCenter(direction, length, height, u);

            Vector3 next =
                FrondCenter(
                    direction, length, height,
                    Mathf.Min(1f, u + 0.001f)
                );

            Vector3 tangent = (next - center).normalized;

            if (tangent.sqrMagnitude < 0.000001f)
                tangent = direction;

            Vector3 reference =
                Mathf.Abs(Vector3.Dot(tangent, Vector3.up)) > 0.95f
                ? Vector3.forward : Vector3.up;

            Vector3 right =
                Vector3.Cross(tangent, reference).normalized;

            Vector3 forward =
                Vector3.Cross(right, tangent).normalized;

            float r = stemRadius *
                Mathf.Lerp(1f, 0.18f, u);

            for (int k = 0; k <= sides; k++)
            {
                float a = k * Mathf.PI * 2f / sides;

                stemVertices.Add(center +
                    (right * Mathf.Cos(a) +
                     forward * Mathf.Sin(a)) * r);
            }
        }

        int bottomCenter = stemVertices.Count;
        stemVertices.Add(FrondCenter(
            direction, length, height, 0f));

        int topCenter = stemVertices.Count;
        stemVertices.Add(FrondCenter(
            direction, length, height, 1f));

        for (int j = 0; j < rings; j++)
        {
            for (int k = 0; k < sides; k++)
            {
                int a = j * row + k;
                int b = a + row;

                stemTriangles.Add(a);
                stemTriangles.Add(b);
                stemTriangles.Add(a + 1);

                stemTriangles.Add(a + 1);
                stemTriangles.Add(b);
                stemTriangles.Add(b + 1);
            }
        }

        int topRow = rings * row;

        for (int k = 0; k < sides; k++)
        {
            stemTriangles.Add(bottomCenter);
            stemTriangles.Add(k + 1);
            stemTriangles.Add(k);

            stemTriangles.Add(topCenter);
            stemTriangles.Add(topRow + k);
            stemTriangles.Add(topRow + k + 1);
        }

        CreateMeshObject(
            parent, "FernStem",
            stemVertices, stemTriangles, stemMaterial
        );

        // 2. 작은 잎 Mesh
        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Color> colors = new List<Color>();

        for (int sideIndex = 0; sideIndex < 2; sideIndex++)
        {
            float sign = sideIndex == 0 ? -1f : 1f;

            for (int i = 0; i < leafletsPerSide; i++)
            {
                float u = 0.08f +
                    i / (float)(leafletsPerSide - 1) * 0.86f;

                Vector3 basePoint =
                    FrondCenter(direction, length, height, u);

                float profile = Mathf.Max(
                    0f, Mathf.Sin(Mathf.PI * u)
                );

                float leafletLength =
                    width * Mathf.Pow(profile, 0.8f);

                float forwardShift =
                    0.12f + 0.08f * u;

                Vector3 tip = basePoint
                    + side * sign * leafletLength
                    + direction * forwardShift;

                tip.y -= 0.05f + 0.08f * u;

                Vector3 nextBase =
                    FrondCenter(
                        direction, length, height,
                        Mathf.Min(1f, u + 0.025f)
                    );

                int a = vertices.Count;

                vertices.Add(basePoint);
                vertices.Add(nextBase);
                vertices.Add(tip);

                Color leafColor = Color.Lerp(
                    new Color(0.16f, 0.40f, 0.13f),
                    new Color(0.32f, 0.58f, 0.20f),
                    Mathf.PingPong(i * 0.17f + index * 0.11f, 1f)
                );

                colors.Add(leafColor);
                colors.Add(leafColor * 1.08f);
                colors.Add(leafColor * 0.92f);

                triangles.Add(a);
                triangles.Add(a + 1);
                triangles.Add(a + 2);

                triangles.Add(a + 2);
                triangles.Add(a + 1);
                triangles.Add(a);
            }
        }

        CreateMeshObject(
            parent, "FernLeaflets",
            vertices, triangles, leafMaterial, colors
        );
    }

    void CreateMeshObject(
        Transform parent,
        string objectName,
        List<Vector3> vertices,
        List<int> triangles,
        Material material,
        List<Color> colors = null)
    {
        GameObject obj = new GameObject(objectName);
        obj.transform.SetParent(parent, false);

        MeshFilter filter = obj.AddComponent<MeshFilter>();
        MeshRenderer renderer = obj.AddComponent<MeshRenderer>();

        Mesh mesh = new Mesh();
        mesh.name = objectName;
        mesh.SetVertices(vertices);
        mesh.SetTriangles(triangles, 0);

        if (colors != null && colors.Count == vertices.Count)
            mesh.SetColors(colors);

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        filter.sharedMesh = mesh;

        if (material != null)
            renderer.sharedMaterial = material;
    }
}