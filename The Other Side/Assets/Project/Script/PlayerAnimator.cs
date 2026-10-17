using System.Collections;
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

    [Header("Locomotion state names")]
    [SerializeField] private string idleState = "Idle Animation";
    [SerializeField] private string walkState = "Walk Animation";
    [SerializeField] private string runState = "Run Animation";
    [SerializeField] private float runAnimationSpeedThreshold = 4f;

    [SerializeField] private Animator animator;
    private int speedHash;
    private int facingHash;
    private int isMovingHash;
    private string persistentState;

    private void Awake()
    {
        if (animator == null)
        {
            animator = GetComponent<Animator>();
        }

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

    public IEnumerator PlayAction(string stateName)
    {
        if (!TryPlayState(stateName))
            yield break;

        yield return null;

        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);
        float duration = stateInfo.length;
        if (duration <= 0f)
            duration = 0.25f;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }

        RestorePreferredState();
    }

    public void SetPersistentState(string stateName)
    {
        if (!HasState(stateName))
            return;

        persistentState = stateName;
        PlayState(stateName);
    }

    public void ClearPersistentState()
    {
        persistentState = null;
        RestorePreferredState();
    }

    private bool TryPlayState(string stateName)
    {
        if (!HasState(stateName))
        {
            Debug.LogWarning("PlayerAnimator: state not found: " + stateName, this);
            return false;
        }

        PlayState(stateName);
        return true;
    }

    private bool HasState(string stateName)
    {
        if (animator == null || string.IsNullOrWhiteSpace(stateName))
            return false;

        int fullPathHash = Animator.StringToHash("Base Layer." + stateName);
        return animator.HasState(0, fullPathHash);
    }

    private void PlayState(string stateName)
    {
        int fullPathHash = Animator.StringToHash("Base Layer." + stateName);
        animator.Play(fullPathHash, 0, 0f);
    }

    private void RestorePreferredState()
    {
        if (animator == null)
            return;

        if (!string.IsNullOrEmpty(persistentState) && HasState(persistentState))
        {
            PlayState(persistentState);
            return;
        }

        string locomotionState = idleState;
        if (movement != null && movement.IsMoving)
        {
            locomotionState = movement.CurrentSpeed >= runAnimationSpeedThreshold
                ? runState
                : walkState;
        }

        if (HasState(locomotionState))
            PlayState(locomotionState);
    }
}
