using System.Collections;
using UnityEngine;

public class StoryPushBoxInteractable : Interactable
{
    [SerializeField] private StoryRouteController route;
    [SerializeField] private Transform player;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private PlayerAnimator playerAnimator;
    [Header("Level Design")]
    [SerializeField] private Transform placementPoint;
    [SerializeField] private Transform pushStartPoint;
    [SerializeField] private float playerDistanceFromBox = 1f;
    [SerializeField] private float alignSpeed = 3.5f;
    [SerializeField] private float placementTolerance = 0.03f;

    private Vector3 startPosition;
    private float playerOffset;
    private float pushDirection;
    private bool isPushing;
    private bool isPlaced;
    private float furthestBoxX;

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
        furthestBoxX = startPosition.x;
        if (playerMovement == null && player != null)
            playerMovement = player.GetComponent<PlayerMovement>();
        if (playerAnimator == null && player != null)
            playerAnimator = player.GetComponentInChildren<PlayerAnimator>(true);
    }

    public override bool CanInteract =>
        base.CanInteract &&
        route != null &&
        (isPlaced ? route.CanClimbPlacedBox : route.CanUsePushBox);

    public override IEnumerator ExecuteInteraction()
    {
        if (!CanInteract)
            yield break;

        if (isPlaced)
        {
            yield return route.ClimbPlacedBox();
            yield break;
        }

        if (isPushing)
            StopPushing();
        else
            yield return StartPushing();
    }

    private void Update()
    {
        if (!isPushing || player == null || placementPoint == null)
            return;

        Vector3 position = transform.position;
        float targetX = placementPoint.position.x;
        float minimumX = Mathf.Min(startPosition.x, targetX);
        float maximumX = Mathf.Max(startPosition.x, targetX);
        float requestedX = Mathf.Clamp(
            player.position.x + playerOffset,
            minimumX,
            maximumX);
        furthestBoxX = Mathf.Max(furthestBoxX, requestedX);
        position.x = furthestBoxX;
        transform.position = position;

        bool reachedTarget = pushDirection > 0f
            ? position.x >= targetX - placementTolerance
            : position.x <= targetX + placementTolerance;

        if (!reachedTarget)
            return;

        isPlaced = true;
        isPushing = false;
        playerMovement?.SetRightOnlyMovement(false);
        playerAnimator?.ClearPersistentState();
        GameAudioManager.Instance?.StopBoxPush();
        transform.position = new Vector3(
            placementPoint.position.x,
            startPosition.y,
            position.z);
        route.NotifyBoxPlaced();
    }

    private IEnumerator StartPushing()
    {
        if (placementPoint == null)
            yield break;

        if (playerMovement == null && player != null)
            playerMovement = player.GetComponent<PlayerMovement>();
        if (playerAnimator == null && player != null)
            playerAnimator = player.GetComponentInChildren<PlayerAnimator>(true);

        pushDirection = 1f;
        if (placementPoint.position.x <= transform.position.x + placementTolerance)
        {
            isPlaced = true;
            route.NotifyBoxPlaced();
            yield break;
        }

        Vector3 targetPosition = pushStartPoint != null
            ? pushStartPoint.position
            : new Vector3(
                transform.position.x - playerDistanceFromBox,
                player.position.y,
                player.position.z);
        targetPosition.z = player.position.z;
        bool restoreMovement = playerMovement != null && playerMovement.enabled;
        if (playerMovement != null)
            playerMovement.enabled = false;
        while (Vector3.Distance(player.position, targetPosition) > 0.03f)
        {
            player.position = Vector3.MoveTowards(
                player.position,
                targetPosition,
                alignSpeed * Time.deltaTime);
            yield return null;
        }

        player.position = targetPosition;
        if (playerMovement != null)
            playerMovement.enabled = restoreMovement;
        playerMovement?.ResetMotion();
        playerMovement?.SetRightOnlyMovement(true);
        playerMovement?.FaceDirection(1f);
        playerOffset = transform.position.x - player.position.x;
        furthestBoxX = Mathf.Max(furthestBoxX, transform.position.x);
        isPushing = true;
        playerAnimator?.SetPersistentState("Pushing Animation");
        GameAudioManager.Instance?.StartBoxPush();
    }

    private void StopPushing()
    {
        isPushing = false;
        playerMovement?.SetRightOnlyMovement(false);
        playerAnimator?.ClearPersistentState();
        GameAudioManager.Instance?.StopBoxPush();
    }

    private void OnDisable()
    {
        if (isPushing)
            StopPushing();
    }
}
