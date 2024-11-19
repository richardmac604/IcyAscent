using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class EnvironmentManager : MonoBehaviour
{
    // Start is called before the first frame update
    public Transform currentEle;
    public GameObject fogLayer;
    private const float fogLevel = 25;

    // Start is called before the first frame update
    void Start()
    {
        fogLayer.SetActive(false);
    }

    // Update is called once per frame
    void Update()
    {
        if (currentEle.position.y >= fogLevel)
        {
            fogLayer.SetActive(true);
        } else
        {
            fogLayer.SetActive(false);
        }
    }
}
