using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Animator playerAnim;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private PlayerInput playerInput;

    // Flip ONLY this transform (child that holds sprites/animators)
    [Header("Visuals")]
    [SerializeField] private Transform spriteHolder; // assign the child that renders Zeus+sword

    [Header("Input Actions")]
    [SerializeField] private InputAction moveAction;
    [SerializeField] private InputAction jumpAction;
    [SerializeField] private InputAction sprintAction;

    [Header("Movement Settings")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float jumpForce = 5f;

    [Header("Ground Check")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.4f;
    [SerializeField] private LayerMask groundMask;

    private Vector2 moveAmt;
    private Vector3 moveDirection;
    private bool isGrounded;
    private bool facingRight = true;

    private void Awake()
    {
        if (!playerInput) playerInput = GetComponent<PlayerInput>();
        if (!rb) rb = GetComponent<Rigidbody>();

        moveAction   = playerInput.actions["Move"];
        jumpAction   = playerInput.actions["Jump"];
        sprintAction = playerInput.actions["Sprint"];

        // Safety: if not assigned, flip the root (not ideal, but works)
        if (!spriteHolder) spriteHolder = transform;
    }

    private void OnEnable()
    {
        moveAction.Enable();
        jumpAction.Enable();
        sprintAction.Enable();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        jumpAction.Disable();
        sprintAction.Disable();
    }

    private void Update()
    {
        moveAmt = moveAction.ReadValue<Vector2>();

        if (jumpAction.WasPressedThisFrame() && isGrounded)
            Jump();

        HandleFlip(moveAmt.x);
        AnimateMovement();
    }

    private void FixedUpdate()
    {
        GroundCheck();
        Move();
    }

    private void Move()
    {
        bool isSprinting = sprintAction.IsPressed();
        float moveSpeed = isSprinting ? sprintSpeed : walkSpeed;

        moveDirection = new Vector3(moveAmt.x, 0f, moveAmt.y).normalized;
        Vector3 delta = moveDirection * moveSpeed * Time.fixedDeltaTime;

        rb.MovePosition(rb.position + delta);
    }

    private void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private void GroundCheck()
    {
        isGrounded = groundCheck
            ? Physics.CheckSphere(groundCheck.position, groundDistance, groundMask)
            : true; // fallback if not assigned
    }

    private void AnimateMovement()
    {
        if (playerAnim)
        {
            bool isMoving = moveAmt.sqrMagnitude > 0.01f;
            playerAnim.SetBool("isWalking", isMoving);
        }
    }

    private void HandleFlip(float xInput)
    {
        if (xInput > 0.01f && !facingRight) Flip();
        else if (xInput < -0.01f && facingRight) Flip();
    }

    private void Flip()
    {
        facingRight = !facingRight;
        float y = facingRight ? 0f : 180f;
        // rotate ONLY the visual child so colliders/rigidbody stay stable
        spriteHolder.localRotation = Quaternion.Euler(0f, y, 0f);
    }
}
