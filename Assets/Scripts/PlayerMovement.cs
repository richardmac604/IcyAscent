using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;

public class PlayerMovement : MonoBehaviour {

    // Arm Movement
    InputHandler inputHandler;
    Rigidbody playerRigidbody;
    public const float movementSpeed = 3f;
    public const float lerpSpeed = 1f;
    public Transform playerLeftHand;
    public Transform playerRightHand;
    public Transform playerLeftArm;
    public Transform playerRightArm;
    private Vector3 playerPosition;

    // Pickaxe use
    private float rayDistance = 2f;
    public LayerMask easyClimbLayer;
    public Transform leftPick;
    public Transform rightPick;
    public const float pullUpSpeed = 2f;
    bool leftPickHit = false;
    bool rightPickHit = false;


    private void Awake() {
        inputHandler = GetComponent<InputHandler>();
        playerRigidbody = GetComponent<Rigidbody>();
    }


    public void HandleAllMovement() {
        HandleArmMovement();
        HandlePickaxeUse();
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

        //playerPosition = playerRigidbody.position;
        //if (playerLeftHand.position.x + leftHandMovement.x > playerPosition.x) {
            //leftHandMovement.x = Mathf.Clamp(leftHandMovement.x, float.MinValue, playerPosition.x - playerLeftHand.position.x);  // Clamp to prevent crossing to the right side
            //leftHandMovement.z = 0;
        //}

        //if (playerRightHand.position.x + rightHandMovement.x < playerPosition.x) {
            //rightHandMovement.x = Mathf.Clamp(rightHandMovement.x, playerPosition.x - playerRightHand.position.x, float.MaxValue);  // Clamp to prevent crossing to the left side
            //rightHandMovement.z = 0;
        //}

        // Move current position of Left/Right hand to current position + movement calculated
        // Relocate leftArm and rightArm target
        if (!leftPickHit) {
            playerLeftArm.position = Vector3.Lerp(playerLeftHand.position, playerLeftHand.position + leftHandMovement, Time.deltaTime);
        }
        if (!rightPickHit) {
            playerRightArm.position = Vector3.Lerp(playerRightHand.position, playerRightHand.position + rightHandMovement, Time.deltaTime);
        }

        // If the pickaxe is stuck (hit) on the wall, don't move the arm but ensure the hand stays attached to the hit point.
        if (leftPickHit) {
            playerLeftArm.position = playerLeftHand.position;
        }
        if (rightPickHit) {
            playerRightArm.position = playerRightHand.position;
        }


    }


    private void HandlePickaxeUse() {
        RaycastHit leftPickaxeHitPoint = new RaycastHit();
        RaycastHit rightPickaxeHitPoint = new RaycastHit();
        Vector3 leftHitPoint;
        Vector3 rightHitPoint;

        // Check if both mouse buttons are held down and the pickaxe hits a climbable surface
        leftPickHit = Input.GetMouseButton(0) && Physics.Raycast(leftPick.position, transform.forward, out leftPickaxeHitPoint, rayDistance, easyClimbLayer);
        rightPickHit = Input.GetMouseButton(1) && Physics.Raycast(rightPick.position, transform.forward, out rightPickaxeHitPoint, rayDistance, easyClimbLayer);

        if (leftPickHit && rightPickHit) {
            // Both pickaxes hitting wall
            // Move entire player body depending on inputHandler.verticalInput and inputHandler.horizontalInput
            leftHitPoint = leftPickaxeHitPoint.point;
            rightHitPoint = rightPickaxeHitPoint.point;
            Vector3 targetPosition = (leftHitPoint + rightHitPoint) / 2;
            MovePlayerTowards(targetPosition);

        } else if (leftPickHit) {
            // Left pickaxe hitting wall
            // Move entire player body depending on inputHandler.verticalInput and inputHandler.horizontalInput towards the leftPickaxeHit location
            leftHitPoint = leftPickaxeHitPoint.point;
            MovePlayerTowards(leftHitPoint);


        } else if (rightPickHit) {
            // Right pickaxe hitting wall
            // Move entire player body depending on inputHandler.verticalInput and inputHandler.horizontalInput towards the rightPickaxeHit location
            rightHitPoint = rightPickaxeHitPoint.point;
            MovePlayerTowards(rightHitPoint);
        }

        Debug.DrawRay(leftPick.position, transform.forward * rayDistance, Color.red);
        Debug.DrawRay(rightPick.position, transform.forward * rayDistance, Color.red);
    }

    private void MovePlayerTowards(Vector3 targetPosition) {

        // Calculate the direction to the target position (pickaxe hit point)
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.z = 0f;

        // Calculate the distance between the player and the target position
        float distanceToTarget = Vector3.Distance(transform.position, targetPosition);

        // Apply movement based on input
        float verticalInput = -inputHandler.verticalInput; // Climbing input
        float horizontalInput = -inputHandler.horizontalInput; // Climbing input

        // Determine how much to move vertically based on input
        Vector3 movement = new Vector3(horizontalInput, verticalInput, 0) * pullUpSpeed * Time.deltaTime;
        movement.z = 0f;

        // Calculate the new position by moving the player towards the target position
        Vector3 newPosition = transform.position + movement;

        // Zero out the Z component to ensure movement only happens in the X and Y axes
        newPosition.z = transform.position.z;

        // Clamp movement in the Y direction so the player cannot move above the target (pickaxe hit point)
        if (leftPickHit) {
            // Ensure the player does not go higher than the left pickaxe hit point
            if (newPosition.y > targetPosition.y) {
                newPosition.y = targetPosition.y;
            }
        }

        if (rightPickHit) {
            // Ensure the player does not go higher than the right pickaxe hit point
            if (newPosition.y > targetPosition.y) {
                newPosition.y = targetPosition.y;
            }
        }

        // Now move the player towards the clamped new position
        transform.position = Vector3.MoveTowards(transform.position, newPosition, movement.magnitude);

    }

}


