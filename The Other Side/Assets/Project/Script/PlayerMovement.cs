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
    }

    private void Update()
    {
        ReadInput();
    }

    private void ReadInput()
    {
        moveInput = 0f;
        isRunning = false;

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null)
            return;

        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            moveInput -= 1f;

        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            moveInput += 1f;

        isRunning = keyboard.leftShiftKey.isPressed
                 || keyboard.rightShiftKey.isPressed;
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
        if (spriteRenderer != null && flipSprite)
        {
            // flipX = true means "draw the sprite mirrored" = looking left
            spriteRenderer.flipX = (facingDirection < 0);
        }

        if (flipByScale)
        {
            // Positive scale.x = right, negative = left
            Vector3 scale = transform.localScale;
            scale.x = Mathf.Abs(scale.x) * facingDirection;
            transform.localScale = scale;
        }
    }

    /// <summary>
    /// Call after interaction / cutscene so control returns cleanly (no wall stick).
    /// </summary>
    public void ResetMotion()
    {
        rb.linearVelocity = Vector2.zero;
        moveInput = 0f;
        isRunning = false;
    }
}
