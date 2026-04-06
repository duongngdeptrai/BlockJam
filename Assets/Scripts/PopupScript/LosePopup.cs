using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class LosePopup : MonoBehaviour
{
    [SerializeField] private Button retryButton;
    [SerializeField] private Button homeButton;

    private void Awake()
    {
        if (retryButton != null)
        {
            retryButton.onClick.AddListener(OnRetryButtonClicked);
        }

        if (homeButton != null)
        {
            homeButton.onClick.AddListener(OnHomeButtonClicked);
        }
    }

    private void OnDestroy()
    {
        if (retryButton != null)
        {
            retryButton.onClick.RemoveListener(OnRetryButtonClicked);
        }

        if (homeButton != null)
        {
            homeButton.onClick.RemoveListener(OnHomeButtonClicked);
        }
    }

    private void OnRetryButtonClicked()
    {
        GamePlayManager.Instance.ResetLevel();
    }

    private void OnHomeButtonClicked()
    {
        StateManager.ToHome();
    }
}
