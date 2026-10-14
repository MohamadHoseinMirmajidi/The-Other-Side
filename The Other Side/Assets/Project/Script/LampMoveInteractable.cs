using System.Collections;
using UnityEngine;

public class LampMoveInteractable : Interactable
{
    [Header("Progress")]
    [SerializeField] private GameProgress gameProgress;

    [Header("Lamp Rail")]
    [SerializeField] private Transform lamp;
    [SerializeField] private Vector3 targetLocalPosition = new Vector3(-0.7f, 0.2f, 0f);
    [SerializeField] private float moveDuration = 1.2f;

    [Header("Player")]
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Transform playerVisual;
    [SerializeField] private float pushPoseAngle = -6f;

    public override bool CanInteract =>
        base.CanInteract &&
        gameProgress != null &&
        gameProgress.CanComplete(GameProgressEvent.LampMoved);

    private void Awake()
    {
        if (gameProgress == null)
            gameProgress = FindAnyObjectByType<GameProgress>();

        if (playerMovement == null)
            playerMovement = FindAnyObjectByType<PlayerMovement>();

        if (playerVisual == null)
        {
            GameObject body = GameObject.Find("Body");
            if (body != null)
                playerVisual = body.transform;
        }

        if (lamp == null)
            lamp = transform;
    }

    public override IEnumerator ExecuteInteraction()
    {
        if (!CanInteract)
            yield break;

        SetAvailable(false);
        SetPlayerControl(false);

        Quaternion startingPlayerRotation = Quaternion.identity;
        if (playerVisual != null)
        {
            startingPlayerRotation = playerVisual.localRotation;
            playerVisual.localRotation =
                startingPlayerRotation * Quaternion.Euler(0f, 0f, pushPoseAngle);
        }

        yield return base.ExecuteInteraction();

        Vector3 startingPosition = lamp.localPosition;
        float elapsed = 0f;
        while (elapsed < moveDuration)
        {
            elapsed += Time.deltaTime;
            float progress = moveDuration <= 0f
                ? 1f
                : Mathf.Clamp01(elapsed / moveDuration);
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

            lamp.localPosition = Vector3.Lerp(
                startingPosition,
                targetLocalPosition,
                smoothProgress);

            yield return null;
        }

        gameProgress.TryComplete(GameProgressEvent.LampMoved);

        if (playerVisual != null)
            playerVisual.localRotation = startingPlayerRotation;

        Collider2D interactionArea = GetComponent<Collider2D>();
        if (interactionArea != null)
            interactionArea.enabled = false;

        SetPlayerControl(true);
    }

    private void SetPlayerControl(bool value)
    {
        if (playerMovement == null)
            return;

        playerMovement.ResetMotion();
        playerMovement.enabled = value;
    }
}
