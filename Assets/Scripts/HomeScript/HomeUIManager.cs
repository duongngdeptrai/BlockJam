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
        playButton.onClick.AddListener(OnPlayButtonClicked);
    }

    private void OnEnable()
    {
        if (txtLevel != null && GamePlayManager.Instance != null)
        {
            txtLevel.text = $"Level\n{GamePlayManager.Instance.GetCurrentLevel()}";
        }
    }

    private void OnPlayButtonClicked()
    {
        if (GamePlayManager.Instance != null)
        {
            GamePlayManager.Instance.StartPlaying();
        }
    }
}
