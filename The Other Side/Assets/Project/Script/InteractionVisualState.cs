using UnityEngine;

public class InteractionVisualState : MonoBehaviour
{
    [Header("Scene Visuals")]
    [SerializeField] private GameObject beforeInteractionVisual;
    [SerializeField] private GameObject afterInteractionVisual;

    [SerializeField] private bool showAfterInteraction;

    private void Awake()
    {
        ApplyState();
    }

    public void Configure(GameObject beforeVisual, GameObject afterVisual)
    {
        beforeInteractionVisual = beforeVisual;
        afterInteractionVisual = afterVisual;
        ApplyState();
    }

    public void ShowBeforeInteraction()
    {
        showAfterInteraction = false;
        ApplyState();
    }

    public void ShowAfterInteraction()
    {
        showAfterInteraction = true;
        ApplyState();
    }

    private void ApplyState()
    {
        if (beforeInteractionVisual != null)
            beforeInteractionVisual.SetActive(!showAfterInteraction);

        if (afterInteractionVisual != null)
            afterInteractionVisual.SetActive(showAfterInteraction);
    }
}
