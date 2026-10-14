using TMPro;
using UnityEngine;

public class GreyboxFlowController : MonoBehaviour
{
    [SerializeField] private GameProgress gameProgress;
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private GameObject startPanel;
    [SerializeField] private GameObject creditsPanel;
    [SerializeField] private TMP_Text objectiveText;
    [SerializeField] private AudioSource progressAudio;

    private AudioClip progressClip;

    private void Awake()
    {
        if (gameProgress == null)
            gameProgress = FindAnyObjectByType<GameProgress>();

        if (playerMovement == null)
            playerMovement = FindAnyObjectByType<PlayerMovement>();

        startPanel.SetActive(true);
        creditsPanel.SetActive(false);
        objectiveText.gameObject.SetActive(false);
        SetPlayerControl(false);
        progressClip = CreateProgressClip();
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

    public void StartGame()
    {
        startPanel.SetActive(false);
        objectiveText.gameObject.SetActive(true);
        SetPlayerControl(true);
        UpdateObjective();
    }

    public void RestartGame()
    {
        gameProgress.RestartGame();
    }

    private void HandleProgressChanged(GameProgressEvent progressEvent)
    {
        if (progressAudio != null && progressClip != null)
            progressAudio.PlayOneShot(progressClip);

        if (progressEvent == GameProgressEvent.ExitOpened)
        {
            objectiveText.gameObject.SetActive(false);
            creditsPanel.SetActive(true);
            SetPlayerControl(false);
            return;
        }

        UpdateObjective();
    }

    private void UpdateObjective()
    {
        if (!gameProgress.HasCompleted(GameProgressEvent.FirstDoorOpened))
            objectiveText.text = "Open the first door";
        else if (!gameProgress.HasCompleted(GameProgressEvent.EnteredBackstage))
            objectiveText.text = "Press E again to enter";
        else if (!gameProgress.HasCompleted(GameProgressEvent.ShadowSeen))
            objectiveText.text = "Look at the unexplained shadow";
        else if (!gameProgress.HasCompleted(GameProgressEvent.BoxPlaced))
            objectiveText.text = "Push the box under the high door handle";
        else if (!gameProgress.HasCompleted(GameProgressEvent.HighDoorOpened))
            objectiveText.text = "Use the box to reach the handle";
        else if (!gameProgress.HasCompleted(GameProgressEvent.EnteredCartRoom))
            objectiveText.text = "Press E again to enter";
        else if (!gameProgress.HasCompleted(GameProgressEvent.CartBrakeReleased))
            objectiveText.text = "A door is hidden behind the cart";
        else if (!gameProgress.HasCompleted(GameProgressEvent.CartMovedAway))
            objectiveText.text = "Move the cart away from the door";
        else if (!gameProgress.HasCompleted(GameProgressEvent.CartDoorOpened))
            objectiveText.text = "Open the door behind the cart";
        else if (!gameProgress.HasCompleted(GameProgressEvent.EnteredBagRoom))
            objectiveText.text = "Press E again to enter";
        else if (!gameProgress.HasCompleted(GameProgressEvent.BagCollected))
            objectiveText.text = "Collect the bag";
        else if (!gameProgress.HasCompleted(GameProgressEvent.HallwayLightSelected))
            objectiveText.text = "Turn on the light beside the curtain";
        else if (!gameProgress.HasCompleted(GameProgressEvent.StageDoorChecked))
            objectiveText.text = "Check the door leading to the stage";
        else if (!gameProgress.HasCompleted(GameProgressEvent.ReturnedToFirstRoom))
            objectiveText.text = "Return through the rooms";
        else if (!gameProgress.HasCompleted(GameProgressEvent.EnteredCourtyard))
            objectiveText.text = "Choose the courtyard door";
        else
            objectiveText.text = "Press E again to leave";
    }

    private void SetPlayerControl(bool enabled)
    {
        if (playerMovement == null)
            return;

        playerMovement.ResetMotion();
        playerMovement.enabled = enabled;
    }

    private static AudioClip CreateProgressClip()
    {
        const int sampleRate = 44100;
        const float duration = 0.08f;
        int sampleCount = Mathf.RoundToInt(sampleRate * duration);
        float[] samples = new float[sampleCount];

        for (int index = 0; index < sampleCount; index++)
        {
            float time = index / (float)sampleRate;
            float fade = 1f - index / (float)sampleCount;
            samples[index] = Mathf.Sin(2f * Mathf.PI * 660f * time) * 0.08f * fade;
        }

        AudioClip clip = AudioClip.Create("Greybox Progress", sampleCount, 1, sampleRate, false);
        clip.SetData(samples, 0);
        return clip;
    }
}
