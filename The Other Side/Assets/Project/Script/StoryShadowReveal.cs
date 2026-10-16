using System.Collections;
using UnityEngine;

public class StoryShadowReveal : MonoBehaviour
{
    [SerializeField] private SpriteRenderer shadowRenderer;
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;
    [SerializeField] private float revealDuration = 0.65f;
    [SerializeField] private AnimationCurve revealEase =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    private Color visibleColor = Color.white;

    public void Configure(
        SpriteRenderer renderer,
        Transform revealStart,
        Transform revealEnd)
    {
        shadowRenderer = renderer;
        startPoint = revealStart;
        endPoint = revealEnd;
    }

    private void Awake()
    {
        if (shadowRenderer == null)
            shadowRenderer = GetComponent<SpriteRenderer>();

        if (shadowRenderer != null)
            visibleColor = shadowRenderer.color;
    }

    public void ResetHidden()
    {
        if (startPoint != null)
            transform.position = startPoint.position;

        if (shadowRenderer != null)
        {
            Color hiddenColor = visibleColor;
            hiddenColor.a = 0f;
            shadowRenderer.color = hiddenColor;
        }

        gameObject.SetActive(false);
    }

    public IEnumerator Reveal()
    {
        Vector3 start = startPoint != null ? startPoint.position : transform.position;
        Vector3 end = endPoint != null ? endPoint.position : transform.position;

        gameObject.SetActive(true);
        transform.position = start;

        float elapsed = 0f;
        while (elapsed < revealDuration)
        {
            elapsed += Time.deltaTime;
            float progress = revealDuration <= 0f
                ? 1f
                : Mathf.Clamp01(elapsed / revealDuration);
            float eased = revealEase == null || revealEase.length == 0
                ? progress
                : revealEase.Evaluate(progress);

            transform.position = Vector3.Lerp(start, end, eased);
            if (shadowRenderer != null)
            {
                Color color = visibleColor;
                color.a *= eased;
                shadowRenderer.color = color;
            }

            yield return null;
        }

        transform.position = end;
        if (shadowRenderer != null)
            shadowRenderer.color = visibleColor;
    }

    public void Hide()
    {
        gameObject.SetActive(false);
    }
}
