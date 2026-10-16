using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public enum GameProgressEvent
{
    TutorialItemRead,
    EnteredKeyRoom,
    BoxPlaced,
    KeyCollected,
    EnteredFuseRoom,
    FusePartCollected,
    FuseRepaired,
    EnteredBackstage,
    WeightCollected,
    EnteredUpperBackstage,
    CurtainOpened,
    DecorationRevealed,
    BagCollected,
    ReturnedToFuseRoom,
    ReturnedToKeyRoom,
    ReturnedToFirstRoom,
    EnteredCourtyard,
    GameFinished,

    // Kept so old scene components can deserialize without losing their data.
    ShadowSeen,
    CartBrakeReleased,
    CartMovedAway,
    HallwayLightSelected,
    CurtainRetracted,
    LampMoved,
    RevealCompleted,
    ExitOpened,
    FirstDoorOpened,
    HighDoorOpened,
    EnteredCartRoom,
    CartDoorOpened,
    EnteredBagRoom,
    StageDoorChecked
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
            case GameProgressEvent.TutorialItemRead:
                return true;
            case GameProgressEvent.EnteredKeyRoom:
                return HasCompleted(GameProgressEvent.TutorialItemRead);
            case GameProgressEvent.BoxPlaced:
                return HasCompleted(GameProgressEvent.EnteredKeyRoom);
            case GameProgressEvent.KeyCollected:
                return HasCompleted(GameProgressEvent.BoxPlaced);
            case GameProgressEvent.EnteredFuseRoom:
                return HasCompleted(GameProgressEvent.KeyCollected);
            case GameProgressEvent.FusePartCollected:
                return HasCompleted(GameProgressEvent.EnteredFuseRoom);
            case GameProgressEvent.FuseRepaired:
                return HasCompleted(GameProgressEvent.FusePartCollected);
            case GameProgressEvent.EnteredBackstage:
                return HasCompleted(GameProgressEvent.FuseRepaired);
            case GameProgressEvent.WeightCollected:
                return HasCompleted(GameProgressEvent.EnteredBackstage);
            case GameProgressEvent.EnteredUpperBackstage:
                return HasCompleted(GameProgressEvent.WeightCollected);
            case GameProgressEvent.CurtainOpened:
                return HasCompleted(GameProgressEvent.EnteredUpperBackstage);
            case GameProgressEvent.DecorationRevealed:
                return HasCompleted(GameProgressEvent.CurtainOpened);
            case GameProgressEvent.BagCollected:
                return HasCompleted(GameProgressEvent.DecorationRevealed);
            case GameProgressEvent.ReturnedToFuseRoom:
                return HasCompleted(GameProgressEvent.BagCollected);
            case GameProgressEvent.ReturnedToKeyRoom:
                return HasCompleted(GameProgressEvent.ReturnedToFuseRoom);
            case GameProgressEvent.ReturnedToFirstRoom:
                return HasCompleted(GameProgressEvent.ReturnedToKeyRoom);
            case GameProgressEvent.EnteredCourtyard:
                return HasCompleted(GameProgressEvent.ReturnedToFirstRoom);
            case GameProgressEvent.GameFinished:
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

}
