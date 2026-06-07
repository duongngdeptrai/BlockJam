using System;
using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using TMPro;

public class InactivityDetector : MonoBehaviour
{
    [SerializeField] private float inactiveTime = 5f;
    [SerializeField] private Image warningObject;
    [SerializeField] private float warningDuration = 2f;
    [SerializeField] private TextMeshProUGUI warningText;

    private float lastInteractionTime;
    private bool hasTriggered;
    private Coroutine warningCoroutine;

    private void Start()
    {
        lastInteractionTime = Time.time;
        hasTriggered = false;
        if (warningObject != null) warningObject.gameObject.SetActive(false);
        if (warningText != null) warningText.gameObject.SetActive(false);
        GameStateMachine.StateChanged += OnStateChanged;
    }

    private void Update()
    {
        if (!GameStateMachine.Is(GameState.Playing)) return;

        if (Input.GetMouseButtonDown(0) || Input.touchCount > 0)
        {
            lastInteractionTime = Time.time;
            hasTriggered = false;
            StopWarning();
        }

        if (!hasTriggered && Time.time - lastInteractionTime >= inactiveTime)
        {
            hasTriggered = true;
            lastInteractionTime = Time.time;
            StartWarning();
        }
    }

    private void OnStateChanged(GameState newState)
    {
        hasTriggered = false;
        lastInteractionTime = Time.time;
        StopWarning();
    }

    private void StartWarning()
    {
        Debug.Log($"Player inactive for {inactiveTime} seconds");
        if (GamePlayManager.Instance != null && GamePlayManager.Instance.gamePlayUI != null)
        {
            GamePlayManager.Instance.gamePlayUI.SubtractTime(inactiveTime);
        }
        if (warningObject == null) return;
        warningObject.gameObject.SetActive(true);
        warningText.gameObject.SetActive(true);
#if UNITY_ANDROID && !UNITY_EDITOR
        Handheld.Vibrate();
#endif
        warningCoroutine = StartCoroutine(AnimateWarning());
        StartCoroutine(FadeAndPopText());
    }

    private void StopWarning()
    {
        if (warningCoroutine != null)
        {
            StopCoroutine(warningCoroutine);
            warningCoroutine = null;
        }
        if (warningObject != null) warningObject.gameObject.SetActive(false);
        if (warningText != null) warningText.gameObject.SetActive(false);
    }

    private IEnumerator AnimateWarning()
    {
        Image image = warningObject;
        if (image == null) yield break;

        Color color = image.color;
        float minAlpha = 0f;
        float maxAlpha = 100f / 255f; // ~0.392
        float fadeDuration = 0.3f;

        for (int i = 0; i <= 2; i++)
        {
            // Fade in: 0 -> 100/255
            float elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                color.a = Mathf.Lerp(minAlpha, maxAlpha, t);
                image.color = color;
                yield return null;
            }

            // Fade out: 100/255 -> 0
            elapsed = 0f;
            while (elapsed < fadeDuration)
            {
                elapsed += Time.deltaTime;
                float t = Mathf.Clamp01(elapsed / fadeDuration);
                color.a = Mathf.Lerp(maxAlpha, minAlpha, t);
                image.color = color;
                yield return null;
            }
        }

        hasTriggered = false;
        StopWarning();
    }

    private IEnumerator FadeAndPopText()
    {
        if (warningText == null) yield break;

        // Trạng thái ban đầu
        Color color = warningText.color;
        color.a = 0f;
        warningText.color = color;

        RectTransform rect = warningText.rectTransform;
        rect.localScale = Vector3.one * 0.5f;

        float duration = 0.4f;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);

            // Alpha: 0 -> 1
            color.a = t;
            warningText.color = color;

            // Scale: 0.5 -> 1.2 -> 0
            float scale;
            if (t < 0.7f)
            {
                // 0.5 -> 1.2
                scale = Mathf.Lerp(0.5f, 1.2f, t / 0.7f);
            }
            else
            {
                // 1.2 -> 0
                scale = Mathf.Lerp(1.2f, 0f, (t - 0.7f) / 0.3f);
            }
            rect.localScale = Vector3.one * scale;

            yield return null;
        }

        // Đảm bảo giá trị cuối chính xác
        color.a = 1f;
        warningText.color = color;
        rect.localScale = Vector3.one;
    }

    private void OnDestroy()
    {
        GameStateMachine.StateChanged -= OnStateChanged;
        StopWarning();
    }
}
