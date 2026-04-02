using TMPro;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class GamePlayManager : MonoBehaviour
{
    public static GamePlayManager Instance { get; private set; }

    [SerializeField] private Button resetButton;
    [SerializeField] private MapController mapController;
    [SerializeField] private TextMeshProUGUI timeText;
    private float currentTime;
    private float timeRemaining;
    private bool isGameRunning;
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

        if (resetButton != null)
        {
            resetButton.onClick.AddListener(OnResetButtonClicked);
        }
    }

    private void OnDestroy()
    {
        if (resetButton != null)
        {
            resetButton.onClick.RemoveListener(OnResetButtonClicked);
        }

        if (Instance == this)
        {
            Instance = null;
        }
    }
    private void Start()
    {
    }

    private void Update()
    {
        if (!isGameRunning)
            return;

        timeRemaining -= Time.deltaTime;
        
        if (timeRemaining < 0)
        {
            timeRemaining = 0;
            isGameRunning = false;
            OnTimeOver();
        }

        UpdateTimeDisplayValue();
    }

    public void OnResetButtonClicked()
    {
        if (mapController != null)
        {
            if (mapController.Reset())
            {
                UpdateTimeDisplay();
                isGameRunning = true;
            }
        }
    }

    public void InitMap()
    {
        if (mapController != null && mapController.InitMap(currentLevel))
        {
            UpdateTimeDisplay();
            isGameRunning = true;
        }
    }

    public void UpdateTimeDisplay()
    {
        if (mapController == null)
            return;

        LevelData levelData = mapController.GetCurrentLevelData();
        if (levelData != null)
        {
            currentTime = levelData.timeLimit;
            timeRemaining = currentTime;
            UpdateTimeDisplayValue();
        }
    }

    public void UpdateTimeDisplayValue()
    {
        if (timeText != null)
        {
            timeText.text = $"Time: {timeRemaining:F0}s";
        }
    }


    public void OnTimeOver()
    {
        Debug.Log("Hết thời gian!");
    }

    public void AdvanceToNextLevel()
    {
        int nextLevel = currentLevel + 1;
        int maxLevel = GetMaxAvailableLevel();

        if (nextLevel > maxLevel)
        {
            Debug.Log("No more levels available.");
            return;
        }

        if (mapController != null && mapController.InitMap(nextLevel))
        {
            currentLevel = nextLevel;
            SaveCurrentProgress();
            UpdateTimeDisplay();
            isGameRunning = true;
        }
    }

    public int GetCurrentLevel()
    {
        return currentLevel;    
    }

    public void SetCurrentLevel(int level)
    {
        currentLevel = Mathf.Max(1, Mathf.Min(level, GetMaxAvailableLevel()));
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
}
