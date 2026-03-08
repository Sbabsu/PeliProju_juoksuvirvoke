using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class ShopPatrolNPC : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator animator;

    [Header("Patrol Points")]
    [SerializeField] private Transform[] patrolPoints;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2.0f;
    [SerializeField] private float stopDistance = 0.5f;
    [SerializeField] private float waitTimeAtPoint = 4f;

    private int currentPointIndex = 0;
    private float waitTimer = 0f;
    private bool isWaiting = false;

    private void Awake()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        if (agent == null)
        {
            Debug.LogError("NavMeshAgent puuttuu objektista: " + gameObject.name, this);
            enabled = false;
            return;
        }

        if (!agent.isOnNavMesh)
        {
            Debug.LogError(gameObject.name + " ei ole NavMeshin päällä!", this);
            enabled = false;
            return;
        }

        if (patrolPoints == null || patrolPoints.Length == 0)
        {
            Debug.LogWarning("Patrol pointit puuttuvat objektista: " + gameObject.name, this);
            SetWalking(false);
            enabled = false;
            return;
        }

        agent.speed = moveSpeed;
        MoveToCurrentPoint();
    }

    private void Update()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;
        if (agent == null || !agent.isOnNavMesh) return;

        if (isWaiting)
        {
            waitTimer -= Time.deltaTime;

            if (waitTimer <= 0f)
            {
                isWaiting = false;
                currentPointIndex = (currentPointIndex + 1) % patrolPoints.Length;
                MoveToCurrentPoint();
            }

            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= stopDistance)
        {
            StartWaiting();
        }

        bool isMoving = agent.velocity.sqrMagnitude > 0.01f && !isWaiting;
        SetWalking(isMoving);
    }

    private void MoveToCurrentPoint()
    {
        if (patrolPoints[currentPointIndex] == null) return;

        agent.isStopped = false;
        agent.SetDestination(patrolPoints[currentPointIndex].position);
        SetWalking(true);
    }

    private void StartWaiting()
    {
        isWaiting = true;
        waitTimer = waitTimeAtPoint;

        agent.isStopped = true;
        agent.ResetPath();

        SetWalking(false);
    }

    private void SetWalking(bool walking)
    {
        if (animator == null) return;

        animator.SetBool("isWalking", walking);
        animator.SetBool("isIdle", !walking);
    }

    private void OnDrawGizmosSelected()
    {
        if (patrolPoints == null || patrolPoints.Length == 0) return;

        Gizmos.color = Color.cyan;

        for (int i = 0; i < patrolPoints.Length; i++)
        {
            if (patrolPoints[i] == null) continue;

            Gizmos.DrawSphere(patrolPoints[i].position, 0.2f);

            Transform next = patrolPoints[(i + 1) % patrolPoints.Length];
            if (next != null)
                Gizmos.DrawLine(patrolPoints[i].position, next.position);
        }
    }
}