using UnityEngine;
using UnityEngine.AI;

public class GuardAI : MonoBehaviour
{
    public NavMeshAgent agent;
    public Transform player;

    public Transform[] patrolPoints;
    private int currentPatrolIndex;

    public float viewDistance = 10f;
    public float viewAngle = 60f;
    public float loseSightTime = 3f;

    private float loseSightTimer;
    private bool chasingPlayer;
    private Vector3 lastKnownPlayerPosition;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        // try to find the player by tag
        if (player == null)
        {
            player = GameObject.FindGameObjectWithTag("Player")?.transform;

            if (player != null)
            {
                Debug.Log("Found player: " + player.name);
            }
            else
            {
                Debug.LogError("Could not find player with tag 'Player'!");
            }
        }

        if (agent == null)
        {
            Debug.LogError("No NavMeshAgent component found on " + gameObject.name, this);
            return;
        }

        if (patrolPoints.Length > 0)
        {
            GoToNextPatrolPoint();
        }
    }


    void Update()
    {
        if (player == null)
        {
            Debug.Log("Player is NULL!", this);
            return;
        }

        if (agent != null && !agent.isOnNavMesh)
        {
            Debug.Log("Agent not on NavMesh!", this);
            return;
        }

        // RAGDOLL FIX: Get the center of the ragdoll (hips position)
        Vector3 playerCenter = GetRagdollCenter();

        bool canSeePlayer = CanSeeRagdollPlayer(playerCenter);

        // DEBUG: Always show distance and if we can see player
        float distance = Vector3.Distance(transform.position, playerCenter);
        Debug.Log($"Distance: {distance:F1} | CanSee: {canSeePlayer} | Chasing: {chasingPlayer}");

        if (canSeePlayer)
        {
            chasingPlayer = true;
            loseSightTimer = loseSightTime;
            lastKnownPlayerPosition = playerCenter; // Store last known position

            // DEBUG: When we see player, set destination immediately
            Debug.Log("PLAYER SPOTTED! Setting chase destination to: " + playerCenter);
        }

        if (chasingPlayer)
        {
            // RAGDOLL FIX: Chase the ragdoll center, not just transform.position
            agent.SetDestination(playerCenter);

            // DEBUG: Show destination line
            Debug.DrawLine(transform.position, agent.destination, Color.red, 0.1f);

            if (!canSeePlayer)
            {
                loseSightTimer -= Time.deltaTime;
                Debug.Log($"Can't see player. Timer: {loseSightTimer:F1}");

                if (loseSightTimer <= 0f)
                {
                    chasingPlayer = false;
                    GoToNextPatrolPoint();
                }
            }
        }
        else
        {
            if (!agent.pathPending && agent.remainingDistance < 0.5f)
            {
                GoToNextPatrolPoint();
            }
        }

        // Simple rotation to face target when chasing
        if (chasingPlayer)
        {
            RotateTowards(playerCenter);
        }
    }

    void GoToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;

        Debug.Log("Going to patrol point: " + currentPatrolIndex);
        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    // RAGDOLL FIX: Get the center of the ragdoll (usually the hips)
    Vector3 GetRagdollCenter()
    {
        // Try to find the hips/pelvis rigidbody (common in ragdolls)
        if (player != null)
        {
            Rigidbody[] rigidbodies = player.GetComponentsInChildren<Rigidbody>();

            Debug.Log($"Found {rigidbodies.Length} rigidbodies on player");

            foreach (Rigidbody rb in rigidbodies)
            {
                // Common ragdoll bone names
                if (rb.name.Contains("Hip"))
                {
                    Debug.Log($"Using ragdoll bone: {rb.name} at position: {rb.transform.position}");
                    return rb.transform.position;
                }
            }

            // If no hips found, try to find any Rigidbody and use that
            if (rigidbodies.Length > 0)
            {
                Debug.Log($"No hips found. Using first rigidbody: {rigidbodies[0].name}");
                return rigidbodies[0].transform.position;
            }
        }

        // Fallback to player transform position
        Debug.Log("Using player transform position as fallback");
        return player != null ? player.position : Vector3.zero;
    }

    // RAGDOLL FIX: Modified to work with ragdolls
    bool CanSeeRagdollPlayer(Vector3 playerCenter)
    {
        Vector3 directionToPlayer = playerCenter - transform.position;
        float distance = directionToPlayer.magnitude;

        // DEBUG: Always draw line to player
        Debug.DrawLine(transform.position, playerCenter, distance < viewDistance ? Color.white : Color.gray);

        // 1. Distance check
        if (distance > viewDistance)
        {
            Debug.Log($"Player too far: {distance} > {viewDistance}");
            return false;
        }

        // 2. Angle check (field of view)
        float angle = Vector3.Angle(transform.forward, directionToPlayer);
        if (angle > viewAngle / 2f)
        {
            Debug.Log($"Player outside view angle: {angle} > {viewAngle / 2f}");
            return false;
        }

        // DEBUG: Show view cone
        Debug.DrawRay(transform.position, transform.forward * viewDistance, Color.green);

        // 3. Line of sight check
        RaycastHit hit;
        Vector3 rayOrigin = transform.position + Vector3.up * 1f; // Eye level

        // FIRST: Try center point
        if (Physics.Raycast(rayOrigin, directionToPlayer.normalized, out hit, viewDistance))
        {
            Debug.Log($"Raycast hit: {hit.transform.name}");

            // Check if we hit the player or any child of the player (ragdoll part)
            if (hit.transform == player || hit.transform.IsChildOf(player))
            {
                Debug.DrawRay(rayOrigin, directionToPlayer.normalized * distance, Color.green);
                Debug.Log("CAN SEE PLAYER (center hit)!");
                return true;
            }
            else
            {
                Debug.DrawRay(rayOrigin, directionToPlayer.normalized * hit.distance, Color.magenta);
                Debug.Log($"Blocked by: {hit.transform.name}");
            }
        }
        else
        {
            Debug.Log("Raycast didn't hit anything (center)");
        }

        // SECOND: Try multiple points on ragdoll (since ray might miss center)
        Vector3[] checkPoints = new Vector3[3];
        checkPoints[0] = playerCenter + Vector3.up * 1.0f; // Head level
        checkPoints[1] = playerCenter; // Center
        checkPoints[2] = playerCenter - Vector3.up * 0.5f; // Lower body

        for (int i = 0; i < checkPoints.Length; i++)
        {
            Vector3 directionToPoint = checkPoints[i] - rayOrigin;

            // Skip if this point is too far
            if (Vector3.Distance(rayOrigin, checkPoints[i]) > viewDistance) continue;

            if (Physics.Raycast(rayOrigin, directionToPoint.normalized, out hit, viewDistance))
            {
                if (hit.transform == player || hit.transform.IsChildOf(player))
                {
                    Debug.DrawRay(rayOrigin, directionToPoint.normalized * Vector3.Distance(rayOrigin, checkPoints[i]), Color.cyan);
                    Debug.Log($"CAN SEE PLAYER (point {i} hit)!");
                    return true;
                }
            }
        }

        // THIRD: Try simpler check - just check if ANY collider from player is within view
        // This bypasses raycast issues for testing
        Collider[] playerColliders = player.GetComponentsInChildren<Collider>();
        foreach (Collider col in playerColliders)
        {
            Vector3 colPosition = col.bounds.center;
            Vector3 directionToCol = colPosition - rayOrigin;
            float colDistance = directionToCol.magnitude;

            if (colDistance <= viewDistance)
            {
                // Check angle to this collider
                float colAngle = Vector3.Angle(transform.forward, directionToCol);
                if (colAngle <= viewAngle / 2f)
                {
                    // Quick raycast to this collider
                    if (Physics.Raycast(rayOrigin, directionToCol.normalized, out hit, viewDistance))
                    {
                        if (hit.transform == player || hit.transform.IsChildOf(player))
                        {
                            Debug.DrawRay(rayOrigin, directionToCol.normalized * colDistance, Color.yellow);
                            Debug.Log($"CAN SEE PLAYER (collider {col.name} hit)!");
                            return true;
                        }
                    }
                }
            }
        }

        Debug.Log("CANNOT SEE PLAYER - all checks failed");
        return false;
    }

    // Simple rotation helper
    void RotateTowards(Vector3 targetPosition)
    {
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.y = 0; // Only rotate on Y axis

        if (direction != Vector3.zero)
        {
            Quaternion targetRotation = Quaternion.LookRotation(direction);
            transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, 5f * Time.deltaTime);
        }
    }

    // For debugging in Scene view
    void OnDrawGizmosSelected()
    {
        if (player != null)
        {
            Vector3 playerCenter = GetRagdollCenter();

            // Draw view cone
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, viewDistance);

            Vector3 leftBoundary = Quaternion.Euler(0, -viewAngle / 2, 0) * transform.forward * viewDistance;
            Vector3 rightBoundary = Quaternion.Euler(0, viewAngle / 2, 0) * transform.forward * viewDistance;

            Gizmos.color = Color.green;
            Gizmos.DrawRay(transform.position, transform.forward * viewDistance);
            Gizmos.DrawRay(transform.position, leftBoundary);
            Gizmos.DrawRay(transform.position, rightBoundary);

            // Draw line to player
            Gizmos.color = chasingPlayer ? Color.red : Color.white;
            Gizmos.DrawLine(transform.position, playerCenter);

            // Draw player center
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(playerCenter, 0.2f);

            // Draw guard's forward direction
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(transform.position, transform.forward * 2f);
        }
    }
}