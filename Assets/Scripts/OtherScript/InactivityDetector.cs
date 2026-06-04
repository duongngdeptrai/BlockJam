using System;
using UnityEngine;
using System.Collections;

public class InactivityDetector : MonoBehaviour
{
    [SerializeField] private float inactiveTime = 5f;
    [SerializeField] private GameObject warningObject;

    private float lastInteractionTime;
    private bool hasTriggered;

    private void Start()
    {
        lastInteractionTime = Time.time;

        if (warningObject != null)
            warningObject.SetActive(false);
    }
    
    private void Update()
    {
        if (Input.GetMouseButton(0) || Input.touchCount > 0)
        {
            lastInteractionTime = Time.time;
            hasTriggered = false;
            return;
        }

        if (!hasTriggered && Time.time - lastInteractionTime >= inactiveTime)
        {
            hasTriggered = true;

            Debug.Log($"Player inactive for {inactiveTime} seconds");

            StartCoroutine(ShowWarning());
        }
    }

    private IEnumerator ShowWarning()
    {
        warningObject.SetActive(true);

        yield return new WaitForSeconds(2f);

        warningObject.SetActive(false);
    }

    public void ResetTimer()
    {
        lastInteractionTime = Time.time;
        hasTriggered = false;
    }
}