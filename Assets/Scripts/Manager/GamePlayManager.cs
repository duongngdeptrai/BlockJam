using System.IO;
using UnityEngine;

public class GamePlayManager : Singleton<GamePlayManager>
{
    [SerializeField] public MapController mapController;
    [SerializeField] public GamePlayUI gamePlayUI;

    private int currentLevel = 1;

    private void Awake()
    {
        base.Awake();
        if (Instance != null && Instance != this) return;
        LoadSavedProgress();
    }

    private void OnEnable()
    {
        GameStateMachine.ToHome();
    }


    public void ResetLevel()
    {
        StartPlaying();
    }

    public void StartPlaying()
    {
        if (mapController == null) return;
        GameStateMachine.ToPlaying();
        mapController.InitMap(currentLevel);
        StartLevelTimer();
    }

    public void AdvanceToNextLevel()
    {
        int nextLevel = currentLevel + 1;
        int maxLevel = GetMaxAvailableLevel();
        if (nextLevel > maxLevel) return;
        if (mapController == null) return;
        currentLevel = nextLevel;
        SaveCurrentProgress();
    }

    public int GetCurrentLevel() => currentLevel;

    private void LoadSavedProgress()
    {
        PlayerProgressData progress = SaveManager.LoadProgress();
        currentLevel = Mathf.Max(GameConstants.MIN_LEVEL, Mathf.Min(progress.currentLevel, GetMaxAvailableLevel()));
    }

    private void SaveCurrentProgress()
    {
        int currentLives = PlayerLivesManager.Instance != null ? PlayerLivesManager.Instance.GetCurrentLives() : 3;
        SaveManager.SaveProgress(currentLevel, currentLives);
    }

    private int GetMaxAvailableLevel()
    {
        int level = 1;
        while (File.Exists(Path.Combine(Application.streamingAssetsPath, "Levels", $"level{level + 1}.json")))
        {
            level++;
        }
        return level;
    }

    private void StartLevelTimer()
    {
        if (mapController == null || gamePlayUI == null) return;
        LevelData levelData = mapController.GetCurrentLevelData();
        if (levelData != null)
        {
            gamePlayUI.StartTimer(levelData.timeLimit);
        }
    }
}
