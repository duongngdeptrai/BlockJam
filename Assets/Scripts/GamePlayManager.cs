using TMPro;
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
            mapController.Reset();
            UpdateTimeDisplay();
            isGameRunning = true;
        }
    }

    public void InitMap()
    {
        UpdateTimeDisplay();
        isGameRunning = true;
        mapController.InitMap();
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

    public int GetCurrentLevel()
    {
        return currentLevel;    
    }

    public void SetCurrentLevel(int level)
    {
        currentLevel = level;
    }
}
