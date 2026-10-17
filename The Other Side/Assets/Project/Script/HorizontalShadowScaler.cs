using UnityEngine;

public class HorizontalShadowScaler : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private StorySectionLayout section;
    [SerializeField] private Vector3 minimumScale = new Vector3(2.5f, 3.5f, 1f);
    [SerializeField] private Vector3 maximumScale = new Vector3(5f, 7f, 1f);

    [Header("Position Relative To Light")]
    [Tooltip("A zero value keeps the current scene offset from the player.")]
    [SerializeField] private Vector3 worldOffset;

    private Quaternion fixedWorldRotation;

    public void Configure(
        Transform targetPlayer,
        StorySectionLayout targetSection,
        Vector3 smallestScale,
        Vector3 largestScale)
    {
        player = targetPlayer;
        section = targetSection;
        minimumScale = smallestScale;
        maximumScale = largestScale;
    }

    private void Awake()
    {
        if (player == null)
            return;

        if (worldOffset == Vector3.zero)
            worldOffset = transform.position - player.position;

        fixedWorldRotation = transform.rotation;
    }

    private void LateUpdate()
    {
        if (player == null || section == null)
            return;

        // Keep the shadow on the light-facing side of the character. The
        // player may flip with a negative X scale, but the shadow must not.
        transform.position = player.position + worldOffset;
        transform.rotation = fixedWorldRotation;

        float progress = section.GetHorizontalProgress(player.position.x);
        Vector3 scale = Vector3.Lerp(minimumScale, maximumScale, progress);

        Transform parent = transform.parent;
        if (parent != null && parent.lossyScale.x < 0f)
            scale.x = -Mathf.Abs(scale.x);
        else
            scale.x = Mathf.Abs(scale.x);

        transform.localScale = scale;
    }
}
