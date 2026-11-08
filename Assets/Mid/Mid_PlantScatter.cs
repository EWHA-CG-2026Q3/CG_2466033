using UnityEngine;
using System.Collections.Generic;

public class Mid_PlantScatter : MonoBehaviour
{
    [Header("Ground")]
    public Transform ground;

    [Header("Plant Originals")]
    public GameObject bamboo, banana, alocasia, monstera, palm, fern;

    [Header("Plant Counts")]
    public int bambooCount = 100;
    public int bananaCount = 45;
    public int alocasiaCount = 100;
    public int monsteraCount = 85;
    public int palmCount = 28;
    public int fernCount = 230;

    [Header("Placement")]
    public float edgeMargin = 0.03f;
    public int randomSeed = 1234;
    public float clusterChance = 0.65f;
    public float clusterRadius = 2.4f;

    [Header("Pavilion")]
    public Transform pavilion;
    public float pavilionHalfWidth = 2.5f;
    public float pavilionHalfDepth = 2.5f;
    public float pavilionClearance = 0.35f;
    public float entranceWidth = 2.6f;
    public float entranceLength = 5f;

    [Header("Railway")]
    public Transform railway;
    public float railwayHalfWidth = 1.7f;

    [Header("Extra Exclusions")]
    public Transform[] exclusionCenters;
    public float[] exclusionRadii;

    [Header("Entrance Path")]
    public Transform entranceStart;
    public float pathWidth = 5f;

    [ContextMenu("Scatter Plants")]
    public void ScatterPlants()
    {
        if (ground == null) return;

        ClearPlants();

        GameObject root = new GameObject("GeneratedPlants");
        root.transform.SetParent(transform, false);

        System.Random rng = new System.Random(randomSeed);
        List<Vector3> trees = new List<Vector3>();

        Scatter(palm, palmCount, 0.85f, 1.25f, 2.4f, null, trees, root.transform, rng);
        Scatter(banana, bananaCount, 0.8f, 1.3f, 1.3f, trees, trees, root.transform, rng);
        Scatter(bamboo, bambooCount, 0.7f, 1.3f, 0.8f, trees, null, root.transform, rng);
        Scatter(alocasia, alocasiaCount, 0.7f, 1.2f, 0.2f, trees, null, root.transform, rng);
        Scatter(monstera, monsteraCount, 0.7f, 1.2f, 0.2f, trees, null, root.transform, rng);
        Scatter(fern, fernCount, 0.6f, 1.1f, 0.1f, trees, null, root.transform, rng);

        Debug.Log("Generated " + root.transform.childCount + " plants");
    }

    void Scatter(GameObject original, int count, float minScale, float maxScale,
        float minDistance, List<Vector3> anchors, List<Vector3> saved,
        Transform parent, System.Random rng)
    {
        if (original == null) return;

        List<Vector3> placed = new List<Vector3>();

        for (int i = 0; i < count; i++)
        {
            for (int attempt = 0; attempt < 250; attempt++)
            {
                Vector3 point = RandomPoint(rng);

                if (anchors != null && anchors.Count > 0 &&
                    rng.NextDouble() < clusterChance)
                {
                    Vector3 center = anchors[rng.Next(anchors.Count)];
                    float angle = Range(rng, 0f, Mathf.PI * 2f);
                    float radius = clusterRadius *
                        Mathf.Sqrt(Range(rng, 0f, 1f));

                    point = center + new Vector3(
                        Mathf.Cos(angle) * radius,
                        0f,
                        Mathf.Sin(angle) * radius
                    );
                }

                if (!InsideGround(point) || Excluded(point)) continue;

                bool tooClose = false;

                foreach (Vector3 other in placed)
                {
                    Vector2 a = new Vector2(point.x, point.z);
                    Vector2 b = new Vector2(other.x, other.z);

                    if (Vector2.Distance(a, b) < minDistance)
                    {
                        tooClose = true;
                        break;
                    }
                }

                if (tooClose) continue;

                GameObject plant = Instantiate(original);
                plant.name = original.name + "_Auto";
                plant.transform.SetParent(parent, true);
                plant.transform.position = point;
                plant.transform.rotation = Quaternion.Euler(
                    0f, Range(rng, 0f, 360f), 0f
                );
                plant.transform.localScale =
                    original.transform.localScale *
                    Range(rng, minScale, maxScale);

                plant.SetActive(true);

                placed.Add(point);
                if (saved != null) saved.Add(point);
                break;
            }
        }
    }

