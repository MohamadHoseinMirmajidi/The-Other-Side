using System.Collections;
using TMPro;
using UnityEngine;

public class DialogueHUD : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private GameProgress gameProgress;
    [SerializeField] private GameObject dialogueBox;
    [SerializeField] private TMP_Text dialogueText;

    [Header("Settings")]
    [SerializeField] private float messageDuration = 3.5f;

    private Coroutine hideRoutine;

    private void Awake()
    {
        if (gameProgress == null)
            gameProgress = FindAnyObjectByType<GameProgress>();

        if (dialogueBox != null)
            dialogueBox.SetActive(false);
    }

    private void OnEnable()
    {
        if (gameProgress != null)
            gameProgress.ProgressChanged += HandleProgressChanged;
    }

    private void OnDisable()
    {
        if (gameProgress != null)
            gameProgress.ProgressChanged -= HandleProgressChanged;
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

    private void HandleProgressChanged(GameProgressEvent progressEvent)
    {
        switch (progressEvent)
        {
            case GameProgressEvent.ShadowSeen:
                ShowDialogue("این سایه ثابت است... اما چیزی جلوی نور نیست.");
                break;

            case GameProgressEvent.BoxPlaced:
                ShowDialogue("حالا می‌توانم روی جعبه بروم و به دستگیره برسم.");
                break;

            case GameProgressEvent.EnteredCartRoom:
                ShowDialogue("انگار یک در پشت این چرخ دستی پنهان شده.");
                break;

            case GameProgressEvent.CartMovedAway:
                ShowDialogue("در اینجا بود.");
                break;

            case GameProgressEvent.EnteredBagRoom:
                ShowDialogue("این همان پرده اصلی نمایش است...");
                break;

            case GameProgressEvent.BagCollected:
                ShowDialogue("این سایه روی پرده چرا شبیه من است؟");
                break;

            case GameProgressEvent.HallwayLightSelected:
                ShowDialogue("پس این سایه تمام مدت مال خودم بود.");
                break;

            case GameProgressEvent.StageDoorChecked:
                ShowDialogue("در صحنه بسته است. باید از همان راه برگردم.");
                break;

            case GameProgressEvent.ReturnedToFirstRoom:
                ShowDialogue("مسیر حیاط حالا باز است.");
                break;
        }
    }

    private IEnumerator HideAfterDelay()
    {
        yield return new WaitForSeconds(messageDuration);
        dialogueBox.SetActive(false);
        hideRoutine = null;
    }
}
