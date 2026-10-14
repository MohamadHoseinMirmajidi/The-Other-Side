using UnityEngine.InputSystem;
using System.Collections;
using UnityEngine;

public class InteractionManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private PlayerInteractionDetector detector;
    [SerializeField] private InteractionPromptUI promptPrefab;

    //[Header("Input")]
    //[SerializeField] private KeyCode interactionKey = KeyCode.E;

    private InteractionPromptUI promptUI;
    private Interactable currentTarget;
    private bool isInteracting;

    private void Awake()
    {
        if (promptPrefab == null)
        {
            Debug.LogError(
                "Interaction Prompt Prefab is not assigned!",
                this
            );
            enabled = false;
            return;
        }

        promptUI = Instantiate(promptPrefab);
        promptUI.Hide();

        if (detector == null)
        {
            Debug.LogError(
                "Player Interaction Detector is not assigned!",
                this
            );
            enabled = false;
        }
    }

    private void Update()
    {
        if (isInteracting || Time.timeScale == 0f)
        {
            promptUI.Hide();
            return;
        }

        currentTarget = FindClosestTarget();

        if (currentTarget == null)
        {
            promptUI.Hide();
            return;
        }

        promptUI.Show(currentTarget);

        if (Keyboard.current != null &&
            Keyboard.current.eKey.wasPressedThisFrame)
        {
            StartCoroutine(PerformInteraction(currentTarget));
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

            float distance = (
                candidate.transform.position - transform.position
            ).sqrMagnitude;

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closestTarget = candidate;
            }
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
        promptUI.Hide();

        yield return StartCoroutine(target.ExecuteInteraction());

        isInteracting = false;
        currentTarget = null;
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        isInteracting = false;
        currentTarget = null;

        if (promptUI != null)
            promptUI.Hide();
    }

    private void OnDestroy()
    {
        if (promptUI != null)
            Destroy(promptUI.gameObject);
    }
}
