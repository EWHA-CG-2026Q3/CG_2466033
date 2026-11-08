using UnityEngine;
using System.Collections.Generic;

public class Mid_PlantScatter : MonoBehaviour
{
    [Header("Ground")]
    public Transform ground;

    [Header("Plant Originals")]
    public GameObject bamboo;
    public GameObject banana;
    public GameObject alocasia;
    public GameObject monstera;
    public GameObject palm;
    public GameObject fern;

    [Header("Plant Counts")]
    public int bambooCount = 35;
    public int bananaCount = 18;
    public int alocasiaCount = 30;
    public int monsteraCount = 25;
    public int palmCount = 12;
    public int fernCount = 70;

    [Header("Placement")]
    [Range(0f, 0.45f)]
    public float edgeMargin = 0.08f;
    public int randomSeed = 1234;

    [Header("Exclusion Areas")]
    public Transform[] exclusionCenters;
    public float[] exclusionRadii;

    [ContextMenu("Scatter Plants")]
    public void ScatterPlants()
    {
        if (ground == null)
        {
            Debug.LogWarning("Ground를 지정해 주세요.");
            return;
        }

        ClearPlants();

        GameObject root = new GameObject("GeneratedPlants");
        root.transform.SetParent(transform, false);

        System.Random rng = new System.Random(randomSeed);
        List<Vector3> treePositions = new List<Vector3>();

        ScatterSpaced(
            palm, palmCount, 0.85f, 1.25f,
            5.5f, root.transform, treePositions, rng
        );

        ScatterCluster(
            banana, bananaCount, 0.8f, 1.3f,
            3, 3.5f, root.transform, treePositions, rng
        );

        ScatterCluster(
            bamboo, bambooCount, 0.7f, 1.3f,
            6, 3.0f, root.transform, null, rng
        );

        ScatterNearTrees(
            alocasia, alocasiaCount, 0.7f, 1.2f,
            treePositions, 1.8f, 6.0f, root.transform, rng
        );

        ScatterNearTrees(
            monstera, monsteraCount, 0.7f, 1.2f,
            treePositions, 2.0f, 7.0f, root.transform, rng
        );

        ScatterCluster(
            fern, fernCount, 0.6f, 1.1f,
            10, 4.0f, root.transform, null, rng
        );

        Debug.Log("군집형 식물 배치 완료!");
    }

    Vector3 RandomGroundPoint(System.Random rng)
    {
        float limit = 5f * (1f - edgeMargin * 2f);

        float x = RandomRange(rng, -limit, limit);
        float z = RandomRange(rng, -limit, limit);

        return ground.TransformPoint(new Vector3(x, 0f, z));
    }

    bool IsInsideGround(Vector3 worldPoint)
    {
        Vector3 local = ground.InverseTransformPoint(worldPoint);
        float limit = 5f * (1f - edgeMargin * 2f);

        return Mathf.Abs(local.x) <= limit &&
               Mathf.Abs(local.z) <= limit;
    }

    float RandomRange(System.Random rng, float min, float max)
    {
        return min + (float)rng.NextDouble() * (max - min);
    }

    bool TryRandomPoint(System.Random rng, out Vector3 point)
    {
        for (int attempt = 0; attempt < 100; attempt++)
        {
            point = RandomGroundPoint(rng);

            if (!IsExcluded(point))
                return true;
        }

        point = Vector3.zero;
        return false;
    }

    void SpawnPlant(
        GameObject original,
        Vector3 position,
        float minScale,
        float maxScale,
        Transform parent,
        System.Random rng)
    {
        GameObject plant = Instantiate(original);

        plant.name = original.name + "_Auto";
        plant.transform.SetParent(parent, true);
        plant.transform.position = position;

        plant.transform.rotation = Quaternion.Euler(
            0f,
            RandomRange(rng, 0f, 360f),
            0f
        );

        float scale = RandomRange(rng, minScale, maxScale);

        plant.transform.localScale =
            original.transform.localScale * scale;

        plant.SetActive(true);
    }

    void ScatterSpaced(
        GameObject original,
        int count,
        float minScale,
        float maxScale,
        float minDistance,
        Transform parent,
        List<Vector3> positions,
        System.Random rng)
    {
        if (original == null || count <= 0)
            return;

        for (int i = 0; i < count; i++)
        {
            bool found = false;
            Vector3 point = Vector3.zero;

            for (int attempt = 0; attempt < 150; attempt++)
            {
                if (!TryRandomPoint(rng, out point))
                    break;

                bool tooClose = false;

                foreach (Vector3 other in positions)
                {
                    float distance = Vector2.Distance(
                        new Vector2(point.x, point.z),
                        new Vector2(other.x, other.z)
                    );

                    if (distance < minDistance)
                    {
                        tooClose = true;
                        break;
                    }
                }

                if (!tooClose)
                {
                    found = true;
                    break;
                }
            }

            if (!found)
                continue;

            SpawnPlant(
                original, point, minScale, maxScale,
                parent, rng
            );

            positions.Add(point);
        }
    }

