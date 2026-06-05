using System;
using UnityEngine;
using System.Collections;

public class InactivityDetector : MonoBehaviour
{
    [SerializeField] private float inactiveTime = 5f;
    [SerializeField] private GameObject warningObject;
    [SerializeField] private float warningDuration = 2f;

    private float lastInteractionTime;
    private bool hasTriggered;
    private Coroutine warningCoroutine;

    private void Start()
    {
        lastInteractionTime = Time.time;
        hasTriggered = false;

        if (warningObject != null)
            warningObject.SetActive(false);

        GameStateMachine.StateChanged += OnStateChanged;
    }

    private void Update()
    {
        if (!GameStateMachine.Is(GameState.Playing))
            return;

        if (Input.GetMouseButtonDown(0) || Input.touchCount > 0)
        {
            lastInteractionTime = Time.time;

            if (hasTriggered)
            {
                hasTriggered = false;
                StopWarning();
            }
        }

        if (!hasTriggered && Time.time - lastInteractionTime >= inactiveTime)
        {
            hasTriggered = true;
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

        if (warningObject == null)
            return;

        warningObject.SetActive(true);

#if UNITY_ANDROID && !UNITY_EDITOR
        Handheld.Vibrate();
#endif

        warningCoroutine = StartCoroutine(AnimateWarning());
    }

    private void StopWarning()
    {
        if (warningCoroutine != null)
        {
            StopCoroutine(warningCoroutine);
            warningCoroutine = null;
        }

        if (warningObject != null)
            warningObject.SetActive(false);
    }

    private IEnumerator AnimateWarning()
    {
        warningObject.transform.localScale = Vector3.zero;
        var canvasGroup = warningObject.GetComponent<CanvasGroup>();
        if (canvasGroup != null) canvasGroup.alpha = 0f;

        float elapsed = 0f;
        float popInDuration = 0.2f;
        while (elapsed < popInDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / popInDuration);
            float ease = 1f + 2.70158f * Mathf.Pow(t - 1f, 3f) + 1.70158f * Mathf.Pow(t - 1f, 2f);
            warningObject.transform.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, ease);
            if (canvasGroup != null) canvasGroup.alpha = t;
            yield return null;
        }
        warningObject.transform.localScale = Vector3.one;
        if (canvasGroup != null) canvasGroup.alpha = 1f;

        yield return new WaitForSeconds(warningDuration);

        elapsed = 0f;
        float fadeOutDuration = 0.3f;
        while (elapsed < fadeOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / fadeOutDuration);
            warningObject.transform.localScale = Vector3.Lerp(Vector3.one, Vector3.zero, t);
            if (canvasGroup != null) canvasGroup.alpha = 1f - t;
            yield return null;
        }

        StopWarning();
    }

    private void OnDestroy()
    {
        GameStateMachine.StateChanged -= OnStateChanged;
        StopWarning();
    }
}
