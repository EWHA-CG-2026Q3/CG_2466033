using UnityEngine;
using System.Collections.Generic;

public class Mid_JungleProps : MonoBehaviour
{
    public Material cartMaterial;
    public Material monkeyMaterial;
    public Material faceMaterial;
    public Material bananaMaterial;
    public Material wheelMaterial;

    [ContextMenu("Build Props")]
    public void BuildProps()
    {
        ClearProps();

        Transform root = new GameObject("GeneratedProps").transform;
        root.SetParent(transform, false);

        Transform train = new GameObject("BoxTrain").transform;
        train.SetParent(root, false);

        BuildCart(train, -1.6f);
        BuildCart(train, 1.6f);

        BuildMonkey(root, new Vector3(-3.5f, 0f, 0.5f), "Monkey_1");
        BuildMonkey(root, new Vector3(-3.8f, 0f, -1.5f), "Monkey_2");

        for (int i = 0; i < 3; i++)
        {
            BuildBananaBunch(
                root,
                new Vector3(-2.5f + i * 1.45f, 0.16f, -3f)
            );
        }
    }

    void BuildCart(Transform parent, float z)
    {
        Transform cart = new GameObject("CargoCart").transform;
        cart.SetParent(parent, false);
        cart.localPosition = new Vector3(0f, 0f, z);

        Part(PrimitiveType.Cube, "Floor", cart,
            new Vector3(0f, 0.65f, 0f),
            new Vector3(2.1f, 0.18f, 2.5f), cartMaterial);

        Part(PrimitiveType.Cube, "LeftWall", cart,
            new Vector3(-0.95f, 1.25f, 0f),
            new Vector3(0.2f, 1.1f, 2.5f), cartMaterial);

        Part(PrimitiveType.Cube, "RightWall", cart,
            new Vector3(0.95f, 1.25f, 0f),
            new Vector3(0.2f, 1.1f, 2.5f), cartMaterial);

        Part(PrimitiveType.Cube, "FrontWall", cart,
            new Vector3(0f, 1.25f, 1.15f),
            new Vector3(2.1f, 1.1f, 0.2f), cartMaterial);

        Part(PrimitiveType.Cube, "BackWall", cart,
            new Vector3(0f, 1.25f, -1.15f),
            new Vector3(2.1f, 1.1f, 0.2f), cartMaterial);

        foreach (float x in new float[] { -0.95f, 0.95f })
        {
            foreach (float wheelZ in new float[] { -0.75f, 0.75f })
            {
                GameObject wheel = Part(
                    PrimitiveType.Cylinder, "Wheel", cart,
                    new Vector3(x, 0.4f, wheelZ),
                    new Vector3(0.38f, 0.12f, 0.38f),
                    wheelMaterial
                );

                wheel.transform.localRotation =
                    Quaternion.Euler(0f, 0f, 90f);
            }
        }
    }

    void BuildMonkey(Transform parent, Vector3 position, string name)
    {
        Transform monkey = new GameObject(name).transform;
        monkey.SetParent(parent, false);
        monkey.localPosition = position;

        Material beige = MakeColorMaterial(
            "MonkeyBeige", new Color(0.77f, 0.70f, 0.55f));
        Material pale = MakeColorMaterial(
            "MonkeyFaceCream", new Color(0.94f, 0.91f, 0.77f));
        Material brown = MakeColorMaterial(
            "MonkeyDarkBrown", new Color(0.23f, 0.22f, 0.17f));
        Material black = MakeColorMaterial(
            "MonkeyEyeBlack", new Color(0.055f, 0.055f, 0.05f));
        Material white = MakeColorMaterial(
            "MonkeyEyeWhite", new Color(0.99f, 0.98f, 0.92f));

        Part(PrimitiveType.Cube, "WoodenBody", monkey,
            new Vector3(0f, 0.64f, 0.12f),
            new Vector3(0.64f, 1.08f, 0.50f), beige);

        foreach (float side in new float[] { -1f, 1f })
        {
            Part(PrimitiveType.Cube, "Arm", monkey,
                new Vector3(side * 0.43f, 0.57f, 0.08f),
                new Vector3(0.20f, 0.84f, 0.24f), beige);

            Part(PrimitiveType.Cube, "Leg", monkey,
                new Vector3(side * 0.19f, 0.15f, 0.12f),
                new Vector3(0.25f, 0.30f, 0.34f), beige);
        }

        Transform head = new GameObject("SolidMonkeyHead").transform;
        head.SetParent(monkey, false);
        head.localPosition = new Vector3(0f, 1.45f, -0.14f);
        head.localScale = Vector3.one * 0.76f;

        Vector2[] outline =
        {
            new Vector2(-0.53f, 0.46f),
            new Vector2(0.53f, 0.46f),
            new Vector2(0.58f, 0.17f),
            new Vector2(0.48f, -0.32f),
            new Vector2(0.29f, -0.64f),
            new Vector2(0f, -0.78f),
            new Vector2(-0.29f, -0.64f),
            new Vector2(-0.48f, -0.32f),
            new Vector2(-0.58f, 0.17f)
        };

        MeshObject("ClosedBeigeHead", head,
            ExtrudeShape(outline, -0.28f, 0.58f), beige);

        Vector2[] creamFace =
        {
            new Vector2(-0.46f, 0.31f),
            new Vector2(0.46f, 0.31f),
            new Vector2(0.47f, -0.20f),
            new Vector2(0.32f, -0.55f),
            new Vector2(0f, -0.68f),
            new Vector2(-0.32f, -0.55f),
            new Vector2(-0.47f, -0.20f)
        };

        MeshObject("CreamFace", head,
            ExtrudeShape(creamFace, -0.315f, 0.055f), pale);

        Vector2[] muzzle =
        {
            new Vector2(-0.22f, 0.04f),
            new Vector2(0.22f, 0.04f),
            new Vector2(0.28f, -0.18f),
            new Vector2(0.25f, -0.45f),
            new Vector2(0.13f, -0.54f),
            new Vector2(-0.13f, -0.54f),
            new Vector2(-0.25f, -0.45f),
            new Vector2(-0.28f, -0.18f)
        };

        MeshObject("LongBrownMuzzle", head,
            ExtrudeShape(muzzle, -0.392f, 0.060f), brown);

        Part(PrimitiveType.Cube, "MouthLine", head,
            new Vector3(0f, -0.47f, -0.462f),
            new Vector3(0.30f, 0.020f, 0.018f), black);

        foreach (float side in new float[] { -1f, 1f })
        {
            Part(PrimitiveType.Sphere, "SmallDarkEar", head,
                new Vector3(side * 0.56f, 0.05f, -0.08f),
                new Vector3(0.16f, 0.21f, 0.12f), brown);

            Part(PrimitiveType.Sphere, "WhiteEye", head,
                new Vector3(side * 0.16f, 0.20f, -0.365f),
                new Vector3(0.24f, 0.24f, 0.09f), white);

            Part(PrimitiveType.Sphere, "Pupil", head,
                new Vector3(side * 0.16f, 0.20f, -0.421f),
                new Vector3(0.095f, 0.11f, 0.05f), black);
        }
    }