    void ScatterCluster(
        GameObject original,
        int count,
        float minScale,
        float maxScale,
        int plantsPerCluster,
        float clusterRadius,
        Transform parent,
        List<Vector3> savedPositions,
        System.Random rng)
    {
        if (original == null || count <= 0)
            return;

        int placed = 0;
        int clusters = Mathf.CeilToInt(
            count / (float)plantsPerCluster
        );

        for (int c = 0; c < clusters; c++)
        {
            int gridSize = Mathf.CeilToInt(Mathf.Sqrt(clusters));

            int cellX = c % gridSize;
            int cellZ = c / gridSize;

            float limit = 5f * (1f - edgeMargin * 2f);
            float cellSize = (limit * 2f) / gridSize;

            Vector3 center = Vector3.zero;
            bool foundCenter = false;

            for (int attempt = 0; attempt < 100; attempt++)
            {
                float x = -limit + (cellX + RandomRange(rng, 0.15f, 0.85f)) * cellSize;
                float z = -limit + (cellZ + RandomRange(rng, 0.15f, 0.85f)) * cellSize;

                center = ground.TransformPoint(new Vector3(x, 0f, z));

                if (!IsExcluded(center))
                {
                    foundCenter = true;
                    break;
                }
            }

            if (!foundCenter)
                continue;

            int target = Mathf.Min(
                plantsPerCluster,
                count - placed
            );

            for (int i = 0; i < target; i++)
            {
                bool found = false;
                Vector3 point = center;

                for (int attempt = 0; attempt < 80; attempt++)
                {
                    float angle = RandomRange(
                        rng, 0f, Mathf.PI * 2f
                    );

                    float radius = clusterRadius *
                        Mathf.Sqrt(RandomRange(rng, 0f, 1f));

                    point = center + new Vector3(
                        Mathf.Cos(angle) * radius,
                        0f,
                        Mathf.Sin(angle) * radius
                    );

                    if (IsInsideGround(point) &&
                        !IsExcluded(point))
                    {
                        found = true;
                        break;
                    }
                }

                if (!found)
                    continue;

                SpawnPlant(
                    original, point, minScale, maxScale,
                    parent, rng
                );

                if (savedPositions != null)
                    savedPositions.Add(point);

                placed++;
            }
        }
    }

    void ScatterNearTrees(
        GameObject original,
        int count,
        float minScale,
        float maxScale,
        List<Vector3> treePositions,
        float minRadius,
        float maxRadius,
        Transform parent,
        System.Random rng)
    {
        if (original == null || count <= 0)
            return;

        if (treePositions.Count == 0)
        {
            ScatterCluster(
                original, count, minScale, maxScale,
                5, 2f, parent, null, rng
            );
            return;
        }

        for (int i = 0; i < count; i++)
        {
            bool found = false;
            Vector3 point = Vector3.zero;

            for (int attempt = 0; attempt < 100; attempt++)
            {
                Vector3 tree = treePositions[
                    rng.Next(treePositions.Count)
                ];

                float angle = RandomRange(
                    rng, 0f, Mathf.PI * 2f
                );

                float radius = RandomRange(
                    rng, minRadius, maxRadius
                );

                point = tree + new Vector3(
                    Mathf.Cos(angle) * radius,
                    0f,
                    Mathf.Sin(angle) * radius
                );

                if (IsInsideGround(point) &&
                    !IsExcluded(point))
                {
                    found = true;
                    break;
                }
            }

            if (!found)
                continue;

            SpawnPlant(
                original, point, minScale, maxScale,
                parent, rng
            );
        }
    }

    bool IsExcluded(Vector3 position)
    {
        if (exclusionCenters == null ||
            exclusionRadii == null)
            return false;

        int count = Mathf.Min(
            exclusionCenters.Length,
            exclusionRadii.Length
        );

        for (int i = 0; i < count; i++)
        {
            if (exclusionCenters[i] == null)
                continue;

            Vector3 center = exclusionCenters[i].position;

            float dx = position.x - center.x;
            float dz = position.z - center.z;

            float distance = Mathf.Sqrt(
                dx * dx + dz * dz
            );

            if (distance < exclusionRadii[i])
                return true;
        }

        return false;
    }

    [ContextMenu("Clear Generated Plants")]
    public void ClearPlants()
    {
        Transform old = transform.Find("GeneratedPlants");

        if (old == null)
            return;

        old.gameObject.SetActive(false);

        if (Application.isPlaying)
            Destroy(old.gameObject);
        else
            DestroyImmediate(old.gameObject);
    }
}