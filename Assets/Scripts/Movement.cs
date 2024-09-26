using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Movement : MonoBehaviour {

    public float moveSpeed = 200f;
    public Transform player; // Reference to player
    public Transform leftHand;
    public Transform rightHand;
    public float pullForce = 5f; // Force for climbing

    private Rigidbody rb;


    private Ray ray;
    //private RaycastHit hit;
    private float rayDistance = 2f;
    public LayerMask ClimbableLayer;
    public LayerMask FloorLayer;

    public float sensitivity = 10f; // Sensitivity of the mouse movement
    private Vector3 lastMousePosition;
    private bool leftHandGrabbing = false;
    private bool rightHandGrabbing = false;



    void Start() {
        rb = GetComponent<Rigidbody>();
    }

    void Update() {
        RaycastHit hit;
        

        if (Input.GetMouseButton(0) && Physics.Raycast(leftHand.position, transform.forward, out hit, rayDistance, ClimbableLayer)) {
            MoveLeftHand();
            leftHandGrabbing = true;
        } else if (Input.GetMouseButton(1) && Physics.Raycast(rightHand.position, transform.forward, out hit, rayDistance, ClimbableLayer)) {
            MoveRightHand();
            rightHandGrabbing = true;
        }

        // For debugging purposes, draw the ray in the Scene view
        Debug.DrawRay(transform.position, transform.forward * rayDistance, Color.red);
    }

    private void MoveLeftHand() {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        Vector3 pullDirection = new Vector3(-mouseX, -mouseY, 0) * sensitivity;

        leftHand.Translate(pullDirection * pullForce * Time.deltaTime);
    }

    private void MoveRightHand() {
        float mouseX = Input.GetAxis("Mouse X");
        float mouseY = Input.GetAxis("Mouse Y");
        Vector3 pullDirection = new Vector3(-mouseX, -mouseY, 0) * sensitivity;

        rightHand.Translate(pullDirection * pullForce * Time.deltaTime);
    }

}

