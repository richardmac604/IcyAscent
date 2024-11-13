using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour
{
    private bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown("p"))
        {
            isPaused = togglePause();
        }
    }

    void OnGUI()
    {
        if (isPaused)
        {
            GUI.Box(
                new Rect(Screen.width / 2 - 100, Screen.height / 2 - 65, 200, 130),
                "Pause Menu"
            );

            GUI.Label(
                new Rect(Screen.width / 2 - 90, Screen.height / 2 - 45, 180, 110),
                "Press P to resume."
            );

            if (GUI.Button(
                new Rect(Screen.width / 2 - 90, Screen.height / 2 - 25, 180, 35),
                new GUIContent("End Game")))
            {
                quitGame();
            }

            if (GUI.Button(
                new Rect(Screen.width / 2 - 90, Screen.height / 2 + 15, 180, 35),
                new GUIContent("Back to Main Menu")))
            {
                restartGame();
                togglePause();
            }
        }
    }


    bool togglePause()
    {
        if (Time.timeScale == 0f) {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            Time.timeScale = 1.0f;
            return (false);
        } else {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            Time.timeScale = 0f;
            return (true);
        }
    }

    void quitGame()
    {
        Application.Quit();
        Debug.Log("error in quitting the game");
    }

    void restartGame()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
