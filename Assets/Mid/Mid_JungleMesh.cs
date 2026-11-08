using UnityEngine;

[RequireComponent(typeof(MeshFilter), typeof(MeshRenderer))]
public class Mid_JungleMesh : MonoBehaviour
{
    public float height = 6f;
    public float radius = 0.18f;
    public int segments = 12;
    public int sections = 6;

    void Start()
    {
        Vector3[] vertices = new Vector3[(sections + 1) * (segments + 1)];
        int[] triangles = new int[sections * segments * 6];

        // 정점 생성
        for (int j = 0; j <= sections; j++)
        {
            float y = height * j / sections;
            float r = radius * (1f + 0.07f * Mathf.Sin(j * Mathf.PI));

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

        // 삼각형 생성
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

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = triangles;
        mesh.RecalculateNormals();

        GetComponent<MeshFilter>().mesh = mesh;
    }
}
