using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimationMovementController : MonoBehaviour
{
    /*
     * Tutorials referred to : https://www.youtube.com/watch?v=4HpC--2iowE
     *                         https://www.youtube.com/watch?v=we4CGmkPQ6Q
     *                         https://www.youtube.com/watch?v=MWQv2Bagwgk
     *                         https://www.youtube.com/watch?v=D0lx90n0s-4
     *                         https://www.youtube.com/watch?v=ydjpNNA5804
     *                         https://www.youtube.com/watch?v=XnKKaL5iwDM
     */
    // public Transform cam; // reference to player camera
    PlayerInput playerInput;
    CharacterController characterController;
    Animator animator;

    // Variables to store Player Input Values
    Vector2 currentMovementInput;
    Vector3 currentMovement;
    // Vector3 reorientCurrentMovement; //Camera Adjusted movement vector

    bool isMovementPressed;
    float rotationFactorInFrame = 5.0f;
    float runFactor = 5.0f;
    bool isJump = false;
    float initialJumpVelo;
    float maxJumpHeight = 2.15f; // maximum height of jump
    float maxJumpTime = 0.80f; // maximum time taken for jump
    bool isCharJump = false;
    // float turnSmoothVelocity;
    // float transitionSmoothTime = 0.1f; // handle character movement transitions
    float gravity = -9.8f;
    float groundedGravity = -0.05f;
    bool jumpAnimating = false;
    int jumpCount = 0; // keep track of the number of times Jump key has been toggled
    Dictionary<int, float> initJumpVelo = new Dictionary<int, float>(); // key-value mapping for different Jump-type velocities ~ accordingly different animation
    Dictionary<int, float> jumpGravity = new Dictionary<int, float>(); // key=value mapping for different Jump gravity influence
    Coroutine currentJumpResetRoutine = null; // coroutine initialiser

    void Awake()
    {
        /* 'Awake' method invoked earlier than the 'Start' method in the Update Lifecycle
            which is the perfect place to set reference variables to instances
        */
        playerInput = new PlayerInput(); // This is an object used to register the Player's movement
        characterController = GetComponent<CharacterController>(); // This is used to check & assign appropriate movement to the character via Key bindings / Action Map
        animator = GetComponent<Animator>(); // This is used to check and attribute the different animations of the character

        // SETTING PLAYER INPUT CALLBACKS ~> 'context' variable is reference to the current input data when callback occurs.
        // This is the Callback defintion to identify the user input when a respective key is pressed ~ we use 'started' attribute

        playerInput.CharacterControl.Movement.started += context => {
            // Debug.Log( context.ReadValue<Vector2>() ); // Log the User Input
            currentMovementInput = context.ReadValue<Vector2>(); // Store WASD character movement keys ~ player inputs

            // Assign values to respective components => Player movement is in X-Z axes...
            currentMovement.x = currentMovementInput.x * runFactor;
            currentMovement.z = currentMovementInput.y * runFactor;

            //Debug.Log(currentMovement.ToString());

            // If any of the keys are pressed by the player - then X or Y values of Vector2 will be not equal to 0 => Set boolean value accordingly
            isMovementPressed = currentMovementInput.x != 0 || currentMovementInput.y != 0;
        }; 

        // This is a callback definition which is executed when the user lets go of the input key ~ we use 'canceled' attribute
        playerInput.CharacterControl.Movement.canceled += context => {
            // Debug.Log( context.ReadValue<Vector2>() ); // Log the User Input
            currentMovementInput = context.ReadValue<Vector2>(); // Store WASD character movement keys ~ player inputs

            // Assign values to respective components => Player movement is in X-Z axes...
            currentMovement.x = currentMovementInput.x;
            currentMovement.z = currentMovementInput.y;

            // If any of the keys are pressed by the player - then X or Y values of Vector2 will be not equal to 0 => Set boolean value accordingly
            isMovementPressed = currentMovementInput.x != 0 || currentMovementInput.y != 0;
        };

        // This is a similar callback for the above 'canceled' method - when user's input is via a controller instead of a keyboard ~ using 'performed'
        playerInput.CharacterControl.Movement.performed += context => {
            // Debug.Log( context.ReadValue<Vector2>() ); // Log the User Input
            currentMovementInput = context.ReadValue<Vector2>(); // Store WASD character movement keys ~ player inputs

            // Assign values to respective components => Player movement is in X-Z axes...
            currentMovement.x = currentMovementInput.x;
            currentMovement.z = currentMovementInput.y;

            // If any of the keys are pressed by the player - then X or Y values of Vector2 will be not equal to 0 => Set boolean value accordingly
            isMovementPressed = currentMovementInput.x != 0 || currentMovementInput.y != 0;
        };

        // This is a callback for the Jump action of character ~'started' callback method
        playerInput.CharacterControl.Jump.started += context => {

            isJump = context.ReadValueAsButton(); //check if jump button os pressed or not
            //Debug.Log(isJump);
        };

        // This is a callback for the Jump action of character ~'canceled' callback method
        playerInput.CharacterControl.Jump.canceled += context => {

            isJump = context.ReadValueAsButton(); //check if jump button os pressed or not
            //Debug.Log(isJump);
        };

        initJumpVars(); // initialise Jump parameters to handle character jump mechanic.
    }

    void initJumpVars()
    {
        // initialise character JUMP variables ~ handle character jump
        float timeToApex = maxJumpTime / 2;

        // First-type Jump specifications
        gravity = (-2 * maxJumpHeight) / Mathf.Pow( timeToApex, 2 );
        initialJumpVelo = (2 * maxJumpHeight) / timeToApex;
        
        // Second-type Jump specifications
        float secondJumpGravity = (-2 * (maxJumpHeight + 1.25f)) / Mathf.Pow( (timeToApex * 1.20f), 2 );
        float secondJumpInitialVelo = (2 * (maxJumpHeight + 1.25f)) / (timeToApex * 1.20f);

        // Third-type Jump specifications
        float thirdJumpGravity = (-2 * (maxJumpHeight + 1.75f)) / Mathf.Pow((timeToApex * 1.35f), 2);
        float thirdJumpInitialVelo = (2 * (maxJumpHeight + 1.75f)) / (timeToApex * 1.35f);

        // assign respective jump-velocity definitions to the Dictionary
        initJumpVelo.Add(1, initialJumpVelo);
        initJumpVelo.Add(2, secondJumpInitialVelo);
        initJumpVelo.Add(3, thirdJumpInitialVelo);
        initJumpVelo.Add(4, thirdJumpInitialVelo); // just for safe keeping

        // assign respective jump-gravity definitions to the Dictionary
        jumpGravity.Add(0, gravity);
        jumpGravity.Add(1, gravity);
        jumpGravity.Add(2, secondJumpGravity);
        jumpGravity.Add(3, thirdJumpGravity);
        jumpGravity.Add(4, thirdJumpGravity); // just for safe keeping
    }

    // handle character jump mechanic
    void handleJump()
    {
        if( !isCharJump && characterController.isGrounded && isJump)
        {
            
            // check to prevent Jump Count if Jump key pressed before the Jump Coroutine cooldown
            if (jumpCount < 3 && currentJumpResetRoutine != null)
            {
                // we do this if the Jump Count is less than 3 or else there'll be a key error in the Dictionary
                // prevent reset of Jump Count if the Jump key is pressed prior to 0.5 seconds cool down
                StopCoroutine(currentJumpResetRoutine);
            }

            isCharJump = true;

            // set boolean parameter to initiate character Jump animation
            animator.SetBool("isJumping", true);
            jumpAnimating = true;

            jumpCount += 1; // update the Jump type by incrementing with every Jump Key toogle
            // animator.SetInteger("jumpCount", jumpCount); // update jumpCount in Animator-Controller

            // Implementation for using along with Euler Integration approach
            // currentMovement.y = initialJumpVelo; 

            // Implementation of Jump mechanic independent of Frame-rate
            /*
            float previousYVelocity = currentMovement.y;
            float newYVelocity = (currentMovement.y + initialJumpVelo);
            float nextYVelocity = (previousYVelocity + newYVelocity) * 0.5f;
            
            currentMovement.y = nextYVelocity;
            */

            // Different Jump implementations ~ height, gravity & animation - based on the Jump-type
            currentMovement.y = initJumpVelo[jumpCount] * 0.5f;

        }
        else if( !isJump && isCharJump && characterController.isGrounded)
        {
            isCharJump = false;
        }
    }

    // Co-Routine function ~ Function pauses execution for sometime & then resumes execution again 
    IEnumerator jumpResetRoutine()
    {
        yield return new WaitForSeconds(0.5f); // Co-Routine pauses for 0.5 seconds before resuming execution 
        jumpCount = 0; // Re-initialise the Jump-Type var
    }

    // handle movement orientation of character i.e. character should face the direction of movement ~this is done via Quaternion functionality (Unity) / Quaternion Nummber system
    void handleRotation()
    {
        
        // This is the OLD implemetation without Player Camera Integration...

        Vector3 posLookAt; // This is vector is used to track where the character is moving next ~ Updatable current Movement variable

        // the change in position the character should point to
        posLookAt.x = currentMovement.x;
        posLookAt.y = 0.0f;
        posLookAt.z = currentMovement.z;

        // Set the Current Rotation / Orientation of the character
        Quaternion currentRotation = transform.rotation;

        // check for player input & assign appropriate orientation / character rotation
        if (isMovementPressed)
        {
            // New rotation / Quaternion target based on the Player's key input / control
            Quaternion targetRotation = Quaternion.LookRotation( posLookAt );
            transform.rotation = Quaternion.Slerp( currentRotation, targetRotation, rotationFactorInFrame );
        }
    }

    // Take care of all the animation state changes..
    void handleAnimation()
    {
        // The boolean parameters in the Animator / Animation Controller which governs the animation state changes
        bool isRunning = animator.GetBool("isRunning");

        if ( isMovementPressed && !isRunning )
        {
            // if the player has pressed corresponding movement key & the character hasn't enacted the running animation.. => set corresponding animation Boolean value for animation transition
            animator.SetBool("isRunning", true);
        }
        else if( !isMovementPressed && isRunning )
        {
            // if the player has not pressed corresponding movement key & the character is still enacting the running animation.. => set corresponding animation Boolean value for animation transition
            animator.SetBool("isRunning", false);
        }
    }

    // Take care of the effect of Gravity...
    void handleGravity()
    {
        // check when character initiates fall ~ if the character is falling then we'd want to increase gravity, also to make height of jump adaptive to duration of key press
        bool isFalling = currentMovement.y <= 0.0f || !isJump; 
        float gravityMultiplier = 2.0f;

        if( characterController.isGrounded )
        {
            if (jumpAnimating)
            {
                // disable boolean parameter for character Jump animation
                animator.SetBool("isJumping", false);
                jumpAnimating = false;

                // call co-routine for jump-type re-check ~ prevent reset of jump count because the jump key was pressed before the countdown
                currentJumpResetRoutine = StartCoroutine(jumpResetRoutine());
                if (jumpCount > 3)
                {
                    jumpCount = 0;
                    // animator.SetInteger("jumpCount", jumpCount); // update jumpCount in Animator-Controller
                }
            }

            currentMovement.y = groundedGravity;
        }
        else if ( isFalling )
        {
            // increase steep of character fall while jumping to increase the Steep of fall
            float previousYVelocity = currentMovement.y;
            
            // float newYVelocity = currentMovement.y + (gravity * gravityMultiplier * Time.deltaTime); // Old implementation

            // New implementation ~ Jump gravity varies with the Jump type..
            float newYVelocity = currentMovement.y + (jumpGravity[jumpCount] * gravityMultiplier * Time.deltaTime);

            // apply upper limit of fall to limit insanely high velocity of fall
            float nextYVelocity = Mathf.Max( (previousYVelocity + newYVelocity) * 0.5f, -20.0f );
            
            currentMovement.y = nextYVelocity;
        }
        else
        {
            float previousYVelocity = currentMovement.y;
            // float newYVelocity = currentMovement.y + (gravity * Time.deltaTime);// Old implementation

            // New implementation ~ Jump gravity varies with the Jump type..
            float newYVelocity = currentMovement.y + (jumpGravity[jumpCount] * Time.deltaTime);

            float nextYVelocity = (previousYVelocity + newYVelocity) * 0.5f;

            // currentMovement.y += gravity * Time.deltaTime; // Euler Integration of Jump which gives choppy results due to dependence of frame-rate i.e Time.deltaTime
            
            currentMovement.y = nextYVelocity;
        }
    }

    // Update is called once per frame
    void Update()
    {
        handleRotation(); // check for appropriate orientation of character based on player input
        handleAnimation(); // check for any animation state changes prior to frame update

        // Move the character based on player input
        characterController.Move( currentMovement * 8f * Time.deltaTime ); // Time.deltaTime ~ is a fraction since the last time the frame was updated (frame rate => fraction < 1)
        

        handleGravity(); // handle the game gravity mechanic
        handleJump(); // handle the character Jump mechanic
    }

    // Enable OR Disable Character Controls Action-Map Monobehavior
    void OnEnable()
    {
        // Enable Character Controls Action Map
        playerInput.CharacterControl.Enable();
    }

    void OnDisable()
    {
        // Disable Character Controls Action Map
        playerInput.CharacterControl.Disable();
    }
}
