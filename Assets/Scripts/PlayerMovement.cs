using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.XR;

public class PlayerMovement : MonoBehaviour {

    // Arm Movement
    InputHandler inputHandler;
    Rigidbody playerRigidbody;
    public const float movementSpeed = 3f;
    public const float lerpSpeed = 1f;
    public Transform playerLeftHand;
    public Transform playerRightHand;
    public Transform playerLeftArmTarget;
    public Transform playerRightArmTarget;
    private Vector3 lastLeftHandPosition;
    private Vector3 lastRightHandPosition;
    public Transform playerLeftShoulder;
    public Transform playerRightShoulder;
    private float leftShoulderToHandLength;
    private float rightShoulderToHandLength;

    private Vector3 playerPosition;

    // Joints
    public float swaySpeed = 5f;
    private Rigidbody leftHandRB;
    private Rigidbody rightHandRB;

    public HingeJoint leftJoint;
    //public HingeJoint leftHinge;
    public ConfigurableJoint rightJoint;



    // Pickaxe use
    private float rayDistance = 2f;
    public LayerMask easyClimbLayer;
    public Transform leftPick;
    public Transform rightPick;
    public const float pullUpSpeed = 2f;
    bool leftPickHit = false;
    bool rightPickHit = false;


    // Physics
    private const float GRAVITY = -9.8f;
    private float velocityResetSpeed = 5f;

    private void Awake() {
        inputHandler = GetComponent<InputHandler>();
        playerRigidbody = GetComponent<Rigidbody>();
        leftHandRB = playerLeftHand.GetComponent<Rigidbody>();
        rightHandRB = playerRightHand.GetComponent<Rigidbody>();
        lastLeftHandPosition = playerLeftHand.position;
        lastRightHandPosition = playerRightHand.position;

        leftShoulderToHandLength = Vector3.Distance(playerLeftHand.position, playerLeftShoulder.position);
        rightShoulderToHandLength = Vector3.Distance(playerRightHand.position, playerRightShoulder.position);
    }

    private void Start() {

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.lockState = CursorLockMode.Confined;
    }


    public void HandleAllMovement() {
        HandleArmMovement();
        HandlePickaxeUse();
        //ApplyPhysics();
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

        playerPosition = playerRigidbody.position;
        if (playerLeftHand.position.x + leftHandMovement.x > playerPosition.x) {
            leftHandMovement.x = Mathf.Clamp(leftHandMovement.x, float.MinValue, playerPosition.x - playerLeftHand.position.x);  // Clamp to prevent crossing to the right side
            leftHandMovement.z = 0;
        }

        if (playerRightHand.position.x + rightHandMovement.x < playerPosition.x) {
            rightHandMovement.x = Mathf.Clamp(rightHandMovement.x, playerPosition.x - playerRightHand.position.x, float.MaxValue);  // Clamp to prevent crossing to the left side
            rightHandMovement.z = 0;
        }

        // Relocate leftArm and rightArm target
        if (!leftPickHit) {
            playerLeftArmTarget.position = Vector3.Lerp(playerLeftHand.position, playerLeftHand.position + leftHandMovement, Time.deltaTime);
        }
        if (!rightPickHit) {
            playerRightArmTarget.position = Vector3.Lerp(playerRightHand.position, playerRightHand.position + rightHandMovement, Time.deltaTime);
        }

    }

    private void ApplyPhysics() {
        if (leftPickHit || rightPickHit) {
            // Gradually reduce the vertical velocity towards zero
            if (playerRigidbody.velocity.y > 0) {
                // If moving up, decrease the upward velocity
                playerRigidbody.velocity += Vector3.down * velocityResetSpeed * Time.deltaTime;
            } else if (playerRigidbody.velocity.y < 0) {
                // If moving down, increase the downward velocity towards zero
                playerRigidbody.velocity += Vector3.up * velocityResetSpeed * Time.deltaTime;
            }

            // Ensure the vertical velocity does not go below 0.1
            if (Mathf.Abs(playerRigidbody.velocity.y) < 0.1f) {
                playerRigidbody.velocity = new Vector3(playerRigidbody.velocity.x, 0, playerRigidbody.velocity.z);
            }
        } else {
            // Apply gravity when no pick in wall
            playerRigidbody.AddForce(Physics.gravity, ForceMode.Acceleration);
        }
    }


