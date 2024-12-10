using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NightCtrl : MonoBehaviour
{
    public GameObject nightTint;
    //public GameObject sunsetTint;
    //public GameObject sunriseTint;
    public Light lightSource;
    // Start is called before the first frame update
    void Start()
    {
        GameObject dlightObject = GameObject.Find("Directional Light");
        lightSource = dlightObject.GetComponent<Light>();
        
        nightTint = GameObject.Find("NightOverlay");
        //sunsetTint = GameObject.Find("SunsetOverlay");
        //sunriseTint = GameObject.Find("SunriseOverlay");
       
        nightTint.SetActive(false);
        //sunsetTint.SetActive(false);
        //sunriseTint.SetActive(false);

    }

    // Update is called once per frame
    void Update()
    {
        //TODO: transition with shader color value
        if (lightSource.transform.eulerAngles.x > 140 && lightSource.transform.eulerAngles.x < 210)
        {
            nightTint.SetActive(true);
        }
        //else if (lightSource.transform.eulerAngles.x > 50 && lightSource.transform.eulerAngles.x < 140)
        //{
        //    sunsetTint.SetActive(true);
        //}
        //else if (lightSource.transform.eulerAngles.x > 210 && lightSource.transform.eulerAngles.x < 360)
        //{
        //    sunriseTint.SetActive(true);

        //} else
        //{
        //    nightTint.SetActive(false);
        //    //sunsetTint.SetActive(false);
        //    sunriseTint.SetActive(false);
        //}
    }
}
