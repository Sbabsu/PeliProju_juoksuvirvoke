using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [Header("---References---")]
    [SerializeField] Rigidbody hips;
    [SerializeField] Animator animator;
    [SerializeField] Transform groundCheckPoint;
    [SerializeField] Transform animRoot;
    [SerializeField] Rigidbody balancerRb;

    [Header("---Booleans---")]
    [SerializeField] bool isMoving;
    [SerializeField] bool isStrafing;
    [SerializeField] bool isSprinting;
    [SerializeField] bool isJumping;
    [SerializeField] bool isStrafing_Left;
    [SerializeField] bool isStrafing_Right;

    [Header("---Settings---")]
    [SerializeField] float speed = 5f;
    [SerializeField] float strafeSpeed = 4f;
    [SerializeField] float sprintSpeed = 8f;
    [SerializeField] float jumpForce = 7f;
    [SerializeField] float turnSpeed = 12f;
    [SerializeField] float forwardThreshold = 0.1f;
    [SerializeField] float maxVelocityChangePerFixed = 6f;

    [Header("---Stamina---")]
    [SerializeField] float maxStamina = 5f;
    [SerializeField] float staminaDrainPerSecond = 1f;
    [SerializeField] float staminaRegenPerSecond = 0.8f;
    [SerializeField] float regenDelay = 1.0f;
    [SerializeField] float sprintReenableThreshold = 0.5f;

    private float _stamina;
    private float _regenTimer;
    private bool _sprintBlocked;

    [SerializeField] float groundCheckDistance = 0.3f;
    [SerializeField] LayerMask groundMask;

    private PickUp _currentPickup;
    private bool _isGrounded;
    private bool _jumpRequested;
    private bool _canMove = true;

    private Vector3 _camForward;
    private Vector3 _camRight;

    private Vector3 _facingDir = Vector3.forward;

    // --- Powerups: speed boost ---
    private float _speedMultiplier = 1f;
    private Coroutine _speedRoutine;

    private void Start()
    {
        if (hips == null) hips = GetComponent<Rigidbody>();

        if (groundCheckPoint == null)
        {
            GameObject groundCheck = new GameObject("GroundCheck");
            groundCheck.transform.parent = transform;
            groundCheck.transform.localPosition = new Vector3(0, 0.1f, 0);
            groundCheckPoint = groundCheck.transform;
        }

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;

        _stamina = maxStamina;
    }

    private void Update()
    {
        if (!_canMove) return;

        // Cache camera yaw basis once per rendered frame
        var cam = Camera.main != null ? Camera.main.transform : null;
        if (cam != null)
        {
            float yaw = cam.eulerAngles.y;
            Quaternion yawRot = Quaternion.Euler(0f, yaw, 0f);
            _camForward = yawRot * Vector3.forward;
            _camRight = yawRot * Vector3.right;
        }
        else
        {
            _camForward = Vector3.forward;
            _camRight = Vector3.right;
        }

        CheckGround();

        // Drive jump anim from grounded state (prevents it getting stuck)
        if (animator != null)
            animator.SetBool("isJumping", !_isGrounded);

        if ((Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Return)) && _isGrounded)
            _jumpRequested = true;

        if (Input.GetKeyDown(KeyCode.E) && _currentPickup != null)
            CollectPickup();

        Vector2 input = GetMovementInput();
        float vertical = input.y;
        float horizontal = input.x;

        isMoving = Mathf.Abs(vertical) > 0.1f || Mathf.Abs(horizontal) > 0.1f;

        isStrafing = Mathf.Abs(horizontal) > 0.1f && Mathf.Abs(vertical) < 0.1f;
        isStrafing_Left = isStrafing && horizontal < -0.1f;
        isStrafing_Right = isStrafing && horizontal > 0.1f;

        if (animator != null)
        {
            animator.SetBool("isMoving", isMoving);
            animator.SetBool("isStrafing", isStrafing);
            animator.SetBool("isStrafing_Left", isStrafing_Left);
            animator.SetBool("isStrafing_Right", isStrafing_Right);
        }

        bool sprintKey = Input.GetKey(KeyCode.LeftShift);
        bool wantsSprint = sprintKey && isMoving && !isStrafing;

        if (_sprintBlocked && _stamina >= sprintReenableThreshold)
            _sprintBlocked = false;

        isSprinting = wantsSprint && !_sprintBlocked && _stamina > 0.01f;

        if (isSprinting)
        {
            _stamina -= staminaDrainPerSecond * Time.deltaTime;
            _regenTimer = regenDelay;

            if (_stamina <= 0f)
            {
                _stamina = 0f;
                _sprintBlocked = true;
                isSprinting = false;
            }
        }
        else
        {
            if (_regenTimer > 0f) _regenTimer -= Time.deltaTime;
            else
            {
                _stamina += staminaRegenPerSecond * Time.deltaTime;
                if (_stamina > maxStamina) _stamina = maxStamina;
            }
        }

        if (animator != null)
            animator.SetBool("isSprinting", isSprinting);
    }

    private void FixedUpdate()
    {
        if (!_canMove) return;

        Vector2 input = GetMovementInput();
        float vertical = input.y;
        float horizontal = input.x;

        Vector3 moveDir = _camForward * vertical + _camRight * horizontal;
        if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();

        // Update facing ONLY when moving forward
        if (vertical > forwardThreshold)
        {
            Vector3 f = _camForward;
            if (f.sqrMagnitude > 0.001f)
                _facingDir = f.normalized;
        }

        if (animRoot != null && _facingDir.sqrMagnitude > 0.001f)
        {
            Quaternion animTarget = Quaternion.LookRotation(_facingDir, Vector3.up);
            animRoot.rotation = Quaternion.Slerp(animRoot.rotation, animTarget, turnSpeed * Time.fixedDeltaTime);
        }

        if (vertical > forwardThreshold && _facingDir.sqrMagnitude > 0.001f && balancerRb != null)
        {
            Quaternion target = Quaternion.LookRotation(_facingDir, Vector3.up);
            balancerRb.MoveRotation(Quaternion.Slerp(balancerRb.rotation, target, turnSpeed * Time.fixedDeltaTime));
        }

        float currentSpeed = speed * _speedMultiplier;

        bool strafingNow = Mathf.Abs(horizontal) > 0.1f && Mathf.Abs(vertical) < 0.1f;
        if (strafingNow) currentSpeed = strafeSpeed * _speedMultiplier;

        if (isSprinting) currentSpeed = sprintSpeed * _speedMultiplier;

        Vector3 desiredHorizontal = moveDir * currentSpeed;

        Vector3 currentVel = hips.linearVelocity;
        Vector3 currentHorizontal = new Vector3(currentVel.x, 0f, currentVel.z);
        Vector3 targetHorizontal = new Vector3(desiredHorizontal.x, 0f, desiredHorizontal.z);

        Vector3 delta = targetHorizontal - currentHorizontal;

        if (delta.magnitude > maxVelocityChangePerFixed)
            delta = delta.normalized * maxVelocityChangePerFixed;

        hips.AddForce(delta, ForceMode.VelocityChange);

        if (_jumpRequested && _isGrounded)
        {
            Jump();
            _jumpRequested = false;
        }
    }

    private void CheckGround()
    {
        Ray ray = new Ray(groundCheckPoint.position, Vector3.down);
        _isGrounded = Physics.Raycast(ray, groundCheckDistance, groundMask);
    }

    private void Jump()
    {
        hips.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private void OnTriggerEnter(Collider other)
    {
        PickUp pickup = other.GetComponent<PickUp>();
        if (pickup != null)
            _currentPickup = pickup;
    }

    private void OnTriggerExit(Collider other)
    {
        PickUp pickup = other.GetComponent<PickUp>();
        if (pickup != null && pickup == _currentPickup)
            _currentPickup = null;
    }

    private void CollectPickup()
    {
        if (_currentPickup != null)
            _currentPickup.CollectItem();
    }

    public void SetMovementEnabled(bool enabled)
    {
        _canMove = enabled;
        if (!enabled && hips != null)
            hips.linearVelocity = new Vector3(0, hips.linearVelocity.y, 0);
    }

    private Vector2 GetAlternativeMovementInput()
    {
        float vertical = 0f;
        float horizontal = 0f;

        if (Input.GetKey(KeyCode.UpArrow)) vertical += 1f;
        if (Input.GetKey(KeyCode.DownArrow)) vertical -= 1f;
        if (Input.GetKey(KeyCode.RightArrow)) horizontal += 1f;
        if (Input.GetKey(KeyCode.LeftArrow)) horizontal -= 1f;

        Vector2 input = new Vector2(horizontal, vertical);
        if (input.magnitude > 1f) input.Normalize();

        return input;
    }

    private Vector2 GetMovementInput()
    {
        float vertical = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");

        Vector2 altInput = GetAlternativeMovementInput();
        if (altInput.sqrMagnitude > 0f)
            return altInput;

        return new Vector2(horizontal, vertical);
    }

    public void ApplySpeedMultiplier(float multiplier, float duration)
    {
        if (_speedRoutine != null)
            StopCoroutine(_speedRoutine);

        _speedRoutine = StartCoroutine(SpeedRoutine(multiplier, duration));
    }

    private IEnumerator SpeedRoutine(float multiplier, float duration)
    {
        float original = _speedMultiplier;
        _speedMultiplier = original * multiplier;

        yield return new WaitForSeconds(duration);

        _speedMultiplier = original;
        _speedRoutine = null;
    }
}
