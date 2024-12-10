using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using UnityEngine;
using Unity.Mathematics;
using Unity.VisualScripting;
using static UnityEngine.ParticleSystem;
using TMPro;

public class PlayerMovement : MonoBehaviour {

    // Constants
    private const float movementSpeed = 3f; // Arm movement speed
    private const float lerpSpeed = 1f;     // Smoothing for player pull up
    private const float pullUpSpeed = 5f;   // How fast the player pulls up with both pickaxes hit
    private const float swaySpeed = 3f;     // How much the player sways by larger value faster
    private const float maxMomentum = 3f;   // Maximum swinging momentum

    // Private Fields
    InputHandler inputHandler;
    Rigidbody playerRigidbody;
    private ConfigurableJoint leftCJoint;
    private ConfigurableJoint rightCJoint;
    private float rayDistance = 50f;           // Length of Ray
    bool leftPickHit = false;                  // Track if left mouse button down and raycast is hitting
    bool rightPickHit = false;                 // Track if right mouse button down and raycast is hitting
    private string leftHitLayerName;           // Current layer player is anchord to
    private string rightHitLayerName;          // Current layer player is anchord to
    private bool isSwinging = false;           // Track if the player is currently swinging (anchored)
    private bool isSlidingDownOnSnow = false;  // Track if the player is sliding down
    private float slidingDuration = 0.5f;      // The time (in seconds) before relocking the Y-axis
    private float slidingTimer = 0f;           // Timer to track how long the player has been sliding
    private bool isSlidingTimerActive = false; // To track if the sliding timer is active
    private bool leftParticleSpawned = false;  // Track if leftPickaxe particle is spawned
    private bool rightParticleSpawned = false; // Track if rightPickaxe particle is spawned
    private float leftShoulderToHandLength;    // Length of left shoulder to hand
    private float rightShoulderToHandLength;   // Length of right shoulder to hand
    private Vector3 lastLeftHandPosition;      // Last recorded position of the left hand
    private Vector3 lastRightHandPosition;     // Last recorded position of the right hand

    // Public Fields
    [Header("Arm Movement Settings")]
    public Transform playerLeftHand;       // Transform for the player's left hand
    public Transform playerRightHand;      // Transform for the player's right hand
    public Transform playerLeftArmTarget;  // Target position for the player's left arm
    public Transform playerRightArmTarget; // Target position for the player's right arm
    public Transform playerLeftShoulder;   // Transform for the player's left shoulder
    public Transform playerRightShoulder;  // Transform for the player's right shoulder
    public Transform leftPick;             // Hit point on the tip of the pickaxe
    public Transform rightPick;            // Hit point on the tip of the pickaxe

    private bool debugMode = false;


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

    private void Update() {
        HandleSlidingTime();
        HandleSlidingAnchorPosition();
        HandleDebugMode();
    }

