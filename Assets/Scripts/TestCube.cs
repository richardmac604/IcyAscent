using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestCube : MonoBehaviour {

    [SerializeField] GameObject HingeObject;
    private float speed = 100f;

    private void Update() {
        if (Input.GetMouseButton(0)) {
            Debug.Log("Holding mouse down");
            float horizontalInput = Input.GetAxis("Mouse X") * speed;
            horizontalInput *= Time.deltaTime;

            Debug.Log(horizontalInput);

            HingeObject.GetComponent<Rigidbody>().AddForce(horizontalInput, 0f,0f);

        }
    }


}
