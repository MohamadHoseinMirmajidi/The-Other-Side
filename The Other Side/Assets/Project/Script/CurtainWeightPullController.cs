using System.Collections;
using UnityEngine;

public class CurtainWeightPullController : MonoBehaviour
{
    [Header("Upper Room Visuals")]
    [SerializeField] private GameObject ropeWithoutWeightVisual;
    [SerializeField] private GameObject ropeWithWeightVisual;
    [SerializeField] private bool keepRopeVisibleWhenWeightIsAttached = true;

    [Header("Movement")]
    [SerializeField] private Transform movingRoot;
    [SerializeField] private Transform startPoint;
    [SerializeField] private Transform endPoint;
    [SerializeField] private float pullDuration = 1.4f;
    [SerializeField] private AnimationCurve pullEase =
        AnimationCurve.EaseInOut(0f, 0f, 1f, 1f);

    public void Configure(
        GameObject withoutWeight,
        GameObject withWeight,
        Transform visualRoot,
        Transform movementStart,
        Transform movementEnd,
        bool keepBaseRope)
    {
        ropeWithoutWeightVisual = withoutWeight;
        ropeWithWeightVisual = withWeight;
        movingRoot = visualRoot;
        startPoint = movementStart;
        endPoint = movementEnd;
        keepRopeVisibleWhenWeightIsAttached = keepBaseRope;
    }

    public void ApplyState(bool hasBeenPulled)
    {
        if (ropeWithoutWeightVisual != null)
        {
            ropeWithoutWeightVisual.SetActive(
                !hasBeenPulled || keepRopeVisibleWhenWeightIsAttached);
        }

        if (ropeWithWeightVisual != null)
            ropeWithWeightVisual.SetActive(hasBeenPulled);

        if (movingRoot != null)
        {
            Transform point = hasBeenPulled ? endPoint : startPoint;
            if (point != null)
                movingRoot.position = point.position;
        }
    }

    public IEnumerator PullDown()
    {
        if (ropeWithoutWeightVisual != null)
            ropeWithoutWeightVisual.SetActive(keepRopeVisibleWhenWeightIsAttached);
        if (ropeWithWeightVisual != null)
            ropeWithWeightVisual.SetActive(true);

        if (movingRoot == null || startPoint == null || endPoint == null)
            yield break;

        Vector3 start = startPoint.position;
        Vector3 end = endPoint.position;
        movingRoot.position = start;

        float elapsed = 0f;
        while (elapsed < pullDuration)
        {
            elapsed += Time.deltaTime;
            float progress = pullDuration <= 0f
                ? 1f
                : Mathf.Clamp01(elapsed / pullDuration);
            float eased = pullEase == null || pullEase.length == 0
                ? progress
                : pullEase.Evaluate(progress);
            movingRoot.position = Vector3.Lerp(start, end, eased);
            yield return null;
        }

        movingRoot.position = end;
    }
}
