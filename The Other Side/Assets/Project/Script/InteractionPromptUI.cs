using TMPro;
using UnityEngine;

public class InteractionPromptUI : MonoBehaviour
{
    [Header("Prompt References")]
    [SerializeField] private GameObject promptRoot;
    [SerializeField] private TextMeshProUGUI promptText;

    [Header("Position")]
    [SerializeField] private Vector3 worldOffset =
        new Vector3(0f, 1f, 0f);

    private Interactable currentTarget;

    private void Awake()
    {
        if (promptRoot == null)
            promptRoot = gameObject;

        if (promptText != null)
            promptText.text = "E";

        Hide();
    }

    private void LateUpdate()
    {
        if (currentTarget == null || !currentTarget.CanInteract)
        {
            Hide();
            return;
        }

        transform.position =
            currentTarget.PromptAnchor.position + worldOffset;
    }

    public void Show(Interactable target)
    {
        if (target == null)
        {
            Hide();
            return;
        }

        currentTarget = target;

        if (promptText != null)
            promptText.text = "E";

        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        if (promptRoot != null && !promptRoot.activeSelf)
            promptRoot.SetActive(true);

        transform.position =
            target.PromptAnchor.position + worldOffset;
    }

    public void Hide()
    {
        currentTarget = null;

        if (promptRoot != null && promptRoot != gameObject)
            promptRoot.SetActive(false);
        else
            gameObject.SetActive(false);
    }
}