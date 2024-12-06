using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TimerManager : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    public float elapsedTime = 0f;
    private bool isTiming = false;

    void Start()
    {
        timerText.text = "Time: 0.000";
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && !isTiming)
        {
            isTiming = true;
        }

        if (isTiming)
        {
            elapsedTime += Time.deltaTime;
            UpdateTimerDisplay();
        }
    }

    // Updates the timer display in the UI
    void UpdateTimerDisplay()
    {
        timerText.text = "Time: " + elapsedTime.ToString("F3") ;
    }
}
