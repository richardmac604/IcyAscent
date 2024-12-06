using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MenuManager : MonoBehaviour {
    public TextMeshProUGUI finishTimeText;

    private void Start() {
        // Check if a finish time is saved
        if (PlayerPrefs.HasKey("FinishTime")) {
            float finishTime = PlayerPrefs.GetFloat("FinishTime");

            // Convert and format the time appropriately
            string formattedTime = FormatTime(finishTime);
            finishTimeText.text = "Fastest time: " + formattedTime;
        }
    }

    // Formats the time based on its length
    private string FormatTime(float timeInSeconds) {
        if (timeInSeconds < 60f) {
            // Less than 60 seconds: Show in seconds
            return timeInSeconds.ToString("F3") + " seconds";
        } else if (timeInSeconds < 3600f) {
            // Less than 60 minutes: Show in minutes and seconds
            int minutes = Mathf.FloorToInt(timeInSeconds / 60f);
            float seconds = timeInSeconds % 60f;
            return $"{minutes} minutes {seconds:F2} seconds";
        } else {
            // 60 minutes or more: Show in hours, minutes, and seconds
            int hours = Mathf.FloorToInt(timeInSeconds / 3600f);
            int minutes = Mathf.FloorToInt((timeInSeconds % 3600f) / 60f);
            float seconds = timeInSeconds % 60f;
            return $"{hours} hours {minutes} minutes {seconds:F2} seconds";
        }
    }

}
