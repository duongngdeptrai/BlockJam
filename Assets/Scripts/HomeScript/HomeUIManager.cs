using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HomeUIManager : MonoBehaviour
{
    [SerializeField] private Button playButton;
    [SerializeField] private Button settingsButton;
    [SerializeField] private TextMeshProUGUI txtLevel;

    private void Awake()
    {
        if (playButton != null)
        {
            //playButton.onClick.AddListener(OnPlayButtonClicked);
        }
        else
        {
            Debug.LogError("HomeUIManager: playButton is not assigned in Inspector.");
        }

        if (settingsButton != null)
        {
            //settingsButton.onClick.AddListener(OnSettingsButtonClicked);
        }
        else
        {
            Debug.LogError("HomeUIManager: settingsButton is not assigned in Inspector.");
        }
    }

    private void OnEnable()
    {
        if (txtLevel != null)
        {
            if (GamePlayManager.Instance != null)
            {
                txtLevel.text = $"Level\n{GamePlayManager.Instance.GetCurrentLevel()}";
            }
            else
            {
                Debug.LogError("HomeUIManager: GamePlayManager.Instance is null! Cannot display current level.");
                txtLevel.text = "Level\n?";
            }
        }
        else
        {
            Debug.LogError("HomeUIManager: txtLevel is not assigned in Inspector.");
        }
    }

    public void OnPlayButtonClicked()
    {
        if (GamePlayManager.Instance == null)
        {
            Debug.LogError("HomeUIManager: GamePlayManager.Instance is null! Cannot start playing.");
            return;
        }
        GamePlayManager.Instance.StartPlaying();
    }

    public void OnSettingsButtonClicked()
    {
        Debug.Log("Settings button clicked.");
    }
}
