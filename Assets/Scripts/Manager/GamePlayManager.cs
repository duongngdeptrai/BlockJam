using System.IO;
using UnityEngine;

public class GamePlayManager : MonoBehaviour
{
    public static GamePlayManager Instance { get; private set; }

    [SerializeField] private MapController mapController;
    [SerializeField] private GamePlayUI gamePlayUI;
    [Header("State UI")]
    [SerializeField] private GameObject homePanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;
    [SerializeField] private GameObject gameplayPanel;

    private int currentLevel = 1;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        LoadSavedProgress();
        StateManager.ConfigurePanels(homePanel, winPanel, losePanel, gameplayPanel);
    }
    private void OnEnable()
    {
        StateManager.ToHome();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            StateManager.ClearPanels();
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }
    public void ResetLevel()
    {
        if (mapController == null)
        {
            return;
        }

        mapController.ReloadCurrentLevel();
        StartLevelTimer();
        StateManager.ToPlaying();
    }

    public void StartPlaying()
    {
        if (mapController == null)
        {
            return;
        }

        mapController.InitMap(currentLevel);
        StartLevelTimer();
        StateManager.ToPlaying();
    }

    public void AdvanceToNextLevel()
    {
        int nextLevel = currentLevel + 1;
        int maxLevel = GetMaxAvailableLevel();

        if (nextLevel > maxLevel)
        {
            return;
        }

        if (mapController == null)
        {
            return;
        }

        mapController.InitMap(nextLevel);
        currentLevel = nextLevel;
        SaveCurrentProgress();
        StartLevelTimer();
        StateManager.ToPlaying();
    }

    public int GetCurrentLevel()
    {
        return currentLevel;    
    }

    private void LoadSavedProgress()
    {
        PlayerProgressData progress = SaveManager.LoadProgress();
        currentLevel = Mathf.Max(1, Mathf.Min(progress.currentLevel, GetMaxAvailableLevel()));
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
        if (mapController == null || gamePlayUI == null)
        {
            return;
        }

        LevelData levelData = mapController.GetCurrentLevelData();
        if (levelData != null)
        {
            gamePlayUI.StartTimer(levelData.timeLimit);
        }
    }

}
