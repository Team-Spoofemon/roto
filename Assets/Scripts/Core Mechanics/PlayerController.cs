using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private Animator playerAnim;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private Transform spriteHolder;
    [SerializeField] private InputAction moveAction;
    [SerializeField] private InputAction jumpAction;
    [SerializeField] private InputAction sprintAction;
    [SerializeField] private InputAction meleeAction;
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.4f;
    [SerializeField] private LayerMask groundMask;
    
    private PlayerCombat playerCombat;
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
        meleeAction = playerInput.actions["Melee"];

        if (!spriteHolder) spriteHolder = transform;
    }

    private void OnEnable()
    {
        moveAction.Enable();
        jumpAction.Enable();
        sprintAction.Enable();
        meleeAction.Enable();
        meleeAction.performed += _ => Melee();
    }

    private void OnDisable()
    {
        moveAction.Disable();
        jumpAction.Disable();
        sprintAction.Disable();
        meleeAction.Disable();
        meleeAction.performed -= _ => Melee();
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

    private void Melee()
    {
        playerCombat.OnMelee();
    }
}
