using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Side-scroller movement: left/right only, smooth accel/decel, frame-rate independent.
/// Uses the NEW Input System (Keyboard.current).
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
    [SerializeField] private bool flipSprite = true;

    private Rigidbody2D rb;
    private SpriteRenderer spriteRenderer;

    private float moveInput;
    private bool isRunning;
    private int facingDirection = 1; // 1 = right, -1 = left

    public float CurrentSpeed => Mathf.Abs(rb.linearVelocity.x);
    public int FacingDirection => facingDirection;
    public bool IsMoving => CurrentSpeed > 0.05f;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();

        rb.gravityScale = 0f;
        rb.constraints = RigidbodyConstraints2D.FreezePositionY
                       | RigidbodyConstraints2D.FreezeRotation;
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
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

        // A / Left Arrow = left
        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
            moveInput -= 1f;

        // D / Right Arrow = right
        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
            moveInput += 1f;

        // Shift = run
        isRunning = keyboard.leftShiftKey.isPressed
                 || keyboard.rightShiftKey.isPressed;
    }

    private void FixedUpdate()
    {
        float targetSpeed = moveInput * (isRunning ? runSpeed : walkSpeed);
        float currentX = rb.linearVelocity.x;

        // Speed up when holding a key, slow down when idle
        float rate = Mathf.Abs(moveInput) > 0.01f ? acceleration : deceleration;
        float newX = Mathf.MoveTowards(currentX, targetSpeed, rate * Time.fixedDeltaTime);

        rb.linearVelocity = new Vector2(newX, 0f);
        UpdateFacing();
    }

    private void UpdateFacing()
    {
        if (Mathf.Abs(moveInput) < 0.01f)
            return;

        int newDirection = moveInput > 0f ? 1 : -1;
        if (newDirection == facingDirection)
            return;

        facingDirection = newDirection;

        if (flipSprite && spriteRenderer != null)
            spriteRenderer.flipX = facingDirection < 0;
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
