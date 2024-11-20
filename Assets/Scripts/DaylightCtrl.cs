using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DaylightCtrl : MonoBehaviour
{
    private Vector3 rotation = Vector3.zero;
    private float rotationSpeed = 15f;
    public Light lightSource;
    // Start is called before the first frame update
    void Start()
    {
        GameObject dlightObject = GameObject.Find("Directional Light");
        lightSource = dlightObject.GetComponent<Light>();
    }
        // Update is called once per frame
        void Update()
    {
        rotation.x = rotationSpeed * Time.deltaTime;
        lightSource.transform.Rotate(rotation, Space.World);
    }

}
