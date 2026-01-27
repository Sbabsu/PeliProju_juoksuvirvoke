using System.Collections;       
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Rendering;

public class ProtoPlayerController : MonoBehaviour
{
    [Header("---References---")]
    [SerializeField] Rigidbody hips;
    [SerializeField] Animator animator;

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

    private void Start()
    {
        Physics.defaultSolverIterations = 10;

        hips = GetComponent<Rigidbody>();
    }

    private void FixedUpdate()
    {
        float vertical = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");

        Vector3 desiredVelocity = new Vector3(horizontal * strafeSpeed, hips.linearVelocity.y, vertical * speed);
        hips.linearVelocity = desiredVelocity;

        //ANIMATION CONTROLS
        isMoving = Mathf.Abs(vertical) > 0.1f;
        isStrafing_Left = horizontal < -0.1f;
        isStrafing_Right = horizontal > 0.1f;
        animator.SetBool("isMoving", isMoving);
        animator.SetBool("isStrafing_Left", isStrafing_Left);
        animator.SetBool("isStrafing_Right", isStrafing_Right);

        if (Input.GetKey(KeyCode.LeftShift) && isMoving)
        {
            animator.SetBool("isSprinting", true);
            Vector3 sprintVelocity = new Vector3(horizontal * strafeSpeed * 1.5f, hips.linearVelocity.y, vertical * sprintSpeed);
            hips.linearVelocity = sprintVelocity;
        }
        else
        {
            animator.SetBool("isSprinting", false);
        }

        Jump();
    }

    private void Jump()
    {
       // if (!Foot.IsGrounded)
       //     return;

        float jump = Input.GetAxis("Jump");

        //hips.AddForce(Vector3.up * Mathf.Max(jump, 0f) * jumpForce, ForceMode.Impulse);
        if (jump > 0f && Foot.IsGrounded)
        {
            hips.AddForce(Vector3.up * jump * jumpForce, ForceMode.Impulse);
            isJumping = true;
            animator.SetBool("isJumping", true);
        }
        //Clear jumping state when landed
        if (isJumping && Foot.IsGrounded)
        {
            isJumping = false;
            animator.SetBool("isJumping", false);
        }
    }
}
