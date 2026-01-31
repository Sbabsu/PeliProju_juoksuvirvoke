using System.Collections;
using System.Collections.Generic;
//using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class ProtoPlayerController : MonoBehaviour
{
    [Header("---References---")]
    [SerializeField] Rigidbody hips;
    [SerializeField] Animator animator;
    [SerializeField] Transform groundCheckPoint; 

    [Header("---Booleans---")]
    [SerializeField] bool isMoving;
    [SerializeField] bool isSprinting;
    [SerializeField] bool isJumping;
    [SerializeField] bool isStrafing_Left;
    [SerializeField] bool isStrafing_Right;

    [Header("---Settings---")]
    [SerializeField] float speed;     //Player speed forward and backward
    [SerializeField] float strafeSpeed;   //Player speed left and right
    [SerializeField] float sprintSpeed;   //Player sprint speed
    [SerializeField] float jumpForce;      //Player jump force

    [SerializeField] float groundCheckDistance = 0.3f; // Slightly longer for safety
    [SerializeField] LayerMask groundMask;
    private PickUp _currentPickup;

    private bool _isGrounded;
    private bool _jumpRequested;

    private void Start()
    {
        Physics.defaultSolverIterations = 10;

        hips = GetComponent<Rigidbody>();

        // Create ground check point if not assigned
        if (groundCheckPoint == null)
        {
            GameObject groundCheck = new GameObject("GroundCheck");
            groundCheck.transform.parent = transform;
            groundCheck.transform.localPosition = new Vector3(0, 0.1f, 0); // Just below player
            groundCheckPoint = groundCheck.transform;
        }
    }

    private void Update()
    {
        // Check if grounded using raycast
        CheckGround();

        // Handle jump input
        if (Input.GetButtonDown("Jump") && _isGrounded)
        {
            _jumpRequested = true;
        }

        if (Input.GetKeyDown(KeyCode.E) && _currentPickup != null)
        {
            CollectPickup();
        }

        // Animation updates
        float vertical = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");

        isMoving = Mathf.Abs(vertical) > 0.1f;
        isStrafing_Left = horizontal < -0.1f;
        isStrafing_Right = horizontal > 0.1f;

        animator.SetBool("isMoving", isMoving);
        animator.SetBool("isStrafing_Left", isStrafing_Left);
        animator.SetBool("isStrafing_Right", isStrafing_Right);

        // Sprint animation
        animator.SetBool("isSprinting", Input.GetKey(KeyCode.LeftShift) && isMoving);

        // Jump animation
        animator.SetBool("isJumping", !_isGrounded && hips.linearVelocity.y > 0.1f);
    }

    private void FixedUpdate()
    {
        float vertical = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");

        // Movement logic
        Vector3 desiredVelocity;
        if (Input.GetKey(KeyCode.LeftShift) && isMoving)
        {
            desiredVelocity = new Vector3(horizontal * strafeSpeed * 1.5f, hips.linearVelocity.y, vertical * sprintSpeed);
        }
        else
        {
            desiredVelocity = new Vector3(horizontal * strafeSpeed, hips.linearVelocity.y, vertical * speed);
        }

        hips.linearVelocity = desiredVelocity;

        // Handle jump in FixedUpdate for physics consistency
        if (_jumpRequested && _isGrounded)
        {
            Jump();
            _jumpRequested = false;
        }
    }

    private void CheckGround()
    {
        // Simple raycast downward
        Ray ray = new Ray(groundCheckPoint.position, Vector3.down);
        _isGrounded = Physics.Raycast(ray, groundCheckDistance, groundMask);

        // Optional: Visualize the ray in Scene view (debug only)
        Debug.DrawRay(groundCheckPoint.position, Vector3.down * groundCheckDistance, _isGrounded ? Color.green : Color.red);
    }

    private void Jump()
    {
        hips.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        animator.SetBool("isJumping", true);
    }

    // Piuckup Logic

    private void OnTriggerEnter(Collider other)
    {
        // Check for pickup items
        PickUp pickup = other.GetComponent<PickUp>();
        if (pickup != null)
        {
            _currentPickup = pickup;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        PickUp pickup = other.GetComponent<PickUp>();
        if (pickup != null && pickup == _currentPickup)
        {
            _currentPickup = null;
        }
    }

    private void CollectPickup()
    {
        if (_currentPickup != null)
        {
            _currentPickup.CollectItem();
            _currentPickup = null;

            // Trigger pickup animation if you have one
            animator.SetTrigger("Pickup");
        }
    }
}
