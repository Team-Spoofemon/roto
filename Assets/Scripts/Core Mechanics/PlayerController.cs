using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    private PlayerInput PlayerInput;
    private InputAction jumpAction;
    private InputAction moveAction;
    private InputAction sprintAction;

    [SerializeField] private Animator playerAnim;

    //public variables
    public Rigidbody rb;
    public float moveSpeed = 5f;
    public float jumpForce = 5f;
    public Transform groundCheck;
    public float groundDistance = 0.2f;
    public LayerMask groundMask;

    private float walkSpeed = 5f;
    private float jumpSpeed = 5f;
    private float sprintSpeed = 10f;


    public void Awake()
    {
        // Gets Components
        PlayerInput = GetComponent<PlayerInput>();
        rb = GetComponent<Rigidbody>();

        // Looks inside input action map
        moveAction = PlayerInput.actions["Move"];
        jumpAction = PlayerInput.actions["Jump"];
        sprintAction = PlayerInput.actions["Sprint"];

    }

    public void Start()
    {
        Debug.Log("Game Started!");
    }

    // Updates every frame
    public void Update()
    {
        // Reads the "move" input
        moveAmt = moveAction.ReadValue<Vector2>();

        // Jump if jump was pressed in this frame
        if (jumpAction.WasPressedThisFrame())
        {
            Jump();
        }
    }

    public void FixedUpdate()
    {
        Vector2 moveInput = move.action.ReadValue<Vector2>();
        moveDirection = new Vector3(moveInput.x, 0f, moveInput.y);

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);

        if (moveInput.x != 0 || moveInput.y != 0)
        {
            playerAnim.SetBool("isWalking", true);
        }
        else
        {
            playerAnim.SetBool("isWalking", false);
        }
    }

    public void Move()
    {
        // Check if sprint is held
        bool isSprinting = sprintAction.IsPressed();

        // if/else for sprinting or walking
        float moveSpeed = isSprinting ? sprintSpeed : walkSpeed;
        
        //Creates movement direction vector and multiplies by speed and delta time
        Vector3 move = (transform.forward * moveAmt.y + transform.right * moveAmt.x) * moveSpeed * Time.deltaTime;
        rb.MovePosition(rb.position + move);
    }


}