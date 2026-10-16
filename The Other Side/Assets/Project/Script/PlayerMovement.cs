using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Side-scroller movement: left/right only, smooth accel/decel, frame-rate independent.
/// Uses the NEW Input System (Keyboard.current).
/// Faces right by default; flips left when moving left.
/// Requires Rigidbody2D (freeze Y, no gravity) + CapsuleCollider2D.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
public class PlayerMovement : MonoBehaviour
{
    [Header("Speed")]
    [SerializeField] private float walkSpeed = 3.5f;
    [SerializeField] private float runSpeed = 5.5f;

    [Header("Feel")]
    [SerializeField] private float acceleration = 18f;
    [SerializeField] private float deceleration = 22f;

    [Header("Facing")]
    // Flip the sprite when direction changes
    [SerializeField] private bool flipSprite = true;
    // Also flip by scale.x (more reliable if Animator overrides flipX)
    [SerializeField] private bool flipByScale = true;

    [SerializeField] private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;
    private InputAction moveAction;
    private InputAction runAction;

    private float moveInput;
    private bool isRunning;

    // 1 = faces right (default), -1 = faces left
    private int facingDirection = 1;

    public float CurrentSpeed => Mathf.Abs(rb.linearVelocity.x);
    public int FacingDirection => facingDirection;
    public bool IsMoving => CurrentSpeed > 0.05f;

    private void Awake()
    {
        if (rb == null)
        {
            rb = GetComponent<Rigidbody2D>();
        }
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezePositionY
                       | RigidbodyConstraints2D.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

        // Default = looking right
        ApplyFacing();

        CreateInputActions();
    }

    private void OnEnable()
    {
        moveAction?.Enable();
        runAction?.Enable();
    }

    private void OnDisable()
    {
        moveAction?.Disable();
        runAction?.Disable();
        ResetMotion();
    }

    private void OnDestroy()
    {
        moveAction?.Dispose();
        runAction?.Dispose();
    }

    private void Update()
    {
        ReadInput();
    }

    private void ReadInput()
    {
        moveInput = moveAction != null
            ? Mathf.Clamp(moveAction.ReadValue<float>(), -1f, 1f)
            : 0f;
        isRunning = runAction != null && runAction.IsPressed();
    }

    private void CreateInputActions()
    {
        moveAction = new InputAction("Move", InputActionType.Value);
        moveAction.AddCompositeBinding("1DAxis")
            .With("Negative", "<Keyboard>/a")
            .With("Positive", "<Keyboard>/d");
        moveAction.AddCompositeBinding("1DAxis")
            .With("Negative", "<Keyboard>/leftArrow")
            .With("Positive", "<Keyboard>/rightArrow");

        runAction = new InputAction("Run", InputActionType.Button);
        runAction.AddBinding("<Keyboard>/leftShift");
        runAction.AddBinding("<Keyboard>/rightShift");
    }

    private void FixedUpdate()
    {
        float targetSpeed = moveInput * (isRunning ? runSpeed : walkSpeed);
        float currentX = rb.linearVelocity.x;

        float rate = Mathf.Abs(moveInput) > 0.01f ? acceleration : deceleration;
        float newX = Mathf.MoveTowards(currentX, targetSpeed, rate * Time.fixedDeltaTime);

        rb.linearVelocity = new Vector2(newX, 0f);

        // Update facing whenever we have input
        UpdateFacing();
    }

    private void UpdateFacing()
    {
        // No input -> keep the current face direction
        if (Mathf.Abs(moveInput) < 0.01f)
            return;

        // Moving right -> 1, moving left -> -1
        int newDirection = 1;
        if (moveInput < 0f)
            newDirection = -1;

        // Direction did not change
        if (newDirection == facingDirection)
            return;

        facingDirection = newDirection;
        ApplyFacing();
    }

    // Actually flip the character visual
    private void ApplyFacing()
    {
        if (flipByScale)
        {
            // Use one flip method at a time. Applying both scale and flipX
            // mirrors the sprite twice and makes it look permanently right-facing.
            if (spriteRenderer != null)
                spriteRenderer.flipX = false;

            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * facingDirection;
            transform.localScale = scale;
            return;
        }

        if (spriteRenderer != null && flipSprite)
            spriteRenderer.flipX = facingDirection < 0;
    }

    public void FaceDirection(float horizontalDirection)
    {
        if (Mathf.Abs(horizontalDirection) < 0.01f)
            return;

        int newDirection = horizontalDirection < 0f ? -1 : 1;
        if (newDirection == facingDirection)
            return;

        facingDirection = newDirection;
        ApplyFacing();
    }

    /// <summary>
    /// Call after interaction / cutscene so control returns cleanly (no wall stick).
    /// </summary>
    public void ResetMotion()
    {
        if (rb == null)
            rb = GetComponent<Rigidbody2D>();

        if (rb != null)
            rb.linearVelocity = Vector2.zero;

        moveInput = 0f;
        isRunning = false;
    }
}
