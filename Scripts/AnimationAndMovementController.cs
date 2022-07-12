using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationAndMovementController : MonoBehaviour
{
    /*
     Tutorial Source on Character / Animation -Controllers from : https://www.youtube.com/watch?v=bXNFxQpp2qk
    */

    PlayerInputs playerInputs;
    CharacterController characterController;

    // Access the Animation Controller in the 'Animator' attribute of the game object attribute
    Animator animator;

    // Declare Behavioral Parameters

    Vector2 currentMovementInput; // store the Player-Key-Input values via Action Map Bindings
    Vector3 currentMovement;
    bool isMovementPressed; // Boolean flag to check if any of the key-binding player inputs are pressed or not

    bool isJump = false; // Boolean flag to check if the JUMP button is pressed by Player or not
    bool isAvatarJump = false; // Boolean flag to check if the character is in JUMP state
    float maxJumpHeight = 0.375f; // maximum height of jump
    float maxJumpTime = 0.30f; // maximum time taken for jump

    float runFactor = 2.0f;
    float runDiagFactor = 1.85f;

    float timeToApex = 0.40f; // maxJumpTime / 2;
    float jumpGravity = (-2*2.15f)/ Mathf.Pow(0.4f, 2); // (-2 * maxJumpHeight) / Mathf.Pow(timeToApex, 2);
    float groundedGravity = -0.5f; // if already grounded, apply a small gravitational force in -ve Y-direction

    float rotationFactorPerFrame = 5.0f; // rotation Update Rate per Frame

    void Awake()
    {
        // The Awake Function is a Life-cycle function which invoked even earlier than the Start Function
        playerInputs = new PlayerInputs();
        characterController = GetComponent<CharacterController>();
        animator = GetComponent<Animator>(); // instantiate the Animator attribute to check & update the animation states with each key-binding action-maps

        // navigate to the Character-Controls ActionMaps -> to Move & then Listen for Player Inputs to Corresponding Actions
        // 'context' -> gives access to the Input data when the starting callback occurs
        // 'playerInputs' has the keybindings defined under the 'PlayerControls' property. We'll be navigating through 'PlayerControls' to access the ActionMaps.
        // 'playerInputs.PlayerControls.Move.started' -> is aclled when the Input System first recieves player input
        // But, if we want to stop the Action translation when the Keys are let go, then we need to use the 'cancel' callback

        // Define the callback
        playerInputs.PlayerControls.Move.started += context => {
            currentMovementInput = context.ReadValue<Vector2>(); // Get User Input values as defined in the Action-maps

            // Store the user-input values
            currentMovement.x = currentMovementInput.x * runFactor;
            currentMovement.z = currentMovementInput.y * runFactor; // Player controls the movement in X & Z Axes.

            // This Boolean checks if any one of the Key-binding Movement Keys are pressed by the player
            isMovementPressed = currentMovementInput.x != 0 || currentMovementInput.y != 0;
        };

        // Get Intermediate Movements using 'performed' -> Allows the Player Avatar to move Diagonally
        playerInputs.PlayerControls.Move.performed += context => {
            currentMovementInput = context.ReadValue<Vector2>(); // Store WASD character movement keys ~ player inputs

            // Assign values to respective components => Player movement is in X-Z axes...
            currentMovement.x = currentMovementInput.x * runDiagFactor;
            currentMovement.z = currentMovementInput.y * runDiagFactor;

            // If any of the keys are pressed by the player - then X or Y values of Vector2 will be not equal to 0 => Set boolean value accordingly
            isMovementPressed = currentMovementInput.x != 0 || currentMovementInput.y != 0;
        };

        // 'Cancel' the input of the Player once the keys are let-go
        playerInputs.PlayerControls.Move.canceled += context => {
            currentMovementInput = context.ReadValue<Vector2>(); // Get User Input values as defined in the Action-maps

            // Store the user-input values
            currentMovement.x = currentMovementInput.x;
            currentMovement.z = currentMovementInput.y; // Player controls the movement in X & Z Axes.

            // This Boolean checks if any one of the Key-binding Movement Keys are pressed by the player
            isMovementPressed = currentMovementInput.x != 0 || currentMovementInput.y != 0; 
        };

        // This is a callback for the Jump action of character ~'started' callback method
        playerInputs.PlayerControls.Jump.started += context => {

            isJump = context.ReadValueAsButton(); //check if jump button os pressed or not
        };

        // This is a callback for the Jump action of character ~'canceled' callback method
        playerInputs.PlayerControls.Jump.canceled += context => {

            isJump = context.ReadValueAsButton(); //check if jump button os pressed or not
        };
    }

    // Take care of all the Avatar Animations & respective Animation Transitions with Key-bindings
    void handleAnimations()
    {
        // This function needs to be invoked with each Frame -> hence needs to be called in the 'Update' method
        // As defined in the Animation Controller, the parameter 'isRunning' controls the avatar transition to the
        // 'Running' state from the 'Idle' state.
        // Access the current state of the Boolean flag parameter from the instantiated animation controller
        bool isRunning = animator.GetBool("isRunning");

        if ( isMovementPressed && !isRunning)
        {
            // Movement Keys are pressed by Player but the Character isn't moving / running
            // set the Boolean parameter in Animation Controller to enable 'Run' animation transition from 'Idle'
            animator.SetBool("isRunning", true);
        }
        else if (!isMovementPressed && isRunning)
        {
            // Movement Keys are NOT pressed by Player but the Character is in moving / running state
            // disable the Boolean parameter in Animation Controller to make animation transition to 'Idle' state
            animator.SetBool("isRunning", false);
        }
    }

    // Take care of Player Rotations i.e. Movement in Left / Right & rotate the character orientation base on Player Input
    void handleRotation()
    {
        // Using Unity's Quaternion Physics to change the Orientation of the Player Avatar based on Player Input
        
        Vector3 positionToLookAt; // destination rotation - where the player will be moving next & orient it self towards
        Quaternion currentRotation; // current orientation of the Character Avatar

        // constantly updated movement details of the layer Avatar -> extract the 'X' & 'Z' attributes
        positionToLookAt.x = currentMovement.x;
        positionToLookAt.y = 0.0f;
        positionToLookAt.z = currentMovement.z;

        // Obtain the Current Rotation Orientation of the Player Avatar
        currentRotation = transform.rotation;

        if (isMovementPressed)
        {
            // Create a new Rotation based on the current player Orientation & Player Key Inputs.
            Quaternion targetRotation = Quaternion.LookRotation( positionToLookAt );

            // Assign the Player Avatar Rotation / Player Orientation with the updated Quaternions [SLERP -> Spherical Interpolation]
            // Transition from 'curretnRotation' to -> 'targetRotation' at the rate of 'rotationFactorPerFrame' per frame.
            // 'rotationFactorPerFrame' -> is a value between 0 to 1 -> closer value to 1 faster is the spherical interpolation
            // Multiplying 'rotationFactorPerFrame' with 'Time.deltaTime' [Not necessary]-> inorder to sync with the frame rate & have an appropriate fractional value with 'rotationFactorPerFrame' set as 1

            transform.rotation = Quaternion.Slerp(currentRotation, targetRotation, rotationFactorPerFrame);
        }
    }

    // Take care of the Gravity Physics on the Player Avatar
    void handleGravity()
    {

        // check when character initiates fall ~ if falling then we'd want to increase gravity, also to make height of jump adaptive to duration of key press
        bool isFalling = currentMovement.y <= 0.0f || !isJump;
        float gravityMultiplier = 2.0f;

        timeToApex = maxJumpTime / 2;
        jumpGravity = (-2 * maxJumpHeight) / Mathf.Pow(timeToApex, 2);

        // Check to see if the Player Avatar is grounded or not.
        if (characterController.isGrounded)
        {
            if (isAvatarJump)
            {
                // disable boolean parameter for character Jump animation
                animator.SetBool("isJumping", false);
                isAvatarJump = false;
            }

            currentMovement.y = groundedGravity; // Update the 'currentMovement' attribute
        }
        else if (isFalling)
        {
            // increase steep of character fall while jumping to increase the Steep of fall
            float previousYVelocity = currentMovement.y;

            // New implementation ~ Jump gravity varies with the Jump type..
            float newYVelocity = currentMovement.y + (jumpGravity * gravityMultiplier * Time.deltaTime);

            // apply upper limit of fall to limit insanely high velocity of fall
            float nextYVelocity = Mathf.Max((previousYVelocity + newYVelocity) * 0.5f, -20.0f);

            currentMovement.y = nextYVelocity;
        }
        else 
        {
            float previousYVelocity = currentMovement.y;
         
            float newYVelocity = currentMovement.y + (jumpGravity * Time.deltaTime);

            float nextYVelocity = (previousYVelocity + newYVelocity) * 0.5f;

            currentMovement.y = nextYVelocity;
            /*
            float gravity = -9.8f;
            currentMovement.y += gravity; // Update the 'currentMovement' attribute
            */
        }
    }

    void handleJump()
    {
        // Initilize parameters defining JUMP behavior

        timeToApex = maxJumpTime / 2;
        float initialJumpVelo = (2 * maxJumpHeight) / timeToApex;

        if (!isAvatarJump && characterController.isGrounded && isJump)
        {
            // The JUMP button is pressed by Player (isJump), character is grounded & character is Not in JUMP state
            isAvatarJump = true;

            // Initiate the Jump Animation by setting the Animation State Transition parameter for JUMP
            animator.SetBool("isJumping", true);

            // Move the character with the appropriate Jump displacement in height
            currentMovement.y = initialJumpVelo;
        }
        else if(isAvatarJump && characterController.isGrounded && !isJump)
        {
            isAvatarJump = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Update Player Orienation & Rotation based on Player Inputs
        handleRotation();

        // Take care of Animation transitions
        handleAnimations();

        // Move the Player Avater with Character-Controller Attribute of the Avatar
        // 'Time.deltaTime' -> is the amount of time since the last frame
        characterController.Move(currentMovement * Time.deltaTime * 5.5f);

        // Take care of the Gravity Physics in the Environment
        handleGravity();

        // handle the Jumping (+ Animation) mechanic
        handleJump();
    }

    // Enable the Action-maps from the Character Controller
    void OnEnable()
    {
        // Enable Character Controls Action Map
        playerInputs.PlayerControls.Enable();
    }

    // Disable the Action-maps from the Character Controller
    void OnDisable()
    {
        // Disable the Character Controls Action Map
        playerInputs.PlayerControls.Disable();
    }
}
