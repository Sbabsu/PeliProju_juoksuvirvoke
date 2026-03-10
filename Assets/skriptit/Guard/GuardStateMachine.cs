using UnityEngine;
using System.Collections;
using UnityEngine.AI;

[RequireComponent(typeof(GuardAI))]
[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(AudioSource))]
public class GuardStateMachine : MonoBehaviour
{
    public enum State
    {
        Patrol,
        IdleWait,
        Chase,
        Search
    }

    [Header("Refs")]
    [SerializeField] private Animator animator;
    [SerializeField] private AudioSource audioSource;

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

    [Header("State Audio")]
    [SerializeField] private AudioClip[] patrolSounds;
    [SerializeField] private AudioClip[] idleSounds;
    [SerializeField] private AudioClip[] chaseSounds;
    [SerializeField] private AudioClip[] searchSounds;
    [SerializeField][Range(0f, 1f)] private float soundVolume = 1f;

    [Header("Patrol Barking")]
    [SerializeField] private bool playPatrolSoundsWhileActive = true;
    [SerializeField] private Vector2 patrolSoundIntervalRange = new Vector2(4f, 8f);
    [SerializeField][Range(0f, 1f)] private float patrolSoundChance = 0.35f;

    [Header("Idle Barking")]
    [SerializeField] private bool playIdleSoundsWhileActive = true;
    [SerializeField] private Vector2 idleSoundIntervalRange = new Vector2(3f, 6f);
    [SerializeField][Range(0f, 1f)] private float idleSoundChance = 0.5f;

    [Header("Audio Priority")]
    [SerializeField] private bool chaseCanInterrupt = true;
    [SerializeField] private bool searchCanInterrupt = false;

    private float patrolSoundTimer;
    private float idleSoundTimer;

    private float _speedMultiplier = 1f;
    private Coroutine _speedRoutine;

    private GuardAI ai;
    private NavMeshAgent agent;

    private void Awake()
    {
        ai = GetComponent<GuardAI>();
        agent = GetComponent<NavMeshAgent>();

        if (animator == null)
            animator = GetComponentInChildren<Animator>();

        if (audioSource == null)
            audioSource = GetComponent<AudioSource>();

        if (audioSource != null)
            audioSource.playOnAwake = false;
    }

    private void Start()
    {
        ResetPatrolSoundTimer();
        ResetIdleSoundTimer();
        EnterPatrol();
    }

