using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SettingsManager : MonoBehaviour
{
    public GameObject audioObj;
    private bool isPaused = false;

    private void Start()
    {
        audioObj = GameObject.Find("Music");
    }

    void Update()
    {
        if (Input.GetKeyDown("p"))
        {
            isPaused = togglePause();
        }
        
        if (Input.GetKeyDown("q"))
        {
            quitGame();
        }
        
        if (Input.GetKey("r"))
        {
            restartGame();
            togglePause();
        }

        if (Input.GetKeyDown("m"))
        {
            MuteMusic();
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
                new Rect(Screen.width / 2 - 90, Screen.height / 2 - 25, 180, 35),
               "Quit Game - Q"
            );

            GUI.Label(
                new Rect(Screen.width / 2 - 90, Screen.height / 2 - 5, 180, 35),
               "Return to Main Menu - R"
            );

            GUI.Label(
                new Rect(Screen.width / 2 - 90, Screen.height / 2 + 15, 180, 35),
                "Mute BG Music - M"
            );
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

    void MuteMusic()
    {
        AudioSource audioSource = audioObj.GetComponent<AudioSource>();
        if (audioSource.isPlaying == true)
        {
            audioSource.Pause();
        } else
        {
            audioSource.Play();
        }
        
        
    }
}
