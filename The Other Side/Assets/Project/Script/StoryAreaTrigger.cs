using UnityEngine;

public enum StoryAreaType
{
    BackstageStairsUp,
    UpperStairsDown
}

[RequireComponent(typeof(Collider2D))]
public class StoryAreaTrigger : MonoBehaviour
{
    [SerializeField] private StoryRouteController route;
    [SerializeField] private StoryAreaType areaType;

    public void Initialize(StoryRouteController storyRoute, StoryAreaType type)
    {
        route = storyRoute;
        areaType = type;
        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponentInParent<PlayerMovement>() != null)
            route?.EnterArea(areaType);
    }
}
