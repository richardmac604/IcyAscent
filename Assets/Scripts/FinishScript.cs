using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FinishScript : MonoBehaviour
{
    private TimerManager timerManager;
    private bool playerWon = false;

    public GameObject winMessageUI;

    private void Start()
    {
        timerManager = FindObjectOfType<TimerManager>();
        if (winMessageUI != null)
        {
            winMessageUI.SetActive(false);
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player") && !playerWon)
        {
            playerWon = true;

            float finishTime = timerManager.elapsedTime;

            if (PlayerPrefs.HasKey("FinishTime"))
            {
                float fastestTime = PlayerPrefs.GetFloat("FinishTime");

                if (finishTime < fastestTime)
                {
                    PlayerPrefs.SetFloat("FinishTime", finishTime);
                }
            }
            else
            {
                PlayerPrefs.SetFloat("FinishTime", finishTime);
            }

            // Show "You Win!" message
            if (winMessageUI != null)
            {
                winMessageUI.SetActive(true);
            }

            StartCoroutine(ShowWinMessageAndGoToMainMenu());
        }
    }

    private IEnumerator ShowWinMessageAndGoToMainMenu()
    {
        yield return new WaitForSeconds(5f);

        winMessageUI.SetActive(false);
        SceneManager.LoadScene("MainMenu");
    }
}
