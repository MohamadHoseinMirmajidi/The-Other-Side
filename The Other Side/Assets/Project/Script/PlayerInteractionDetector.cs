using System.Collections.Generic;
using UnityEngine;

public class PlayerInteractionDetector : MonoBehaviour
{
    private readonly HashSet<Interactable> nearbyInteractables =
        new HashSet<Interactable>();

    public IEnumerable<Interactable> NearbyInteractables =>
        nearbyInteractables;

    private void OnTriggerEnter2D(Collider2D other)
    {
        Interactable interactable =
            other.GetComponentInParent<Interactable>();

        if (interactable != null)
        {
            nearbyInteractables.Add(interactable);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Interactable interactable =
            other.GetComponentInParent<Interactable>();

        if (interactable != null)
        {
            nearbyInteractables.Remove(interactable);
        }
    }

    public bool IsNearby(Interactable interactable)
    {
        return interactable != null &&
               nearbyInteractables.Contains(interactable);
    }
}