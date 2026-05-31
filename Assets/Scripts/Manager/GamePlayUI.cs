using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GamePlayUI : MonoBehaviour
{
    [SerializeField] private Button resetButton;
    [SerializeField] private Button homeButton;
    [SerializeField] private TextMeshProUGUI timeText;
    private float timeRemaining;

    private void Awake()
    {
        if (resetButton != null)
        {
            resetButton.onClick.AddListener(OnResetButtonClicked);
        }
        else
        {
            Debug.LogError("GamePlayUI: resetButton is not assigned in Inspector.");
        }

        if (homeButton != null)
        {
            homeButton.onClick.AddListener(OnHomeButtonClicked);
        }
        else
        {
            Debug.LogError("GamePlayUI: homeButton is not assigned in Inspector.");
        }
    }

    private void Update()
    {
        if (!GameStateMachine.Is(GameState.Playing)) return;

        timeRemaining -= Time.deltaTime;
        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            UpdateTimeDisplay();
            GameStateMachine.ToLose();
            return;
        }

        UpdateTimeDisplay();
    }

    private void OnDestroy()
    {
        if (resetButton != null)
        {
            resetButton.onClick.RemoveListener(OnResetButtonClicked);
        }
        if (homeButton != null)
        {
            homeButton.onClick.RemoveListener(OnHomeButtonClicked);
        }
    }

    public void StartTimer(float timeLimit)
    {
        timeRemaining = Mathf.Max(0f, timeLimit);
        UpdateTimeDisplay();
    }

    private void UpdateTimeDisplay()
    {
        if (timeText == null)
        {
            Debug.LogError("GamePlayUI: timeText is null! Assign TextMeshProUGUI in Inspector.");
            return;
        }
        timeText.text = $"Time: {timeRemaining:F0}s";
    }

    private void OnResetButtonClicked()
    {
        if (GamePlayManager.Instance == null)
        {
            Debug.LogError("GamePlayUI: GamePlayManager.Instance is null! Cannot reset level.");
            return;
        }
        GamePlayManager.Instance.ResetLevel();
    }

    private void OnHomeButtonClicked()
    {
        GameStateMachine.ToHome();
    }
}