    Mesh ExtrudeShape(Vector2[] outline, float frontZ, float depth)
    {
        int n = outline.Length;
        List<Vector3> vertices = new List<Vector3>();
        List<int> tris = new List<int>();

        Vector2 center = Vector2.zero;

        for (int i = 0; i < n; i++)
            center += outline[i];

        center /= n;

        for (int i = 0; i < n; i++)
        {
            int j = (i + 1) % n;

            AddFaceTriangle(vertices, tris,
                new Vector3(center.x, center.y, frontZ),
                new Vector3(outline[i].x, outline[i].y, frontZ),
                new Vector3(outline[j].x, outline[j].y, frontZ));

            AddFaceTriangle(vertices, tris,
                new Vector3(center.x, center.y, frontZ + depth),
                new Vector3(outline[j].x, outline[j].y, frontZ + depth),
                new Vector3(outline[i].x, outline[i].y, frontZ + depth));

            Vector3 a = new Vector3(
                outline[i].x, outline[i].y, frontZ);
            Vector3 b = new Vector3(
                outline[j].x, outline[j].y, frontZ);
            Vector3 c = new Vector3(
                outline[i].x, outline[i].y, frontZ + depth);
            Vector3 d = new Vector3(
                outline[j].x, outline[j].y, frontZ + depth);

            AddFaceTriangle(vertices, tris, a, b, c);
            AddFaceTriangle(vertices, tris, b, d, c);
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices.ToArray();
        mesh.triangles = tris.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    void AddFaceTriangle(List<Vector3> vertices,
        List<int> tris, Vector3 a, Vector3 b, Vector3 c)
    {
        int start = vertices.Count;

        vertices.Add(a);
        vertices.Add(b);
        vertices.Add(c);

        tris.Add(start);
        tris.Add(start + 1);
        tris.Add(start + 2);
    }

    void AddTriangle(List<int> tris, int a, int b, int c)
    {
        tris.Add(a);
        tris.Add(b);
        tris.Add(c);
    }

    Material MakeColorMaterial(string name, Color color)
    {
        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");

        if (shader == null)
            shader = Shader.Find("Unlit/Color");

        if (shader == null)
            shader = Shader.Find("Standard");

        Material material = new Material(shader);
        material.name = name;

        if (material.HasProperty("_BaseColor"))
            material.SetColor("_BaseColor", color);

        if (material.HasProperty("_Color"))
            material.SetColor("_Color", color);

        if (material.HasProperty("_Cull"))
            material.SetFloat("_Cull", 0f);

        return material;
    }

    void BuildBananaBunch(Transform parent, Vector3 position)
    {
        Transform bunch = new GameObject("BananaBunch").transform;
        bunch.SetParent(parent, false);
        bunch.localPosition = position + Vector3.up * 0.23f;

        Material yellow = bananaMaterial != null
            ? bananaMaterial
            : MakeColorMaterial(
                "BananaYellow",
                new Color(1f, 0.83f, 0.06f)
            );

        Material stem = MakeColorMaterial(
            "BananaStemBrown",
            new Color(0.36f, 0.24f, 0.09f)
        );

        Part(
            PrimitiveType.Cylinder,
            "BrownStem",
            bunch,
            new Vector3(-0.02f, 0.60f, 0f),
            new Vector3(0.08f, 0.12f, 0.08f),
            stem
        ).transform.localRotation = Quaternion.Euler(0f, 90f, 90f);

        for (int layer = 0; layer < 2; layer++)
        {
            int count = layer == 0 ? 5 : 4;
            float spread = layer == 0 ? 42f : 32f;
            float scale = layer == 0 ? 1f : 0.88f;

            for (int i = 0; i < count; i++)
            {
                float t = count == 1
                    ? 0.5f
                    : i / (float)(count - 1);

                float angle = Mathf.Lerp(-spread, spread, t);

                Transform banana = new GameObject(
                    "Banana_" + layer + "_" + i
                ).transform;

                banana.SetParent(bunch, false);

                banana.localPosition = new Vector3(
                    0f,
                    layer * 0.15f,
                    (t - 0.5f) * 0.06f
                );

                banana.localRotation = Quaternion.Euler(
                    layer == 0 ? -8f : 8f,
                    angle,
                    0f
                );

                banana.localScale = Vector3.one * scale;

                MeshObject(
                    "CurvedBanana",
                    banana,
                    BuildBananaMesh(),
                    yellow
                );
            }
        }
    }

    Mesh BuildBananaMesh()
    {
        const int rings = 21;
        const int sides = 12;

        Vector3[] vertices = new Vector3[rings * sides + 2];
        List<int> tris = new List<int>();

        for (int i = 0; i < rings; i++)
        {
            float t = i / (float)(rings - 1);
            float bend = Mathf.Sin(Mathf.PI * t);

            Vector3 center = new Vector3(
                0.98f * t,
                0.50f - 0.24f * bend + 0.06f * t,
                0f);

            Vector3 tangent = new Vector3(
                0.98f,
                -0.24f * Mathf.PI * Mathf.Cos(Mathf.PI * t) + 0.06f,
                0f).normalized;

            Vector3 normal = new Vector3(
                -tangent.y, tangent.x, 0f);

            float taper = Mathf.Pow(
                Mathf.Max(0f, Mathf.Sin(Mathf.PI * t)),
                0.55f);

            float radius = 0.006f + 0.105f * taper;

            for (int j = 0; j < sides; j++)
            {
                float angle = 2f * Mathf.PI * j / sides;

                vertices[i * sides + j] = center + radius *
                    (normal * Mathf.Cos(angle) +
                     Vector3.forward * Mathf.Sin(angle));
            }
        }

        for (int i = 0; i < rings - 1; i++)
        {
            for (int j = 0; j < sides; j++)
            {
                int a = i * sides + j;
                int b = i * sides + (j + 1) % sides;
                int c = (i + 1) * sides + j;
                int d = (i + 1) * sides + (j + 1) % sides;

                AddTriangle(tris, a, c, b);
                AddTriangle(tris, b, c, d);
            }
        }

        int firstCap = rings * sides;
        int lastCap = firstCap + 1;

        vertices[firstCap] = new Vector3(0f, 0.50f, 0f);
        vertices[lastCap] = new Vector3(0.98f, 0.56f, 0f);

        for (int j = 0; j < sides; j++)
        {
            int next = (j + 1) % sides;

            AddTriangle(tris, firstCap, next, j);

            AddTriangle(
                tris,
                lastCap,
                (rings - 1) * sides + j,
                (rings - 1) * sides + next);
        }

        Mesh mesh = new Mesh();
        mesh.vertices = vertices;
        mesh.triangles = tris.ToArray();
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        return mesh;
    }

    GameObject MeshObject(
        string name, Transform parent, Mesh mesh, Material material)
    {
        GameObject obj = new GameObject(name);
        obj.transform.SetParent(parent, false);

        obj.AddComponent<MeshFilter>().sharedMesh = mesh;
        obj.AddComponent<MeshRenderer>().sharedMaterial = material;

        return obj;
    }

    GameObject Part(
        PrimitiveType type, string name, Transform parent,
        Vector3 position, Vector3 scale, Material material)
    {
        GameObject obj = GameObject.CreatePrimitive(type);
        obj.name = name;
        obj.transform.SetParent(parent, false);
        obj.transform.localPosition = position;
        obj.transform.localScale = scale;

        if (material != null)
            obj.GetComponent<Renderer>().sharedMaterial = material;

        return obj;
    }

    [ContextMenu("Clear Props")]
    public void ClearProps()
    {
        Transform old = transform.Find("GeneratedProps");

        if (old == null)
            return;

        old.gameObject.SetActive(false);

        if (Application.isPlaying)
            Destroy(old.gameObject);
        else
            DestroyImmediate(old.gameObject);
    }
}