    Vector3 RandomPoint(System.Random rng)
    {
        float limit = 5f * (1f - edgeMargin * 2f);

        return ground.TransformPoint(new Vector3(
            Range(rng, -limit, limit),
            0f,
            Range(rng, -limit, limit)
        ));
    }

    bool InsideGround(Vector3 point)
    {
        Vector3 p = ground.InverseTransformPoint(point);
        float limit = 5f * (1f - edgeMargin * 2f);

        return Mathf.Abs(p.x) <= limit &&
               Mathf.Abs(p.z) <= limit;
    }

    bool Excluded(Vector3 position)
    {
        if (entranceStart != null)
        {
            Vector3 p = position - entranceStart.position;

            if (Mathf.Abs(p.x) < pathWidth * 0.5f && p.z < 0f)
                return true;
        }
        if (pavilion != null)
        {
            Vector3 center = pavilion.position;

            float dx = position.x - center.x;
            float dz = position.z - center.z;

            float halfWidth = pavilionHalfWidth + pavilionClearance;
            float halfDepth = pavilionHalfDepth + pavilionClearance;

            if (Mathf.Abs(dx) < halfWidth &&
                Mathf.Abs(dz) < halfDepth)
                return true;

            if (railway != null)
            {
                Vector3 railCenter = railway.position;

                float left = Mathf.Min(
                    center.x + halfWidth,
                    railCenter.x
                );

                float right = Mathf.Max(
                    center.x + halfWidth,
                    railCenter.x
                );

                if (position.x >= left &&
                    position.x <= right &&
                    Mathf.Abs(dz) < halfDepth)
                    return true;
            }
        }

        if (railway != null)
        {
            Mid_Railway rail = railway.GetComponent<Mid_Railway>();

            if (rail != null)
            {
                Vector3 p = railway.InverseTransformPoint(position);

                float halfLength = rail.length * 0.5f;

                if (Mathf.Abs(p.z) <= halfLength)
                {
                    float t = Mathf.InverseLerp(
                        -halfLength, halfLength, p.z
                    );

                    float u = Mathf.Clamp01(
                        Mathf.InverseLerp(
                            rail.curveStart, rail.curveEnd, t
                        )
                    );

                    float centerX = rail.curveAmount *
                        u * u * (3f - 2f * u);

                    if (Mathf.Abs(p.x - centerX) <
                        railwayHalfWidth + 0.8f)
                        return true;
                }
            }
        }

        if (exclusionCenters != null && exclusionRadii != null)
        {
            int count = Mathf.Min(
                exclusionCenters.Length, exclusionRadii.Length
            );

            for (int i = 0; i < count; i++)
            {
                if (exclusionCenters[i] == null) continue;

                Vector3 c = exclusionCenters[i].position;

                if (Vector2.Distance(
                    new Vector2(position.x, position.z),
                    new Vector2(c.x, c.z)
                ) < exclusionRadii[i])
                    return true;
            }
        }

        return false;
    }

    float Range(System.Random rng, float min, float max)
    {
        return min + (float)rng.NextDouble() * (max - min);
    }

    [ContextMenu("Clear Generated Plants")]
    public void ClearPlants()
    {
        Transform old = transform.Find("GeneratedPlants");
        if (old == null) return;

        old.gameObject.SetActive(false);

        if (Application.isPlaying)
            Destroy(old.gameObject);
        else
            DestroyImmediate(old.gameObject);
    }
}