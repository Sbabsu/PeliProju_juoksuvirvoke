using UnityEngine;

public class Balancer : MonoBehaviour
{
    public Transform target;
    void Update()
    {
        transform.position = target.position;
    }
}
