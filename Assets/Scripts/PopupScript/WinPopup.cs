using UnityEngine;
using UnityEngine.UI;

public class WinPopup : MonoBehaviour
{
    [SerializeField] private Button retryButton;
    [SerializeField] private Button nextLevelButton;
    private void Awake()
    {
        retryButton.onClick.AddListener(OnRetryButtonClicked);
        nextLevelButton.onClick.AddListener(OnNextLevelButtonClicked);
    }

    private void OnRetryButtonClicked()
    {
        GamePlayManager.Instance.OnResetButtonClicked();
    }

    private void OnNextLevelButtonClicked()
    {
        GamePlayManager.Instance.AdvanceToNextLevel();
    }
}
