using System.Collections;
using TMPro;
using UnityEngine;

public class DialogueHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private TMP_Text dialogueText;

    [Header("Settings")]
    [SerializeField] private float messageDuration = 3.5f;

    private Coroutine hideRoutine;

    public float MessageDuration => messageDuration;

    private void Awake()
    {
        if (dialogueBox != null)
            dialogueBox.SetActive(false);
    }

    public void ShowDialogue(string message)
    {
        if (dialogueBox == null || dialogueText == null)
            return;

        if (hideRoutine != null)
            StopCoroutine(hideRoutine);

        PersianDisplayText.Apply(dialogueText, message);
        dialogueBox.SetActive(true);
        hideRoutine = StartCoroutine(HideAfterDelay());
    }

    public void HideDialogue()
    {
        if (hideRoutine != null)
            StopCoroutine(hideRoutine);

        hideRoutine = null;
        if (dialogueBox != null)
            dialogueBox.SetActive(false);
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(messageDuration);
        dialogueBox.SetActive(false);
        hideRoutine = null;
    }
}
