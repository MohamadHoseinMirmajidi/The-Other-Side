using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionManager : MonoBehaviour
{
    [SerializeField] private PlayerInteractionDetector detector;

    [Header("Tutorial Paper Prompt Only")]
    [SerializeField] private Interactable tutorialPromptTarget;
    [SerializeField] private InteractionPromptUI tutorialPrompt;

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

        tutorialPrompt?.Hide();
    }

    private void Update()
    {
        if (isInteracting || Time.timeScale == 0f)
        {
            tutorialPrompt?.Hide();
            return;
        }

        currentTarget = FindClosestTarget();
        UpdateTutorialPrompt();

        if (currentTarget == null || Keyboard.current == null)
            return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
            StartCoroutine(PerformInteraction(currentTarget));
    }

    private void UpdateTutorialPrompt()
    {
        if (tutorialPrompt == null)
            return;

        if (currentTarget != null &&
            currentTarget == tutorialPromptTarget &&
            currentTarget.CanInteract)
        {
            tutorialPrompt.Show(currentTarget);
        }
        else
        {
            tutorialPrompt.Hide();
        }
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
        tutorialPrompt?.Hide();
        yield return StartCoroutine(target.ExecuteInteraction());
        isInteracting = false;
        currentTarget = null;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        isInteracting = false;
        currentTarget = null;
        tutorialPrompt?.Hide();
    }
}
