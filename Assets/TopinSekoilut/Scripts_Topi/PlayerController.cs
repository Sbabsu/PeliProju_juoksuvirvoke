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

    [SerializeField] float groundCheckDistance = 0.3f;
    [SerializeField] LayerMask groundMask;

    private PickUp _currentPickup;
    private bool _isGrounded;
    private bool _jumpRequested;
    private bool _canMove = true;

    // Cached camera vectors (updated in Update, used in FixedUpdate)
    private Vector3 _camForwardFlat;
    private Vector3 _camRightFlat;

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
    }

    private void Update()
    {
        if (!_canMove) return;

        CheckGround();

        // Cache camera vectors once per rendered frame (plays nicer with Cinemachine)
        Transform cam = Camera.main.transform;

        _camForwardFlat = cam.forward;
        _camForwardFlat.y = 0f;
        if (_camForwardFlat.sqrMagnitude > 0.0001f) _camForwardFlat.Normalize();

        _camRightFlat = cam.right;
        _camRightFlat.y = 0f;
        if (_camRightFlat.sqrMagnitude > 0.0001f) _camRightFlat.Normalize();

        // Jump request
        animator.SetBool("isJumping", !_isGrounded);
        if ((Input.GetButtonDown("Jump") || Input.GetKeyDown(KeyCode.Return)) && _isGrounded)
            _jumpRequested = true;

        // Pickup
        if (Input.GetKeyDown(KeyCode.E) && _currentPickup != null)
            CollectPickup();

        // Movement input (WASD + Arrow keys)
        Vector2 input = GetMovementInput();
        float vertical = input.y;
        float horizontal = input.x;

        isMoving = Mathf.Abs(vertical) > 0.1f || Mathf.Abs(horizontal) > 0.1f;

        isStrafing = Mathf.Abs(horizontal) > 0.1f && Mathf.Abs(vertical) < 0.1f;
        isStrafing_Left = isStrafing && horizontal < -0.1f;
        isStrafing_Right = isStrafing && horizontal > 0.1f;

        isSprinting = Input.GetKey(KeyCode.LeftShift) && isMoving && !isStrafing;

        // Animator params
        animator.SetBool("isMoving", isMoving);
        animator.SetBool("isStrafing", isStrafing);
        animator.SetBool("isStrafing_Left", isStrafing_Left);
        animator.SetBool("isStrafing_Right", isStrafing_Right);
        animator.SetBool("isSprinting", isSprinting);
    }

    private void FixedUpdate()
    {
        if (!_canMove) return;

        Vector2 input = GetMovementInput();
        float vertical = input.y;
        float horizontal = input.x;

        // Camera-relative movement using cached vectors
        Vector3 moveDir = _camForwardFlat * vertical + _camRightFlat * horizontal;
        if (moveDir.sqrMagnitude > 1f) moveDir.Normalize();

        // Update facing ONLY when moving forward
        if (vertical > forwardThreshold)
        {
            Vector3 f = _camForwardFlat;
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

        bool strafingNow = Mathf.Abs(horizontal) > 0.1f && Mathf.Abs(vertical) < 0.1f;
        if (strafingNow) currentSpeed = strafeSpeed;

        if (Input.GetKey(KeyCode.LeftShift) && isMoving && !strafingNow)
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
        // isJumping bool is driven by grounded state in Update()
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

    // Arrow keys input
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

    // WASD axes + Arrow override
    private Vector2 GetMovementInput()
    {
        float vertical = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");

        Vector2 altInput = GetAlternativeMovementInput();
        if (altInput.sqrMagnitude > 0f)
            return altInput;

        return new Vector2(horizontal, vertical);
    }
}
