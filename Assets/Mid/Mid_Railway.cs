using UnityEngine;

public class Mid_Railway : MonoBehaviour
{
    public float length = 28f;
    public float railSpacing = 1.8f;
    public float sleeperSpacing = 1.2f;

    [Header("Curve")]
    public float curveAmount = 3f;
    public float curveStart = 0.25f;
    public float curveEnd = 0.75f;
    public int railSegments = 80;

    public Material railMaterial;
    public Material sleeperMaterial;


    Vector3 CenterPoint(float z)
    {
        float t = Mathf.InverseLerp(
            -length * 0.5f, length * 0.5f, z
        );

        float u = Mathf.InverseLerp(curveStart, curveEnd, t);
        u = Mathf.Clamp01(u);

        float offset = curveAmount * (u * u * (3f - 2f * u));

        return new Vector3(offset, 0f, z);
    }

    Vector3 Tangent(float z)
    {
        float step = 0.05f;
        return (CenterPoint(z + step) - CenterPoint(z - step)).normalized;
    }

    [ContextMenu("Build Railway")]
    public void BuildRailway()
    {
        ClearRailway();

        GameObject root = new GameObject("GeneratedRailway");
        root.transform.SetParent(transform, false);

        int segments = Mathf.Max(2, railSegments);

        for (int i = 0; i < segments; i++)
        {
            float z0 = Mathf.Lerp(-length / 2f, length / 2f, i / (float)segments);
            float z1 = Mathf.Lerp(-length / 2f, length / 2f, (i + 1f) / segments);

            CreateRailSegment(z0, z1, -railSpacing / 2f, root.transform);
            CreateRailSegment(z0, z1, railSpacing / 2f, root.transform);
        }

        int count = Mathf.FloorToInt(length / sleeperSpacing);

        for (int i = 0; i <= count; i++)
        {
            float z = -length / 2f + i * sleeperSpacing;
            Vector3 center = CenterPoint(z);
            Vector3 tangent = Tangent(z);

            Create(
                "Sleeper_" + i,
                center + Vector3.up * 0.045f,
                Quaternion.LookRotation(tangent),
                new Vector3(railSpacing + 0.65f, 0.12f, 0.3f),
                sleeperMaterial,
                root.transform
            );
        }
    }

    void CreateRailSegment(float z0, float z1, float offset, Transform parent)
    {
        Vector3 c0 = CenterPoint(z0);
        Vector3 c1 = CenterPoint(z1);

        Vector3 tangent0 = Tangent(z0);
        Vector3 tangent1 = Tangent(z1);

        Vector3 right0 = new Vector3(tangent0.z, 0f, -tangent0.x);
        Vector3 right1 = new Vector3(tangent1.z, 0f, -tangent1.x);

        Vector3 p0 = c0 + right0 * offset;
        Vector3 p1 = c1 + right1 * offset;

        Vector3 direction = p1 - p0;

        Create(
            "Rail",
            (p0 + p1) * 0.5f + Vector3.up * 0.16f,
            Quaternion.LookRotation(direction.normalized),
            new Vector3(0.12f, 0.15f, direction.magnitude + 0.015f),
            railMaterial,
            parent
        );
    }

    void Create(string name, Vector3 position,
        Quaternion rotation, Vector3 scale,
        Material material, Transform parent)
    {
        GameObject obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
        obj.name = name;
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = position;
        obj.transform.localRotation = rotation;
        obj.transform.localScale = scale;

        if (material != null)
            obj.GetComponent<Renderer>().sharedMaterial = material;
    }

    [ContextMenu("Clear Railway")]
    public void ClearRailway()
    {
        Transform old = transform.Find("GeneratedRailway");
        if (old == null) return;

        old.gameObject.SetActive(false);

        if (Application.isPlaying)
            Destroy(old.gameObject);
        else
            DestroyImmediate(old.gameObject);
    }
}