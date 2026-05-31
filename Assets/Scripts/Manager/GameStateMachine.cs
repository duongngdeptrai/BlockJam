using System;
using UnityEngine;

public static class GameStateMachine
{
    public static event Action<GameState> StateChanged;

    public static GameState CurrentState { get; private set; } = GameState.Home;

    public static bool Is(GameState state)
    {
        return CurrentState == state;
    }

    public static void SetState(GameState newState)
    {
        CurrentState = newState;
        StateChanged?.Invoke(newState);
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
}
