using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerManager : MonoBehaviour {

    PlayerMovement playerMovement;
    InputHandler inputHandler;
    ResetPlayer resetPlayer;

    private void Awake() {
        playerMovement = GetComponent<PlayerMovement>();
        inputHandler = GetComponent<InputHandler>();
        resetPlayer = GetComponent<ResetPlayer>();
    }

    private void Update() {
        inputHandler.HandleAllInputs();
        resetPlayer.CheckState();
    }

    private void FixedUpdate() {
        playerMovement.HandleAllMovement();
    }

}
