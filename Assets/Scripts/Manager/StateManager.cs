using System;
using UnityEngine;

public static class StateManager
{
    public static event Action<GameState> StateChanged;

    private static GameObject homePanel;
    private static GameObject winPanel;
    private static GameObject losePanel;
    private static GameObject gameplayPanel;

    public static GameState CurrentState { get; private set; } = GameState.Home;

    public static bool Is(GameState state)
    {
        return CurrentState == state;
    }

    public static void SetState(GameState newState)
    {
        CurrentState = newState;
        ApplyStateVisuals(newState);
        StateChanged?.Invoke(newState);
    }

    public static void ConfigurePanels(GameObject home, GameObject win, GameObject lose, GameObject gameplay)
    {
        homePanel = home;
        winPanel = win;
        losePanel = lose;
        gameplayPanel = gameplay;
        ApplyStateVisuals(CurrentState);
    }

    public static void ClearPanels()
    {
        homePanel = null;
        winPanel = null;
        losePanel = null;
        gameplayPanel = null;
    }

    public static void ToHome()
    {
        SetState(GameState.Home);
    }

    public static void ToPlaying()
    {
        SetState(GameState.Playing);
    }

    public static void ToWin()
    {
        SetState(GameState.Win);
    }

    public static void ToLose()
    {
        SetState(GameState.Lose);
    }

    private static void ApplyStateVisuals(GameState state)
    {
        if (homePanel != null)
        {
            homePanel.SetActive(state == GameState.Home);
        }

        if (winPanel != null)
        {
            winPanel.SetActive(state == GameState.Win);
        }

        if (losePanel != null)
        {
            losePanel.SetActive(state == GameState.Lose);
        }

        if (gameplayPanel != null)
        {
            //gameplayPanel.SetActive(state != GameState.Home);
        }
    }
}