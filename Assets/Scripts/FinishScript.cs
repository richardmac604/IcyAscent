using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FinishScript : MonoBehaviour {
    private TimerManager timerManager;

    private void Start() {
        timerManager = FindObjectOfType<TimerManager>();
    }

    private void OnTriggerEnter(Collider other) {
        // Check if the player hit the flag pole
        if (other.CompareTag("Player")) {
            float finishTime = timerManager.elapsedTime;

            // Check if there's an existing fastest time
            if (PlayerPrefs.HasKey("FinishTime")) {
                float fastestTime = PlayerPrefs.GetFloat("FinishTime");

                // Update the fastest time if the current finish time is better
                if (finishTime < fastestTime) {
                    PlayerPrefs.SetFloat("FinishTime", finishTime);
                }
            } else {
                // If no fastest time is stored, save the current finish time as the fastest
                PlayerPrefs.SetFloat("FinishTime", finishTime);
            }

            // Load the menu scene
            SceneManager.LoadScene("MainMenu");
        }
    }
}
