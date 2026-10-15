using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionManager : MonoBehaviour
{
    [SerializeField] private PlayerInteractionDetector detector;

    private Interactable currentTarget;
    private bool isInteracting;

    private void Awake()
    {
        if (detector == null)
            detector = GetComponentInChildren<PlayerInteractionDetector>();

        if (detector == null)
        {
            Debug.LogError("Player Interaction Detector is not assigned.", this);
            enabled = false;
        }
    }

    private void Update()
    {
        if (isInteracting || Time.timeScale == 0f)
            return;

        currentTarget = FindClosestTarget();
        if (currentTarget == null || Keyboard.current == null)
            return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
            StartCoroutine(PerformInteraction(currentTarget));
    }

    private Interactable FindClosestTarget()
    {
        Interactable closestTarget = null;
        float closestDistance = float.MaxValue;

        foreach (Interactable candidate in detector.NearbyInteractables)
        {
            if (candidate == null || !candidate.CanInteract)
                continue;

            float distance = (candidate.transform.position - transform.position).sqrMagnitude;
            if (distance >= closestDistance)
                continue;

            closestDistance = distance;
            closestTarget = candidate;
        }

        return closestTarget;
    }

    private IEnumerator PerformInteraction(Interactable target)
    {
        if (isInteracting || target == null ||
            !target.CanInteract || !detector.IsNearby(target))
        {
            yield break;
        }

        isInteracting = true;
        yield return StartCoroutine(target.ExecuteInteraction());
        isInteracting = false;
        currentTarget = null;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        isInteracting = false;
        currentTarget = null;
    }
}
