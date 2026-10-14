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
    ExitOpened
}

public class GameProgress : MonoBehaviour
{
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
            case GameProgressEvent.EnteredBackstage:
                return true;

            case GameProgressEvent.ShadowSeen:
                return HasCompleted(GameProgressEvent.EnteredBackstage);

            case GameProgressEvent.CartBrakeReleased:
                return HasCompleted(GameProgressEvent.ShadowSeen);

            case GameProgressEvent.CartMovedAway:
                return HasCompleted(GameProgressEvent.CartBrakeReleased);

            case GameProgressEvent.BagCollected:
            case GameProgressEvent.HallwayLightSelected:
                return HasCompleted(GameProgressEvent.CartMovedAway);

            case GameProgressEvent.CurtainRetracted:
                return HasCompleted(GameProgressEvent.HallwayLightSelected);

            case GameProgressEvent.LampMoved:
                return HasCompleted(GameProgressEvent.CurtainRetracted);

            case GameProgressEvent.RevealCompleted:
                return HasCompleted(GameProgressEvent.LampMoved);

            case GameProgressEvent.ExitOpened:
                return HasCompleted(GameProgressEvent.BagCollected) &&
                       HasCompleted(GameProgressEvent.RevealCompleted);

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
            Debug.Log("Placeholder flow completed. Credits can start here.", this);

        return true;
    }

    public void RestartGame()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.rKey.wasPressedThisFrame)
            RestartGame();
    }
}
