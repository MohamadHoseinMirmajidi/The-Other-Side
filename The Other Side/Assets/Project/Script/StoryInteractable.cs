using System.Collections;
using UnityEngine;

public enum StoryInteractionType
{
    TutorialPaper,
    HallwayDoor,
    CourtyardDoor,
    KeyRoomDoor,
    FuseBox,
    Toolbox,
    Weight,
    LockedStageDoor,
    CurtainRope,
    Backpack,
    BackstageReturnDoor,
    FuseReturnDoor,
    KeyRoomReturnDoor
}

public class StoryInteractable : Interactable
{
    [SerializeField] private StoryRouteController route;
    [SerializeField] private StoryInteractionType interactionType;

    public void Initialize(
        StoryRouteController storyRoute,
        StoryInteractionType type)
    {
        route = storyRoute;
        interactionType = type;
    }

    public override bool CanInteract =>
        base.CanInteract &&
        route != null &&
        route.CanInteract(interactionType);

    public override IEnumerator ExecuteInteraction()
    {
        if (!CanInteract)
            yield break;

        yield return route.HandleInteraction(interactionType, gameObject);
    }
}
