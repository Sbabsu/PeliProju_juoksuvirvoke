using UnityEngine;

public class SecurityCameraDetect : MonoBehaviour
{
    [SerializeField] private GuardAI guardAI;
    [SerializeField] private GuardStateMachine guardStateMachine;
    [SerializeField] private Transform player;
    [SerializeField] private Light visionLight;

    [Header("Detection")]
    [SerializeField] private float viewDistance = 12f;
    [SerializeField] private float viewAngle = 60f;

    private void Update()
    {
        Debug.Log("SecurityCameraDetect is running");

        if (guardAI == null)
        {
            Debug.Log("guardAI missing");
            return;
        }

        if (guardStateMachine == null)
        {
            Debug.Log("guardStateMachine missing");
            return;
        }

        if (player == null)
        {
            Debug.Log("player missing");
            return;
        }

        if (visionLight == null)
        {
            Debug.Log("visionLight missing");
            return;
        }

        Transform eye = visionLight.transform;

        Vector3 origin = eye.position;
        Vector3 targetPos = player.position + Vector3.up * 1.2f;
        Vector3 dirToPlayer = targetPos - origin;

        float distance = dirToPlayer.magnitude;
        Debug.Log("Distance = " + distance);

        float angle = Vector3.Angle(eye.forward, dirToPlayer.normalized);
        Debug.Log("Angle = " + angle);

        if (distance <= viewDistance && angle <= viewAngle)
        {
            Debug.Log("PLAYER DETECTED");

            guardAI.SetPlayerTarget(player);
            guardStateMachine.ForceChase();
        }
    }
}