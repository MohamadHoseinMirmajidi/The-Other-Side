using System.Collections;
using UnityEngine;

public class BagPickupInteractable : Interactable
{
    [Header("Bag Pickup")]
    [SerializeField] private GameProgress gameProgress;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Transform playerVisual;
    [SerializeField] private GameObject carriedBag;
    [SerializeField] private GameObject worldBag;
    [SerializeField] private float pickupPoseDuration = 0.25f;
    [SerializeField] private Vector2 pickupPoseOffset = new Vector2(0f, -0.08f);

    public override bool CanInteract =>
        base.CanInteract &&
        gameProgress != null &&
        gameProgress.CanComplete(GameProgressEvent.BagCollected);

    private void Awake()
    {
        if (gameProgress == null)
            gameProgress = FindAnyObjectByType<GameProgress>();

        if (playerMovement == null)
            playerMovement = FindAnyObjectByType<PlayerMovement>();

        if (carriedBag != null)
            carriedBag.SetActive(false);
    }

    public override IEnumerator ExecuteInteraction()
    {
        if (!CanInteract)
            yield break;

        SetAvailable(false);

        if (playerMovement != null)
        {
            playerMovement.ResetMotion();
            playerMovement.enabled = false;
        }

        Vector3 startingPosition = Vector3.zero;
        if (playerVisual != null)
        {
            startingPosition = playerVisual.localPosition;
            playerVisual.localPosition += (Vector3)pickupPoseOffset;
        }

        yield return base.ExecuteInteraction();

        if (pickupPoseDuration > 0f)
            yield return new WaitForSeconds(pickupPoseDuration);

        bool collected = gameProgress.TryComplete(GameProgressEvent.BagCollected);

        if (playerVisual != null)
            playerVisual.localPosition = startingPosition;

        if (playerMovement != null)
            playerMovement.enabled = true;

        if (!collected)
        {
            SetAvailable(true);
            yield break;
        }

        if (carriedBag != null)
            carriedBag.SetActive(true);

        if (worldBag != null)
            worldBag.SetActive(false);
    }
}
