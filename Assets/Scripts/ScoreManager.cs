using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TimerManager : MonoBehaviour
{
    public TextMeshProUGUI timerText; // Reference to display the timer
    private float elapsedTime = 0f;   // Tracks the elapsed time
    private bool isTiming = false;   // Tracks whether the timer is running

    // Start is called before the first frame update
    void Start()
    {
        timerText.text = "Time: 0.000"; // Initialize the timer display
    }

    // Update is called once per frame
    void Update()
    {
        // Start the timer on left mouse click
        if (Input.GetMouseButtonDown(0) && !isTiming)
        {
            isTiming = true; // Start timing
        }

        // Update the timer if it's running
        if (isTiming)
        {
            elapsedTime += Time.deltaTime; // Increment the elapsed time
            UpdateTimerDisplay();         // Update the timer display
        }
    }

    // Updates the timer display in the UI
    void UpdateTimerDisplay()
    {
        timerText.text = "Time: " + elapsedTime.ToString("F3");
    }
}
