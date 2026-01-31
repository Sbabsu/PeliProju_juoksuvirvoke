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

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        GoToNextPatrolPoint();

    }
  

    void Update()
    {
        if (agent != null && !agent.isOnNavMesh)
        Debug.Log("Agent not on NavMesh!", this);

        if (player == null)
        Debug.Log("Player is NULL!", this);
        if (CanSeePlayer())
        {
            chasingPlayer = true;
            loseSightTimer = loseSightTime;
        }

        if (chasingPlayer)
        {
            agent.SetDestination(player.position);

            if (!CanSeePlayer())
            {
                loseSightTimer -= Time.deltaTime;
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
    }

    void GoToNextPatrolPoint()
    {
        if (patrolPoints.Length == 0) return;

        agent.SetDestination(patrolPoints[currentPatrolIndex].position);
        currentPatrolIndex = (currentPatrolIndex + 1) % patrolPoints.Length;
    }

    bool CanSeePlayer()
    {
        Vector3 directionToPlayer = player.position - transform.position;
        float distance = directionToPlayer.magnitude;

        if (distance > viewDistance) return false;

        float angle = Vector3.Angle(transform.forward, directionToPlayer);
        if (angle > viewAngle / 2f) return false;

        RaycastHit hit;
        if (Physics.Raycast(transform.position + Vector3.up, directionToPlayer.normalized, out hit, viewDistance))
        {
            if (hit.transform == player)
            {
                return true;
            }
        }

        return false;
    }
}
