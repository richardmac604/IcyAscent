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
    public const float pullUpSpeed = 2f;
    bool leftPickHit = false;
    bool rightPickHit = false;


    // Joints
    private HingeJoint leftJoint;
    private HingeJoint rightJoint;
    private float swaySpeed = 5f;

    public float maxMomentum = 20f; // Maximum momentum to carry
    private Vector3 accumulatedMomentum = Vector3.zero; // Momentum accumulator
    private bool isSwinging = false; // Is the player currently swinging



    // Physics
    private const float GRAVITY = -9.8f;
    private float velocityResetSpeed = 5f;

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
        Cursor.lockState = CursorLockMode.Confined;
    }


    public void HandleAllMovement() {
        HandleArmMovement();
        HandlePickaxeUse();
        ApplyPhysics();
    }

    private void HandleArmMovement() {

        // Calculate movement in world space
        Vector3 globalMovement = (Vector3.up * inputHandler.verticalInput) + (Vector3.right * inputHandler.horizontalInput);
        globalMovement.Normalize();
        globalMovement *= movementSpeed;

        // Switch the localspace movement of leftHand to world space
        Vector3 leftHandMovement = playerLeftHand.TransformDirection(globalMovement);

        // Switch the localspace movement of rightHand to world space with flipped y axis
        Vector3 rightHandMovement = playerRightHand.TransformDirection(new Vector3(globalMovement.x, -globalMovement.y, globalMovement.z));

        playerPosition = playerRigidbody.position;

        // Clamp Left Hand movement
        if (playerLeftHand.position.x + leftHandMovement.x > playerPosition.x) {
            leftHandMovement.x = Mathf.Clamp(leftHandMovement.x, float.MinValue, playerPosition.x - playerLeftHand.position.x);
            leftHandMovement.z = 0;
        }

        // Clamp Right Hand movement
        if (playerRightHand.position.x + rightHandMovement.x < playerPosition.x) {
            rightHandMovement.x = Mathf.Clamp(rightHandMovement.x, playerPosition.x - playerRightHand.position.x, float.MaxValue);
            rightHandMovement.z = 0;
        }

        // Update arm target positions
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

    private void DestroyJoints() {
        //Debug.Log("Destroying Joints");
        //Debug.Log($"LeftPickHit:{leftPickHit}\nLeft Joint: {leftJoint}\nRightPickHit:{rightPickHit}\nRight Joint: {rightJoint}");
        if (!leftPickHit && leftJoint != null) Destroy(leftJoint);
        if (!rightPickHit && rightJoint != null) Destroy(rightJoint);
    }

    private void CreateJoints(Vector3 leftHit, Vector3 rightHit) {

        if (leftHit != Vector3.zero) {
            // Left pickaxe hit the wall
            if (leftJoint == null) {
                leftJoint = transform.gameObject.AddComponent<HingeJoint>();
                leftJoint.axis = Vector3.forward;

                leftJoint.anchor = leftJoint.transform.InverseTransformPoint(leftPick.position);
                leftJoint.connectedAnchor = leftHit;

                leftJoint.useLimits = true;
                JointLimits leftLimits = leftJoint.limits;
                leftLimits.min = 25f;
                leftLimits.max = 25f;

                //leftJoint.useMotor = true; // Enable motor for swinging
                //leftJoint.motor = new JointMotor { force = swaySpeed, targetVelocity = 0 };
                //leftJoint.useAcceleration = true;
            }
            Debug.DrawRay(leftJoint.anchor, transform.forward * rayDistance, Color.blue);
            Debug.DrawRay(leftJoint.connectedAnchor, transform.forward * rayDistance, Color.red);
        } 
        //else if (rightHit != Vector3.zero) {
        //    // Right pickaxe hit the wall
        //    if (rightJoint == null) {
        //        rightJoint = transform.gameObject.AddComponent<HingeJoint>();
        //        //leftJoint = playerLeftArmTarget.gameObject.AddComponent<HingeJoint>();
        //        rightJoint.axis = Vector3.forward;

        //        rightJoint.anchor = playerRightHand.position - transform.position;
        //        rightJoint.connectedAnchor = rightHit - playerRightHand.parent.position;

        //        rightJoint.useMotor = true; // Enable motor for swinging
        //        rightJoint.motor = new JointMotor { force = swaySpeed, targetVelocity = 0 };
        //        rightJoint.useAcceleration = true;
        //    }
        //}
    }
    void ReleaseSwing() {
        isSwinging = false;

        // Calculate the current momentum based on swing speed and direction
        Vector3 swingDirection = playerLeftHand.forward; // or any direction of your choice
        float swingForce = swaySpeed; // Use your swing speed here

        // Accumulate momentum (clamped to maxMomentum)
        accumulatedMomentum += swingDirection * swingForce;
        accumulatedMomentum = Vector3.ClampMagnitude(accumulatedMomentum, maxMomentum);
    }

    void ApplyMomentum() {
        // Apply the accumulated momentum to the player Rigidbody
        playerRigidbody.AddForce(accumulatedMomentum, ForceMode.Impulse);
        accumulatedMomentum = Vector3.zero; // Reset momentum after applying
    }

    private void OnCollisionEnter(Collision collision) {
        // Reset momentum if the pickaxe hits a wall
        if (collision.gameObject.CompareTag("Wall")) // Adjust based on your wall tag
        {
            accumulatedMomentum = Vector3.zero; // Reset momentum upon hitting a wall
        }
    }

    private void AttachJoint(Vector3 leftHit, Vector3 rightHit) {

        CreateJoints(leftHit, rightHit);

        if (leftHit != Vector3.zero && rightHit != Vector3.zero) {
            DestroyJoints();
            // Both pickaxes hit the wall
            Vector3 targetPosition = (leftHit + rightHit) / 2;
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
        } else if (leftHit != Vector3.zero) {
            // Left pickaxe hit the wall
            float horizontalInput = inputHandler.horizontalInput; // Climbing input
            //leftJoint.motor = new JointMotor { force = swaySpeed, targetVelocity = horizontalInput * swaySpeed };
            playerRigidbody.AddForce(new Vector3(horizontalInput*swaySpeed,0,0), ForceMode.Acceleration);

            playerLeftArmTarget.position = lastLeftHandPosition;

        } else if (rightHit != Vector3.zero) {
            // Right pickaxe hit the wall
            float horizontalInput = inputHandler.horizontalInput; // Climbing input
            rightJoint.motor = new JointMotor { force = swaySpeed, targetVelocity = horizontalInput * swaySpeed };

            playerRightArmTarget.position = lastRightHandPosition;
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
            //AttachJoint(leftHitPoint, rightHitPoint);
            MovePlayerTowards(targetPosition);

        } else if (leftPickHit) {
            // Left pickaxe hitting wall
            // Move entire player body depending on inputHandler.verticalInput and inputHandler.horizontalInput towards the leftPickaxeHit location
            leftHitPoint = leftPickaxeHitPoint.point;
            lastLeftHandPosition = playerLeftHand.position;
            //MovePlayerTowards(leftHitPoint);
            AttachJoint(leftHitPoint, Vector3.zero);

        } else if (rightPickHit) {
            // Right pickaxe hitting wall
            // Move entire player body depending on inputHandler.verticalInput and inputHandler.horizontalInput towards the rightPickaxeHit location
            rightHitPoint = rightPickaxeHitPoint.point;
            lastRightHandPosition = playerRightHand.position;
            //AttachJoint(Vector3.zero, rightHitPoint);
            MovePlayerTowards(rightHitPoint);
        }

        //Debug.DrawRay(leftPick.position, transform.forward * rayDistance, Color.red);
        //Debug.DrawRay(rightPick.position, transform.forward * rayDistance, Color.red);
    }

    private void MovePlayerTowards(Vector3 targetPosition) {

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
                transform.position = Vector3.MoveTowards(transform.position, newPosition, movement.magnitude);
            } else {
                // Calculate future left shoulder position
                Vector3 simulatedLeftShoulderPos = newPosition + (playerLeftShoulder.position - transform.position);
                leftShoulderToHandVector = simulatedLeftShoulderPos - lastLeftHandPosition;

                if (leftShoulderToHandVector.magnitude < leftShoulderToHandLength) {
                    // If the future leftShoulder position are within the length of the arms move
                    transform.position = Vector3.MoveTowards(transform.position, newPosition, movement.magnitude);
                }
            }
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


