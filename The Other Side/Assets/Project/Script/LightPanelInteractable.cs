using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;

public class LightPanelInteractable : Interactable
{
    [Header("Progress")]
    [SerializeField] private GameProgress gameProgress;

    [Header("Route Light")]
    [SerializeField] private Light2D[] routeLights = new Light2D[0];
    [SerializeField] private float targetIntensity = 0.55f;
    [SerializeField] private float fadeDuration = 0.65f;

    [Header("Panel Feedback")]
    [SerializeField] private SpriteRenderer[] panelIndicators = new SpriteRenderer[0];
    [SerializeField] private Color offIndicatorColor = new Color(0.2f, 0.23f, 0.23f, 1f);
    [SerializeField] private Color onIndicatorColor = new Color(0.95f, 0.78f, 0.3f, 1f);

    [Header("Optional Audio")]
    [SerializeField] private AudioSource clickAudio;
    [SerializeField] private AudioSource humAudio;

    public override bool CanInteract =>
        base.CanInteract &&
        gameProgress != null &&
        gameProgress.CanComplete(GameProgressEvent.HallwayLightSelected);

    private void Awake()
    {
        if (gameProgress == null)
            gameProgress = FindAnyObjectByType<GameProgress>();

        FindPanelIndicators();

        bool isAlreadyOn = gameProgress != null &&
                           gameProgress.HasCompleted(GameProgressEvent.HallwayLightSelected);

        if (isAlreadyOn)
        {
            ApplyFinalState();
            SetAvailable(false);
        }
        else
        {
            ApplyOffState();
        }
    }

    private void FindPanelIndicators()
    {
        if (panelIndicators != null && panelIndicators.Length > 0)
            return;

        List<SpriteRenderer> foundIndicators = new List<SpriteRenderer>();
        string[] names = { "Panel Light 1", "Panel Light 2", "Panel Light 3" };

        foreach (string indicatorName in names)
        {
            GameObject indicator = GameObject.Find(indicatorName);
            if (indicator != null && indicator.TryGetComponent(out SpriteRenderer renderer))
                foundIndicators.Add(renderer);
        }

        panelIndicators = foundIndicators.ToArray();
    }

    public override IEnumerator ExecuteInteraction()
    {
        if (!CanInteract)
            yield break;

        SetAvailable(false);
        yield return base.ExecuteInteraction();

        if (!gameProgress.TryComplete(GameProgressEvent.HallwayLightSelected))
        {
            SetAvailable(true);
            yield break;
        }

        if (clickAudio != null && clickAudio.clip != null)
            clickAudio.Play();

        if (humAudio != null && humAudio.clip != null)
            humAudio.Play();

        float elapsed = 0f;
        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            float progress = fadeDuration <= 0f
                ? 1f
                : Mathf.Clamp01(elapsed / fadeDuration);
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

            SetRouteIntensity(targetIntensity * smoothProgress);
            UpdateIndicators(progress);

            yield return null;
        }

        ApplyFinalState();
    }

    private void ApplyOffState()
    {
        SetRouteIntensity(0f);

        foreach (SpriteRenderer indicator in panelIndicators)
        {
            if (indicator != null)
                indicator.color = offIndicatorColor;
        }
    }

    private void ApplyFinalState()
    {
        SetRouteIntensity(targetIntensity);

        foreach (SpriteRenderer indicator in panelIndicators)
        {
            if (indicator != null)
                indicator.color = onIndicatorColor;
        }
    }

    private void SetRouteIntensity(float intensity)
    {
        foreach (Light2D routeLight in routeLights)
        {
            if (routeLight != null)
                routeLight.intensity = intensity;
        }
    }

    private void UpdateIndicators(float progress)
    {
        if (panelIndicators.Length == 0)
            return;

        for (int i = 0; i < panelIndicators.Length; i++)
        {
            SpriteRenderer indicator = panelIndicators[i];
            if (indicator == null)
                continue;

            float start = (float)i / panelIndicators.Length;
            float localProgress = Mathf.InverseLerp(start, 1f, progress);
            indicator.color = Color.Lerp(
                offIndicatorColor,
                onIndicatorColor,
                localProgress);
        }
    }
}
