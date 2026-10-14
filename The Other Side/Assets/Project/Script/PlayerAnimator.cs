using UnityEngine;

/// <summary>
/// Pushes movement state into Animator every frame.
/// Place on the same object as PlayerMovement, or on a child with the Animator.
/// </summary>
[RequireComponent(typeof(Animator))]
public class PlayerAnimator : MonoBehaviour
{
    [Header("Wiring")]
    [SerializeField] private PlayerMovement movement;

    [Header("Animator parameter names")]
    [SerializeField] private string speedParam = "Speed";
    [SerializeField] private string facingParam = "FacingDirection";
    [SerializeField] private string isMovingParam = "IsMoving";

    private Animator animator;
    private int speedHash;
    private int facingHash;
    private int isMovingHash;

    private void Awake()
    {
        animator = GetComponent<Animator>();

        if (movement == null)
            movement = GetComponentInParent<PlayerMovement>();

        if (movement == null)
        {
            Debug.LogError("PlayerAnimator: no PlayerMovement found.", this);
            return;
        }

        // Hash once — faster than string lookup every frame
        speedHash = Animator.StringToHash(speedParam);
        facingHash = Animator.StringToHash(facingParam);
        isMovingHash = Animator.StringToHash(isMovingParam);
    }

    private void Update()
    {
        if (movement == null)
            return;

        animator.SetFloat(speedHash, movement.CurrentSpeed);
        animator.SetInteger(facingHash, movement.FacingDirection);
        animator.SetBool(isMovingHash, movement.IsMoving);
    }
}