using System.Collections;
using UnityEngine;

public class StoryPushBoxInteractable : Interactable
{
    [SerializeField] private StoryRouteController route;
    [SerializeField] private Transform player;
    [Header("Level Design")]
    [SerializeField] private Transform placementPoint;
    [SerializeField] private float playerDistanceFromBox = 1f;
    [SerializeField] private float placementTolerance = 0.03f;

    private Vector3 startPosition;
    private float playerOffset;
    private float pushDirection;
    private bool isPushing;
    private bool isPlaced;

    public void Configure(
        StoryRouteController storyRoute,
        Transform playerTransform,
        Transform targetPoint)
    {
        route = storyRoute;
        player = playerTransform;
        placementPoint = targetPoint;
    }

    private void Awake()
    {
        startPosition = transform.position;
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
        if (!isPushing || player == null || placementPoint == null)
            return;

        Vector3 position = transform.position;
        float targetX = placementPoint.position.x;
        float minimumX = Mathf.Min(startPosition.x, targetX);
        float maximumX = Mathf.Max(startPosition.x, targetX);
        position.x = Mathf.Clamp(player.position.x + playerOffset, minimumX, maximumX);
        transform.position = position;

        bool reachedTarget = pushDirection > 0f
            ? position.x >= targetX - placementTolerance
            : position.x <= targetX + placementTolerance;

        if (!reachedTarget)
            return;

        isPlaced = true;
        isPushing = false;
        transform.position = new Vector3(
            placementPoint.position.x,
            placementPoint.position.y,
            position.z);
        route.NotifyBoxPlaced();
    }

    private void StartPushing()
    {
        if (placementPoint == null)
            return;

        pushDirection = Mathf.Sign(placementPoint.position.x - transform.position.x);
        if (Mathf.Approximately(pushDirection, 0f))
        {
            isPlaced = true;
            route.NotifyBoxPlaced();
            return;
        }

        Vector3 playerPosition = player.position;
        playerPosition.x = transform.position.x - pushDirection * playerDistanceFromBox;
        playerPosition.y = transform.position.y;
        player.position = playerPosition;
        playerOffset = transform.position.x - player.position.x;
        isPushing = true;
    }

    private void StopPushing()
    {
        isPushing = false;
    }
}
