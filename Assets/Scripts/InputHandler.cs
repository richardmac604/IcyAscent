using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputHandler : MonoBehaviour {

    InputActions inputActions;
    public Vector2 movementInput;

    public float verticalInput;
    public float horizontalInput;

    private void OnEnable() {
        inputActions = new InputActions();
        inputActions.Player.MouseMovement.performed += i => movementInput = i.ReadValue<Vector2>();
        inputActions.Enable();
    }

    private void OnDisable() {
        inputActions.Disable();
    }
    public void HandleAllInputs() {
        HandleMovementInput();
    }
    private void HandleMovementInput() {
        verticalInput = movementInput.y;
        horizontalInput = movementInput.x;
    }

}
