using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using TMPro;
using Unity.Mathematics;
using UnityEngine;
using static UnityEngine.ParticleSystem;

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
    public LayerMask layermask;
    public Transform leftPick;
    public Transform rightPick;
    public const float pullUpSpeed = 5f;
    bool leftPickHit = false;
    bool rightPickHit = false;
    private string leftHitLayerName;
    private string rightHitLayerName;

    // Joints
    private ConfigurableJoint leftCJoint;
    private ConfigurableJoint rightCJoint;
    private bool isSwinging = false;
    private float swaySpeed = 3f;
    private float maxMomentum = 3f; // Maximum swing momentum

    // Particles
    [SerializeField] private ParticleSystem iceParticles;
    [SerializeField] private ParticleSystem snowParticles;
    [SerializeField] private ParticleSystem metalParticles;
    [SerializeField] private ParticleSystem woodParticles;
    private ParticleSystem particleInstance;
    private bool leftParticleSpawned = false;
    private bool rightParticleSpawned = false;

    // Audio
    public AudioSource iceHitSound;
    public AudioSource snowHitSound;
    public AudioSource metalHitSound;
    public AudioSource woodHitSound;


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

        // Update arm target positions
        if (!leftPickHit) {
            playerLeftArmTarget.position = Vector3.Lerp(playerLeftHand.position, playerLeftHand.position + leftHandMovement, Time.deltaTime);
        }
        if (!rightPickHit) {
            playerRightArmTarget.position = Vector3.Lerp(playerRightHand.position, playerRightHand.position + rightHandMovement, Time.deltaTime);
        }
    }

    private void ApplyPhysics() {
        Vector3 customGravity = new Vector3(0, -10.81f, 0); // Gravity

        if (isSwinging) {
            // Apply gravity
            playerRigidbody.isKinematic = false;
            playerRigidbody.AddForce(customGravity, ForceMode.Acceleration);
        } else if (leftPickHit && rightPickHit) {
            // Completely stop the player from sliding
            playerRigidbody.isKinematic = true; // Temporarily disable physics motion
        } else {
            // Apply gravity
            playerRigidbody.isKinematic = false;
            playerRigidbody.AddForce(customGravity, ForceMode.Acceleration);
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

                // Set primary axis for stability and control
                leftCJoint.axis = transform.InverseTransformDirection(Vector3.forward);

                // Limit unwanted motion to keep player upright
                leftCJoint.xMotion = ConfigurableJointMotion.Locked;
                leftCJoint.yMotion = ConfigurableJointMotion.Locked;
                leftCJoint.zMotion = ConfigurableJointMotion.Locked;

                // Only allow free rotation on x axis
                leftCJoint.angularXMotion = ConfigurableJointMotion.Free;
                leftCJoint.angularYMotion = ConfigurableJointMotion.Locked;
                leftCJoint.angularZMotion = ConfigurableJointMotion.Locked;

                leftCJoint.enablePreprocessing = false;
            }
        } else if (rightHit != Vector3.zero) {
            // Right pickaxe hit the wall
            if (rightCJoint == null) {

                rightCJoint = transform.gameObject.AddComponent<ConfigurableJoint>();

                rightCJoint.anchor = rightCJoint.transform.InverseTransformPoint(rightPick.position);
                rightCJoint.connectedAnchor = rightHit;

                // Set primary axis for stability and control
                rightCJoint.axis = transform.InverseTransformDirection(Vector3.forward);

                // Limit unwanted motion to keep player upright
                rightCJoint.xMotion = ConfigurableJointMotion.Locked;
                rightCJoint.yMotion = ConfigurableJointMotion.Locked;
                rightCJoint.zMotion = ConfigurableJointMotion.Locked;

                // Only allow free rotation on x axis
                rightCJoint.angularXMotion = ConfigurableJointMotion.Free;
                rightCJoint.angularYMotion = ConfigurableJointMotion.Locked;
                rightCJoint.angularZMotion = ConfigurableJointMotion.Locked;

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
        // Reset hit layers at the start of the function
        leftHitLayerName = null;
        rightHitLayerName = null;
        RaycastHit leftPickaxeHitPoint = new RaycastHit();
        RaycastHit rightPickaxeHitPoint = new RaycastHit();
        Vector3 leftHitPoint;
        Vector3 rightHitPoint;

        // Check if both mouse buttons are held down and raycast
        leftPickHit = Input.GetMouseButton(0) && Physics.Raycast(leftPick.position, transform.forward, out leftPickaxeHitPoint, rayDistance);
        rightPickHit = Input.GetMouseButton(1) && Physics.Raycast(rightPick.position, transform.forward, out rightPickaxeHitPoint, rayDistance);

        DestroyJoints();
        if (leftPickHit || rightPickHit) {
            GetHitLayer(
                leftPickHit ? leftPickaxeHitPoint : default, // Pass the valid left RaycastHit or default if not casted
                rightPickHit ? rightPickaxeHitPoint : default // Pass the valid right RaycastHit or default if not casted
            );
        }

        var hitCombination = (leftHitLayerName, rightHitLayerName);
        // Use switch-case to handle different combinations of hit layers
        switch (hitCombination) {
            case ("Ice", "Ice"):
                // Both Pickaxes hit ICE surface
                leftHitPoint = leftPickaxeHitPoint.point;
                rightHitPoint = rightPickaxeHitPoint.point;
                lastLeftHandPosition = playerLeftHand.position;
                lastRightHandPosition = playerRightHand.position;
                MovePlayerTowards((leftHitPoint + rightHitPoint) / 2);

                // Handle particles and sound for both pickaxes
                HandleParticlesAndSound(leftHitPoint, ref leftParticleSpawned, iceParticles, iceHitSound);
                HandleParticlesAndSound(rightHitPoint, ref rightParticleSpawned, iceParticles, iceHitSound);

                break;

            case ("Wood", "Wood"):
                // Both Pickaxes hit WOOD surface
                leftHitPoint = leftPickaxeHitPoint.point;
                rightHitPoint = rightPickaxeHitPoint.point;
                lastLeftHandPosition = playerLeftHand.position;
                lastRightHandPosition = playerRightHand.position;
                MovePlayerTowards((leftHitPoint + rightHitPoint) / 2);

                // Handle particles and sound for both pickaxes
                HandleParticlesAndSound(leftHitPoint, ref leftParticleSpawned, woodParticles, woodHitSound);
                HandleParticlesAndSound(rightHitPoint, ref rightParticleSpawned, woodParticles, woodHitSound);

                break;

            case ("Ice", "Rock"):
                // Left Pickaxe on Ice, Right Pickaxe on Rock
                leftHitPoint = leftPickaxeHitPoint.point;
                rightHitPoint = rightPickaxeHitPoint.point;

                lastLeftHandPosition = playerLeftHand.position;
                AttachJoint(leftHitPoint, Vector3.zero);

                // Handle particles and sound for both pickaxes
                HandleParticlesAndSound(leftHitPoint, ref leftParticleSpawned, iceParticles, iceHitSound);
                HandleParticlesAndSound(rightHitPoint, ref rightParticleSpawned, metalParticles, metalHitSound);

                break;

            case ("Rock", "Ice"):
                // Right Pickaxe on Ice, Left Pickaxe on Rock
                leftHitPoint = leftPickaxeHitPoint.point;
                rightHitPoint = rightPickaxeHitPoint.point;

                lastRightHandPosition = playerRightHand.position;
                AttachJoint(Vector3.zero, rightHitPoint);

                // Handle particles and sound for both pickaxes
                HandleParticlesAndSound(leftHitPoint, ref leftParticleSpawned, metalParticles, metalHitSound);
                HandleParticlesAndSound(rightHitPoint, ref rightParticleSpawned, iceParticles, iceHitSound);

                break;

            case ("Snow", "Rock"):
                // Right Pickaxe on Ice, Left Pickaxe on Rock
                leftHitPoint = leftPickaxeHitPoint.point;
                rightHitPoint = rightPickaxeHitPoint.point;

                lastLeftHandPosition = playerLeftHand.position;
                AttachJoint(leftHitPoint, Vector3.zero);

                // Handle particles and sound for both pickaxes
                HandleParticlesAndSound(leftHitPoint, ref leftParticleSpawned, snowParticles, snowHitSound);
                HandleParticlesAndSound(rightHitPoint, ref rightParticleSpawned, metalParticles, metalHitSound);

                break;

            case ("Rock", "Snow"):
                // Right Pickaxe on Ice, Left Pickaxe on Rock
                leftHitPoint = leftPickaxeHitPoint.point;
                rightHitPoint = rightPickaxeHitPoint.point;

                lastRightHandPosition = playerRightHand.position;
                AttachJoint(Vector3.zero, rightHitPoint);

                // Handle particles and sound for both pickaxes
                HandleParticlesAndSound(leftHitPoint, ref rightParticleSpawned, snowParticles, snowHitSound);
                HandleParticlesAndSound(rightHitPoint, ref leftParticleSpawned, metalParticles, metalHitSound);

                break;

            case ("Rock", "Rock"):
                // Both Pickaxes hit Rock
                leftHitPoint = leftPickaxeHitPoint.point;
                rightHitPoint = rightPickaxeHitPoint.point;

                // Handle particles and sound for both pickaxes
                HandleParticlesAndSound(leftHitPoint, ref leftParticleSpawned, metalParticles, metalHitSound);
                HandleParticlesAndSound(rightHitPoint, ref rightParticleSpawned, metalParticles, metalHitSound);

                break;

            case ("Ice", null):
                // Left Pickaxe hit ICE surface
                leftHitPoint = leftPickaxeHitPoint.point;
                lastLeftHandPosition = playerLeftHand.position;
                AttachJoint(leftHitPoint, Vector3.zero);

                // Handle particles and sound for left pickaxe
                HandleParticlesAndSound(leftHitPoint, ref leftParticleSpawned, iceParticles, iceHitSound);
                rightParticleSpawned = false;

                break;

            case ("Wood", null):
                // Left Pickaxe hit WOOD surface
                leftHitPoint = leftPickaxeHitPoint.point;
                lastLeftHandPosition = playerLeftHand.position;
                AttachJoint(leftHitPoint, Vector3.zero);

                // Handle particles and sound for left pickaxe
                HandleParticlesAndSound(leftHitPoint, ref leftParticleSpawned, woodParticles, woodHitSound);
                rightParticleSpawned = false;

                break;

            case ("Snow", null):
                // Left Pickaxe hit SNOW surface
                leftHitPoint = leftPickaxeHitPoint.point;
                lastLeftHandPosition = playerLeftHand.position;
                AttachJoint(leftHitPoint, Vector3.zero);

                // Handle particles and sound for left pickaxe
                HandleParticlesAndSound(leftHitPoint, ref leftParticleSpawned, snowParticles, snowHitSound);
                rightParticleSpawned = false;

                break;

            case ("Rock", null):
                // Left Pickaxe hit ROCK surface
                leftHitPoint = leftPickaxeHitPoint.point;

                // Handle particles and sound for left pickaxe
                HandleParticlesAndSound(leftHitPoint, ref leftParticleSpawned, metalParticles, metalHitSound);
                rightParticleSpawned = false;

                break;

            case ("Metal", null):
                // Left Pickaxe hit METAL surface
                leftHitPoint = leftPickaxeHitPoint.point;
                lastLeftHandPosition = playerLeftHand.position;
                AttachJoint(leftHitPoint, Vector3.zero);

                // Handle particles and sound for left pickaxe
                HandleParticlesAndSound(leftHitPoint, ref leftParticleSpawned, metalParticles, metalHitSound);
                rightParticleSpawned = false;

                break;

            case (null, "Ice"):
                // Right Pickaxe hit ICE surface
                rightHitPoint = rightPickaxeHitPoint.point;
                lastRightHandPosition = playerRightHand.position;
                AttachJoint(Vector3.zero, rightHitPoint);

                // Handle particles and sound for right pickaxe
                HandleParticlesAndSound(rightHitPoint, ref rightParticleSpawned, iceParticles, iceHitSound);
                leftParticleSpawned = false;

                break;

            case (null, "Wood"):
                // Right Pickaxe hit WOOD surface
                rightHitPoint = rightPickaxeHitPoint.point;
                lastRightHandPosition = playerRightHand.position;
                AttachJoint(Vector3.zero, rightHitPoint);

                // Handle particles and sound for right pickaxe
                HandleParticlesAndSound(rightHitPoint, ref rightParticleSpawned, woodParticles, woodHitSound);
                leftParticleSpawned = false;

                break;

            case (null, "Snow"):
                // Right Pickaxe hit SNOW surface
                rightHitPoint = rightPickaxeHitPoint.point;
                lastRightHandPosition = playerRightHand.position;
                AttachJoint(Vector3.zero, rightHitPoint);

                // Handle particles and sound for right pickaxe
                HandleParticlesAndSound(rightHitPoint, ref rightParticleSpawned, snowParticles, snowHitSound);
                leftParticleSpawned = false;

                break;

            case (null, "Rock"):
                // Right Pickaxe hit ROCK surface
                rightHitPoint = rightPickaxeHitPoint.point;

                // Handle particles and sound for right pickaxe
                HandleParticlesAndSound(rightHitPoint, ref rightParticleSpawned, metalParticles, metalHitSound);
                leftParticleSpawned = false;

                break;

            case (null, "Metal"):
                // Right Pickaxe hit ROCK surface
                rightHitPoint = rightPickaxeHitPoint.point;
                lastRightHandPosition = playerRightHand.position;
                AttachJoint(Vector3.zero, rightHitPoint);

                // Handle particles and sound for right pickaxe
                HandleParticlesAndSound(rightHitPoint, ref rightParticleSpawned, metalParticles, metalHitSound);
                leftParticleSpawned = false;

                break;

            case (null, null):
                // No pickaxe hit anything
                leftParticleSpawned = false;
                rightParticleSpawned = false;
                break;

            default:
                break;
        }
    }

    private void GetHitLayer(RaycastHit leftRaycastHit, RaycastHit rightRaycastHit) {
        // Handle left raycast hit
        if (leftRaycastHit.collider != null) {
            int leftLayer = leftRaycastHit.transform.gameObject.layer;
            leftHitLayerName = LayerMask.LayerToName(leftLayer);
            Debug.Log($"Left Raycast Hit - Layer: {leftLayer} - Layer Name: {leftHitLayerName}");
        }

        // Handle right raycast hit
        if (rightRaycastHit.collider != null) {
            int rightLayer = rightRaycastHit.transform.gameObject.layer;
            rightHitLayerName = LayerMask.LayerToName(rightLayer);
            Debug.Log($"Right Raycast Hit - Layer: {rightLayer} - Layer Name: {rightHitLayerName}");
        }
    }

    private void spawnParticles(Vector3 leftPosition, Vector3 rightPosition, ParticleSystem particleType) {
        // Check and spawn particles at the left position if it's valid
        if (leftPosition != Vector3.zero) {
            Instantiate(particleType, leftPosition, Quaternion.identity);
        }

        // Check and spawn particles at the right position if it's valid
        if (rightPosition != Vector3.zero) {
            Instantiate(particleType, rightPosition, Quaternion.identity);
        }
    }

    private void PlayHitSound(AudioSource soundEffect) {
        soundEffect.Play();
    }

    private void HandleParticlesAndSound(Vector3 hitPoint, ref bool particleSpawnedFlag, ParticleSystem particles, AudioSource sound) {
        if (!particleSpawnedFlag) {
            PlayHitSound(sound);
            spawnParticles(hitPoint, Vector3.zero, particles);
            particleSpawnedFlag = true;
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