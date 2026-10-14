using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class Interactable : MonoBehaviour
{
    [Header("Interaction Settings")]
    [SerializeField] private bool isAvailable = true;
    [SerializeField] private float interactionDuration = 0f;

    [Header("Prompt")]
    [SerializeField] private Transform promptAnchor;

    [Header("Interaction Event")]
    [SerializeField] private UnityEvent onInteract;
    
    public Transform PromptAnchor => promptAnchor != null
        ? promptAnchor
        : transform;

    public virtual bool CanInteract =>
        isAvailable &&
        isActiveAndEnabled &&
        gameObject.activeInHierarchy;

    public void SetAvailable(bool value)
    {
        isAvailable = value;
    }

    public virtual IEnumerator ExecuteInteraction()
    {
        if (!CanInteract)
            yield break;

        onInteract?.Invoke();

        if (interactionDuration > 0f)
        {
            yield return new WaitForSeconds(interactionDuration);
        }
    }
}
