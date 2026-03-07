using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class CustomerAI : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private NavMeshAgent agent;
    [SerializeField] private Animator animator;
    [SerializeField] private Transform[] points;

    [Header("Movement")]
    [SerializeField] private float moveSpeed = 2f;
    [SerializeField] private float stopDistance = 0.8f;

    [Header("Waiting")]
    [SerializeField] private float waitTimeAtPoint = 5f;

    [Header("Shelf Look")]
    [SerializeField] private Transform[] lookTargets;
    [SerializeField] private float lookRotateSpeed = 5f;

    [Header("Animation")]
    [SerializeField] private string movingBoolParameter = "isMoving";
    [SerializeField] private string speedFloatParameter = "Speed";
    [SerializeField] private float animLerpSpeed = 8f;

    private int currentPointIndex = 0;
    private bool isWaiting = false;
    private float currentAnimSpeed = 0f;

    private void Start()
    {
        if (agent == null)
            agent = GetComponent<NavMeshAgent>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        agent.speed = moveSpeed;
        agent.stoppingDistance = stopDistance;
        agent.autoBraking = true;

        if (points != null && points.Length > 0)
        {
            agent.SetDestination(points[currentPointIndex].position);
        }
    }

    private void Update()
    {
        if (agent == null || animator == null || points == null || points.Length == 0)
            return;

        agent.speed = moveSpeed;
        agent.stoppingDistance = stopDistance;

        if (!isWaiting)
        {
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance + 0.05f)
            {
                StartCoroutine(WaitAtPoint());
            }
        }
        else
        {
            RotateTowardsLookTarget();
        }

        UpdateAnimation();
    }

    private IEnumerator WaitAtPoint()
    {
        isWaiting = true;

        agent.isStopped = true;
        agent.ResetPath();

        UpdateAnimation();

        yield return new WaitForSeconds(waitTimeAtPoint);

        currentPointIndex++;
        if (currentPointIndex >= points.Length)
            currentPointIndex = 0;

        agent.isStopped = false;
        agent.SetDestination(points[currentPointIndex].position);

        isWaiting = false;
    }

    private void RotateTowardsLookTarget()
    {
        if (lookTargets == null || lookTargets.Length == 0)
            return;

        if (currentPointIndex < 0 || currentPointIndex >= lookTargets.Length)
            return;

        Transform target = lookTargets[currentPointIndex];
        if (target == null)
            return;

        Vector3 dir = target.position - transform.position;
        dir.y = 0f;

        if (dir.sqrMagnitude < 0.001f)
            return;

        Quaternion targetRot = Quaternion.LookRotation(dir.normalized, Vector3.up);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRot, lookRotateSpeed * Time.deltaTime);
    }

    private void UpdateAnimation()
    {
        if (animator == null || agent == null)
            return;

        float realSpeed = agent.velocity.magnitude;
        bool moving = realSpeed > 0.1f && !agent.isStopped;

        animator.SetBool("isMoving", moving);
        animator.SetFloat("Speed", realSpeed);
    }
}