using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EnvironmentManager : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform currentEle;
    public GameObject clouds1;
    public GameObject clouds2;
    private const float fogLevel = 25;

    // Start is called before the first frame update
    void Start()
    {
        clouds1 = GameObject.Find("CloudsSystem");
        clouds2 = GameObject.Find("CloudsSystem2");
        clouds1.SetActive(false);
        clouds2.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (currentEle.position.y >= fogLevel)
        {
            clouds1.SetActive(true);
            clouds2.SetActive(true);
        } else
        {
            clouds1.SetActive(false);
            clouds2.SetActive(false);
        }
    }
}
