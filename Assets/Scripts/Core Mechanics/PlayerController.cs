using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private Rigidbody rb;
    private PlayerInput PlayerInput;
    private InputAction jumpAction;
    private InputAction moveAction;
    private InputAction sprintAction;

    private Vector2 moveAmt;

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
        // Calls Move every physics step (50x/sec)
        Move();
    }
    public void Jump()
    {
        // Instant force upward (0, 1, 0)
        rb.AddForce(Vector3.up * jumpSpeed, ForceMode.Impulse);
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