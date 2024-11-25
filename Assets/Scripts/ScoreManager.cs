using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TimerManager : MonoBehaviour
{
    public TextMeshProUGUI timerText;
    private float elapsedTime = 0f;
    private bool isTiming = false;

    // Start is called before the first frame update
    void Start()
    {
        timerText.text = "Time: 0.000";
    }

    // Update is called once per frame
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
