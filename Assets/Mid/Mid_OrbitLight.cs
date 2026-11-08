using UnityEngine;

public class Mid_OrbitLight : MonoBehaviour
{
    public int lightCount = 12;
    public float width = 1.2f;
    public float height = 1.5f;
    public float orbitSpeed = 25f;
    public float dotSize = 0.045f;

    Transform orbit;

    void Start()
    {
        orbit = new GameObject("LightOrbit").transform;
        orbit.SetParent(transform, false);

        Shader shader = Shader.Find("Universal Render Pipeline/Unlit");
        Material mat = new Material(shader);

        Color yellow = new Color(1f, 0.9f, 0.15f);
        mat.SetColor("_BaseColor", yellow);

        if (mat.HasProperty("_EmissionColor"))
        {
            mat.EnableKeyword("_EMISSION");
            mat.SetColor("_EmissionColor", yellow * 3f);
        }

        for (int i = 0; i < lightCount; i++)
        {
            float angle = i * Mathf.PI * 2f / lightCount;

            GameObject dot = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            dot.name = "LightDot_" + i;
            dot.transform.SetParent(orbit, false);

            dot.transform.localPosition = new Vector3(
                Mathf.Cos(angle) * width,
                Mathf.Sin(angle) * height,
                0f
            );

            dot.transform.localScale = Vector3.one * dotSize;
            dot.GetComponent<Renderer>().sharedMaterial = mat;

            Destroy(dot.GetComponent<Collider>());
        }
    }

    void Update()
    {
        if (orbit == null) return;

        orbit.localRotation = Quaternion.Euler(
            0f,
            0f,
            Time.time * orbitSpeed
        );
    }
}