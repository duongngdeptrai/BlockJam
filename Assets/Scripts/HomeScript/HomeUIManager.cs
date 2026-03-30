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
        txtLevel.text = $"Level:\n{GamePlayManager.Instance.GetCurrentLevel()}";
    }

    private void OnPlayButtonClicked()
    {
        GamePlayManager.Instance.InitMap();
        gameObject.SetActive(false);
    }
}
