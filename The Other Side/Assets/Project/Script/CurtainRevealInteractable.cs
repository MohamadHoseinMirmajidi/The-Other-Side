using System.Collections;
using UnityEngine;

public class CurtainRevealInteractable : Interactable
{
    [Header("Progress")]
    [SerializeField] private GameProgress gameProgress;

    [Header("Curtain")]
    [SerializeField] private Transform curtainVisual;
    [SerializeField] private SpriteMask shadowMask;
    [SerializeField] private float retractDuration = 0.9f;
    [SerializeField] private Vector3 retractedOffset = new Vector3(-1.7f, 0f, 0f);
    [SerializeField] private float retractedWidth = 0.55f;
    [SerializeField] private float retractedMaskWidth = 0.35f;

    [Header("Player")]
    [SerializeField] private PlayerMovement playerMovement;

    private Vector3 startingCurtainPosition;
    private Vector3 startingCurtainScale;
    private Vector3 startingMaskPosition;
    private Vector3 startingMaskScale;

    public override bool CanInteract =>
        base.CanInteract &&
        gameProgress != null &&
        gameProgress.CanComplete(GameProgressEvent.CurtainRetracted);

    private void Awake()
    {
        if (gameProgress == null)
            gameProgress = FindAnyObjectByType<GameProgress>();

        if (playerMovement == null)
            playerMovement = FindAnyObjectByType<PlayerMovement>();

        if (curtainVisual == null)
            curtainVisual = transform;

        if (shadowMask == null)
        {
            GameObject receiver = GameObject.Find("Dress Shadow Receiver");
            if (receiver != null)
                shadowMask = receiver.GetComponent<SpriteMask>();
        }

        startingCurtainPosition = curtainVisual.localPosition;
        startingCurtainScale = curtainVisual.localScale;

        if (shadowMask != null)
        {
            startingMaskPosition = shadowMask.transform.position;
            startingMaskScale = shadowMask.transform.localScale;
        }
    }

    public override IEnumerator ExecuteInteraction()
    {
        if (!CanInteract)
            yield break;

        SetAvailable(false);
        SetPlayerControl(false);
        yield return base.ExecuteInteraction();

        float elapsed = 0f;
        while (elapsed < retractDuration)
        {
            elapsed += Time.deltaTime;
            float progress = retractDuration <= 0f
                ? 1f
                : Mathf.Clamp01(elapsed / retractDuration);
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

            Vector3 targetPosition = startingCurtainPosition + retractedOffset;
            Vector3 targetScale = new Vector3(
                retractedWidth,
                startingCurtainScale.y,
                startingCurtainScale.z);

            curtainVisual.localPosition = Vector3.Lerp(
                startingCurtainPosition,
                targetPosition,
                smoothProgress);
            curtainVisual.localScale = Vector3.Lerp(
                startingCurtainScale,
                targetScale,
                smoothProgress);

            UpdateShadowMask(smoothProgress);
            yield return null;
        }

        gameProgress.TryComplete(GameProgressEvent.CurtainRetracted);

        Collider2D interactionArea = GetComponent<Collider2D>();
        if (interactionArea != null)
            interactionArea.enabled = false;

        SetPlayerControl(true);
    }

    private void UpdateShadowMask(float progress)
    {
        if (shadowMask == null)
            return;

        Vector3 targetPosition = startingMaskPosition + retractedOffset;
        Vector3 targetScale = new Vector3(
            retractedMaskWidth,
            startingMaskScale.y,
            startingMaskScale.z);

        shadowMask.transform.position = Vector3.Lerp(
            startingMaskPosition,
            targetPosition,
            progress);
        shadowMask.transform.localScale = Vector3.Lerp(
            startingMaskScale,
            targetScale,
            progress);
    }

    private void SetPlayerControl(bool value)
    {
        if (playerMovement == null)
            return;

        playerMovement.ResetMotion();
        playerMovement.enabled = value;
    }
}
