using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using System.Collections;

public class PlayerController : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private Animator playerAnim;
    [SerializeField] private Rigidbody rb;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private Transform spriteHolder;
    [SerializeField] private Transform playerShadow;

    [Header("Camera Facing")]
    [SerializeField] private Transform cameraTransform;

    [Header("Input Actions")]
    [SerializeField] private InputAction moveAction;
    [SerializeField] private InputAction jumpAction;
    [SerializeField] private InputAction sprintAction;
    [SerializeField] private InputAction meleeAction;

    [Header("Movement")]
    [SerializeField] private float walkSpeed = 5f;
    [SerializeField] private float sprintSpeed = 10f;
    [SerializeField] private float jumpForce = 5f;
    [SerializeField] private float extraGravity = 20f;
    [SerializeField] private float inputRotation = 0f;

    [Header("Ground Detection")]
    [SerializeField] private Transform groundCheck;
    [SerializeField] private float groundDistance = 0.4f;
    [SerializeField] private LayerMask groundMask;

    [Header("Shadow Settings")]
    [SerializeField] private float minScale = 0.6f;
    [SerializeField] private float maxJumpHeight = 3f;
    [SerializeField] private float scaleSmooth = 10f;

    [Header("Drowning Settings")]
    [SerializeField] private float sinkSpeed = 0.5f;
    [SerializeField] private float sinkDepth = 2f;

    private PlayerCombat playerCombat;
    private Vector2 moveAmt;
    private Vector3 moveDirection;
    private bool isGrounded;
    private bool facingRight = true;
    private bool isDead = false;
    private bool isSinking = false;
    private System.Action<InputAction.CallbackContext> meleeCallback;
    private float initialScale;
    private bool hasInitializedShadow = false;

    private const float FlipHysteresis = 0.08f;

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

        if (rb != null)
        {
            rb.interpolation = RigidbodyInterpolation.Interpolate;
            rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
            rb.freezeRotation = true;
        }

        CacheActions();
        meleeCallback = _ => Melee();
        RefreshCameraReference();
    }

    private void Start()
    {
        RefreshCameraReference();

        if (playerShadow)
        {
            playerShadow.SetParent(null);
            initialScale = playerShadow.localScale.x;
        }
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        CacheActions();

        if (moveAction != null) moveAction.Enable();
        if (jumpAction != null) jumpAction.Enable();
        if (sprintAction != null) sprintAction.Enable();
        if (meleeAction != null)
        {
            meleeAction.Enable();
            meleeAction.performed -= meleeCallback;
            meleeAction.performed += meleeCallback;
        }

        RefreshCameraReference();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;

        if (meleeAction != null)
            meleeAction.performed -= meleeCallback;

        if (moveAction != null) moveAction.Disable();
        if (jumpAction != null) jumpAction.Disable();
        if (sprintAction != null) sprintAction.Disable();
        if (meleeAction != null) meleeAction.Disable();
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        StartCoroutine(RefreshCameraNextFrame());
    }

    private IEnumerator RefreshCameraNextFrame()
    {
        yield return null;
        RefreshCameraReference();
    }

    private void CacheActions()
    {
        if (playerInput == null || playerInput.actions == null)
            return;

        moveAction = playerInput.actions["Move"];
        jumpAction = playerInput.actions["Jump"];
        sprintAction = playerInput.actions["Sprint"];
        meleeAction = playerInput.actions["Melee"];
    }

    private void RefreshCameraReference()
    {
        if (cameraTransform != null && cameraTransform.gameObject.scene.IsValid())
            return;

        Camera mainCam = Camera.main;
        if (mainCam != null)
            cameraTransform = mainCam.transform;
    }

    private void Update()
    {
        if (isDead || isSinking)
            return;

        if (moveAction == null)
            CacheActions();

        RefreshCameraReference();

        moveAmt = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;

        if (jumpAction != null && jumpAction.WasPressedThisFrame() && isGrounded)
            Jump();

        HandleFlip();
        AnimateMovement();
    }

    private void FixedUpdate()
    {
        if (isDead || isSinking)
            return;

        GroundCheck();
        Move();
        ApplyExtraGravity();
    }

    private void LateUpdate()
    {
        RefreshCameraReference();

        if (!isSinking)
            UpdateSpriteRotation();

        if (!playerShadow || isSinking)
            return;

        Ray ray = new Ray(transform.position + Vector3.up * 0.5f, Vector3.down);
        if (Physics.Raycast(ray, out RaycastHit hit, 10f, groundMask))
        {
            playerShadow.position = new Vector3(transform.position.x, hit.point.y + 0.75f, transform.position.z - 0.5f);
            playerShadow.gameObject.SetActive(true);

            if (!hasInitializedShadow)
            {
                playerShadow.localScale = Vector3.one * initialScale;
                hasInitializedShadow = true;
                return;
            }

            float targetScale = initialScale;
            if (!isGrounded)
            {
                float height = Mathf.Max(0f, transform.position.y - hit.point.y);
                float t = Mathf.Clamp01(height / maxJumpHeight);
                targetScale = Mathf.Lerp(initialScale, initialScale * minScale, t);
            }

            float newScale = Mathf.Lerp(playerShadow.localScale.x, targetScale, Time.deltaTime * scaleSmooth);
            playerShadow.localScale = Vector3.one * newScale;
        }
        else
        {
            playerShadow.gameObject.SetActive(false);
        }
    }

    private void Move()
    {
        bool isSprinting = sprintAction != null && sprintAction.IsPressed();
        float moveSpeed = isSprinting ? sprintSpeed : walkSpeed;

        Vector3 desired;

        if (cameraTransform != null)
        {
            Vector3 forward = cameraTransform.forward;
            forward.y = 0f;
            if (forward.sqrMagnitude > 0.0001f)
                forward.Normalize();
            else
                forward = Vector3.forward;

            Vector3 right = cameraTransform.right;
            right.y = 0f;
            if (right.sqrMagnitude > 0.0001f)
                right.Normalize();
            else
                right = Vector3.right;

            desired = (right * moveAmt.x + forward * moveAmt.y);
        }
        else
        {
            Vector3 raw = new Vector3(moveAmt.x, 0f, moveAmt.y);
            if (inputRotation != 0f)
                raw = Quaternion.Euler(0f, inputRotation, 0f) * raw;
            desired = raw;
        }

        moveDirection = desired.sqrMagnitude > 0.0001f ? desired.normalized : Vector3.zero;

        Vector3 flatVel = new Vector3(rb.velocity.x, 0f, rb.velocity.z);
        Vector3 targetVel = moveDirection * moveSpeed;
        Vector3 velChange = targetVel - flatVel;

        rb.AddForce(velChange, ForceMode.VelocityChange);
    }

    private void HandleFlip()
    {
        float x = moveAmt.x;

        if (x > FlipHysteresis)
            facingRight = true;
        else if (x < -FlipHysteresis)
            facingRight = false;
    }

    private void UpdateSpriteRotation()
    {
        if (!cameraTransform)
            return;

        Vector3 forward = cameraTransform.forward;
        forward.y = 0f;
        if (forward.sqrMagnitude < 0.0001f)
            return;

        Quaternion faceCam = Quaternion.LookRotation(forward.normalized, Vector3.up);
        float flipY = facingRight ? 0f : 180f;

        spriteHolder.rotation = faceCam * Quaternion.Euler(0f, flipY, 0f);
    }

    private void ApplyExtraGravity()
    {
        if (!isGrounded)
            rb.AddForce(Vector3.down * extraGravity, ForceMode.Acceleration);
    }

    private void Jump()
    {
        rb.velocity = new Vector3(rb.velocity.x, jumpForce, rb.velocity.z);
    }

    private void GroundCheck()
    {
        if (groundCheck == null)
            return;

        isGrounded = Physics.CheckSphere(groundCheck.position, groundDistance, groundMask);
    }

    private void AnimateMovement()
    {
        if (!playerAnim)
            return;

        bool isMoving = moveAmt.sqrMagnitude > 0.01f;
        playerAnim.SetBool("isWalking", isMoving);
    }

    private void Melee()
    {
        if (playerCombat != null)
            playerCombat.OnMelee();
    }

    public void SetInputRotation(float degrees)
    {
        inputRotation = degrees;
    }

    public void SetCameraTransform(Transform cam)
    {
        cameraTransform = cam;
    }

    private void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Water") && !isDead && !isSinking)
            StartCoroutine(DrownSequence());
    }

    private IEnumerator DrownSequence()
    {
        isSinking = true;

        if (moveAction != null) moveAction.Disable();
        if (jumpAction != null) jumpAction.Disable();
        if (sprintAction != null) sprintAction.Disable();
        if (meleeAction != null) meleeAction.Disable();

        rb.velocity = Vector3.zero;
        rb.isKinematic = true;
        rb.useGravity = false;

        if (playerShadow)
            playerShadow.gameObject.SetActive(false);

        float startY = transform.position.y;
        float targetY = startY - sinkDepth;

        while (transform.position.y > targetY)
        {
            transform.position -= new Vector3(0f, sinkSpeed * Time.deltaTime, 0f);
            yield return null;
        }

        Die();
    }

    private void Die()
    {
        isDead = true;

        if (playerShadow)
            playerShadow.gameObject.SetActive(false);

        LevelManager.TriggerPlayerDeath();
    }

    public void Revive()
    {
        if (this == null)
            return;

        isDead = false;
        isSinking = false;
        moveAmt = Vector2.zero;
        moveDirection = Vector3.zero;

        if (rb != null)
        {
            rb.isKinematic = false;
            rb.useGravity = true;
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
        }

        CacheActions();

        if (moveAction != null) moveAction.Enable();
        if (jumpAction != null) jumpAction.Enable();
        if (sprintAction != null) sprintAction.Enable();
        if (meleeAction != null) meleeAction.Enable();

        RefreshCameraReference();

        if (playerShadow)
            playerShadow.gameObject.SetActive(true);
    }
}