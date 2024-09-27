using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GroundCheck : MonoBehaviour {


    public Transform playerFeet;     // Reference to player's feet position (can use transform.position if appropriate)
    private float rayDistance = 0.25f; // Distance to check for the ground
    public LayerMask floorLayer;     // Layer to specify what is considered the floor


    private bool isGrounded = false;
    private Vector3 velocity;

    private float moveSpeed = 5f;

    private void Update() {
        HandleMovement();
        CheckForGround();
    }

    void HandleMovement() {
        // Capture input for movement (WASD or arrow keys)
        float moveX = Input.GetAxis("Horizontal");
        float moveZ = Input.GetAxis("Vertical");

        // Move based on player input
        Vector3 movement = new Vector3(moveX, 0, moveZ).normalized * moveSpeed;
        transform.Translate(movement, Space.World);

        // Apply the velocity to the player's position
        transform.Translate(velocity * Time.deltaTime);
    }


    private void CheckForGround() {
        RaycastHit hit;
        // Cast ray downward from player's feet
        if (Physics.Raycast(playerFeet.position, Vector3.down, out hit, rayDistance, floorLayer)) {
            Debug.Log("Ground detected below player!");
            isGrounded = true;
            // Additional logic for when the player is grounded (e.g., allow jumping)
        } else {
            Debug.Log("No ground detected.");
            isGrounded = false;
            // Logic for when the player is not grounded (e.g., disable jumping or apply fall behavior)
        }

        // For debugging purposes, draw the ray in the Scene view
        Debug.DrawRay(playerFeet.position, Vector3.down * rayDistance, Color.red);
    }
}
