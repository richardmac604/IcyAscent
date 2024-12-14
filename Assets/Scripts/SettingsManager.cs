using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour
{
    private float volume = 1f;
    private bool isPaused = false;

    void Update()
    {
        if (Input.GetKeyDown("p"))
        {
            isPaused = togglePause();
        }

        if (Input.GetKeyDown("q") && isPaused)
        {
            quitGame();
        }

        if (Input.GetKey("m") && isPaused)
        {
            returnToMenu();
            togglePause();
        }

        if (Input.GetKey("r") && isPaused)
        {
            restartGame();
            togglePause();
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
                "Resume Game - P"
            );

            GUI.Label(
                new Rect(Screen.width / 2 - 90, Screen.height / 2 - 25, 160, 90),
               "Quit Game - Q"
            );

            GUI.Label(
                new Rect(Screen.width / 2 - 90, Screen.height / 2 - 5, 140, 70),
               "Main Menu - M"
            );

            GUI.Label(
                new Rect(Screen.width / 2 - 90, Screen.height / 2 + 20, 120, 50),
                "Restart - R"
            );


            GUI.Label(new Rect(Screen.width / 2 - 90, Screen.height / 2 + 45, 100, 30),
                "Volume Control"
            );

            volume = GUI.Slider(new Rect(Screen.width / 2 + 7,
                Screen.height / 2 + 50, 60, 25), volume, 0, 0, 1,
                GUI.skin.horizontalSlider, GUI.skin.horizontalSliderThumb,
                true, 0, GUI.skin.horizontalSlider
                );

            // Adjust audio volume, ensure soundAssets is not null before using it
            if (SoundAssets.Instance != null)
            {
                AudioListener.volume = volume;
            }
            else
            {
                Debug.LogWarning("SoundAssets instance is missing.");
            }
        }
    }

    bool togglePause()
    {
        if (Time.timeScale == 0f)
        {
            Cursor.visible = false;
            Cursor.lockState = CursorLockMode.Locked;
            Time.timeScale = 1.0f;
            return false;
        }
        else
        {
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            Time.timeScale = 0f;
            return true;
        }
    }

    void quitGame()
    {
        Application.Quit();
        Debug.Log("Quit the game");
    }

    void returnToMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }

    void restartGame()
    {
        SceneManager.LoadScene("MainLevel");
    }
}
