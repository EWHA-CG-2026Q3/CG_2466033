using UnityEngine;
using System.Collections.Generic;

[ExecuteAlways]
public class Mid_MonkeyRasterizer : MonoBehaviour
{
    [Header("Painting")]
    [Range(128, 1024)] public int resolution = 512;
    public float paintingWidth = 1.8f;
    public float paintingHeight = 1.8f;
    public float frameWidth = 0.12f;
    public float frameDepth = 0.10f;

    [Header("Placement")]
    public bool faceForward = true;

    private GameObject generated;
    private Texture2D paintingTexture;
    private Material paintingMaterial;
    private Material frameMaterial;
    private Material backMaterial;

    void OnEnable()
    {
        BuildPainting();
    }

    [ContextMenu("Rebuild Painting")]
    public void BuildPainting()
    {
        if (paintingWidth <= 0f || paintingHeight <= 0f ||
            resolution < 2)
            return;

        Transform old = transform.Find("GeneratedMonkeyPainting");

        if (old != null)
        {
            old.gameObject.SetActive(false);
            DestroyImmediate(old.gameObject);
        }

        CleanupResources();

        generated = new GameObject("GeneratedMonkeyPainting");
        generated.transform.SetParent(transform, false);

        paintingTexture = new Texture2D(
            resolution, resolution, TextureFormat.RGBA32, false
        );

        paintingTexture.name = "MonkeyRasterizedTexture";
        paintingTexture.filterMode = FilterMode.Point;
        paintingTexture.wrapMode = TextureWrapMode.Clamp;

        Color[] pixels = new Color[resolution * resolution];

        Color background = Hex("#193E37");

        for (int i = 0; i < pixels.Length; i++)
            pixels[i] = background;

        DrawArtwork(pixels);

        paintingTexture.SetPixels(pixels);
        paintingTexture.Apply();

        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");

        if (shader == null)
            shader = Shader.Find("Unlit/Texture");

        paintingMaterial = new Material(shader);
        paintingMaterial.name = "MonkeyPaintingMaterial";
        paintingMaterial.mainTexture = paintingTexture;

        if (paintingMaterial.HasProperty("_BaseMap"))
            paintingMaterial.SetTexture("_BaseMap", paintingTexture);

        if (paintingMaterial.HasProperty("_BaseColor"))
            paintingMaterial.SetColor("_BaseColor", Color.white);

        Shader colorShader = Shader.Find("Universal Render Pipeline/Lit");

        if (colorShader == null)
            colorShader = Shader.Find("Standard");

        frameMaterial = new Material(colorShader);
        frameMaterial.name = "MonkeyFrameMaterial";
        frameMaterial.color = Hex("#684026");

        backMaterial = new Material(colorShader);
        backMaterial.name = "MonkeyBackMaterial";
        backMaterial.color = Hex("#30271E");

        CreateQuad(
            "MonkeyArtwork",
            Vector3.zero,
            paintingWidth,
            paintingHeight,
            paintingMaterial
        );

        CreateBox(
            "FrameTop",
            new Vector3(0f, paintingHeight * 0.5f + frameWidth * 0.5f, -0.02f),
            new Vector3(paintingWidth + frameWidth * 2f, frameWidth, frameDepth),
            frameMaterial
        );

        CreateBox(
            "FrameBottom",
            new Vector3(0f, -paintingHeight * 0.5f - frameWidth * 0.5f, -0.02f),
            new Vector3(paintingWidth + frameWidth * 2f, frameWidth, frameDepth),
            frameMaterial
        );

        CreateBox(
            "FrameLeft",
            new Vector3(-paintingWidth * 0.5f - frameWidth * 0.5f, 0f, -0.02f),
            new Vector3(frameWidth, paintingHeight, frameDepth),
            frameMaterial
        );

        CreateBox(
            "FrameRight",
            new Vector3(paintingWidth * 0.5f + frameWidth * 0.5f, 0f, -0.02f),
            new Vector3(frameWidth, paintingHeight, frameDepth),
            frameMaterial
        );

        CreateBox(
            "FrameBacking",
            new Vector3(0f, 0f, -0.09f),
            new Vector3(
                paintingWidth + frameWidth,
                paintingHeight + frameWidth,
                0.05f
            ),
            backMaterial
        );
    }

