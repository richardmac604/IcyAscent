using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour {

    PlayerMovement playerMovement;
    InputHandler inputHandler;

    private void Awake() {
        playerMovement = GetComponent<PlayerMovement>();
        inputHandler = GetComponent<InputHandler>();
    }

    private void Update() {
        inputHandler.HandleAllInputs();
    }

    private void FixedUpdate() {
        playerMovement.HandleAllMovement();
    }

}
