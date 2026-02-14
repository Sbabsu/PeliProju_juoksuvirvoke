using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(GuardAI))]
[RequireComponent(typeof(NavMeshAgent))]
public class GuardStateMachine : MonoBehaviour
{
    public enum State
    {
        Patrol,     // walking between points
        IdleWait,   // waiting at a point
        Chase,      // running after player
        Search      // walking to last known position then waiting a bit
    }

    [Header("Refs")]
    [SerializeField] private Animator animator;

    [Header("State")]
    [SerializeField] private State state = State.Patrol;

    [Header("Lose Sight")]
    [SerializeField] private float loseSightTime = 3f;
    private float loseSightTimer;

    [Header("Idle At Patrol Point")]
    [Tooltip("If true uses a random wait time, otherwise uses Fixed Wait Time.")]
    [SerializeField] private bool randomWait = true;

    [SerializeField] private float fixedWaitTime = 2f;
    [SerializeField] private Vector2 randomWaitRange = new Vector2(1.5f, 4f);

    private float waitTimer;

    [Header("Search")]
    [SerializeField] private float searchDuration = 2.5f;
    private float searchTimer;
    private Vector3 lastKnownPlayerPos;

    [Header("Movement")]
    [SerializeField] private float patrolSpeed = 3.5f;
    [SerializeField] private float chaseSpeed = 5.0f;
    [SerializeField] private float reachedDistance = 0.6f;

    private GuardAI ai;
    private NavMeshAgent agent;

    private void Awake()
    {
        ai = GetComponent<GuardAI>();
        agent = GetComponent<NavMeshAgent>();
        if (animator == null) animator = GetComponentInChildren<Animator>();
    }

    private void Start()
    {
        EnterPatrol();
    }

    private void Update()
    {
        if (ai.player == null) return;
        if (agent == null || !agent.isOnNavMesh) return;

        Vector3 playerCenter = ai.GetRagdollCenter();
        bool canSee = ai.CanSeeRagdollPlayer(playerCenter);

        // Global transition: if we can see player, always chase
        if (canSee)
        {
            lastKnownPlayerPos = playerCenter;
            loseSightTimer = loseSightTime;

            if (state != State.Chase)
                EnterChase();
        }

        switch (state)
        {
            case State.Patrol:
                TickPatrol(canSee);
                break;

            case State.IdleWait:
                TickIdleWait(canSee);
                break;

            case State.Chase:
                TickChase(playerCenter, canSee);
                break;

            case State.Search:
                TickSearch(canSee);
                break;
        }
    }

    // PATROL (Walk between points)
    private void EnterPatrol()
    {
        state = State.Patrol;
        agent.speed = patrolSpeed;

        SetAnimBools(walking: true, running: false);

        if (ai.patrolPoints != null && ai.patrolPoints.Length > 0)
            ai.GoToNextPatrolPoint();
        else
            EnterIdleWait(); // no patrol points, just idle
    }

    private void TickPatrol(bool canSee)
    {
        if (canSee) return;

        if (ai.patrolPoints == null || ai.patrolPoints.Length == 0)
        {
            EnterIdleWait();
            return;
        }

        // reached patrol destination -> wait/idle
        if (!agent.pathPending && agent.remainingDistance <= reachedDistance)
        {
            EnterIdleWait();
        }
    }

    // IDLE WAIT (at patrol point)
    private void EnterIdleWait()
    {
        state = State.IdleWait;

        // stop moving while idling
        agent.ResetPath();

        SetAnimBools(walking: false, running: false);

        waitTimer = randomWait
            ? Random.Range(randomWaitRange.x, randomWaitRange.y)
            : fixedWaitTime;
    }

    private void TickIdleWait(bool canSee)
    {
        if (canSee) return;

        waitTimer -= Time.deltaTime;
        if (waitTimer <= 0f)
        {
            EnterPatrol();
        }
    }

    // CHASE (Run)
    private void EnterChase()
    {
        state = State.Chase;
        agent.speed = chaseSpeed;

        SetAnimBools(walking: false, running: true);
    }

    private void TickChase(Vector3 playerCenter, bool canSee)
    {
        agent.SetDestination(playerCenter);
        ai.RotateTowards(playerCenter);

        if (!canSee)
        {
            loseSightTimer -= Time.deltaTime;
            if (loseSightTimer <= 0f)
            {
                EnterSearch(lastKnownPlayerPos);
            }
        }
    }

    // SEARCH (Walk to last seen, then wait a bit)
    private void EnterSearch(Vector3 lastPos)
    {
        state = State.Search;
        agent.speed = patrolSpeed;

        SetAnimBools(walking: true, running: false);

        searchTimer = searchDuration;
        agent.SetDestination(lastPos);
    }

    private void TickSearch(bool canSee)
    {
        if (canSee) return;

        bool reached = !agent.pathPending && agent.remainingDistance <= reachedDistance;

        if (reached)
        {
            // once there, "search" by idling for searchDuration
            agent.ResetPath();
            SetAnimBools(walking: false, running: false);

            searchTimer -= Time.deltaTime;
            if (searchTimer <= 0f)
            {
                EnterPatrol();
            }
        }
    }

    // Animator helper
    private void SetAnimBools(bool walking, bool running)
    {
        if (animator == null) return;

        animator.SetBool("isWalking", walking);
        animator.SetBool("isRunning", running);

        // If you use isIdle in transitions, keep it consistent:
        animator.SetBool("isIdle", !walking && !running);
    }
}