    void DrawArtwork(Color[] pixels)
    {
        Color darkGreen = Hex("#193E37");
        Color forest = Hex("#286052");
        Color teal = Hex("#3F8270");
        Color lime = Hex("#91AC55");
        Color ochre = Hex("#C18B4D");
        Color cream = Hex("#E6D0A8");
        Color beige = Hex("#CDB99D");
        Color shadow = Hex("#9D8168");
        Color darkBrown = Hex("#3C302B");
        Color mouth = Hex("#542A32");
        Color red = Hex("#A34A4B");
        Color tongue = Hex("#DB827B");

        Polygon(pixels, forest,
            V(0, 0), V(220, 0), V(95, 512), V(0, 512));

        Polygon(pixels, teal,
            V(320, 0), V(512, 0), V(512, 512), V(440, 512));

        Polygon(pixels, lime,
            V(0, 390), V(100, 512), V(140, 512), V(65, 340));

        Polygon(pixels, darkGreen,
            V(390, 512), V(512, 360), V(512, 512));

        Polygon(pixels, ochre,
            V(30, 80), V(145, 30), V(95, 160));

        Polygon(pixels, forest,
            V(365, 30), V(490, 110), V(440, 210));

        Polygon(pixels, shadow,
            V(105, 345), V(405, 345), V(365, 220),
            V(295, 105), V(255, 75), V(215, 105), V(145, 220)
        );

        Polygon(pixels, beige,
            V(125, 330), V(385, 330), V(350, 225),
            V(285, 120), V(255, 95), V(225, 120), V(160, 225)
        );

        Polygon(pixels, cream,
            V(150, 315), V(245, 330), V(255, 100), V(195, 175)
        );

        Polygon(pixels, ochre,
            V(265, 330), V(365, 315), V(315, 175), V(255, 100)
        );

        Polygon(pixels, darkBrown,
            V(140, 255), V(112, 245), V(118, 285), V(148, 280)
        );

        Polygon(pixels, darkBrown,
            V(370, 255), V(398, 245), V(392, 285), V(362, 280)
        );

        Polygon(pixels, darkBrown,
            V(175, 305), V(215, 320), V(245, 305),
            V(222, 290));

        Polygon(pixels, darkBrown,
            V(275, 305), V(320, 320), V(345, 302),
            V(320, 290));

        Polygon(pixels, cream,
            V(185, 310), V(215, 313), V(235, 305),
            V(215, 301));

        Polygon(pixels, cream,
            V(285, 310), V(315, 313), V(335, 304),
            V(315, 301));

        Polygon(pixels, shadow,
            V(215, 285), V(250, 265), V(295, 285),
            V(270, 245), V(235, 245));

        Polygon(pixels, darkBrown,
            V(232, 258), V(280, 258), V(270, 238),
            V(245, 238));

        Polygon(pixels, mouth,
            V(205, 230), V(235, 210), V(280, 210),
            V(315, 235), V(305, 160), V(275, 125),
            V(235, 125), V(205, 160));

        Polygon(pixels, red,
            V(225, 195), V(285, 195), V(295, 155),
            V(275, 135), V(235, 135), V(215, 160));

        Polygon(pixels, tongue,
            V(230, 155), V(255, 170), V(282, 155),
            V(275, 135), V(240, 135));

        Polygon(pixels, cream,
            V(220, 220), V(290, 220), V(280, 210),
            V(232, 210));

        Polygon(pixels, darkBrown,
            V(195, 145), V(235, 105), V(255, 75),
            V(275, 105), V(315, 145), V(285, 115),
            V(255, 95), V(225, 115)
        );

        Polygon(pixels, lime,
            V(0, 40), V(60, 70), V(90, 25));

        Polygon(pixels, teal,
            V(410, 45), V(512, 20), V(512, 95));

        Polygon(pixels, ochre,
            V(35, 440), V(85, 480), V(120, 425));

        Polygon(pixels, lime,
            V(405, 430), V(475, 485), V(512, 455));
    }

    Vector2 V(float x, float y)
    {
        return new Vector2(
            x / 512f * (resolution - 1),
            y / 512f * (resolution - 1)
        );
    }

    void Polygon(Color[] pixels, Color color, params Vector2[] points)
    {
        if (points.Length < 3)
            return;

        for (int i = 1; i < points.Length - 1; i++)
        {
            FillTriangle(
                pixels,
                points[0],
                points[i],
                points[i + 1],
                color
            );
        }
    }

