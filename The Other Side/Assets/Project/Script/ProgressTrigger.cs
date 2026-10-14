using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class ProgressTrigger : MonoBehaviour
{
    [SerializeField] private GameProgress gameProgress;
    [SerializeField] private GameProgressEvent progressEvent;

    private void Awake()
    {
        if (gameProgress == null)
            gameProgress = FindAnyObjectByType<GameProgress>();

        GetComponent<Collider2D>().isTrigger = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponentInParent<PlayerMovement>() == null)
            return;

        gameProgress?.TryComplete(progressEvent);
    }
}
