using UnityEngine;

public class PanelVisibilityController : MonoBehaviour
{
    [SerializeField] private GameObject homePanel;
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;
    [SerializeField] private GameObject gameplayPanel;

    private void OnEnable()
    {
        GameStateMachine.StateChanged += ApplyStateVisuals;
        ApplyStateVisuals(GameStateMachine.CurrentState);
    }

    private void OnDisable()
    {
        GameStateMachine.StateChanged -= ApplyStateVisuals;
    }

    private void ApplyStateVisuals(GameState state)
    {
        if (homePanel != null)
        {
            homePanel.SetActive(state == GameState.Home);
        }

        if (winPanel != null)
        {
            winPanel.SetActive(state == GameState.Win);
        }

        if (losePanel != null)
        {
            losePanel.SetActive(state == GameState.Lose);
        }

        // if (gameplayPanel != null)
        // {
        //     gameplayPanel.SetActive(state != GameState.Home);
        // }
    }
}
