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
    public Transform leftHandTarget;
    public Transform rightHandTarget;
    public Transform leftPick;
    public Transform rightPick;
    private bool leftPickGrabbing = false;
    private bool rightPickGrabbing = false;

    private float pullForce = 20f; // Force for climbing

    void Start() {
        rb = GetComponent<Rigidbody>();
    }

    void Update() {
        RaycastHit hit;
        // Check if both mouse buttons are held down
        bool leftButtonHeld = Input.GetMouseButton(0) && Physics.Raycast(leftPick.position, transform.forward, out hit, rayDistance, ClimbableLayer);
        bool rightButtonHeld = Input.GetMouseButton(1) && Physics.Raycast(rightPick.position, transform.forward, out hit, rayDistance, ClimbableLayer);

        // Move both hands if both buttons are pressed
        if (leftButtonHeld && rightButtonHeld) {
            if (!leftPickGrabbing && !rightPickGrabbing) {
                // Store initial mouse position on first click
                lastMousePosition = Input.mousePosition;
            }
            MoveBothHands();
            leftPickGrabbing = true;
            rightPickGrabbing = true;
        }
        // Move left hand if only left button is pressed
        else if (leftButtonHeld) {
            if (!leftPickGrabbing) {
                // Store initial mouse position on first click
                lastMousePosition = Input.mousePosition;
            }
            MoveLeftHand();
            leftPickGrabbing = true;
        }
        // Move right hand if only right button is pressed
        else if (rightButtonHeld) {
            if (!rightPickGrabbing) {
                // Store initial mouse position on first click
                lastMousePosition = Input.mousePosition;
            }
            MoveRightHand();
            rightPickGrabbing = true;
        }
        // Reset grabbing state if no buttons are pressed
        else {
            leftPickGrabbing = false;
            rightPickGrabbing = false;
        }


        Debug.DrawRay(leftPick.position, transform.forward * rayDistance, Color.red);
        Debug.DrawRay(rightPick.position, transform.forward * rayDistance, Color.red);
    }


    private void MoveBothHands() {

        // Calculate mouse movement since the last frame
        Vector3 mouseMovement = Input.mousePosition - lastMousePosition;
        float deltaX = mouseMovement.x * sensitivity;
        float deltaY = mouseMovement.y * sensitivity;

        // Calculate the pull direction based on mouse movement (adjust axis as needed)
        Vector3 pullDirection = new Vector3(-deltaX, -deltaY, 0) * pullForce;

        // Move the leftPick position
        Vector3 newLeftPickPosition = leftPick.position + pullDirection * Time.deltaTime;
        Vector3 newRightPickPosition = rightPick.position + pullDirection * Time.deltaTime;

        // Update position of left pick using lerp
        leftPick.position = Vector3.Lerp(leftPick.position, newLeftPickPosition, Time.deltaTime * 1);
        leftHandTarget.position = leftPick.position;

        // Update position of right pick using lerp
        rightPick.position = Vector3.Lerp(rightPick.position, newRightPickPosition, Time.deltaTime * 1);
        rightHandTarget.position = rightPick.position;

        lastMousePosition = Input.mousePosition;

    }

    private void MoveLeftHand() {

        // Calculate mouse movement since the last frame
        Vector3 mouseMovement = Input.mousePosition - lastMousePosition;
        float deltaX = mouseMovement.x * sensitivity;
        float deltaY = mouseMovement.y * sensitivity;

        // Calculate the pull direction based on mouse movement (adjust axis as needed)
        Vector3 pullDirection = new Vector3(-deltaX, -deltaY, 0) * pullForce;

        // Move the leftPick position
        Vector3 newLeftPickPosition = leftPick.position + pullDirection * Time.deltaTime;

        // Update position of left pick using lerp
        leftPick.position = Vector3.Lerp(leftPick.position, newLeftPickPosition, Time.deltaTime * 1);
        leftHandTarget.position = leftPick.position;

        lastMousePosition = Input.mousePosition;
    }

    private void MoveRightHand() {

        // Calculate mouse movement since the last frame
        Vector3 mouseMovement = Input.mousePosition - lastMousePosition;
        float deltaX = mouseMovement.x * sensitivity;
        float deltaY = mouseMovement.y * sensitivity;

        // Calculate the pull direction based on mouse movement (adjust axis as needed)
        Vector3 pullDirection = new Vector3(-deltaX, -deltaY, 0) * pullForce;

        // Move the leftPick position
        Vector3 newRightPickPosition = rightPick.position + pullDirection * Time.deltaTime;

        // Update position of right pick using lerp
        rightPick.position = Vector3.Lerp(rightPick.position, newRightPickPosition, Time.deltaTime * 1);
        rightHandTarget.position = rightPick.position;

        lastMousePosition = Input.mousePosition;
    }

}

