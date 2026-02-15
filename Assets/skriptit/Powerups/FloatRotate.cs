using UnityEngine;

public class FloatRotate : MonoBehaviour
{
    [Header("Float")]
    public float floatHeight = 0.25f;   // kuinka paljon ylös/alas
    public float floatSpeed = 2f;       // kuinka nopeasti “aaltoilee”

    [Header("Rotate")]
    public float rotateSpeed = 90f;     // astetta / sekunti
    public Vector3 rotateAxis = Vector3.up;

    Vector3 startPos;

    void Start()
    {
        startPos = transform.position;
    }

    void Update()
    {
        // Kelluminen
        float y = Mathf.Sin(Time.time * floatSpeed) * floatHeight;
        transform.position = startPos + new Vector3(0f, y, 0f);

        // Pyöriminen
        transform.Rotate(rotateAxis.normalized, rotateSpeed * Time.deltaTime, Space.World);
    }
}
