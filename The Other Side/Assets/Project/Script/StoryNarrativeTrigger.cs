using UnityEngine;

public enum StoryNarrativeAreaType
{
    BackstageCurtainIntro,
    ReturnFuseShadow
}

[RequireComponent(typeof(Collider2D))]
public class StoryNarrativeTrigger : MonoBehaviour
{
    [SerializeField] private StoryRouteController route;
    [SerializeField] private StoryNarrativeAreaType areaType;

    public void Initialize(
        StoryRouteController storyRoute,
        StoryNarrativeAreaType type)
    {
        route = storyRoute;
        areaType = type;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponentInParent<PlayerMovement>() == null)
            return;

        route?.EnterNarrativeArea(areaType);
    }
}
