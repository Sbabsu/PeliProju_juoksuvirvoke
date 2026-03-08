using UnityEngine;

public class SecurityCameraDetect : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GuardStateMachine guardStateMachine;
    [SerializeField] private Transform player;
    [SerializeField] private Light visionLight;

    [Header("Line of Sight")]
    [SerializeField] private LayerMask obstacleMask = ~0;

    private Renderer playerRenderer;

    private void Start()
    {
        if (player != null)
            playerRenderer = player.GetComponentInChildren<Renderer>();
    }

    private void Update()
    {
        if (guardStateMachine == null || player == null || visionLight == null)
            return;

        Transform eye = visionLight.transform;

        Vector3 origin = eye.position;

        Vector3 targetPos;
        if (playerRenderer != null)
            targetPos = playerRenderer.bounds.center;
        else
            targetPos = player.position + Vector3.up * 1.2f;

        Vector3 dirToPlayer = targetPos - origin;
        float distance = dirToPlayer.magnitude;

        // Käytä suoraan spotlightin rangea
        if (distance > visionLight.range)
            return;

        // Käytä suoraan spotlightin kulmaa
        float angle = Vector3.Angle(eye.forward, dirToPlayer.normalized);
        if (angle > visionLight.spotAngle * 0.5f)
            return;

        // Tarkista että mikään ei ole välissä
        if (Physics.Raycast(origin, dirToPlayer.normalized, out RaycastHit hit, visionLight.range, obstacleMask))
        {
            if (hit.transform != player && hit.transform.root != player)
                return;
        }

        // Pelaaja havaittu kamerassa
        guardStateMachine.TriggerCameraChase(player);
    }
}