    private void HandlePickaxeUse() {
        RaycastHit leftPickaxeHitPoint = new RaycastHit();
        RaycastHit rightPickaxeHitPoint = new RaycastHit();

        // Check if both mouse buttons are held down and the pickaxe hits a climbable surface
        leftPickHit = Input.GetMouseButton(0) && Physics.Raycast(leftPick.position, transform.forward, out leftPickaxeHitPoint, rayDistance, easyClimbLayer);
        rightPickHit = Input.GetMouseButton(1) && Physics.Raycast(rightPick.position, transform.forward, out rightPickaxeHitPoint, rayDistance, easyClimbLayer);



        if (leftPickHit && rightPickHit) {
            // Both pickaxes hitting wall
            // Move entire player body depending on inputHandler.verticalInput and inputHandler.horizontalInput
            //leftHitPoint = leftPickaxeHitPoint.point;
            //rightHitPoint = rightPickaxeHitPoint.point;
            //Vector3 targetPosition = (leftHitPoint + rightHitPoint) / 2;
            lastLeftHandPosition = playerLeftHand.position;
            lastRightHandPosition = playerRightHand.position;
            MovePlayerTowards(leftPickaxeHitPoint, rightPickaxeHitPoint);


        } else if (leftPickHit) {
            // Left pickaxe hitting wall


            // Move entire player body depending on inputHandler.verticalInput and inputHandler.horizontalInput towards the leftPickaxeHit location
            //leftHitPoint = leftPickaxeHitPoint.point;
            lastLeftHandPosition = playerLeftHand.position;

            MovePlayerTowards(leftPickaxeHitPoint, rightPickaxeHitPoint);


        } else if (rightPickHit) {
            // Right pickaxe hitting wall
            // Move entire player body depending on inputHandler.verticalInput and inputHandler.horizontalInput towards the rightPickaxeHit location
            //rightHitPoint = rightPickaxeHitPoint.point;

            lastRightHandPosition = playerRightHand.position;
            MovePlayerTowards(leftPickaxeHitPoint, rightPickaxeHitPoint);
        }


        //if (Input.GetMouseButtonUp(0) && leftJoint != null) Destroy(leftJoint);
        if (Input.GetMouseButtonUp(1) && rightJoint != null) Destroy(rightJoint);

        Debug.DrawRay(leftPick.position, transform.forward * rayDistance, Color.red);
        Debug.DrawRay(rightPick.position, transform.forward * rayDistance, Color.red);
    }

    private void ApplySwingForce(float horizontalInput, Vector3 anchorPoint) {
        // Calculate the direction relative to the hand's anchored point
        Vector3 directionToAnchor = (anchorPoint - playerRigidbody.position).normalized;

        // Calculate a perpendicular direction for the swing force
        Vector3 swingDirection = Vector3.Cross(directionToAnchor, Vector3.forward);

        // Apply force along the swing direction based on player input
        playerRigidbody.AddForce(swingDirection * horizontalInput * swaySpeed, ForceMode.Force);
    }

    //private void MovePlayerTowards(RaycastHit leftRay, RaycastHit rightRay) {
    //    Vector3 leftHitPoint;
    //    Vector3 rightHitPoint;

    //    // Calculate the direction to the target position (pickaxe hit point)
    //    //Vector3 direction = (targetPosition - transform.position).normalized;
    //    //direction.z = 0f;

    //    float verticalInput = inputHandler.verticalInput; // Climbing input
    //    float horizontalInput = inputHandler.horizontalInput; // Climbing input

    //    // Determine how much to move vertically based on input
    //    Vector3 movement = new Vector3(horizontalInput, verticalInput, 0) * pullUpSpeed * Time.deltaTime;




