using UnityEngine;

public class Mid_PaintingSwing : MonoBehaviour
{
    public float swingAngle = 8f;
    public float swingSpeed = 1.2f;

    private Quaternion initialRotation;

    void Start()
    {
        initialRotation = transform.localRotation;
    }

    void Update()
    {
        float angle = Mathf.Sin(Time.time * swingSpeed)
                    * swingAngle;

        transform.localRotation =
            initialRotation * Quaternion.Euler(angle, 0f, 0f);
    }
}