    private void HandleDebugMode() {
        // Toggle debug mode when Right Shift is pressed
        if (Input.GetKeyDown(KeyCode.RightShift)) {
            debugMode = !debugMode;
            playerRigidbody.isKinematic = debugMode;
            playerRigidbody.useGravity = !debugMode;
        }

        if (debugMode) {
            Vector3 debugMovement = Vector3.zero;

            if (Input.GetKey(KeyCode.UpArrow)) {
                debugMovement += Vector3.up;
            }
            if (Input.GetKey(KeyCode.DownArrow)) {
                debugMovement += Vector3.down;
            }
            if (Input.GetKey(KeyCode.LeftArrow)) {
                debugMovement += Vector3.left;
            }
            if (Input.GetKey(KeyCode.RightArrow)) {
                debugMovement += Vector3.right;
            }

            transform.position += debugMovement * 20f * Time.deltaTime;
        }
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
        if (debugMode == false) {
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
    }

    private void HandleSlidingTime() {
        // Handle sliding timer
        if (isSlidingTimerActive) {
            slidingTimer += Time.deltaTime;
            if (slidingTimer >= slidingDuration) {
                RelockYMotion();
                isSlidingTimerActive = false;
            }
        }
    }

    private void HandleSlidingAnchorPosition() {
        if (isSlidingDownOnSnow) {
            if (leftCJoint != null) {
                UpdateConnectedAnchorPosition(leftCJoint, leftPick);
            }
            if (rightCJoint != null) {
                UpdateConnectedAnchorPosition(rightCJoint, rightPick);
            }
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

    private void UpdateConnectedAnchorPosition(ConfigurableJoint joint, Transform pickaxe) {
        if (joint != null) {
            // Update the connected anchor to follow the pickaxe's current position
            joint.connectedAnchor = joint.transform.InverseTransformPoint(pickaxe.position);
        }
    }

    private void RelockYMotion() {
        if (leftCJoint != null) {
            leftCJoint.yMotion = ConfigurableJointMotion.Locked;
        }

        if (rightCJoint != null) {
            rightCJoint.yMotion = ConfigurableJointMotion.Locked;
        }

        // Reset the sliding state
        isSlidingDownOnSnow = false;
    }

    private void EnableSlidingOnSnow(ConfigurableJoint joint) {
        // Allow limited movement along the Y-axis to simulate sliding down
        joint.yMotion = ConfigurableJointMotion.Limited;

        // Start the sliding timer
        slidingTimer = 0f;
        isSlidingTimerActive = true;

        // Configure the joint limit for sliding
        SoftJointLimit yLimit = new SoftJointLimit {
            limit = 2f  // Adjust this to control the amount of allowed sliding
        };
        joint.linearLimit = yLimit;
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
                // Check if the player is hitting snow and apply sliding effect
                if (leftHitLayerName == "Snow") {
                    isSlidingDownOnSnow = true;
                    EnableSlidingOnSnow(leftCJoint);
                } else {
                    isSlidingDownOnSnow = false;
                }
            }
        } else if (rightHit != Vector3.zero) {
            // Right pickaxe hit the wall
            if (rightCJoint == null) {
                rightCJoint = SetupJoint(rightPick.position, rightHit);
                // Check if the player is hitting snow and apply sliding effect
                if (rightHitLayerName == "Snow") {
                    isSlidingDownOnSnow = true;
                    EnableSlidingOnSnow(rightCJoint);
                } else {
                    isSlidingDownOnSnow = false;
                }
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
                HandleBothPickaxesHitSameSurface(leftPickaxeHitPoint, rightPickaxeHitPoint, ParticleManager.ParticleType.Ice, SoundManager.Sound.IceHit, ParticleManager.ParticleType.Ice, SoundManager.Sound.IceHit);
                break;

            case ("Ice", "Snow"):
                // Left Pickaxe hit ICE, right hit SNOW
                HandleBothPickaxesHitSameSurface(leftPickaxeHitPoint, rightPickaxeHitPoint, ParticleManager.ParticleType.Ice, SoundManager.Sound.IceHit, ParticleManager.ParticleType.Snow, SoundManager.Sound.SnowHit);
                break;

            case ("Ice", "Rock"):
                // Left Pickaxe hit ICE, right hit ROCK
                HandleMixedPickaxesHits(leftPickaxeHitPoint, rightPickaxeHitPoint, ParticleManager.ParticleType.Ice, SoundManager.Sound.IceHit, ParticleManager.ParticleType.Rock, SoundManager.Sound.RockHit, true);
                break;

            case ("Ice", "Metal"):
                // Left Pickaxe hit ICE, right hit METAL
                HandleMixedPickaxesHits(leftPickaxeHitPoint, rightPickaxeHitPoint, ParticleManager.ParticleType.Ice, SoundManager.Sound.IceHit, ParticleManager.ParticleType.Metal, SoundManager.Sound.MetalHit, true);
                break;

            case ("Ice", "Wood"):
                // Left Pickaxe hit ICE, right hit WOOD
                HandleBothPickaxesHitSameSurface(leftPickaxeHitPoint, rightPickaxeHitPoint, ParticleManager.ParticleType.Ice, SoundManager.Sound.IceHit, ParticleManager.ParticleType.Wood, SoundManager.Sound.WoodHit);
                break;

            case ("Snow", "Ice"):
                // Left Pickaxe hit SNOW, right hit ICE
                HandleBothPickaxesHitSameSurface(leftPickaxeHitPoint, rightPickaxeHitPoint, ParticleManager.ParticleType.Snow, SoundManager.Sound.SnowHit, ParticleManager.ParticleType.Ice, SoundManager.Sound.IceHit);
                break;

            case ("Snow", "Snow"):
                // Both Pickaxes hit SNOW surface
                HandleBothPickaxesHitSameSurface(leftPickaxeHitPoint, rightPickaxeHitPoint, ParticleManager.ParticleType.Snow, SoundManager.Sound.SnowHit, ParticleManager.ParticleType.Snow, SoundManager.Sound.SnowHit);
                break;

            case ("Snow", "Rock"):
                // Left Pickaxe hit SNOW, right hit ROCK
                HandleMixedPickaxesHits(leftPickaxeHitPoint, rightPickaxeHitPoint, ParticleManager.ParticleType.Snow, SoundManager.Sound.SnowHit, ParticleManager.ParticleType.Rock, SoundManager.Sound.RockHit, true);
                break;

            case ("Snow", "Metal"):
                // Left Pickaxe hit SNOW, right hit METAL
                HandleMixedPickaxesHits(leftPickaxeHitPoint, rightPickaxeHitPoint, ParticleManager.ParticleType.Snow, SoundManager.Sound.SnowHit, ParticleManager.ParticleType.Metal, SoundManager.Sound.MetalHit, true);
                break;

            case ("Snow", "Wood"):
                // Left Pickaxe hit SNOW, right hit WOOD
                HandleBothPickaxesHitSameSurface(leftPickaxeHitPoint, rightPickaxeHitPoint, ParticleManager.ParticleType.Snow, SoundManager.Sound.SnowHit, ParticleManager.ParticleType.Wood, SoundManager.Sound.WoodHit);
                break;

            case ("Rock", "Ice"):
                // Left Pickaxe hit ROCK, right hit ICE
                HandleMixedPickaxesHits(leftPickaxeHitPoint, rightPickaxeHitPoint, ParticleManager.ParticleType.Rock, SoundManager.Sound.RockHit, ParticleManager.ParticleType.Ice, SoundManager.Sound.IceHit, false);
                break;

            case ("Rock", "Snow"):
                // Left Pickaxe hit ROCK, right hit SNOW
                HandleMixedPickaxesHits(leftPickaxeHitPoint, rightPickaxeHitPoint, ParticleManager.ParticleType.Rock, SoundManager.Sound.RockHit, ParticleManager.ParticleType.Snow, SoundManager.Sound.SnowHit, false);
                break;

            case ("Rock", "Metal"):
                // Left Pickaxe hit ROCK, right hit METAL
                leftHitPoint = leftPickaxeHitPoint.point;
                rightHitPoint = rightPickaxeHitPoint.point;

                // Handle particles and sound for both pickaxes
                HandleParticlesAndSound(leftHitPoint, ref leftParticleSpawned, ParticleManager.ParticleType.Rock, SoundManager.Sound.RockHit);
                HandleParticlesAndSound(rightHitPoint, ref rightParticleSpawned, ParticleManager.ParticleType.Metal, SoundManager.Sound.MetalHit);
                break;

            case ("Rock", "Rock"):
                // Both Pickaxes hit ROCK
                leftHitPoint = leftPickaxeHitPoint.point;
                rightHitPoint = rightPickaxeHitPoint.point;

                // Handle particles and sound for both pickaxes
                HandleParticlesAndSound(leftHitPoint, ref leftParticleSpawned, ParticleManager.ParticleType.Rock, SoundManager.Sound.RockHit);
                HandleParticlesAndSound(rightHitPoint, ref rightParticleSpawned, ParticleManager.ParticleType.Rock, SoundManager.Sound.RockHit);
                break;

            case ("Rock", "Wood"):
                // Left Pickaxe hit ROCK, right hit WOOD
                HandleMixedPickaxesHits(leftPickaxeHitPoint, rightPickaxeHitPoint, ParticleManager.ParticleType.Rock, SoundManager.Sound.RockHit, ParticleManager.ParticleType.Wood, SoundManager.Sound.WoodHit, false);
                break;

            case ("Metal", "Ice"):
                // Left Pickaxe hit METAL, right hit ICE
                HandleMixedPickaxesHits(leftPickaxeHitPoint, rightPickaxeHitPoint, ParticleManager.ParticleType.Metal, SoundManager.Sound.MetalHit, ParticleManager.ParticleType.Ice, SoundManager.Sound.IceHit, false);
                break;

            case ("Metal", "Snow"):
                // Left Pickaxe hit METAL, right hit SNOW
                HandleMixedPickaxesHits(leftPickaxeHitPoint, rightPickaxeHitPoint, ParticleManager.ParticleType.Metal, SoundManager.Sound.MetalHit, ParticleManager.ParticleType.Snow, SoundManager.Sound.SnowHit, false);
                break;

            case ("Metal", "Rock"):
                // Left Pickaxe hit METAL, right hit ROCK
                leftHitPoint = leftPickaxeHitPoint.point;
                rightHitPoint = rightPickaxeHitPoint.point;

                // Handle particles and sound for both pickaxes
                HandleParticlesAndSound(leftHitPoint, ref leftParticleSpawned, ParticleManager.ParticleType.Metal, SoundManager.Sound.MetalHit);
                HandleParticlesAndSound(rightHitPoint, ref rightParticleSpawned, ParticleManager.ParticleType.Rock, SoundManager.Sound.RockHit);
                break;

            case ("Metal", "Metal"):
                // Both Pickaxes hit METAL
                leftHitPoint = leftPickaxeHitPoint.point;
                rightHitPoint = rightPickaxeHitPoint.point;

                // Handle particles and sound for both pickaxes
                HandleParticlesAndSound(leftHitPoint, ref leftParticleSpawned, ParticleManager.ParticleType.Metal, SoundManager.Sound.MetalHit);
                HandleParticlesAndSound(rightHitPoint, ref rightParticleSpawned, ParticleManager.ParticleType.Metal, SoundManager.Sound.MetalHit);
                break;

            case ("Metal", "Wood"):
                // Left Pickaxe hit METAL, right hit WOOD
                HandleMixedPickaxesHits(leftPickaxeHitPoint, rightPickaxeHitPoint, ParticleManager.ParticleType.Metal, SoundManager.Sound.MetalHit, ParticleManager.ParticleType.Wood, SoundManager.Sound.WoodHit, false);
                break;

            case ("Wood", "Metal"):
                // Left Pickaxe hit WOOD, right hit METAL
                HandleBothPickaxesHitSameSurface(leftPickaxeHitPoint, rightPickaxeHitPoint, ParticleManager.ParticleType.Wood, SoundManager.Sound.WoodHit, ParticleManager.ParticleType.Metal, SoundManager.Sound.MetalHit);
                break;

            case ("Wood", "Ice"):
                // Left Pickaxe hit WOOD, right hit ICE
                HandleBothPickaxesHitSameSurface(leftPickaxeHitPoint, rightPickaxeHitPoint, ParticleManager.ParticleType.Wood, SoundManager.Sound.WoodHit, ParticleManager.ParticleType.Ice, SoundManager.Sound.IceHit);
                break;

            case ("Wood", "Snow"):
                // Left Pickaxe hit WOOD, right hit SNOW
                HandleBothPickaxesHitSameSurface(leftPickaxeHitPoint, rightPickaxeHitPoint, ParticleManager.ParticleType.Wood, SoundManager.Sound.WoodHit, ParticleManager.ParticleType.Snow, SoundManager.Sound.SnowHit);
                break;

            case ("Wood", "Rock"):
                // Left Pickaxe hit WOOD, right hit ROCK
                HandleMixedPickaxesHits(leftPickaxeHitPoint, rightPickaxeHitPoint, ParticleManager.ParticleType.Wood, SoundManager.Sound.WoodHit, ParticleManager.ParticleType.Rock, SoundManager.Sound.RockHit, true);
                break;

            case ("Wood", "Wood"):
                // Both Pickaxes hit WOOD
                HandleBothPickaxesHitSameSurface(leftPickaxeHitPoint, rightPickaxeHitPoint, ParticleManager.ParticleType.Wood, SoundManager.Sound.WoodHit, ParticleManager.ParticleType.Wood, SoundManager.Sound.WoodHit);
                break;

            // LEFT pickaxe hit cases
            case ("Ice", null):
                // Left Pickaxe hit ICE
                HandleMixedPickaxesHits(leftPickaxeHitPoint, rightPickaxeHitPoint, ParticleManager.ParticleType.Ice, SoundManager.Sound.IceHit, ParticleManager.ParticleType.None, SoundManager.Sound.None, true);
                break;

            case ("Snow", null):
                // Left Pickaxe hit SNOW
                HandleMixedPickaxesHits(leftPickaxeHitPoint, rightPickaxeHitPoint, ParticleManager.ParticleType.Snow, SoundManager.Sound.SnowHit, ParticleManager.ParticleType.None, SoundManager.Sound.None, true);
                break;

            case ("Rock", null):
                // Left Pickaxe hit ROCK
                leftHitPoint = leftPickaxeHitPoint.point;

                // Handle particles and sound for left pickaxe
                HandleParticlesAndSound(leftHitPoint, ref leftParticleSpawned, ParticleManager.ParticleType.Rock, SoundManager.Sound.RockHit);
                break;

            case ("Metal", null):
                // Left Pickaxe hit METAL
                leftHitPoint = leftPickaxeHitPoint.point;

                // Handle particles and sound for left pickaxe
                HandleParticlesAndSound(leftHitPoint, ref leftParticleSpawned, ParticleManager.ParticleType.Metal, SoundManager.Sound.MetalHit);
                break;

            case ("Wood", null):
                // Left Pickaxe hit WOOD
                HandleMixedPickaxesHits(leftPickaxeHitPoint, rightPickaxeHitPoint, ParticleManager.ParticleType.Wood, SoundManager.Sound.WoodHit, ParticleManager.ParticleType.None, SoundManager.Sound.None, true);
                break;

            // RIGHT pickaxe hit cases
            case (null, "Ice"):
                // Right Pickaxe hit ICE
                HandleMixedPickaxesHits(leftPickaxeHitPoint, rightPickaxeHitPoint, ParticleManager.ParticleType.None, SoundManager.Sound.None, ParticleManager.ParticleType.Ice, SoundManager.Sound.IceHit, false);
                break;

            case (null, "Snow"):
                // Right Pickaxe hit SNOW
                HandleMixedPickaxesHits(leftPickaxeHitPoint, rightPickaxeHitPoint, ParticleManager.ParticleType.None, SoundManager.Sound.None, ParticleManager.ParticleType.Snow, SoundManager.Sound.SnowHit, false);
                break;

            case (null, "Rock"):
                // Right Pickaxe hit ROCK
                rightHitPoint = rightPickaxeHitPoint.point;

                // Handle particles and sound for right pickaxe
                HandleParticlesAndSound(rightHitPoint, ref rightParticleSpawned, ParticleManager.ParticleType.Rock, SoundManager.Sound.RockHit);
                break;

            case (null, "Metal"):
                // Right Pickaxe hit METAL
                rightHitPoint = rightPickaxeHitPoint.point;

                // Handle particles and sound for right pickaxe
                HandleParticlesAndSound(rightHitPoint, ref rightParticleSpawned, ParticleManager.ParticleType.Metal, SoundManager.Sound.MetalHit);
                break;

            case (null, "Wood"):
                // Right Pickaxe hit WOOD
                HandleMixedPickaxesHits(leftPickaxeHitPoint, rightPickaxeHitPoint, ParticleManager.ParticleType.None, SoundManager.Sound.None, ParticleManager.ParticleType.Wood, SoundManager.Sound.WoodHit, false);
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

    private void HandleBothPickaxesHitSameSurface(RaycastHit leftPickaxeHitPoint, RaycastHit rightPickaxeHitPoint, ParticleManager.ParticleType leftParticles, SoundManager.Sound leftSound, ParticleManager.ParticleType rightParticles, SoundManager.Sound rightSound) {
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

    private void HandleMixedPickaxesHits(RaycastHit leftPickaxeHitPoint, RaycastHit rightPickaxeHitPoint, ParticleManager.ParticleType leftParticles, SoundManager.Sound leftSound, ParticleManager.ParticleType rightParticles, SoundManager.Sound rightSound, bool isLeft) {
        // Get hit points
        Vector3 leftHitPoint = leftPickaxeHitPoint.point;
        Vector3 rightHitPoint = rightPickaxeHitPoint.point;

        // Handle particles and sound
        if (leftParticles != ParticleManager.ParticleType.None) {
            HandleParticlesAndSound(leftHitPoint, ref leftParticleSpawned, leftParticles, leftSound);
        } else {
            leftParticleSpawned = false;
        }
        if (rightParticles != ParticleManager.ParticleType.None) {
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
        }

        // Handle right raycast hit
        if (rightRaycastHit.collider != null) {
            int rightLayer = rightRaycastHit.transform.gameObject.layer;
            rightHitLayerName = LayerMask.LayerToName(rightLayer);
        }
    }

    private void HandleParticlesAndSound(Vector3 hitPoint, ref bool particleSpawnedFlag, ParticleManager.ParticleType particle, SoundManager.Sound sound) {
        if (!particleSpawnedFlag) {
            SoundManager.PlaySound(sound);
            ParticleManager.Instance.PlayParticle(particle, hitPoint);
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
