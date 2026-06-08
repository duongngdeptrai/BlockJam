using System;
using UnityEngine;
using UnityEngine.UI;

public class WinPopup : MonoBehaviour
{
    [SerializeField] private Button retryButton;
    [SerializeField] private Button nextLevelButton;

    private void Awake()
    {
        if (retryButton != null)
        {
            //retryButton.onClick.AddListener(OnRetryButtonClicked);
        }
        else
        {
            Debug.LogError("WinPopup: retryButton is not assigned in Inspector.", this);
        }

        if (nextLevelButton != null)
        {
            //nextLevelButton.onClick.AddListener(OnNextLevelButtonClicked);
        }
        else
        {
            Debug.LogError("WinPopup: nextLevelButton is not assigned in Inspector.", this);
        }
    }

    public void OnRetryButtonClicked()
    {
        if (GamePlayManager.Instance == null)
        {
            Debug.LogError("WinPopup: GamePlayManager.Instance is null! Cannot retry level.", this);
            return;
        }
        try
        {
            GamePlayManager.Instance.ResetLevel();
        }
        catch (Exception e)
        {
            Debug.LogError($"WinPopup: Exception in ResetLevel: {e.Message}\n{e.StackTrace}", this);
        }
    }

    public void OnNextLevelButtonClicked()
    {
        if (GamePlayManager.Instance == null)
        {
            Debug.LogError("WinPopup: GamePlayManager.Instance is null! Cannot advance to next level.", this);
            return;
        }
        try
        {
            GamePlayManager.Instance.AdvanceToNextLevel();
            GameStateMachine.ToHome();
        }
        catch (Exception e)
        {
            Debug.LogError($"WinPopup: Exception in AdvanceToNextLevel: {e.Message}\n{e.StackTrace}", this);
        }
    }
}
