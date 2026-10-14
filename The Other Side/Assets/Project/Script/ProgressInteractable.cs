using System.Collections;
using UnityEngine;

public class ProgressInteractable : Interactable
{
    [Header("Game Progress")]
    [SerializeField] private GameProgress gameProgress;
    [SerializeField] private GameProgressEvent progressEvent;

    [Header("Placeholder Result")]
    [SerializeField] private GameObject[] hideOnComplete = new GameObject[0];

    public override bool CanInteract =>
        base.CanInteract &&
        gameProgress != null &&
        gameProgress.CanComplete(progressEvent);

    private void Awake()
    {
        if (gameProgress == null)
            gameProgress = FindAnyObjectByType<GameProgress>();
    }

    public override IEnumerator ExecuteInteraction()
    {
        if (!CanInteract)
            yield break;

        yield return base.ExecuteInteraction();

        if (!gameProgress.TryComplete(progressEvent))
            yield break;

        SetAvailable(false);

        foreach (GameObject target in hideOnComplete)
        {
            if (target != null)
                target.SetActive(false);
        }
    }
}
