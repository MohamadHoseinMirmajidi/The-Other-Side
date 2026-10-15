using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public enum GameProgressEvent
{
    EnteredBackstage,
    ShadowSeen,
    CartBrakeReleased,
    CartMovedAway,
    BagCollected,
    HallwayLightSelected,
    CurtainRetracted,
    LampMoved,
    RevealCompleted,
    ExitOpened,
    FirstDoorOpened,
    BoxPlaced,
    HighDoorOpened,
    EnteredCartRoom,
    CartDoorOpened,
    EnteredBagRoom,
    StageDoorChecked,
    ReturnedToFirstRoom,
    EnteredCourtyard
}

public class GameProgress : MonoBehaviour
{
    [Header("Pause")]
    [SerializeField] private GameObject pausePanel;
    [SerializeField] private PlayerMovement playerMovement;

    private readonly HashSet<GameProgressEvent> completedEvents =
        new HashSet<GameProgressEvent>();

    private bool isPaused;

    public event Action<GameProgressEvent> ProgressChanged;

    private void Awake()
    {
        if (playerMovement == null)
            playerMovement = FindAnyObjectByType<PlayerMovement>();

        if (pausePanel == null)
            pausePanel = FindPausePanel();

        SetPausePanel(false);
        Time.timeScale = 1f;
        AudioListener.pause = false;
    }

    public bool HasCompleted(GameProgressEvent progressEvent)
    {
        return completedEvents.Contains(progressEvent);
    }

    public bool CanComplete(GameProgressEvent progressEvent)
    {
        if (HasCompleted(progressEvent))
            return false;

        switch (progressEvent)
        {
            case GameProgressEvent.FirstDoorOpened:
                return true;
            case GameProgressEvent.EnteredBackstage:
                return HasCompleted(GameProgressEvent.FirstDoorOpened);
            case GameProgressEvent.ShadowSeen:
                return HasCompleted(GameProgressEvent.EnteredBackstage);
            case GameProgressEvent.BoxPlaced:
                return HasCompleted(GameProgressEvent.ShadowSeen);
            case GameProgressEvent.HighDoorOpened:
                return HasCompleted(GameProgressEvent.BoxPlaced);
            case GameProgressEvent.EnteredCartRoom:
                return HasCompleted(GameProgressEvent.HighDoorOpened);
            case GameProgressEvent.CartBrakeReleased:
                return HasCompleted(GameProgressEvent.EnteredCartRoom);
            case GameProgressEvent.CartMovedAway:
                return HasCompleted(GameProgressEvent.CartBrakeReleased);
            case GameProgressEvent.CartDoorOpened:
                return HasCompleted(GameProgressEvent.CartMovedAway);
            case GameProgressEvent.EnteredBagRoom:
                return HasCompleted(GameProgressEvent.CartDoorOpened);
            case GameProgressEvent.BagCollected:
                return HasCompleted(GameProgressEvent.EnteredBagRoom);
            case GameProgressEvent.HallwayLightSelected:
                return HasCompleted(GameProgressEvent.BagCollected);
            case GameProgressEvent.StageDoorChecked:
                return HasCompleted(GameProgressEvent.HallwayLightSelected);
            case GameProgressEvent.ReturnedToFirstRoom:
                return HasCompleted(GameProgressEvent.StageDoorChecked);
            case GameProgressEvent.EnteredCourtyard:
                return HasCompleted(GameProgressEvent.ReturnedToFirstRoom);
            case GameProgressEvent.ExitOpened:
                return HasCompleted(GameProgressEvent.EnteredCourtyard);
            default:
                return false;
        }
    }

    public bool TryComplete(GameProgressEvent progressEvent)
    {
        if (!CanComplete(progressEvent))
            return false;

        completedEvents.Add(progressEvent);
        ProgressChanged?.Invoke(progressEvent);
        Debug.Log($"Progress completed: {progressEvent}", this);

        if (progressEvent == GameProgressEvent.ExitOpened)
            Debug.Log("Gameplay flow completed.", this);

        return true;
    }

    public void RestartGame()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }

        if (Keyboard.current.rKey.wasPressedThisFrame)
            RestartGame();
    }

    public void PauseGame()
    {
        if (isPaused)
            return;

        isPaused = true;
        SetPlayerControl(false);
        SetPausePanel(true);
        Time.timeScale = 0f;
        AudioListener.pause = true;
    }

    public void ResumeGame()
    {
        if (!isPaused)
            return;

        isPaused = false;
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SetPausePanel(false);
        SetPlayerControl(true);
    }

    private void SetPlayerControl(bool enabled)
    {
        if (playerMovement == null)
            return;

        playerMovement.ResetMotion();
        playerMovement.enabled = enabled;
    }

    private void SetPausePanel(bool active)
    {
        if (pausePanel != null)
            pausePanel.SetActive(active);
    }

    private static GameObject FindPausePanel()
    {
        RectTransform[] panels = FindObjectsByType<RectTransform>(
            FindObjectsInactive.Include);

        foreach (RectTransform panel in panels)
        {
            if (panel.name == "Pause Panel")
                return panel.gameObject;
        }

        return null;
    }
}
