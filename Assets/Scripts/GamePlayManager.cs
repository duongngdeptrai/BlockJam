using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GamePlayManager : MonoBehaviour
{
    [SerializeField] private Button resetButton;
    [SerializeField] private MapController mapController;
    [SerializeField] private TextMeshProUGUI timeText;
    private float currentTime;
    private float timeRemaining;
    private bool isGameRunning;

    private void Awake()
    {
        if (resetButton != null)
        {
            resetButton.onClick.AddListener(OnResetButtonClicked);
        }
    }

    private void Start()
    {
        UpdateTimeDisplay();
        isGameRunning = true;
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

    private void OnResetButtonClicked()
    {
        if (mapController != null)
        {
            mapController.Reset();
            UpdateTimeDisplay();
            isGameRunning = true;
        }
    }

    private void UpdateTimeDisplay()
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

    private void UpdateTimeDisplayValue()
    {
        if (timeText != null)
        {
            timeText.text = $"Time: {timeRemaining:F0}s";
        }
    }


    private void OnTimeOver()
    {
        Debug.Log("Hết thời gian!");
    }
}