    //    if (leftPickHit && rightPickHit) {
    //        Vector3 targetPosition = (leftRay.point + rightRay.point) / 2;


    //        playerLeftArmTarget.position = lastLeftHandPosition;
    //        playerRightArmTarget.position = lastRightHandPosition;
    //    } else if (leftPickHit) {
    //        leftHitPoint = leftRay.point;
    //        Debug.Log(leftJoint);

    //        if (leftJoint == null) {
    //            // Create a joint for the left hand and anchor it to the hit point on the wall
    //            leftJoint = playerLeftArmTarget.gameObject.AddComponent<HingeJoint>();
    //            leftJoint.axis = Vector3.forward;
    //            leftJoint.connectedBody = leftRay.transform.gameObject.GetComponent<Rigidbody>();  // No connected body, fixed to the wall
    //            leftJoint.autoConfigureConnectedAnchor = true;
    //            //leftJoint.anchor = leftHitPoint;  // Anchor at the hand's current position
    //            //leftJoint.connectedAnchor = leftHitPoint;  // Hand is anchored to the hit point on the wall
    //        }

    //        playerLeftArmTarget.position = lastLeftHandPosition;

    //        Vector3 swingDirection = new Vector3(inputHandler.horizontalInput, inputHandler.verticalInput, 0f) *swaySpeed;
    //        playerRigidbody.AddForce(swingDirection, ForceMode.Impulse);


    //        // Apply swing force to the player rigidbody
    //        //ApplySwingForce(horizontalInput, leftRay.point);


    //    } else if (rightPickHit) {
    //        rightHitPoint = rightRay.point;

    //        playerRightArmTarget.position = lastRightHandPosition;
    //    }

    //}





