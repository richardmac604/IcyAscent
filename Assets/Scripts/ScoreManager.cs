using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class ScoreManager : MonoBehaviour {
    public Transform currentEle;
    public TextMeshProUGUI scoreText;
    private float highestEle;
    
    // Start is called before the first frame update
    void Start()
    {
        highestEle = currentEle.position.y;
    }

    // Update is called once per frame
    void Update()
    {
        if(currentEle.position.y > highestEle)
        {
            highestEle = currentEle.position.y;
            UpdateScore();
        }
    }

    void UpdateScore()
    {
        int score = Mathf.FloorToInt(highestEle);
        scoreText.text = "Best Height: " + score;
    }
}
