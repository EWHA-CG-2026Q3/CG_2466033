using UnityEngine;
using System.Collections.Generic;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Mid_MonsteraMesh : MonoBehaviour
{
    [Header("Plant")]
    [Range(3, 12)] public int leafCount = 7;
    public float plantHeight = 2.8f;
    public float leafLength = 1.5f;
    public float leafWidth = 0.95f;
    public float stemRadius = 0.085f;

    [Header("Materials")]
    public Material stemMaterial;
    public Material leafMaterial;

    void OnEnable()
    {
        BuildPlant();
    }

    [ContextMenu("Rebuild Monstera")]
    void BuildPlant()
    {
        if (plantHeight <= 0f || leafLength <= 0f ||
            leafWidth <= 0f || stemRadius <= 0f)
            return;

        Transform old = transform.Find("GeneratedMonstera");
        if (old != null)
        {
            old.gameObject.SetActive(false);
            DestroyImmediate(old.gameObject);
        }

        MeshFilter mainFilter = GetComponent<MeshFilter>();
        Mesh oldMesh = mainFilter.sharedMesh;
        mainFilter.sharedMesh = null;

        if (oldMesh != null && oldMesh.name == "MonsteraMainStem")
            DestroyImmediate(oldMesh);

        GameObject root = new GameObject("GeneratedMonstera");
        root.transform.SetParent(transform, false);

        // 중심 줄기
        Vector3 trunkBottom = Vector3.zero;
        Vector3 trunkTop = new Vector3(0f, plantHeight * 0.75f, 0f);

        Mesh trunk = MakeTube(
            trunkBottom, trunkTop, stemRadius * 1.35f,
            stemRadius * 0.75f, 20, 12
        );
        trunk.name = "MonsteraMainStem";
        mainFilter.sharedMesh = trunk;

        if (stemMaterial != null)
            GetComponent<MeshRenderer>().sharedMaterial = stemMaterial;

        for (int i = 0; i < leafCount; i++)
        {
            float u = i / (float)Mathf.Max(1, leafCount - 1);
            float angle = i * 137.5f * Mathf.Deg2Rad;

            Vector3 direction = new Vector3(
                Mathf.Cos(angle), 0f, Mathf.Sin(angle)
            );

            Vector3 start = new Vector3(
                0f, plantHeight * (0.20f + 0.52f * u), 0f
            );

            Vector3 end = start
                + direction * Mathf.Lerp(0.85f, 0.45f, u)
                + Vector3.up * Mathf.Lerp(0.60f, 0.85f, u);

            CreateStem(root.transform, start, end);

            CreateLeaf(
                root.transform,
                end,
                direction,
                leafLength * Mathf.Lerp(0.85f, 1.10f, u),
                leafWidth * Mathf.Lerp(0.85f, 1.10f, u)
            );
        }
    }

    Mesh MakeTube(
        Vector3 start,
        Vector3 end,
        float bottomRadius,
        float topRadius,
        int rings,
        int sides)
    {
        int row = sides + 1;

        Vector3 axis = (end - start).normalized;
        if (axis.sqrMagnitude < 0.000001f)
            axis = Vector3.up;

        Vector3 reference =
            Mathf.Abs(Vector3.Dot(axis, Vector3.up)) > 0.95f
            ? Vector3.forward : Vector3.up;

        Vector3 right = Vector3.Cross(axis, reference).normalized;
        Vector3 forward = Vector3.Cross(right, axis).normalized;

        Vector3[] vertices =
            new Vector3[(rings + 1) * row + 2];

        int[] triangles =
            new int[rings * sides * 6 + sides * 6];

        for (int j = 0; j <= rings; j++)
        {
            float u = j / (float)rings;
            Vector3 center = Vector3.Lerp(start, end, u);

            float r = Mathf.Lerp(bottomRadius, topRadius, u);

            for (int k = 0; k <= sides; k++)
            {
                float a = k * Mathf.PI * 2f / sides;

                vertices[j * row + k] = center +
                    (right * Mathf.Cos(a) +
                     forward * Mathf.Sin(a)) * r;
            }
        }

        int bottom = (rings + 1) * row;
        int top = bottom + 1;

        vertices[bottom] = start;
        vertices[top] = end;

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
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    void CreateStem(Transform parent, Vector3 start, Vector3 end)
    {
        Mesh mesh = MakeTube(
            start, end,
            stemRadius * 0.75f,
            stemRadius * 0.45f,
            12, 10
        );

        CreateMeshObject(parent, "MonsteraPetiole",
            mesh, stemMaterial);
    }

    void CreateLeaf(
        Transform parent,
        Vector3 start,
        Vector3 direction,
        float length,
        float width)
    {
        int rows = 100;
        int columns = 12;
        int stride = columns + 1;

        Vector3 forward = direction.normalized;
        Vector3 side = Vector3.Cross(Vector3.up, forward).normalized;

        List<Vector3> vertices = new List<Vector3>();
        List<int> triangles = new List<int>();
        List<Vector2> uvs = new List<Vector2>();

        // 잎의 중앙 곡선
        Vector3 Center(float u)
        {
            Vector3 p = start + forward * (length * u);
            p.y += 0.25f * Mathf.Sin(Mathf.PI * u);
            p.y -= 0.38f * u * u;
            return p;
        }

        // 길쭉한 몬스테라 잎 윤곽
        float Profile(float u)
        {
            float s = Mathf.Max(0f, Mathf.Sin(Mathf.PI * u));
            return Mathf.Pow(s, 0.85f) * (1.1f - 0.25f * u);
        }

        // 잎 양쪽을 각각 생성
        for (int sideIndex = 0; sideIndex < 2; sideIndex++)
        {
            float sign = sideIndex == 0 ? -1f : 1f;

            int baseIndex = vertices.Count;

            for (int i = 0; i <= rows; i++)
            {
                float u = i / (float)rows;

                Vector3 center = Center(u);
                float fullWidth = width * Profile(u);

                // 5개의 비스듬한 갈라짐
                float cut = 0f;

                for (int k = 0; k < 5; k++)
                {
                    float cutPosition = 0.18f + k * 0.155f;

                    // 갈라짐이 바깥쪽에서 안쪽으로 비스듬히 이어짐
                    float distance = Mathf.Abs(u - cutPosition);

                    float cutWidth = 0.022f;

                    if (distance < cutWidth)
                    {
                        float amount = 1f - distance / cutWidth;
                        cut = Mathf.Max(cut, 0.75f * amount);
                    }
                }

                for (int j = 0; j <= columns; j++)
                {
                    float v = j / (float)columns;

                    // 중앙 잎맥에서 가장자리까지
                    float w = fullWidth * v;

                    // 가장자리에서 깊게 파고드는 틈
                    float edgeCut = cut * Mathf.Pow(v, 2.5f);

                    Vector3 point =
                        center + side * sign * w * (1f - edgeCut);

                    point.y -= 0.13f * v * v * Profile(u);

                    vertices.Add(point);
                    uvs.Add(new Vector2(u, (sign * v + 1f) * 0.5f));
                }
            }

            for (int i = 0; i < rows; i++)
            {
                for (int j = 0; j < columns; j++)
                {
                    int a = baseIndex + i * stride + j;
                    int b = a + stride;

                    if (sign > 0f)
                    {
                        triangles.Add(a);
                        triangles.Add(b);
                        triangles.Add(a + 1);

                        triangles.Add(a + 1);
                        triangles.Add(b);
                        triangles.Add(b + 1);
                    }
                    else
                    {
                        triangles.Add(a);
                        triangles.Add(a + 1);
                        triangles.Add(b);

                        triangles.Add(a + 1);
                        triangles.Add(b + 1);
                        triangles.Add(b);
                    }
                }
            }
        }

        Mesh mesh = new Mesh();
        mesh.name = "MonsteraLeaf";
        mesh.vertices = vertices.ToArray();
        mesh.uv = uvs.ToArray();
        mesh.triangles = triangles.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        CreateMeshObject(parent, "MonsteraLeaf", mesh, leafMaterial);
    }

    void CreateMeshObject(
        Transform parent,
        string objectName,
        Mesh mesh,
        Material material)
    {
        GameObject obj = new GameObject(objectName);
        obj.transform.SetParent(parent, false);

        MeshFilter filter = obj.AddComponent<MeshFilter>();
        MeshRenderer renderer = obj.AddComponent<MeshRenderer>();

        filter.sharedMesh = mesh;

        if (material != null)
            renderer.sharedMaterial = material;
    }
}