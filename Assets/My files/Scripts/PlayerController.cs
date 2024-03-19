using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;


//Player controls, movemonet, camera , and audio
public class PlayerController : NetworkBehaviour
{
    // References to components for visual and audio feedback.
    public Camera cam;
    public CharacterController controller;
    public AudioSource audioSource;
    public AudioClip walkingSound;
    public AudioClip sprintingSound;


    // Player movement details and states
    private float xRotation = 0f;
    private Vector3 playerVelocity;
    private bool isGrounded;
    private bool crouching = false;
    private bool sprinting = false;
    private bool lerpCrouch = false;
    private float crouchTimer = 0f;


    // Configuration for player movement and camera.
    public float speed = 5f;
    public float gravity = -9.8f;
    public float jumpHeight = 3f;
    public float xSensitivity = 30f;
    public float ySensitivity = 30f;

    // Camera is only active for the player that owns this object
    public override void OnNetworkSpawn() {
    if (!IsOwner) {
        cam.enabled = false;
        // Disable any scripts or components that should only be active for the owning player.
    } else {
        cam.enabled = true;
    }
}

    void Start()
    {
        // Initialize character controller and audio source components
        controller = GetComponent<CharacterController>();
        audioSource = GetComponent<AudioSource>();
    }

    void Update()
    {
        // Ensure Update logic runs only for the owner.
        if (IsOwner)
        {
            ProcessInput();
        }
    }

    // Gathers and processes player input for movement, looking around, and actions like jumping.
    void ProcessInput()
    {
        Vector2 movementInput = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical"));
        Vector2 lookInput = new Vector2(Input.GetAxis("Mouse X"), Input.GetAxis("Mouse Y"));

        ProcessMove(movementInput);
        ProcessLook(lookInput);
        HandleFootstepSounds(movementInput);

        if (Input.GetButtonDown("Jump"))
        {
            Jump();
        }

        if (Input.GetKeyDown(KeyCode.LeftControl))
        {
            Crouch();
        }

        if (Input.GetKeyDown(KeyCode.LeftShift))
        {
            Sprint();
        }
    }

    // Handles camera movement based on mouse input
    public void ProcessLook(Vector2 input)
    {
        if (!cam.gameObject.activeInHierarchy) return; // Ensure we only process look for active cameras.

        float mouseX = input.x;
        float mouseY = input.y;

        xRotation -= (mouseY * Time.deltaTime) * ySensitivity;
        xRotation = Mathf.Clamp(xRotation, -80f, 80f);
        cam.transform.localRotation = Quaternion.Euler(xRotation, 0, 0);
        transform.Rotate(Vector3.up * (mouseX * Time.deltaTime) * xSensitivity);
    }
    
     // Moves the player based on input and applies gravity.
    public void ProcessMove(Vector2 input)
    {
        
        if (!controller.enabled) return;

        isGrounded = controller.isGrounded;

        if (lerpCrouch)
        {
            crouchTimer += Time.deltaTime;
            float p = crouchTimer / 1;
            p *= p;
            controller.height = crouching ? Mathf.Lerp(controller.height, 1, p) : Mathf.Lerp(controller.height, 2, p);
            if (p > 1)
            {
                lerpCrouch = false;
                crouchTimer = 0f;
            }
        }

        Vector3 moveDirection = Vector3.zero;
        moveDirection.x = input.x;
        moveDirection.z = input.y;
        controller.Move(transform.TransformDirection(moveDirection) * speed * Time.deltaTime);

        playerVelocity.y += gravity * Time.deltaTime;
        if (isGrounded && playerVelocity.y < 0)
            playerVelocity.y = -2f;
        controller.Move(playerVelocity * Time.deltaTime);
    }

    //Method checking if we are running or walking to play the desinated sound for the action
    void HandleFootstepSounds(Vector2 input)
    {
        bool isMoving = input.sqrMagnitude > 0.01f;
        bool shouldPlayWalkingSound = isMoving && !sprinting;
        bool shouldPlaySprintingSound = isMoving && sprinting;

        if (shouldPlayWalkingSound)
        {
            if (audioSource.clip != walkingSound)
            {
                audioSource.clip = walkingSound;
                audioSource.loop = true;
                audioSource.Play();
            }
            else if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
        else if (shouldPlaySprintingSound)
        {
            if (audioSource.clip != sprintingSound)
            {
                audioSource.clip = sprintingSound;
                audioSource.loop = true;
                audioSource.Play();
            }
            else if (!audioSource.isPlaying)
            {
                audioSource.Play();
            }
        }
        else
        {
            if (audioSource.isPlaying)
            {
                audioSource.Pause();
            }
        }
    }

    //Handling Jump, Crouch and Sprint :

    public void Jump()
    {
        if (isGrounded)
        {
            playerVelocity.y = Mathf.Sqrt(jumpHeight * -3.0f * gravity);
        }
    }

    public void Crouch()
    {
        crouching = !crouching;
        crouchTimer = 0;
        lerpCrouch = true;
    }

    public void Sprint()
    {
        sprinting = !sprinting;
        speed = sprinting ? 8 : 5;
    }
}
