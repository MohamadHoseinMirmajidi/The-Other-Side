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
    private static bool startGameplayAfterReload;

    private readonly HashSet<GameProgressEvent> completedEvents =
        new HashSet<GameProgressEvent>();

    public event Action<GameProgressEvent> ProgressChanged;

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
        startGameplayAfterReload = true;
        ReloadCurrentScene();
    }

    public void ReturnToMainMenu()
    {
        startGameplayAfterReload = false;
        ReloadCurrentScene();
    }

    public static bool ConsumeStartGameplayAfterReload()
    {
        bool shouldStartGameplay = startGameplayAfterReload;
        startGameplayAfterReload = false;
        return shouldStartGameplay;
    }

    private static void ReloadCurrentScene()
    {
        Time.timeScale = 1f;
        AudioListener.pause = false;
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        if (Keyboard.current.rKey.wasPressedThisFrame)
            RestartGame();
    }
}