    private void MovePlayerTowards(RaycastHit leftRay, RaycastHit rightRay) {

        Vector3 targetPosition = leftRay.point;

        // Calculate the direction to the target position (pickaxe hit point)
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.z = 0f;

        float verticalInput = inputHandler.verticalInput; // Climbing input
        float horizontalInput = inputHandler.horizontalInput; // Climbing input

        // Determine how much to move vertically based on input
        Vector3 movement = new Vector3(horizontalInput, verticalInput, 0) * pullUpSpeed * Time.deltaTime;
        //movement.z = 0f;

        // Calculate the new position by moving the player towards the target position
        Vector3 newPosition = transform.position + movement;

        // Zero out the Z component to ensure movement only happens in the X and Y axis
        newPosition.z = transform.position.z;

        newPosition.y = Mathf.Clamp(newPosition.y, transform.position.y - 20f, targetPosition.y + 10f);

        Vector3 leftShoulderToHandVector = playerLeftShoulder.position - lastLeftHandPosition;
        Vector3 rightShoulderToHandVector = playerRightShoulder.position - lastRightHandPosition;


        if (leftPickHit && rightPickHit) {
            if (leftShoulderToHandVector.magnitude < leftShoulderToHandLength && rightShoulderToHandVector.magnitude < rightShoulderToHandLength) {
                // If the leftShoulder position and rightShoulder position are within the length of arms move
                transform.position = Vector3.MoveTowards(transform.position, newPosition, movement.magnitude);
            } else {
                // Calculate future left shoulder position
                Vector3 simulatedLeftShoulderPos = newPosition + (playerLeftShoulder.position - transform.position);
                leftShoulderToHandVector = simulatedLeftShoulderPos - lastLeftHandPosition;

                // Calculate future right shoulder position
                Vector3 simulatedRightShoulderPos = newPosition + (playerRightShoulder.position - transform.position);
                rightShoulderToHandVector = simulatedRightShoulderPos - lastRightHandPosition;

                // If the future leftShoulder and rightShoulder position are within the length of arms move
                if (leftShoulderToHandVector.magnitude < leftShoulderToHandLength && rightShoulderToHandVector.magnitude < rightShoulderToHandLength) {
                    transform.position = Vector3.MoveTowards(transform.position, newPosition, movement.magnitude);
                }
            }
            // Reset location of both hands
            playerLeftArmTarget.position = lastLeftHandPosition;
            playerRightArmTarget.position = lastRightHandPosition;
        } else if (leftPickHit) {
            if (leftShoulderToHandVector.magnitude < leftShoulderToHandLength) {
                // If the leftShoulder position and rightShoulder position are within the length of arms move

                if (leftJoint == null) {
                    // Create a joint for the left hand and anchor it to the hit point on the wall
                    leftJoint = playerLeftArmTarget.gameObject.AddComponent<HingeJoint>();
                    leftJoint.axis = Vector3.forward;
                    leftJoint.connectedBody = leftRay.transform.gameObject.GetComponent<Rigidbody>();  // No connected body, fixed to the wall
                    leftJoint.autoConfigureConnectedAnchor = true;
                    //leftJoint.anchor = leftHitPoint;  // Anchor at the hand's current position
                    //leftJoint.connectedAnchor = leftHitPoint;  // Hand is anchored to the hit point on the wall
                }

                transform.position = Vector3.MoveTowards(transform.position, newPosition, movement.magnitude);
                Vector3 swingDirection = new Vector3(inputHandler.horizontalInput, inputHandler.verticalInput, 0f) *swaySpeed;
            //playerLeftArmTarget.GetComponent<Rigidbody>().AddForce(swingDirection, ForceMode.Acceleration);
            //playerRigidbody.AddForce(swingDirection, ForceMode.Acceleration);
            //ApplySwing();
        }
        //else {


        //    // Calculate future left shoulder position
        //    Vector3 simulatedLeftShoulderPos = newPosition + (playerLeftShoulder.position - transform.position);
        //    leftShoulderToHandVector = simulatedLeftShoulderPos - lastLeftHandPosition;

        //    if (leftShoulderToHandVector.magnitude < leftShoulderToHandLength) {

        //        if (leftJoint == null) {
        //            // Create a joint for the left hand and anchor it to the hit point on the wall
        //            leftJoint = playerLeftArmTarget.gameObject.AddComponent<HingeJoint>();
        //            leftJoint.axis = Vector3.forward;
        //            leftJoint.connectedBody = leftRay.transform.gameObject.GetComponent<Rigidbody>();  // No connected body, fixed to the wall
        //            leftJoint.autoConfigureConnectedAnchor = true;
        //            //leftJoint.anchor = leftHitPoint;  // Anchor at the hand's current position
        //            //leftJoint.connectedAnchor = leftHitPoint;  // Hand is anchored to the hit point on the wall
        //        }
        //        // If the future leftShoulder position are within the length of the arms move
        //        transform.position = Vector3.MoveTowards(transform.position, newPosition, movement.magnitude);
        //        Vector3 swingDirection = new Vector3(inputHandler.horizontalInput, inputHandler.verticalInput, 0f) * swaySpeed;
        //        //playerLeftArmTarget.GetComponent<Rigidbody>().AddForce(swingDirection, ForceMode.Acceleration);
        //        playerRigidbody.AddForce(swingDirection, ForceMode.Acceleration);
        //    }
        //}
        // Reset left hand location
        playerLeftArmTarget.position = lastLeftHandPosition;
        } else if (rightPickHit) {
            if (rightShoulderToHandVector.magnitude < rightShoulderToHandLength) {
                // If the rightShoulder position are within the length of arms move
                transform.position = Vector3.MoveTowards(transform.position, newPosition, movement.magnitude);
            } else {

                // Calculate future right shoulder position
                Vector3 simulatedRightShoulderPos = newPosition + (playerRightShoulder.position - transform.position);
                rightShoulderToHandVector = simulatedRightShoulderPos - lastRightHandPosition;

                if (rightShoulderToHandVector.magnitude < rightShoulderToHandLength) {
                    // If the future rightShoulder position is within the length of arms move
                    transform.position = Vector3.MoveTowards(transform.position, newPosition, movement.magnitude);
                }
            }
            // Reset right hand location
            playerRightArmTarget.position = lastRightHandPosition;
        }

    }

}


