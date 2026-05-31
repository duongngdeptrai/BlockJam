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
            //retryButton.onClick.AddListener(OnRetryButtonClicked);
        }
        else
        {
            Debug.LogError("LosePopup: retryButton is not assigned in Inspector.", this);
        }

        if (homeButton != null)
        {
           // homeButton.onClick.AddListener(OnHomeButtonClicked);
        }
        else
        {
            Debug.LogError("LosePopup: homeButton is not assigned in Inspector.", this);
        }
    }

    public void OnRetryButtonClicked()
    {
        if (GamePlayManager.Instance == null)
        {
            Debug.LogError("LosePopup: GamePlayManager.Instance is null! Cannot retry level.", this);
            return;
        }
        GamePlayManager.Instance.ResetLevel();
    }

    public void OnHomeButtonClicked()
    {
        GameStateMachine.ToHome();
    }
}
