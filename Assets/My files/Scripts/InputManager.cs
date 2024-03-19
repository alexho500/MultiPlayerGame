using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

//Handles input for a PlayerController component
public class InputManager : MonoBehaviour
{
    // Reference to the new input system actions.
    private PlayerInput playerInput;

    //Actions for the on foot movement
    private PlayerInput.OnFootActions OnFoot;

    //Rrferences to playerController script 
    private PlayerController playerController;

    void Awake()
    {
        // Initialize the PlayerInput.
        playerInput = new PlayerInput();
        // Assign the on-foot actions from the PlayerInput.
        OnFoot = playerInput.OnFoot;

        playerController = GetComponent<PlayerController>();

         // Subscribe to input actions with corresponding methods in PlayerController.
        OnFoot.Jump.performed += ctx => playerController.Jump();
        OnFoot.Crouch.performed += ctx => playerController.Crouch();
        OnFoot.Sprint.performed += ctx => playerController.Sprint();
    }
    // Processes player movement using physics updates.
    void FixedUpdate()
    {
        if (playerController != null && playerController.IsOwner)
        {
            
            Vector2 movementInput = OnFoot.Movement.ReadValue<Vector2>();
            playerController.ProcessMove(movementInput);
        }
    }
    // Updates camera orientation based on player input.
    void LateUpdate()
    {
        if (playerController != null && playerController.IsOwner)
        {
            
            Vector2 lookInput = OnFoot.Look.ReadValue<Vector2>();
            playerController.ProcessLook(lookInput);
        }
    }

    private void OnEnable()
    {
        
        //Actions are disabled when Gameobjects are disabled
        if (playerController.IsOwner)
        {
            OnFoot.Enable();
        }
    }

    private void OnDisable()
    {
        //Actions are disabled when Gameobjects are disabled
        if (playerController.IsOwner)
        {
           
            OnFoot.Disable();
        }
    }
}