    private void Update()
    {
        if (ai.player == null) return;
        if (agent == null || !agent.isOnNavMesh) return;

        Vector3 playerCenter = ai.GetRagdollCenter();
        bool canSee = ai.CanSeeRagdollPlayer(playerCenter);

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
                TickPatrolAudio();
                break;

            case State.IdleWait:
                TickIdleWait(canSee);
                TickIdleAudio();
                break;

            case State.Chase:
                TickChase(playerCenter, canSee);
                break;

            case State.Search:
                TickSearch(canSee);
                break;
        }
    }

    private void EnterPatrol()
    {
        state = State.Patrol;
        ApplySpeed(patrolSpeed);

        SetAnimBools(walking: true, running: false);

        if (ai.patrolPoints != null && ai.patrolPoints.Length > 0)
            ai.GoToNextPatrolPoint();
        else
            EnterIdleWait();
    }

    private void TickPatrol(bool canSee)
    {
        if (canSee) return;

        if (ai.patrolPoints == null || ai.patrolPoints.Length == 0)
        {
            EnterIdleWait();
            return;
        }

        if (!agent.pathPending && agent.remainingDistance <= reachedDistance)
        {
            EnterIdleWait();
        }
    }

    private void EnterIdleWait()
    {
        state = State.IdleWait;

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

    private void EnterChase()
    {
        state = State.Chase;
        ApplySpeed(chaseSpeed);

        SetAnimBools(walking: false, running: true);
        TryPlayRandomSound(chaseSounds, chaseCanInterrupt);
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

    private void EnterSearch(Vector3 lastPos)
    {
        state = State.Search;
        ApplySpeed(patrolSpeed);

        SetAnimBools(walking: true, running: false);
        TryPlayRandomSound(searchSounds, searchCanInterrupt);

        searchTimer = searchDuration;
        agent.SetDestination(lastPos);
    }

    private void TickSearch(bool canSee)
    {
        if (canSee) return;

        bool reached = !agent.pathPending && agent.remainingDistance <= reachedDistance;

        if (reached)
        {
            agent.ResetPath();
            SetAnimBools(walking: false, running: false);

            searchTimer -= Time.deltaTime;
            if (searchTimer <= 0f)
            {
                EnterPatrol();
            }
        }
    }

    private void TickPatrolAudio()
    {
        if (!playPatrolSoundsWhileActive || patrolSounds == null || patrolSounds.Length == 0)
            return;

        patrolSoundTimer -= Time.deltaTime;
        if (patrolSoundTimer > 0f)
            return;

        if (Random.value <= patrolSoundChance)
            TryPlayRandomSound(patrolSounds, false);

        ResetPatrolSoundTimer();
    }

    private void TickIdleAudio()
    {
        if (!playIdleSoundsWhileActive || idleSounds == null || idleSounds.Length == 0)
            return;

        idleSoundTimer -= Time.deltaTime;
        if (idleSoundTimer > 0f)
            return;

        if (Random.value <= idleSoundChance)
            TryPlayRandomSound(idleSounds, false);

        ResetIdleSoundTimer();
    }

    private void ResetPatrolSoundTimer()
    {
        patrolSoundTimer = Random.Range(patrolSoundIntervalRange.x, patrolSoundIntervalRange.y);
    }

    private void ResetIdleSoundTimer()
    {
        idleSoundTimer = Random.Range(idleSoundIntervalRange.x, idleSoundIntervalRange.y);
    }

    private void ApplySpeed(float baseSpeed)
    {
        agent.speed = baseSpeed * _speedMultiplier;

        if (animator != null)
            animator.speed = _speedMultiplier;
    }

    public void ApplySpeedMultiplier(float multiplier, float duration)
    {
        if (_speedRoutine != null)
            StopCoroutine(_speedRoutine);

        _speedRoutine = StartCoroutine(SpeedRoutine(multiplier, duration));
    }

    private IEnumerator SpeedRoutine(float multiplier, float duration)
    {
        _speedMultiplier = multiplier;

        switch (state)
        {
            case State.Patrol:
            case State.Search:
                ApplySpeed(patrolSpeed);
                break;

            case State.Chase:
                ApplySpeed(chaseSpeed);
                break;
        }

        yield return new WaitForSeconds(duration);

        _speedMultiplier = 1f;

        switch (state)
        {
            case State.Patrol:
            case State.Search:
                ApplySpeed(patrolSpeed);
                break;

            case State.Chase:
                ApplySpeed(chaseSpeed);
                break;
        }
    }

    private void SetAnimBools(bool walking, bool running)
    {
        if (animator == null) return;

        animator.SetBool("isWalking", walking);
        animator.SetBool("isRunning", running);
        animator.SetBool("isIdle", !walking && !running);
    }

    private bool TryPlayRandomSound(AudioClip[] clips, bool interruptCurrent)
    {
        if (audioSource == null || clips == null || clips.Length == 0)
            return false;

        if (audioSource.isPlaying)
        {
            if (!interruptCurrent)
                return false;

            audioSource.Stop();
        }

        AudioClip clip = clips[Random.Range(0, clips.Length)];
        if (clip == null)
            return false;

        audioSource.PlayOneShot(clip, soundVolume);
        return true;
    }

    public void TriggerCameraChase(Transform spottedPlayer)
    {
        if (spottedPlayer != null)
            ai.SetPlayerTarget(spottedPlayer);

        lastKnownPlayerPos = ai.GetRagdollCenter();
        loseSightTimer = loseSightTime;

        EnterChase();
    }
}