using System.Collections;
using UnityEngine;

public enum RouteDoorRole
{
    FirstDoor,
    HighHandleDoor,
    CartDoor,
    ReturnFromBagRoom,
    ReturnFromCartRoom,
    ReturnFromShadowRoom,
    LockedStageDoor,
    CourtyardDoor
}

public class RouteDoorInteractable : Interactable
{
    private StoryRouteController routeController;
    private SpriteRenderer doorRenderer;

    public RouteDoorRole Role { get; private set; }
    public bool IsOpen { get; private set; }

    public override bool CanInteract =>
        base.CanInteract &&
        routeController != null &&
        routeController.CanUseDoor(Role);

    public void Initialize(
        StoryRouteController controller,
        RouteDoorRole role,
        SpriteRenderer renderer)
    {
        routeController = controller;
        Role = role;
        doorRenderer = renderer;
    }

    public void Open()
    {
        IsOpen = true;

        if (doorRenderer != null)
            doorRenderer.color = new Color(0.32f, 0.62f, 0.42f, 1f);
    }

    public override IEnumerator ExecuteInteraction()
    {
        if (!CanInteract)
            yield break;

        yield return base.ExecuteInteraction();
        yield return routeController.UseDoor(this);
    }
}
