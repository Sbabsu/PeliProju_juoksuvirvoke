using UnityEngine;

public class PulseUI : MonoBehaviour
{
    [Header("Pulse")]
    public float scaleAmount = 0.06f; // 0.03–0.10 hyvä
    public float speed = 2.0f;        // 1–3 hyvä

    Vector3 baseScale;

    void Awake()
    {
        baseScale = transform.localScale;
    }

    void Update()
    {
        float s = 1f + Mathf.Sin(Time.time * speed) * scaleAmount;
        transform.localScale = baseScale * s;
    }
}
