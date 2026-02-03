using UnityEngine;

public class Balancer : MonoBehaviour
{
    public Rigidbody rb;
    public Transform target;
    public float followSpeed = 25f;

    void Reset()
    {
        rb = GetComponent<Rigidbody>();
    }

    void FixedUpdate()
    {
        if (!rb || !target) return;
        Vector3 p = Vector3.Lerp(rb.position, target.position, followSpeed * Time.fixedDeltaTime);
        rb.MovePosition(p);
    }
}
