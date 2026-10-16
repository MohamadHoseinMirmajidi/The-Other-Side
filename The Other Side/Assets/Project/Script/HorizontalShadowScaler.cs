using UnityEngine;

public class HorizontalShadowScaler : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private StorySectionLayout section;
    [SerializeField] private Vector3 minimumScale = new Vector3(2.5f, 3.5f, 1f);
    [SerializeField] private Vector3 maximumScale = new Vector3(5f, 7f, 1f);

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

    private void LateUpdate()
    {
        if (player == null || section == null)
            return;

        float progress = section.GetHorizontalProgress(player.position.x);
        transform.localScale = Vector3.Lerp(minimumScale, maximumScale, progress);
    }
}
