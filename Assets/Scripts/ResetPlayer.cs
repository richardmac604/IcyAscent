using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResetPlayer : MonoBehaviour {

    private Vector3 startingPosition;
    private Quaternion startingRotation;
    private Rigidbody rb;

    void Start() {
        startingPosition = transform.position;
        startingRotation = transform.rotation;
        rb = GetComponent<Rigidbody>();
    }

    public void CheckState() {
        // Check if the player has fallen below a certain height
        bool hasFallen = transform.position.y < 0.3f;
        // Check if the player is rotated 90 or -90 degrees on the Z-axis
        float zRotation = transform.eulerAngles.z;
        bool isRotatedSideways = (zRotation >= 80f && zRotation <= 95f) || (zRotation >= 270f && zRotation <= 285f);

        // If the player has fallen or is sideways, reset position and rotation
        if (hasFallen && isRotatedSideways) {
            rb.velocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            transform.position = startingPosition;
            transform.rotation = startingRotation;
        }
    }
}

