using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{

    //public variables
    public Rigidbody rb;
    public float moveSpeed = 6f;
    public float jumpForce = 5f;
    public Transform groundCheck;
    public float groundDistance = 0.2f;
    public LayerMask groundMask;

    // input action items
    public InputActionReference move;
    public InputActionReference jump;
    public InputActionReference fire;

    // private variables
    private Vector3 moveDirection;
    private bool isGrounded;
    private bool jumpPressed;

    void Start()
    {

        //input enablers
        move.action.Enable();
        jump.action.Enable();
        fire.action.Enable();


        //jump input callback
        jump.action.performed += ctx => OnJump();
        jump.action.canceled += ctx => jumpPressed = false;
    }

    private void OnDestroy()
    {
        jump.action.performed -= ctx => OnJump();
        jump.action.canceled -= ctx => jumpPressed = false;
    }

    void OnJump()
    {
        //checks if player is on ground
        if (isGrounded)
        {
            jumpPressed = true;
        }
    }

    void Update()
    {
        Vector2 moveInput = move.action.ReadValue<Vector2>();
        moveDirection = new Vector3(moveInput.x, 0f, moveInput.y);

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
    }

    void FixedUpdate()
    {
        rb.velocity = new Vector3(
            moveDirection.x * moveSpeed,
            rb.velocity.y,
            moveDirection.z * moveSpeed
        );

        if (jumpPressed)
        {
            rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
            jumpPressed = false;
        }
    }
}