using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Animator playerAnim;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private Transform spriteHolder;
    [SerializeField] private Transform playerShadow;

    [Header("Input Actions")]
    [SerializeField] private InputAction moveAction;
    [SerializeField] private InputAction jumpAction;
    [SerializeField] private InputAction sprintAction;
    [SerializeField] private InputAction meleeAction;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float jumpForce = 5f;

    [Header("Ground Detection")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.4f;
    [SerializeField] private LayerMask groundMask;

    [Header("Shadow Settings")]
    [SerializeField] private float minScale = 0.6f;
    [SerializeField] private float maxJumpHeight = 3f;
    [SerializeField] private float scaleSmooth = 10f;

    [Header("Drowning Settings")]
    [SerializeField] private float sinkSpeed = 1.5f;
    [SerializeField] private float sinkDepth = 2f;

    private PlayerCombat playerCombat;
    private Vector2 moveAmt;
    private Vector3 moveDirection;
    private bool isGrounded;
    private bool facingRight = true;
    private bool isDead = false;
    private bool isSinking = false;
    private System.Action<InputAction.CallbackContext> meleeCallback;
    private float lockedShadowY;
    private float basePlayerY;
    private float initialScale;

    private void Awake()
    {
        playerInput ??= GetComponent<PlayerInput>();
        rb ??= GetComponent<Rigidbody>();
        playerCombat ??= GetComponent<PlayerCombat>();

        if (!spriteHolder)
        {
            if (transform.childCount > 0)
                spriteHolder = transform.GetChild(0);
            else
                spriteHolder = transform;
        }

        rb.interpolation = RigidbodyInterpolation.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.freezeRotation = true;

        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        sprintAction = playerInput.actions["Sprint"];
        meleeAction = playerInput.actions["Melee"];
        meleeCallback = ctx => Melee();
    }

    private void Start()
    {
        if (playerShadow)
        {
            lockedShadowY = playerShadow.position.y;
            playerShadow.SetParent(null);
            initialScale = playerShadow.localScale.x;
        }

        basePlayerY = transform.position.y;
    }

    private void OnEnable()
    {
        moveAction.Enable();
        jumpAction.Enable();
        sprintAction.Enable();
        meleeAction.Enable();
        meleeAction.performed += meleeCallback;
    }

    private void OnDisable()
    {
        meleeAction.performed -= meleeCallback;
        moveAction.Disable();
        jumpAction.Disable();
        sprintAction.Disable();
        meleeAction.Disable();
    }

    private void Update()
    {
        if (isDead || isSinking) return;

        moveAmt = moveAction.ReadValue<Vector2>();
        if (jumpAction.WasPressedThisFrame() && isGrounded)
            Jump();
        HandleFlip();
        AnimateMovement();
    }

    private void FixedUpdate()
    {
        if (isDead || isSinking) return;

        GroundCheck();
        Move();
    }

    private void Move()
    {
        bool isSprinting = sprintAction.IsPressed();
        float moveSpeed = isSprinting ? sprintSpeed : walkSpeed;
        moveDirection = new Vector3(moveAmt.x, 0f, moveAmt.y).normalized;
        float airControl = isGrounded ? 1f : 0.6f;
        Vector3 targetVelocity = moveDirection * moveSpeed * airControl;
        Vector3 velocityChange = targetVelocity - new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(velocityChange, ForceMode.VelocityChange);
    }

    private void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        rb.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
    }

    private void GroundCheck()
    {
        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
    }

    private void AnimateMovement()
    {
        if (!playerAnim) return;
        bool isMoving = moveAmt.sqrMagnitude > 0.01f;
        playerAnim.SetBool("isWalking", isMoving);
    }

    private void HandleFlip()
    {
        float xVelocity = rb.velocity.x;
        if (xVelocity > 0.1f && !facingRight)
            Flip();
        else if (xVelocity < -0.1f && facingRight)
            Flip();
    }

    private void Flip()
    {
        facingRight = !facingRight;
        float y = facingRight ? 0f : 180f;
        spriteHolder.localRotation = Quaternion.Euler(0f, y, 0f);
    }

    private void Melee()
    {
        playerCombat.OnMelee();
    }

    private void LateUpdate()
    {
        if (!playerShadow || isSinking) return;

        Vector3 pos = playerShadow.position;
        pos.x = transform.position.x;
        pos.z = transform.position.z;
        pos.y = lockedShadowY;
        playerShadow.position = pos;

        float height = Mathf.Max(0f, transform.position.y - basePlayerY);
        float t = Mathf.Clamp01(height / maxJumpHeight);
        float targetScale = Mathf.Lerp(initialScale, initialScale * minScale, t);
        float newScale = Mathf.Lerp(playerShadow.localScale.x, targetScale, Time.deltaTime * scaleSmooth);
        playerShadow.localScale = Vector3.one * newScale;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Water") && !isDead && !isSinking)
            StartCoroutine(DrownSequence());
    }

    private IEnumerator DrownSequence()
    {
        isSinking = true;

        // Disable all input
        moveAction.Disable();
        jumpAction.Disable();
        sprintAction.Disable();
        meleeAction.Disable();

        // Turn off physics to avoid fight with manual sinking
        rb.velocity = Vector3.zero;
        rb.useGravity = false;
        rb.isKinematic = false;

        // Hide shadow
        if (playerShadow)
            playerShadow.gameObject.SetActive(false);

        Vector3 startPos = transform.position;
        Vector3 endPos = startPos - new Vector3(0f, sinkDepth, 0f);

        float elapsed = 0f;
        float duration = sinkDepth / sinkSpeed;

        // Smoothly sink using transform control
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            transform.position = Vector3.Lerp(startPos, endPos, elapsed / duration);
            yield return null;
        }

        rb.useGravity = true;
        Die();
    }

    private void Die()
    {
        isDead = true;
        rb.isKinematic = true;
        rb.useGravity = true;
        LevelManager.TriggerPlayerDeath();
    }

    public void Revive()
    {
        isDead = false;
        isSinking = false;
        rb.isKinematic = false;
        rb.useGravity = true;
        moveAction.Enable();
        jumpAction.Enable();
        sprintAction.Enable();
        meleeAction.Enable();

        if (playerShadow)
            playerShadow.gameObject.SetActive(true);
    }
}
