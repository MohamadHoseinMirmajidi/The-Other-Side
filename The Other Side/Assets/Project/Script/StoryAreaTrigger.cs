using System.Collections;
using UnityEngine;

public enum StoryAreaType
{
    BackstageStairsUp,
    UpperStairsDown
}

[RequireComponent(typeof(Collider2D))]
public class StoryAreaTrigger : Interactable
{
    [SerializeField] private StoryRouteController route;
    [SerializeField] private StoryAreaType areaType;

    public void Initialize(StoryRouteController storyRoute, StoryAreaType type)
    {
        route = storyRoute;
        areaType = type;
        GetComponent<Collider2D>().isTrigger = true;
    }

    public override bool CanInteract =>
        base.CanInteract &&
        route != null &&
        route.CanUseStairs(areaType);

    public override IEnumerator ExecuteInteraction()
    {
        if (!CanInteract)
            yield break;

        yield return route.UseStairs(areaType);
    }
}
