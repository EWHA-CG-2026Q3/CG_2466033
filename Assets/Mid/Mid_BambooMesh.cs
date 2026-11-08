using UnityEngine;

[ExecuteAlways]
[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Mid_BambooMesh : MonoBehaviour
{
    public float height = 6f;
    public float radius = 0.18f;
    public int segments = 12;
    public int sections = 30;

    public Material stemMaterial;
    public Material leafMaterial;

    void OnEnable()
    {
        BuildBamboo();
    }

    void BuildBamboo()
    {
        if (segments < 3 || sections < 1 || height <= 0f || radius <= 0f)
            return;

        Transform old = transform.Find("GeneratedBamboo");

        if (old != null)
        {
            old.gameObject.SetActive(false);
            DestroyImmediate(old.gameObject);
        }

        // 대나무 줄기 정점
        Vector3[] vertices = new Vector3[(sections + 1) * (segments + 1)];
        int[] triangles = new int[sections * segments * 6];

        for (int j = 0; j <= sections; j++)
        {
            float y = height * j / sections;
            float r = radius;

            float phase = (j % 5) / 5f;
            float node = Mathf.Exp(-Mathf.Pow((phase - 0.2f) * 8f, 2f));

            r = radius * (1f + 0.12f * node);

            for (int i = 0; i <= segments; i++)
            {
                float angle = 2f * Mathf.PI * i / segments;
                int index = j * (segments + 1) + i;

                vertices[index] = new Vector3(
                    r * Mathf.Cos(angle),
                    y,
                    r * Mathf.Sin(angle)
                );
            }
        }

        int t = 0;

        for (int j = 0; j < sections; j++)
        {
            for (int i = 0; i < segments; i++)
            {
                int a = j * (segments + 1) + i;
                int b = a + segments + 1;

                triangles[t++] = a;
                triangles[t++] = b;
                triangles[t++] = a + 1;

                triangles[t++] = a + 1;
                triangles[t++] = b;
                triangles[t++] = b + 1;
            }
        }

        // 줄기 위아래 뚜껑
        int[] closedTriangles = new int[triangles.Length + segments * 6];

        triangles.CopyTo(closedTriangles, 0);

        int capIndex = triangles.Length;

        for (int i = 1; i < segments - 1; i++)
        {
            // 아래쪽
            closedTriangles[capIndex++] = 0;
            closedTriangles[capIndex++] = i;
            closedTriangles[capIndex++] = i + 1;

            // 위쪽
            int top = sections * (segments + 1);

            closedTriangles[capIndex++] = top;
            closedTriangles[capIndex++] = top + i + 1;
            closedTriangles[capIndex++] = top + i;
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = closedTriangles;
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().sharedMesh = mesh;

        if (stemMaterial != null)
            GetComponent<MeshRenderer>().sharedMaterial = stemMaterial;

        // 가지와 잎 생성
        GameObject root = new GameObject("GeneratedBamboo");
        root.transform.SetParent(transform, false);

        for (int level = 0; level < 5; level++)
        {
            float y = height * (0.38f + level * 0.13f);

            for (int side = 0; side < 3; side++)
            {
                float angle = (side * 120f + level * 43f) * Mathf.Deg2Rad;

                Vector3 direction = new Vector3(
                    Mathf.Cos(angle), 0f, Mathf.Sin(angle)
                );

                Vector3 start = new Vector3(0f, y, 0f);
                Vector3 end = start + direction * 0.85f
                    + Vector3.up * 0.38f;

                CreateBranch(root.transform, start, end);

                for (int k = 0; k < 3; k++)
                {
                    float distance = 0.48f + k * 0.18f;
                    Vector3 leafStart = Vector3.Lerp(start, end, distance);

                    float spread = (k % 2 == 0) ? 1f : -1f;

                    Vector3 sideways = Vector3.Cross(Vector3.up, direction);

                    Vector3 leafDirection =
                        direction * 0.65f +
                        sideways * spread * 0.55f +
                        Vector3.up * (0.15f - k * 0.1f);

                    CreateLeaf(
                        root.transform,
                        leafStart,
                        leafDirection.normalized,
                        0.85f + k * 0.1f,
                        0.09f
                    );
                }
            }
        }
    }

    void CreateBranch(Transform parent, Vector3 start, Vector3 end)
    {
        GameObject branch = new GameObject("Branch");
        branch.transform.SetParent(parent, false);

        MeshFilter filter = branch.AddComponent<MeshFilter>();
        MeshRenderer renderer = branch.AddComponent<MeshRenderer>();

        Vector3 direction = end - start;
        Vector3 right = Vector3.Cross(direction.normalized, Vector3.up);

        if (right.sqrMagnitude < 0.001f)
            right = Vector3.right;

        right.Normalize();
        right *= 0.018f;

        Vector3[] vertices = new Vector3[]
        {
            start - right,
            start + right,
            end - right * 0.5f,
            end + right * 0.5f
        };

        int[] triangles = new int[]
        {
            0, 2, 1,
            1, 2, 3,
            1, 2, 0,
            3, 2, 1
        };

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        filter.sharedMesh = mesh;

        if (stemMaterial != null)
            renderer.sharedMaterial = stemMaterial;
    }

    void CreateLeaf(
        Transform parent,
        Vector3 start,
        Vector3 direction,
        float length,
        float width)
    {
        GameObject leaf = new GameObject("Leaf");
        leaf.transform.SetParent(parent, false);

        MeshFilter filter = leaf.AddComponent<MeshFilter>();
        MeshRenderer renderer = leaf.AddComponent<MeshRenderer>();

        Vector3 right = Vector3.Cross(direction, Vector3.up);

        if (right.sqrMagnitude < 0.001f)
            right = Vector3.right;

        right.Normalize();

        Vector3[] vertices = new Vector3[7];

        // 잎 중심선: 끝으로 갈수록 아래로 휘어짐
        for (int i = 0; i < 7; i++)
        {
            float u = i / 6f;
            float w = Mathf.Sin(u * Mathf.PI) * width;

            Vector3 center = start + direction * length * u;
            center.y -= 0.22f * u * u;

            if (i == 0 || i == 6)
            {
                vertices[i] = center;
            }
            else
            {
                // 양옆 정점은 아래에서 별도로 생성
                vertices[i] = center;
            }
        }

        Vector3[] leafVertices = new Vector3[14];

        for (int i = 0; i < 7; i++)
        {
            float u = i / 6f;
            float w = Mathf.Sin(u * Mathf.PI) * width;

            leafVertices[i * 2] = vertices[i] - right * w;
            leafVertices[i * 2 + 1] = vertices[i] + right * w;
        }

        int[] triangles = new int[72];
        int t = 0;

        for (int i = 0; i < 6; i++)
        {
            int a = i * 2;
            int b = a + 2;

            triangles[t++] = a;
            triangles[t++] = b;
            triangles[t++] = a + 1;

            triangles[t++] = a + 1;
            triangles[t++] = b;
            triangles[t++] = b + 1;

            // 잎 뒷면
            triangles[t++] = a + 1;
            triangles[t++] = b;
            triangles[t++] = a;

            triangles[t++] = b + 1;
            triangles[t++] = b;
            triangles[t++] = a + 1;
        }

        Mesh mesh = new Mesh();
        mesh.vertices = leafVertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        filter.sharedMesh = mesh;

        if (leafMaterial != null)
            renderer.sharedMaterial = leafMaterial;
    }
}
