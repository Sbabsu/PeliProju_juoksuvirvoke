using UnityEngine;
using UnityEngine.AI;

public class GuardAI : MonoBehaviour
{
    [Header("Refs")]
    public NavMeshAgent agent;
    public Transform player;

    [Header("Patrol")]
    public Transform[] patrolPoints;
    [HideInInspector] public int currentPatrolIndex;

    [Header("Vision")]
    public float viewDistance = 10f;
    public float viewAngle = 60f;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;
            if (player == null)
                Debug.LogError("Could not find player with tag 'Player'!");
        }

        if (agent == null)
        {
            Debug.LogError("No NavMeshAgent component found on " + gameObject.name, this);
            return;
        }

        if (patrolPoints != null && patrolPoints.Length > 0)
            GoToNextPatrolPoint();
    }

    // -------- Public helpers used by the AI state --------

    public void GoToNextPatrolPoint()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    public Vector3 GetRagdollCenter()
    {
        if (player != null)
        {
            Rigidbody[] rigidbodies = player.GetComponentsInChildren<Rigidbody>();
            foreach (Rigidbody rb in rigidbodies)
            {
                if (rb.name.Contains("Hip"))
                    return rb.transform.position;
            }

            if (rigidbodies.Length > 0)
                return rigidbodies[0].transform.position;
        }

        return player != null ? player.position : Vector3.zero;
    }

    public bool CanSeeRagdollPlayer(Vector3 playerCenter)
    {
        Vector3 directionToPlayer = playerCenter - transform.position;
        float distance = directionToPlayer.magnitude;

        if (distance > viewDistance) return false;

        float angle = Vector3.Angle(transform.forward, directionToPlayer);
        if (angle > viewAngle / 2f) return false;

        RaycastHit hit;
        Vector3 rayOrigin = transform.position + Vector3.up * 1f; // eye level

        // Raycast to center
        if (Physics.Raycast(rayOrigin, directionToPlayer.normalized, out hit, viewDistance))
        {
            if (hit.transform == player || hit.transform.IsChildOf(player))
                return true;
        }

        // Try a couple points (helps ragdolls)
        Vector3[] points =
        {
            playerCenter + Vector3.up * 1.0f,
            playerCenter,
            playerCenter - Vector3.up * 0.5f
        };

        for (int i = 0; i < points.Length; i++)
        {
            Vector3 dir = points[i] - rayOrigin;
            if (dir.magnitude > viewDistance) continue;

            if (Physics.Raycast(rayOrigin, dir.normalized, out hit, viewDistance))
            {
                if (hit.transform == player || hit.transform.IsChildOf(player))
                    return true;
            }
        }

        return false;
    }

    public void RotateTowards(Vector3 targetPosition, float turnSpeed = 5f)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0;

        if (direction.sqrMagnitude > 0.0001f)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, turnSpeed * Time.deltaTime);
        }
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, viewDistance);

        Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle / 2, 0) * transform.forward * viewDistance;
        Vector3 rightBoundary = Quaternion.Euler(0, viewAngle / 2, 0) * transform.forward * viewDistance;

        Gizmos.color = Color.green;
        Gizmos.DrawRay(transform.position, transform.forward * viewDistance);
        Gizmos.DrawRay(transform.position, leftBoundary);
        Gizmos.DrawRay(transform.position, rightBoundary);

        if (player != null)
        {
            Vector3 center = GetRagdollCenter();
            Gizmos.color = Color.red;
            Gizmos.DrawLine(transform.position, center);
            Gizmos.DrawSphere(center, 0.2f);
        }
    }
}
