using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GamePlayUI : MonoBehaviour
{
    [SerializeField] private Button resetButton;
    [SerializeField] private TextMeshProUGUI timeText;
    private float timeRemaining;

    private void Awake()
    {
        if (resetButton != null)
        {
            resetButton.onClick.AddListener(OnResetButtonClicked);
        }
    }

    private void Update()
    {
        if ( StateManager.CurrentState != GameState.Playing)
        {
            return;
        }

        timeRemaining -= Time.deltaTime;
        if (timeRemaining <= 0f)
        {
            timeRemaining = 0f;
            UpdateTimeDisplay();
            StateManager.ToLose();
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
    }

    public void StartTimer(float timeLimit)
    {
        timeRemaining = Mathf.Max(0f, timeLimit);
        UpdateTimeDisplay();
    }

    private void UpdateTimeDisplay()
    {
        if (timeText != null)
        {
            timeText.text = $"Time: {timeRemaining:F0}s";
        }
    }

    private void OnResetButtonClicked()
    {
        if (GamePlayManager.Instance != null)
        {
            GamePlayManager.Instance.ResetLevel();
        }
    }
}