    void FillTriangle(
        Color[] pixels,
        Vector2 a,
        Vector2 b,
        Vector2 c,
        Color color)
    {
        int minX = Mathf.Clamp(
            Mathf.FloorToInt(Mathf.Min(a.x, Mathf.Min(b.x, c.x))),
            0, resolution - 1
        );

        int maxX = Mathf.Clamp(
            Mathf.CeilToInt(Mathf.Max(a.x, Mathf.Max(b.x, c.x))),
            0, resolution - 1
        );

        int minY = Mathf.Clamp(
            Mathf.FloorToInt(Mathf.Min(a.y, Mathf.Min(b.y, c.y))),
            0, resolution - 1
        );

        int maxY = Mathf.Clamp(
            Mathf.CeilToInt(Mathf.Max(a.y, Mathf.Max(b.y, c.y))),
            0, resolution - 1
        );

        for (int y = minY; y <= maxY; y++)
        {
            for (int x = minX; x <= maxX; x++)
            {
                Vector2 p = new Vector2(x + 0.5f, y + 0.5f);

                if (IsInsideTriangle(p, a, b, c))
                    pixels[y * resolution + x] = color;
            }
        }
    }

    bool IsInsideTriangle(
        Vector2 p,
        Vector2 a,
        Vector2 b,
        Vector2 c)
    {
        float denominator =
            (b.y - c.y) * (a.x - c.x) +
            (c.x - b.x) * (a.y - c.y);

        if (Mathf.Abs(denominator) < 0.000001f)
            return false;

        float w1 =
            ((b.y - c.y) * (p.x - c.x) +
             (c.x - b.x) * (p.y - c.y)) / denominator;

        float w2 =
            ((c.y - a.y) * (p.x - c.x) +
             (a.x - c.x) * (p.y - c.y)) / denominator;

        float w3 = 1f - w1 - w2;

        return w1 >= -0.00001f &&
               w2 >= -0.00001f &&
               w3 >= -0.00001f;
    }

    Color Hex(string value)
    {
        ColorUtility.TryParseHtmlString(value, out Color color);
        return color;
    }

    void CreateQuad(
        string objectName,
        Vector3 position,
        float width,
        float height,
        Material material)
    {
        GameObject obj = new GameObject(objectName);
        obj.transform.SetParent(generated.transform, false);
        obj.transform.localPosition = position;

        MeshFilter filter = obj.AddComponent<MeshFilter>();
        MeshRenderer renderer = obj.AddComponent<MeshRenderer>();

        Mesh mesh = new Mesh();
        mesh.name = objectName;

        mesh.vertices = new Vector3[]
        {
            new Vector3(-width * 0.5f, -height * 0.5f, 0f),
            new Vector3(width * 0.5f, -height * 0.5f, 0f),
            new Vector3(width * 0.5f, height * 0.5f, 0f),
            new Vector3(-width * 0.5f, height * 0.5f, 0f)
        };

        mesh.uv = new Vector2[]
        {
            new Vector2(0f, 0f),
            new Vector2(1f, 0f),
            new Vector2(1f, 1f),
            new Vector2(0f, 1f)
        };

        mesh.triangles = new int[]
        {
            0, 1, 2,
            0, 2, 3
        };

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();

        filter.sharedMesh = mesh;
        renderer.sharedMaterial = material;
    }

    void CreateBox(
        string objectName,
        Vector3 position,
        Vector3 scale,
        Material material)
    {
        GameObject obj = GameObject.CreatePrimitive(
            PrimitiveType.Cube
        );

        obj.name = objectName;
        obj.transform.SetParent(generated.transform, false);
        obj.transform.localPosition = position;
        obj.transform.localScale = scale;

        Collider collider = obj.GetComponent<Collider>();

        if (collider != null)
            DestroyImmediate(collider);

        obj.GetComponent<MeshRenderer>().sharedMaterial = material;
    }

    void CleanupResources()
    {
        if (paintingTexture != null)
            DestroyImmediate(paintingTexture);

        if (paintingMaterial != null)
            DestroyImmediate(paintingMaterial);

        if (frameMaterial != null)
            DestroyImmediate(frameMaterial);

        if (backMaterial != null)
            DestroyImmediate(backMaterial);
    }

    void OnDisable()
    {
        CleanupResources();
    }
}