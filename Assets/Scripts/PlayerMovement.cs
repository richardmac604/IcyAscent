using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.Mathematics;
using UnityEngine;

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

    // Pickaxe use
    private float rayDistance = 50f;
    public LayerMask easyClimbLayer;
    public Transform leftPick;
    public Transform rightPick;
    public const float pullUpSpeed = 5f;
    bool leftPickHit = false;
    bool rightPickHit = false;

    // Joints
    private ConfigurableJoint leftCJoint;
    private ConfigurableJoint rightCJoint;
    private bool isSwinging = false;
    private float swaySpeed = 2f;
    private float maxMomentum = 3f; // Maximum swing momentum


    private void Awake() {
        inputHandler = GetComponent<InputHandler>();
        playerRigidbody = GetComponent<Rigidbody>();

        lastLeftHandPosition = playerLeftHand.position;
        lastRightHandPosition = playerRightHand.position;

        leftShoulderToHandLength = Vector3.Distance(playerLeftHand.position, playerLeftShoulder.position);
        rightShoulderToHandLength = Vector3.Distance(playerRightHand.position, playerRightShoulder.position);
    }

    private void Start() {
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }


    public void HandleAllMovement() {
        HandleArmMovement();
        HandlePickaxeUse();
        ApplyPhysics();
    }

    private void HandleArmMovement() {

        // Calculate movement in world space -Vector3.right so moving mouse right brings arms to the right mouse left arms to the right
        Vector3 globalMovement = (Vector3.forward * inputHandler.verticalInput) + (Vector3.right * inputHandler.horizontalInput);
        globalMovement.Normalize();
        globalMovement *= movementSpeed;

        // Switch the localspace movement of leftHand to world space
        Vector3 leftHandMovement = playerLeftHand.TransformDirection(globalMovement);

        // Switch the localspace movement of rightHand to world space with flipped movements
        Vector3 rightHandMovement = playerRightHand.TransformDirection(new Vector3(globalMovement.x, globalMovement.y, -globalMovement.z));

        playerPosition = playerRigidbody.position;

        //// Clamp Left Hand movement
        //if (playerLeftHand.position.x + leftHandMovement.x > playerPosition.x) {
        //    leftHandMovement.x = Mathf.Clamp(leftHandMovement.x, float.MinValue, playerPosition.x - playerLeftHand.position.x);
        //    leftHandMovement.z = 0;
        //}

        //// Clamp Right Hand movement
        //if (playerRightHand.position.x + rightHandMovement.x < playerPosition.x) {
        //    rightHandMovement.x = Mathf.Clamp(rightHandMovement.x, playerPosition.x - playerRightHand.position.x, float.MaxValue);
        //    rightHandMovement.z = 0;
        //}

        // Update arm target positions
        if (!leftPickHit) {
            playerLeftArmTarget.position = Vector3.Lerp(playerLeftHand.position, playerLeftHand.position + leftHandMovement, Time.deltaTime);
        }
        if (!rightPickHit) {
            playerRightArmTarget.position = Vector3.Lerp(playerRightHand.position, playerRightHand.position + rightHandMovement, Time.deltaTime);
        }
    }


    private void ApplyPhysics() {
        if (isSwinging) {
            playerRigidbody.useGravity = true;
        } else if (leftPickHit && rightPickHit) {
            playerRigidbody.velocity = Vector3.zero;
            playerRigidbody.angularVelocity = Vector3.zero;
            playerRigidbody.useGravity = false;
        }
    }

    private void DestroyJoints() {
        if (rightPickHit && leftPickHit) {
            Destroy(rightCJoint);
            Destroy(leftCJoint);
            isSwinging = false;
        }
        if (!leftPickHit && leftCJoint != null) {
            Destroy(leftCJoint);
            isSwinging = false;
        }
        if (!rightPickHit && rightCJoint != null) {
            Destroy(rightCJoint);
            isSwinging = false;
        }
    }

    private void CreateJoints(Vector3 leftHit, Vector3 rightHit) {

        if (leftHit != Vector3.zero) {
            // Left pickaxe hit the wall
            if (leftCJoint == null) {
                leftCJoint = transform.gameObject.AddComponent<ConfigurableJoint>();

                leftCJoint.anchor = leftCJoint.transform.InverseTransformPoint(leftPick.position);
                leftCJoint.connectedAnchor = leftHit;

                // Set primary and secondary axes for stability and control
                leftCJoint.axis = transform.InverseTransformDirection(Vector3.forward);
                //leftCJoint.secondaryAxis = transform.InverseTransformDirection(Vector3.up);

                // Limit unwanted motion to keep player upright
                leftCJoint.xMotion = ConfigurableJointMotion.Locked;
                leftCJoint.yMotion = ConfigurableJointMotion.Locked;
                leftCJoint.zMotion = ConfigurableJointMotion.Locked;

                // Only allow free rotation on x axis
                leftCJoint.angularXMotion = ConfigurableJointMotion.Free;
                leftCJoint.angularYMotion = ConfigurableJointMotion.Locked;
                leftCJoint.angularZMotion = ConfigurableJointMotion.Locked;

                // Swing angle limits
                //SoftJointLimit swingLimit = new SoftJointLimit();
                //swingLimit.bounciness = 0.3f;
                //swingLimit.limit = -180f;
                //leftCJoint.lowAngularXLimit = swingLimit;
                //swingLimit.limit = 180f;
                //leftCJoint.highAngularXLimit = swingLimit;
                leftCJoint.enablePreprocessing = false;
            }
        } else if (rightHit != Vector3.zero) {
            // Right pickaxe hit the wall
            if (rightCJoint == null) {

                rightCJoint = transform.gameObject.AddComponent<ConfigurableJoint>();

                rightCJoint.anchor = rightCJoint.transform.InverseTransformPoint(rightPick.position);
                rightCJoint.connectedAnchor = rightHit;

                // Set primary and secondary axes for stability and control
                rightCJoint.axis = transform.InverseTransformDirection(Vector3.forward);
                //rightCJoint.secondaryAxis = transform.InverseTransformDirection(Vector3.up);

                // Limit unwanted motion to keep player upright
                rightCJoint.xMotion = ConfigurableJointMotion.Locked;
                rightCJoint.yMotion = ConfigurableJointMotion.Locked;
                rightCJoint.zMotion = ConfigurableJointMotion.Locked;

                // Only allow free rotation on x axis
                rightCJoint.angularXMotion = ConfigurableJointMotion.Free;
                rightCJoint.angularYMotion = ConfigurableJointMotion.Locked;
                rightCJoint.angularZMotion = ConfigurableJointMotion.Locked;

                // Swing angle limits
                //SoftJointLimit swingLimit = new SoftJointLimit();
                //swingLimit.bounciness = 0.3f;
                //swingLimit.limit = -180f;
                //rightCJoint.lowAngularXLimit = swingLimit;
                //swingLimit.limit = 180;
                //rightCJoint.highAngularXLimit = swingLimit;
                rightCJoint.enablePreprocessing = false;
            }
        }
    }

    private void AttachJoint(Vector3 leftHit, Vector3 rightHit) {

        CreateJoints(leftHit, rightHit);

        if (leftHit != Vector3.zero && rightHit != Vector3.zero) {
            DestroyJoints();
        } else {
            isSwinging = true;

            float angMag = playerRigidbody.angularVelocity.magnitude;
            float mag = playerRigidbody.velocity.magnitude;

            // Clamp linear velocity
            if (mag > maxMomentum) {
                Vector3 clampedVelocity = playerRigidbody.velocity.normalized * maxMomentum;
                playerRigidbody.velocity = clampedVelocity;
            }

            // Clamp angular velocity
            if (angMag > maxMomentum) {
                Vector3 clampedAngularVelocity = playerRigidbody.angularVelocity.normalized * maxMomentum;
                playerRigidbody.angularVelocity = clampedAngularVelocity;
            }

            // Apply forces based on player input
            playerRigidbody.AddForce(new Vector3(Mathf.Clamp(inputHandler.horizontalInput, -30f, 30f) * swaySpeed, 0, 0), ForceMode.Acceleration);

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

        DestroyJoints();

        if (leftPickHit && rightPickHit) {
            // Both pickaxes hitting wall
            // Move entire player body depending on inputHandler.verticalInput and inputHandler.horizontalInput
            leftHitPoint = leftPickaxeHitPoint.point;
            rightHitPoint = rightPickaxeHitPoint.point;
            lastLeftHandPosition = playerLeftHand.position;
            lastRightHandPosition = playerRightHand.position;
            Vector3 targetPosition = (leftHitPoint + rightHitPoint) / 2;
            MovePlayerTowards(targetPosition);

        } else if (leftPickHit) {
            // Left pickaxe hitting wall
            // Move entire player body depending on inputHandler.verticalInput and inputHandler.horizontalInput towards the leftPickaxeHit location
            leftHitPoint = leftPickaxeHitPoint.point;
            lastLeftHandPosition = playerLeftHand.position;
            AttachJoint(leftHitPoint, Vector3.zero);

        } else if (rightPickHit) {
            // Right pickaxe hitting wall
            // Move entire player body depending on inputHandler.verticalInput and inputHandler.horizontalInput towards the rightPickaxeHit location
            rightHitPoint = rightPickaxeHitPoint.point;
            lastRightHandPosition = playerRightHand.position;
            AttachJoint(Vector3.zero, rightHitPoint);
        }

    }

    private void MovePlayerTowards(Vector3 targetPosition) {
        isSwinging = false;
        // Calculate the direction to the target position (pickaxe hit point)
        Vector3 direction = (targetPosition - transform.position).normalized;
        direction.z = 0f;

        float verticalInput = inputHandler.verticalInput; // Climbing input
        float horizontalInput = inputHandler.horizontalInput; // Climbing input

        // Determine how much to move vertically based on input
        Vector3 movement = new Vector3(horizontalInput, verticalInput, 0) * pullUpSpeed * Time.deltaTime;

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
                transform.position = Vector3.Lerp(transform.position, Vector3.MoveTowards(transform.position, newPosition, movement.magnitude), Time.fixedDeltaTime * pullUpSpeed);
            } else {
                // Calculate future left shoulder position
                Vector3 simulatedLeftShoulderPos = newPosition + (playerLeftShoulder.position - transform.position);
                leftShoulderToHandVector = simulatedLeftShoulderPos - lastLeftHandPosition;

                // Calculate future right shoulder position
                Vector3 simulatedRightShoulderPos = newPosition + (playerRightShoulder.position - transform.position);
                rightShoulderToHandVector = simulatedRightShoulderPos - lastRightHandPosition;

                // If the future leftShoulder and rightShoulder position are within the length of arms move
                if (leftShoulderToHandVector.magnitude < leftShoulderToHandLength && rightShoulderToHandVector.magnitude < rightShoulderToHandLength) {
                    transform.position = Vector3.Lerp(transform.position, Vector3.MoveTowards(transform.position, newPosition, movement.magnitude), Time.fixedDeltaTime * pullUpSpeed);
                }
            }
            // Reset location of both hands
            playerLeftArmTarget.position = lastLeftHandPosition;
            playerRightArmTarget.position = lastRightHandPosition;
        }
    }

}


