using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour {

    private Rigidbody rb;

    private Ray ray;
    private float rayDistance = 2f;
    public LayerMask ClimbableLayer;

    private float sensitivity = 10f; // Sensitivity of the mouse movement
    private Vector3 lastMousePosition;


    public Transform player; // Reference to player
    public Transform leftHand;
    public Transform rightHand;
    private bool leftHandGrabbing = false;
    private bool rightHandGrabbing = false;

    private float pullForce = 20f; // Force for climbing

    void Start() {
        rb = GetComponent<Rigidbody>();
    }

    void Update() {
        RaycastHit hit;
        // Check if both mouse buttons are held down
        bool leftButtonHeld = Input.GetMouseButton(0) && Physics.Raycast(leftHand.position, transform.forward, out hit, rayDistance, ClimbableLayer);
        bool rightButtonHeld = Input.GetMouseButton(1) && Physics.Raycast(rightHand.position, transform.forward, out hit, rayDistance, ClimbableLayer);

        // Move both hands if both buttons are pressed
        if (leftButtonHeld && rightButtonHeld) {
            if (!leftHandGrabbing && !rightHandGrabbing) {
                // Store initial mouse position on first click
                lastMousePosition = Input.mousePosition;
            }
            MoveBothHands();
            leftHandGrabbing = true;
            rightHandGrabbing = true;
        }
        // Move left hand if only left button is pressed
        else if (leftButtonHeld) {
            if (!leftHandGrabbing) {
                // Store initial mouse position on first click
                lastMousePosition = Input.mousePosition;
            }
            MoveLeftHand();
            leftHandGrabbing = true;
        }
        // Move right hand if only right button is pressed
        else if (rightButtonHeld) {
            if (!rightHandGrabbing) {
                // Store initial mouse position on first click
                lastMousePosition = Input.mousePosition;
            }
            MoveRightHand();
            rightHandGrabbing = true;
        }
        // Reset grabbing state if no buttons are pressed
        else {
            leftHandGrabbing = false;
            rightHandGrabbing = false;
        }


        Debug.DrawRay(leftHand.position, transform.forward * rayDistance, Color.red);
        Debug.DrawRay(rightHand.position, transform.forward * rayDistance, Color.red);
    }


    private void MoveBothHands() {

        // Calculate mouse movement since the last frame
        Vector3 mouseMovement = Input.mousePosition - lastMousePosition;
        float deltaX = mouseMovement.x * sensitivity * Time.deltaTime;
        float deltaY = mouseMovement.y * sensitivity * Time.deltaTime;

        // Calculate the pull direction for both hands
        Vector3 pullDirection = new Vector3(-deltaX, -deltaY, 0) * pullForce;

        // Move both hands simultaneously
        transform.Translate(pullDirection * Time.deltaTime);
        transform.Translate(pullDirection * Time.deltaTime);

        // Update last mouse position
        lastMousePosition = Input.mousePosition;
    }

    private void MoveLeftHand() {

        // Calculate mouse movement since the last frame
        Vector3 mouseMovement = Input.mousePosition - lastMousePosition;
        float deltaX = mouseMovement.x * sensitivity * Time.deltaTime;
        float deltaY = mouseMovement.y * sensitivity * Time.deltaTime;

        Vector3 pullDirection = new Vector3(-deltaX, -deltaY, 0) * pullForce;
        transform.Translate(pullDirection * Time.deltaTime);

        // Update last mouse position
        lastMousePosition = Input.mousePosition;
    }

    private void MoveRightHand() {

        // Calculate mouse movement since the last frame
        Vector3 mouseMovement = Input.mousePosition - lastMousePosition;
        float deltaX = mouseMovement.x * sensitivity * Time.deltaTime;
        float deltaY = mouseMovement.y * sensitivity * Time.deltaTime;

        Vector3 pullDirection = new Vector3(-deltaX, -deltaY, 0) * pullForce;
        transform.Translate(pullDirection * Time.deltaTime);

        // Update last mouse position
        lastMousePosition = Input.mousePosition;
    }

}

