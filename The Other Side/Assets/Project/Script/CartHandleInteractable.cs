using System.Collections;
using UnityEngine;

public class CartHandleInteractable : Interactable
{
    [SerializeField] private CartController cart;

    public override bool CanInteract =>
        base.CanInteract &&
        cart != null &&
        !cart.IsAttached &&
        !cart.IsParked;

    public override IEnumerator ExecuteInteraction()
    {
        if (!CanInteract)
            yield break;

        yield return base.ExecuteInteraction();

        if (!cart.IsBrakeReleased)
        {
            cart.PlayLockedFeedback();
            yield break;
        }

        cart.StartPushPull();
    }
}
