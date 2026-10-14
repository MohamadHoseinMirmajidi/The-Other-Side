using UnityEngine;

public class ControlledNarrativeShadow : MonoBehaviour
{
    [Header("Required References")]
    [SerializeField] private GameProgress gameProgress;
    [SerializeField] private SpriteRenderer sourceRenderer;
    [SerializeField] private Transform lightSource;
    [SerializeField] private Transform receiver;
    [SerializeField] private SpriteMask clipMask;
    [SerializeField] private SpriteRenderer shadowRenderer;

    [Header("Optional Attached Visual")]
    [SerializeField] private SpriteRenderer attachedSourceRenderer;
    [SerializeField] private SpriteRenderer attachedShadowRenderer;

    [Header("Controlled Projection")]
    [SerializeField] private Vector2 sourceOrigin;
    [SerializeField] private Vector2 receiverOffset;
    [SerializeField] private Vector2 sourceMovement = Vector2.one;
    [SerializeField] private Vector2 lightMovement = new Vector2(-0.35f, -0.1f);
    [SerializeField] private Vector2 scaleMultiplier = Vector2.one;
    [SerializeField] private float lightScaleInfluence = 0.08f;

    [Header("Progress")]
    [SerializeField] private bool requiresProgressEvent;
    [SerializeField] private GameProgressEvent requiredEvent;
    [SerializeField] private bool clarifyAfterProgressEvent;
    [SerializeField] private GameProgressEvent clarifyEvent;
    [SerializeField] private Vector2 unclearScale = new Vector2(1.5f, 0.7f);

    private Vector3 startingLightPosition;

    private void Awake()
    {
        if (gameProgress == null)
            gameProgress = FindAnyObjectByType<GameProgress>();

        if (lightSource != null)
            startingLightPosition = lightSource.position;
    }

    private void LateUpdate()
    {
        bool canShow = !requiresProgressEvent ||
                       gameProgress.HasCompleted(requiredEvent);

        if (!canShow || sourceRenderer == null || sourceRenderer.sprite == null)
        {
            shadowRenderer.enabled = false;
            SetAttachedShadowVisible(false);
            return;
        }

        shadowRenderer.enabled = true;
        shadowRenderer.sprite = sourceRenderer.sprite;
        shadowRenderer.flipX = sourceRenderer.flipX ^
                               sourceRenderer.transform.lossyScale.x < 0f;
        shadowRenderer.flipY = sourceRenderer.flipY;

        Vector2 sourceDelta = (Vector2)sourceRenderer.transform.position - sourceOrigin;
        Vector2 lightDelta = lightSource == null
            ? Vector2.zero
            : (Vector2)(lightSource.position - startingLightPosition);

        Vector2 targetPosition = (Vector2)receiver.position + receiverOffset;
        targetPosition += Vector2.Scale(sourceDelta, sourceMovement);
        targetPosition += Vector2.Scale(lightDelta, lightMovement);
        shadowRenderer.transform.position = targetPosition;

        Vector3 sourceScale = sourceRenderer.transform.lossyScale;
        Vector2 finalScale = new Vector2(
            Mathf.Abs(sourceScale.x) * scaleMultiplier.x,
            Mathf.Abs(sourceScale.y) * scaleMultiplier.y);

        if (clarifyAfterProgressEvent &&
            !gameProgress.HasCompleted(clarifyEvent))
        {
            finalScale = Vector2.Scale(finalScale, unclearScale);
            shadowRenderer.color = new Color(0.04f, 0.04f, 0.06f, 0.8f);
        }
        else if (clarifyAfterProgressEvent)
        {
            shadowRenderer.color = new Color(0.38f, 0.4f, 0.46f, 0.35f);
        }

        float lightScale = 1f;
        if (lightSource != null)
        {
            lightScale += Mathf.Abs(lightDelta.x) * lightScaleInfluence;
            finalScale *= lightScale;
        }

        shadowRenderer.transform.localScale = new Vector3(
            finalScale.x,
            finalScale.y,
            1f);

        UpdateAttachedShadow(lightScale);
    }

    private void UpdateAttachedShadow(float lightScale)
    {
        if (attachedSourceRenderer == null || attachedShadowRenderer == null)
            return;

        bool shouldShow = attachedSourceRenderer.gameObject.activeInHierarchy &&
                          attachedSourceRenderer.sprite != null;
        SetAttachedShadowVisible(shouldShow);

        if (!shouldShow)
            return;

        attachedShadowRenderer.sprite = attachedSourceRenderer.sprite;
        attachedShadowRenderer.flipX = attachedSourceRenderer.flipX ^
                                       attachedSourceRenderer.transform.lossyScale.x < 0f;
        attachedShadowRenderer.flipY = attachedSourceRenderer.flipY;

        Vector2 sourceOffset =
            attachedSourceRenderer.transform.position - sourceRenderer.transform.position;
        Vector2 projectedOffset = Vector2.Scale(sourceOffset, scaleMultiplier);

        attachedShadowRenderer.transform.position =
            shadowRenderer.transform.position +
            new Vector3(projectedOffset.x, projectedOffset.y, -0.01f);

        Vector3 attachedScale = attachedSourceRenderer.transform.lossyScale;
        attachedShadowRenderer.transform.localScale = new Vector3(
            Mathf.Abs(attachedScale.x) * scaleMultiplier.x * lightScale,
            Mathf.Abs(attachedScale.y) * scaleMultiplier.y * lightScale,
            1f);
    }

    private void SetAttachedShadowVisible(bool value)
    {
        if (attachedShadowRenderer != null)
            attachedShadowRenderer.enabled = value;
    }
}
