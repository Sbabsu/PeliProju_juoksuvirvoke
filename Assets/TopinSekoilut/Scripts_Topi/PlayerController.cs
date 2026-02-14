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

    [Header("---Stamina---")]
    [SerializeField] float maxStamina = 5f;              // seconds of sprint
    [SerializeField] float staminaDrainPerSecond = 1f;   // 1 = drains maxStamina in 5s if maxStamina=5
    [SerializeField] float staminaRegenPerSecond = 0.8f; // regen speed
    [SerializeField] float regenDelay = 1.0f;            // wait after sprint stops before regen
    [SerializeField] float sprintReenableThreshold = 0.5f; // must regen to this to sprint again

    private float _stamina;
    private float _regenTimer;
    private bool _sprintBlocked; // true when stamina is empty


    [SerializeField] float groundCheckDistance = 0.3f;
    [SerializeField] LayerMask groundMask;

    private PickUp _currentPickup;
    private bool _isGrounded;
    private bool _jumpRequested;
    private bool _canMove = true;

    private Vector3 _facingDir = Vector3.forward; // remembered facing

    private void Start()
    {
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

        CheckGround();

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

        animator.SetBool("isMoving", isMoving);
        animator.SetBool("isStrafing", isStrafing);
        animator.SetBool("isStrafing_Left", isStrafing_Left);
        animator.SetBool("isStrafing_Right", isStrafing_Right);

        bool sprintKey = Input.GetKey(KeyCode.LeftShift);
        bool wantsSprint = sprintKey && isMoving && !isStrafing;

        // If stamina recovered enough, unblock sprinting
        if (_sprintBlocked && _stamina >= sprintReenableThreshold)
            _sprintBlocked = false;

        // Decide if we can sprint this frame
        isSprinting = wantsSprint && !_sprintBlocked && _stamina > 0.01f;

        // Drain / regen stamina
        if (isSprinting)
        {
            _stamina -= staminaDrainPerSecond * Time.deltaTime;
            _regenTimer = regenDelay;

            if (_stamina <= 0f)
            {
                _stamina = 0f;
                _sprintBlocked = true;
                isSprinting = false; // stop sprint immediately
            }
        }
        else
        {
            if (_regenTimer > 0f)
            {
                _regenTimer -= Time.deltaTime;
            }
            else
            {
                _stamina += staminaRegenPerSecond * Time.deltaTime;
                if (_stamina > maxStamina) _stamina = maxStamina;
            }
        }

        animator.SetBool("isSprinting", isSprinting);
    }

    private void FixedUpdate()
    {
        if (!_canMove) return;

        Vector2 input = GetMovementInput();
        float vertical = input.y;
        float horizontal = input.x;

        // --- CAMERA RELATIVE MOVEMENT ---
        Transform cam = Camera.main.transform;

        Vector3 camForward = cam.forward;
        camForward.y = 0;
        camForward.Normalize();

        Vector3 camRight = cam.right;
        camRight.y = 0;
        camRight.Normalize();

        Vector3 moveDir = camForward * vertical + camRight * horizontal;
        if (moveDir.sqrMagnitude > 1f)
            moveDir.Normalize();

        // Update facing ONLY when moving forward (W)
        if (vertical > forwardThreshold)
        {
            Vector3 f = camForward;
            f.y = 0f;
            if (f.sqrMagnitude > 0.001f)
                _facingDir = f.normalized;
        }

        // Rotate animated copy root always toward remembered facing
        if (animRoot != null && _facingDir.sqrMagnitude > 0.001f)
        {
            Quaternion animTarget = Quaternion.LookRotation(_facingDir, Vector3.up);
            animRoot.rotation = Quaternion.Slerp(animRoot.rotation, animTarget, turnSpeed * Time.fixedDeltaTime);
        }

        // Rotate BALANCER root ONLY when moving forward
        if (vertical > forwardThreshold && _facingDir.sqrMagnitude > 0.001f && balancerRb != null)
        {
            Vector3 flat = _facingDir;
            flat.y = 0f;

            if (flat.sqrMagnitude > 0.001f)
            {
                Quaternion target = Quaternion.LookRotation(flat.normalized, Vector3.up);
                balancerRb.MoveRotation(Quaternion.Slerp(balancerRb.rotation, target, turnSpeed * Time.fixedDeltaTime));
            }
        }

        float currentSpeed = speed;

        bool isStrafing = Mathf.Abs(horizontal) > 0.1f && Mathf.Abs(vertical) < 0.1f;
        if (isStrafing) currentSpeed = strafeSpeed;

        if (isSprinting)
            currentSpeed = sprintSpeed;

        Vector3 desiredVelocity = moveDir * currentSpeed;
        desiredVelocity.y = hips.linearVelocity.y;

        hips.linearVelocity = desiredVelocity;

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
        animator.SetBool("isJumping", true);
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
        if (!enabled)
            hips.linearVelocity = new Vector3(0, hips.linearVelocity.y, 0);
    }

    private Vector2 GetAlternativeMovementInput()
    {
        float vertical = 0f;
        float horizontal = 0f;

        if (Input.GetKey(KeyCode.UpArrow))
            vertical += 1f;

        if (Input.GetKey(KeyCode.DownArrow))
            vertical -= 1f;

        if (Input.GetKey(KeyCode.RightArrow))
            horizontal += 1f;

        if (Input.GetKey(KeyCode.LeftArrow))
            horizontal -= 1f;

        Vector2 input = new Vector2(horizontal, vertical);
        if (input.magnitude > 1f)
            input.Normalize();

        return input;
    }

    private Vector2 GetMovementInput()
    {
        // Default WASD / Input Manager axes
        float vertical = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");

        // Alternative arrow keys
        Vector2 altInput = GetAlternativeMovementInput();

        // If arrow keys are being pressed, override default input
        if (altInput.sqrMagnitude > 0f)
            return altInput;

        return new Vector2(horizontal, vertical);
    }
}
