using UnityEngine;

public class SecurityCameraSweep : MonoBehaviour
{
    [Header("Rotation")]
    [SerializeField] private Transform cameraHead;

    [SerializeField] private float rotationSpeed = 30f;
    [SerializeField] private float maxAngle = 45f;

    private float currentAngle = 0f;
    private int direction = 1;

    private void Start()
    {
        if (cameraHead == null)
            cameraHead = transform;
    }

    private void Update()
    {
        float step = rotationSpeed * Time.deltaTime * direction;
        currentAngle += step;

        if (currentAngle >= maxAngle)
        {
            currentAngle = maxAngle;
            direction = -1;
        }
        else if (currentAngle <= -maxAngle)
        {
            currentAngle = -maxAngle;
            direction = 1;
        }

        cameraHead.localRotation = Quaternion.Euler(0f, currentAngle, 0f);
    }
}