using System.Collections;
using UnityEngine;

public class StoryPushBoxInteractable : Interactable
{
    private StoryRouteController route;
    private Transform player;
    private float targetX;
    private float startX;
    private float playerOffset;
    private bool isPushing;
    private bool isPlaced;

    public void Initialize(
        StoryRouteController storyRoute,
        Transform playerTransform,
        float minimumX,
        float placementX)
    {
        route = storyRoute;
        player = playerTransform;
        startX = minimumX;
        targetX = placementX;
    }

    public override bool CanInteract =>
        base.CanInteract &&
        !isPlaced &&
        route != null &&
        route.CanUsePushBox;

    public override IEnumerator ExecuteInteraction()
    {
        if (!CanInteract)
            yield break;

        if (isPushing)
            StopPushing();
        else
            StartPushing();
    }

    private void Update()
    {
        if (!isPushing || player == null)
            return;

        Vector3 position = transform.position;
        position.x = Mathf.Clamp(player.position.x + playerOffset, startX, targetX);
        transform.position = position;

        if (position.x < targetX - 0.03f)
            return;

        isPlaced = true;
        isPushing = false;
        transform.position = new Vector3(targetX, position.y, position.z);
        route.NotifyBoxPlaced();
    }

    private void StartPushing()
    {
        Vector3 playerPosition = player.position;
        playerPosition.x = transform.position.x - 1f;
        player.position = playerPosition;
        playerOffset = transform.position.x - player.position.x;
        isPushing = true;
    }

    private void StopPushing()
    {
        isPushing = false;
    }
}
