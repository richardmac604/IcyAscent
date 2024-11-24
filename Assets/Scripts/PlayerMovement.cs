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
    [SerializeField] private ParticleSystem rockParticles;
    [SerializeField] private ParticleSystem woodParticles;
    private ParticleSystem particleInstance;
    private bool leftParticleSpawned = false;
    private bool rightParticleSpawned = false;

    // Audio
    public AudioSource iceHitSound;
    public AudioSource snowHitSound;
    public AudioSource metalHitSound;
    public AudioSource woodHitSound;
    public AudioSource rockHitSound;


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

    private ConfigurableJoint SetupJoint(Vector3 anchorPosition, Vector3 connectedPosition) {
        ConfigurableJoint joint = transform.gameObject.AddComponent<ConfigurableJoint>();
        joint.anchor = joint.transform.InverseTransformPoint(anchorPosition);
        joint.connectedAnchor = connectedPosition;

        // Set primary axis and restrict motions
        joint.axis = transform.InverseTransformDirection(Vector3.forward);
        joint.xMotion = ConfigurableJointMotion.Locked;
        joint.yMotion = ConfigurableJointMotion.Locked;
        joint.zMotion = ConfigurableJointMotion.Locked;
        joint.angularXMotion = ConfigurableJointMotion.Free;
        joint.angularYMotion = ConfigurableJointMotion.Locked;
        joint.angularZMotion = ConfigurableJointMotion.Locked;
        joint.enablePreprocessing = false;

        return joint;
    }


    private void CreateJoints(Vector3 leftHit, Vector3 rightHit) {

        if (leftHit != Vector3.zero) {
            // Left pickaxe hit the wall
            if (leftCJoint == null) {
                leftCJoint = SetupJoint(leftPick.position, leftHit);
            }
        } else if (rightHit != Vector3.zero) {
            // Right pickaxe hit the wall
            if (rightCJoint == null) {
                rightCJoint = SetupJoint(rightPick.position, rightHit);
            }
        }
    }

    private void HandleSwingingJoint(Vector3 leftHit, Vector3 rightHit) {

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
        // switch case to handle different combinations of hit layers
        switch (hitCombination) {
            case ("Ice", "Ice"):
                // Both Pickaxes hit ICE surface
                HandleBothPickaxesHitSameSurface(leftPickaxeHitPoint, rightPickaxeHitPoint, iceParticles, iceHitSound, iceParticles, iceHitSound);
                break;

            case ("Ice", "Snow"):
                // Left Pickaxe hit ICE, right hit SNOW
                HandleBothPickaxesHitSameSurface(leftPickaxeHitPoint, rightPickaxeHitPoint, iceParticles, iceHitSound, snowParticles, snowHitSound);
                break;

            case ("Ice", "Rock"):
                // Left Pickaxe hit ICE, right hit ROCK
                HandleMixedPickaxesHits(leftPickaxeHitPoint, rightPickaxeHitPoint, iceParticles, iceHitSound, rockParticles, rockHitSound, true);
                break;

            case ("Ice", "Metal"):
                // Left Pickaxe hit ICE, right hit METAL
                HandleBothPickaxesHitSameSurface(leftPickaxeHitPoint, rightPickaxeHitPoint, iceParticles, iceHitSound, metalParticles, metalHitSound);
                break;

            case ("Ice", "Wood"):
                // Left Pickaxe hit ICE, right hit WOOD
                HandleBothPickaxesHitSameSurface(leftPickaxeHitPoint, rightPickaxeHitPoint, iceParticles, iceHitSound, woodParticles, woodHitSound);
                break;

            case ("Snow", "Ice"):
                // Left Pickaxe hit SNOW, right hit ICE
                HandleBothPickaxesHitSameSurface(leftPickaxeHitPoint, rightPickaxeHitPoint, snowParticles, snowHitSound, iceParticles, iceHitSound);
                break;

            case ("Snow", "Snow"):
                // Both Pickaxes hit SNOW surface
                HandleBothPickaxesHitSameSurface(leftPickaxeHitPoint, rightPickaxeHitPoint, snowParticles, snowHitSound, snowParticles, snowHitSound);
                break;

            case ("Snow", "Rock"):
                // Left Pickaxe hit SNOW, right hit ROCK
                HandleMixedPickaxesHits(leftPickaxeHitPoint, rightPickaxeHitPoint, snowParticles, snowHitSound, rockParticles, rockHitSound, true);
                break;

            case ("Snow", "Metal"):
                // Left Pickaxe hit SNOW, right hit METAL
                HandleBothPickaxesHitSameSurface(leftPickaxeHitPoint, rightPickaxeHitPoint, snowParticles, snowHitSound, metalParticles, metalHitSound);
                break;

            case ("Snow", "Wood"):
                // Left Pickaxe hit SNOW, right hit WOOD
                HandleBothPickaxesHitSameSurface(leftPickaxeHitPoint, rightPickaxeHitPoint, snowParticles, snowHitSound, woodParticles, woodHitSound);
                break;

            case ("Rock", "Ice"):
                // Left Pickaxe hit ROCK, right hit ICE
                HandleMixedPickaxesHits(leftPickaxeHitPoint, rightPickaxeHitPoint, rockParticles, rockHitSound, iceParticles, iceHitSound, false);
                break;

            case ("Rock", "Snow"):
                // Left Pickaxe hit ROCK, right hit SNOW
                HandleMixedPickaxesHits(leftPickaxeHitPoint, rightPickaxeHitPoint, rockParticles, rockHitSound, snowParticles, snowHitSound, false);
                break;

            case ("Rock", "Metal"):
                // Left Pickaxe hit ROCK, right hit METAL
                HandleMixedPickaxesHits(leftPickaxeHitPoint, rightPickaxeHitPoint, rockParticles, rockHitSound, metalParticles, metalHitSound, false);
                break;

            case ("Rock", "Rock"):
                // Both Pickaxes hit ROCK
                leftHitPoint = leftPickaxeHitPoint.point;
                rightHitPoint = rightPickaxeHitPoint.point;

                // Handle particles and sound for both pickaxes
                HandleParticlesAndSound(leftHitPoint, ref leftParticleSpawned, rockParticles, rockHitSound);
                HandleParticlesAndSound(rightHitPoint, ref rightParticleSpawned, rockParticles, rockHitSound);
                break;

            case ("Rock", "Wood"):
                // Left Pickaxe hit ROCK, right hit WOOD
                HandleMixedPickaxesHits(leftPickaxeHitPoint, rightPickaxeHitPoint, rockParticles, rockHitSound, woodParticles, woodHitSound, false);
                break;

            case ("Metal", "Ice"):
                // Left Pickaxe hit METAL, right hit ICE
                HandleBothPickaxesHitSameSurface(leftPickaxeHitPoint, rightPickaxeHitPoint, metalParticles, metalHitSound, iceParticles, iceHitSound);
                break;

            case ("Metal", "Snow"):
                // Left Pickaxe hit METAL, right hit SNOW
                HandleBothPickaxesHitSameSurface(leftPickaxeHitPoint, rightPickaxeHitPoint, metalParticles, metalHitSound, snowParticles, snowHitSound);
                break;

            case ("Metal", "Rock"):
                // Left Pickaxe hit METAL, right hit ROCK
                HandleMixedPickaxesHits(leftPickaxeHitPoint, rightPickaxeHitPoint, metalParticles, metalHitSound, rockParticles, rockHitSound, true);
                break;

            case ("Metal", "Metal"):
                // Both Pickaxes hit METAL
                HandleBothPickaxesHitSameSurface(leftPickaxeHitPoint, rightPickaxeHitPoint, metalParticles, metalHitSound, metalParticles, metalHitSound);
                break;

            case ("Metal", "Wood"):
                // Left Pickaxe hit METAL, right hit WOOD
                HandleBothPickaxesHitSameSurface(leftPickaxeHitPoint, rightPickaxeHitPoint, metalParticles, metalHitSound, woodParticles, woodHitSound);
                break;

            case ("Wood", "Metal"):
                // Left Pickaxe hit WOOD, right hit METAL
                HandleBothPickaxesHitSameSurface(leftPickaxeHitPoint, rightPickaxeHitPoint, woodParticles, woodHitSound, metalParticles, metalHitSound);
                break;

            case ("Wood", "Ice"):
                // Left Pickaxe hit WOOD, right hit ICE
                HandleBothPickaxesHitSameSurface(leftPickaxeHitPoint, rightPickaxeHitPoint, woodParticles, woodHitSound, iceParticles, iceHitSound);
                break;

            case ("Wood", "Snow"):
                // Left Pickaxe hit WOOD, right hit SNOW
                HandleBothPickaxesHitSameSurface(leftPickaxeHitPoint, rightPickaxeHitPoint, woodParticles, woodHitSound, snowParticles, snowHitSound);
                break;

            case ("Wood", "Rock"):
                // Left Pickaxe hit WOOD, right hit ROCK
                HandleMixedPickaxesHits(leftPickaxeHitPoint, rightPickaxeHitPoint, woodParticles, woodHitSound, rockParticles, rockHitSound, true);
                break;

            case ("Wood", "Wood"):
                // Both Pickaxes hit WOOD
                HandleBothPickaxesHitSameSurface(leftPickaxeHitPoint, rightPickaxeHitPoint, woodParticles, woodHitSound, woodParticles, woodHitSound);
                break;

            // LEFT pickaxe hit cases
            case ("Ice", null):
                // Left Pickaxe hit ICE
                HandleMixedPickaxesHits(leftPickaxeHitPoint, rightPickaxeHitPoint, iceParticles, iceHitSound, null, null, true);
                break;

            case ("Snow", null):
                // Left Pickaxe hit SNOW
                HandleMixedPickaxesHits(leftPickaxeHitPoint, rightPickaxeHitPoint, snowParticles, snowHitSound, null, null, true);
                break;

            case ("Rock", null):
                // Left Pickaxe hit ROCK
                leftHitPoint = leftPickaxeHitPoint.point;

                // Handle particles and sound for left pickaxe
                HandleParticlesAndSound(leftHitPoint, ref leftParticleSpawned, rockParticles, rockHitSound);
                break;

            case ("Metal", null):
                // Left Pickaxe hit METAL
                HandleMixedPickaxesHits(leftPickaxeHitPoint, rightPickaxeHitPoint, metalParticles, metalHitSound, null, null, true);
                break;

            case ("Wood", null):
                // Left Pickaxe hit WOOD
                HandleMixedPickaxesHits(leftPickaxeHitPoint, rightPickaxeHitPoint, woodParticles, woodHitSound, null, null, true);
                break;

            // RIGHT pickaxe hit cases
            case (null, "Ice"):
                // Right Pickaxe hit ICE
                HandleMixedPickaxesHits(leftPickaxeHitPoint, rightPickaxeHitPoint, null, null, iceParticles, iceHitSound, false);
                break;

            case (null, "Snow"):
                // Right Pickaxe hit SNOW
                HandleMixedPickaxesHits(leftPickaxeHitPoint, rightPickaxeHitPoint, null, null, snowParticles, snowHitSound, false);
                break;

            case (null, "Rock"):
                // Right Pickaxe hit ROCK
                rightHitPoint = rightPickaxeHitPoint.point;

                // Handle particles and sound for right pickaxe
                HandleParticlesAndSound(rightHitPoint, ref rightParticleSpawned, rockParticles, rockHitSound);
                break;

            case (null, "Metal"):
                // Right Pickaxe hit METAL
                HandleMixedPickaxesHits(leftPickaxeHitPoint, rightPickaxeHitPoint, null, null, metalParticles, metalHitSound, false);
                break;

            case (null, "Wood"):
                // Right Pickaxe hit WOOD
                HandleMixedPickaxesHits(leftPickaxeHitPoint, rightPickaxeHitPoint, null, null, woodParticles, woodHitSound, false);
                break;

            case (null, null):
                // No pickaxe hit anything
                leftParticleSpawned = false;
                rightParticleSpawned = false;
                break;

            default:
                // No pickaxe hit anything or invalid combination
                break;
        }
    }

    private void HandleBothPickaxesHitSameSurface(RaycastHit leftPickaxeHitPoint, RaycastHit rightPickaxeHitPoint, ParticleSystem leftParticles, AudioSource leftSound, ParticleSystem rightParticles, AudioSource rightSound) {
        // Get hit points
        Vector3 leftHitPoint = leftPickaxeHitPoint.point;
        Vector3 rightHitPoint = rightPickaxeHitPoint.point;

        // Update player hand positions
        lastLeftHandPosition = playerLeftHand.position;
        lastRightHandPosition = playerRightHand.position;

        // Handle particles and sound
        HandleParticlesAndSound(leftHitPoint, ref leftParticleSpawned, leftParticles, leftSound);
        HandleParticlesAndSound(rightHitPoint, ref rightParticleSpawned, rightParticles, rightSound);

        // Move player towards the midpoint of the hit points
        MovePlayerTowards((leftHitPoint + rightHitPoint) / 2);

    }

    private void HandleMixedPickaxesHits(RaycastHit leftPickaxeHitPoint, RaycastHit rightPickaxeHitPoint, ParticleSystem leftParticles, AudioSource leftSound, ParticleSystem rightParticles, AudioSource rightSound, bool isLeft) {
        // Get hit points
        Vector3 leftHitPoint = leftPickaxeHitPoint.point;
        Vector3 rightHitPoint = rightPickaxeHitPoint.point;

        // Handle particles and sound
        if (leftParticles != null) { 
            HandleParticlesAndSound(leftHitPoint, ref leftParticleSpawned, leftParticles, leftSound);
        } else {
            leftParticleSpawned = false;
        }
        if (rightParticles != null) { 
            HandleParticlesAndSound(rightHitPoint, ref rightParticleSpawned, rightParticles, rightSound);
        } else {
            rightParticleSpawned = false;
        }

        if (isLeft) {
            // Update last hand position for the left hand
            lastLeftHandPosition = playerLeftHand.position;
            // Attach joint only for the left pickaxe
            HandleSwingingJoint(leftHitPoint, Vector3.zero);
        } else {
            // Update last hand position for the right hand
            lastRightHandPosition = playerRightHand.position;
            // Attach joint only for the right pickaxe
            HandleSwingingJoint(Vector3.zero, rightHitPoint);
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