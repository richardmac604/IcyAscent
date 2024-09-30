using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    InputHandler inputHandler;
    Rigidbody playerRigidbody;
    public const float movementSpeed = 3f;
    public const float lerpSpeed = 1f;

    Vector3 moveDirection;

    public Transform playerLeftHand;
    public Transform playerRightHand;
    public Transform playerLeftArm;
    public Transform playerRightArm;


    private void Awake() {
        inputHandler = GetComponent<InputHandler>();
        playerRigidbody = GetComponent<Rigidbody>();
    }


    public void HandleAllMovement() {
        HandleArmMovement();
    }


    private void HandleArmMovement() {

        // Calculate Left Hand movement from input
        Vector3 leftHandMovement = playerLeftHand.up * inputHandler.verticalInput;
        leftHandMovement += playerLeftHand.right * inputHandler.horizontalInput;

        // Calculate Right Hand movement from input -playerRightHand.up since y axis is flipped in local space
        Vector3 rightHandMovement = -playerRightHand.up * inputHandler.verticalInput;
        rightHandMovement += playerRightHand.right * inputHandler.horizontalInput;

        // Normalize the movement 
        leftHandMovement.Normalize();
        rightHandMovement.Normalize();

        // Multiply by movespeed
        leftHandMovement *= movementSpeed;
        rightHandMovement *= movementSpeed;

        Vector3 playerPosition = playerRigidbody.position;
        if (playerLeftHand.position.x + leftHandMovement.x > playerPosition.x) {
            leftHandMovement.x = Mathf.Clamp(leftHandMovement.x, float.MinValue, playerPosition.x - playerLeftHand.position.x);  // Clamp to prevent crossing to the right side
            leftHandMovement.z = 0;
        }

        if (playerRightHand.position.x + rightHandMovement.x < playerPosition.x) {
            rightHandMovement.x = Mathf.Clamp(rightHandMovement.x, playerPosition.x - playerRightHand.position.x, float.MaxValue);  // Clamp to prevent crossing to the left side
            rightHandMovement.z = 0;
        }

        // Move current position of Left/Right hand to current position + movement calculated
        // Relocate leftArm and rightArm target
        playerLeftArm.position = Vector3.Lerp(playerLeftHand.position, playerLeftHand.position + leftHandMovement, Time.deltaTime);
        playerRightArm.position = Vector3.Lerp(playerRightHand.position, playerRightHand.position + rightHandMovement, Time.deltaTime);

    }

}
