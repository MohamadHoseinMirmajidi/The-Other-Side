using System.Collections;
using UnityEngine;

public class CartBrakeInteractable : Interactable
{
    [SerializeField] private CartController cart;
    [SerializeField] private GameProgress gameProgress;

    public override bool CanInteract =>
        base.CanInteract &&
        cart != null &&
        gameProgress != null &&
        gameProgress.CanComplete(GameProgressEvent.CartBrakeReleased);

    public override IEnumerator ExecuteInteraction()
    {
        if (!CanInteract)
            yield break;

        yield return base.ExecuteInteraction();

        if (gameProgress.TryComplete(GameProgressEvent.CartBrakeReleased))
        {
            cart.ReleaseBrake();
            SetAvailable(false);
        }
    }
}
