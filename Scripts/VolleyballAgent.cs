using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using Unity.MLAgents.Policies;

public class VolleyballAgent : Agent
{
    public GameObject area;
    Rigidbody agentRb;
    BehaviorParameters behaviorParameters;
    public Team teamId;

    // Obtain the volley-ball's location for observations & environment state updates
    public GameObject ball;
    Rigidbody ballRb;

    VolleyballSettings volleyballSettings;
    VolleyballEnvController envController;

    // Controls Agent jump behavior
    float jumpingTime;
    Vector3 jumpTargetPos;
    Vector3 jumpStartingPos;
    float agentRot;

    public Collider[] hitGroundColliders = new Collider[3];
    EnvironmentParameters resetParams;

    void Start()
    {
        /*
         * This method is invoked when the environment is first rendered. 
         * Here, the Parent 'VolleyBallArea Environment' Parent Object is instantiated for later reference.
        */
        envController = area.GetComponent<VolleyballEnvController>();
    }

    public override void Initialize()
    {
        /*
         * Initialize the AI Agent's parameters as well as
         * Constants / Objects determining the Agent~Environment Interactions.
        */

        volleyballSettings = FindObjectOfType<VolleyballSettings>();
        behaviorParameters = gameObject.GetComponent<BehaviorParameters>();

        agentRb = GetComponent<Rigidbody>();
        ballRb = ball.GetComponent<Rigidbody>();

        // 'AgentRot' ->  Ensure symmetry between Agents while learning policy
        // This is so that the learnt policy is tranferrable acorss the Agents.
        if (teamId == Team.Blue)
        {
            agentRot = -1;
        }
        else
        {
            agentRot = 1;
        }
    }


    void MoveTowards( Vector3 targetPos, Rigidbody rb, float targetVel, float maxVel)
    {
        /*
         * Determine Agent Movement - moves the agent towards a certain target position.
         * 
         * Arguments ->  * 'RigidBody rb': The rigid body to be moved
         *               * 'Vector3 targetPos': The target position the Agent has to be moved towards
         *               * 'float targetVel': The velocity while moving towards the target position
         *               * 'float maxVel': Maximum possible velocity of the Agent
        */

        var moveToPos = targetPos - rb.worldCenterOfMass;
        var velocityTarget = Time.fixedDeltaTime * targetVel * moveToPos;
        if (float.IsNaN(velocityTarget.x) == false)
        {
            rb.velocity = Vector3.MoveTowards(
                rb.velocity, velocityTarget, maxVel);
        }
    }

    public bool CheckIfGrounded()
    {
        /*
         * Check if the Agent is on the ground of the play area or is still in a Jump State.
         * This is done to enable / disable Jump action.
        */

        var o = gameObject;
        var grounded = false; // boolean flag for grounded or not

        var distToGround = 0.75f; // The distance of the center of Agent's game object from its bottom end
        var distTolerance = 0.1f; // Distance Tolerace i.e. delta value for Raycast interference

        // Using Raycasts to Check if Agent is grounded or not
        if (Physics.Raycast(o.transform.position, Vector3.down, distToGround + distTolerance))
        {
            //Debug.Log("Grounded");
            grounded = true;
        }
        else
        {
            //Debug.Log("Not Grounded");
            grounded = false;
        }        

        return grounded;
    }

    void OnCollisionEnter(Collision c)
    {
        /*
         * Invoked when the agent collides with the volleyball
        */
        if (c.gameObject.CompareTag("ball"))
        {
            envController.UpdateLastHitter(teamId);
        }
    }

    public void Jump()
    {
        /*
         * Initiate the Jumping Sequence of the Agent. 
        */
        jumpingTime = 0.2f;
        jumpStartingPos = agentRb.position;
    }

    public void MoveAgent(ActionSegment<int> act)
    {
        /*
         * This method is invoked when the Agent is about to take an Action.
         * The 'Decision Requester' attached with the Agent object selects an action from the Action-space
         * for the Agent to enact.
         * 
         * Declare variables for Agent's movement orientation & angular rotational movements
         *
         * 'Behavior parameters' -> script for the Agent in Unity's Property Inspector [of the Agent] is as follows for the 'Actions' section:
         * 
         *  'Actions' : 
         *      'Continuous Action' : 0
         *      
         *      'Discrete Branches' : 3
         *          - Branch 0 Size = 3  | [No movement, move forward, move backward] ~ a vector of size 3
         *          - Branch 1 Size = 3  | [No movement, move left, move right] ~ a vector of size 3
         *          - Branch 2 Size = 3  | [No rotation, rotate clockwise, rotate anti-clockwise] ~ a vector of size 3
         *          - Branch 3 Size = 2  | [No Jump, jump] ~ a vector of size 2
         *      
         * This defines the Action Space for the Agent. The Agent has a Discrete Action Space instead of a Continuous one.
        */
        var dirToGo = Vector3.zero;
        var rotateDir = Vector3.zero;

        var grounded = CheckIfGrounded(); // check if the Agent has been grounded or not

        /*
         * Actions passed into this method [actionBuffers.DiscreteActions] is an array of integers mapped to  
         * specific Agent behaviors. They need to be assigned to variables so that they can be referenced
         * for translating the acction to Agent Behavior.
        */

        var dirToGoForwardAction = act[0]; // Agent move forwards / backwards
        var rotateDirAction = act[1]; // Agent Rotational movement
        var dirToGoSideAction = act[2]; // Agent move Side ways
        var jumpAction = act[3]; // Agent Jump Action

        /*
         * Every object in Unity has a 'transform' class which stores its position, rotation & scale.
         * We use this 'transform' attribute to create vector & orient the Agent to appropriate direction.
         * 
         * We map the Actions to respective behaviors:
         * 
         *      -> 'dirToGoForwardAction':  0 : Do Nothing
         *                                  1 : Move forward
         *                                  2 : Move backward
         *      
         *      -> 'rotateDirAction':  0 : Do Nothing
         *                             1 : Rotate clockwise
         *                             2 : Rotate anti-clockwise
         *                             
         *      -> 'dirToGoSideAction':  0 : Do Nothing
         *                               1 : Move Left
         *                               2 : Move Right
         *
         *      -> 'jumpAction':  0 : Don't Jump
         *                        1 : Jump
         *
        */

        // assigning appropriate movement actions for Agent.
        if (dirToGoForwardAction == 1)
            dirToGo = (grounded ? 1f : 0.5f) * transform.forward * 1f;

        else if (dirToGoForwardAction == 2)
            dirToGo = (grounded ? 1f : 0.5f) * transform.forward * volleyballSettings.speedReductionFactor * -1f;

        // Agent Rotational movement -> Assign appropriate movements with corresponding Action values
        if (rotateDirAction == 1)
            rotateDir = transform.up * -1f;

        else if (rotateDirAction == 2)
            rotateDir = transform.up * 1f;

        // Agent Sideways movement -> Assign appropriate movements with corresponding Action values
        if (dirToGoSideAction == 1)
            dirToGo = (grounded ? 1f : 0.5f) * transform.right * volleyballSettings.speedReductionFactor * -1f;
        
        else if (dirToGoSideAction == 2)
            dirToGo = (grounded ? 1f : 0.5f) * transform.right * volleyballSettings.speedReductionFactor;

        if (jumpAction == 1)
        {
            // Initiate JUMP Action only if the AI Agent is grounded
            if (((jumpingTime <= 0f) && grounded))
            {
                Jump();
            }
        }

        // 'volleyballSettings.speedReductionFactor' -> is a speed reduction factor for slowing backwards & strafe movements 
        // to make Agent movements more realistic.

        // Now apply the translated values of the Agent's action to real movements of AI Agent to interact with the Gema environment
        
        transform.Rotate(rotateDir, Time.fixedDeltaTime * 200f); // Rotate the agent into appropriate orientation
        
        agentRb.AddForce(agentRot * dirToGo * volleyballSettings.agentRunSpeed, ForceMode.VelocityChange); // Move the Agent

        // JUMP Action Sequence -> to make the Agent JUMP physically
        if (jumpingTime > 0f)
        {
            jumpTargetPos =
                new Vector3(agentRb.position.x,
                    jumpStartingPos.y + volleyballSettings.agentJumpHeight,
                    agentRb.position.z) + agentRot * dirToGo;

            MoveTowards(jumpTargetPos, agentRb, volleyballSettings.agentJumpVelocity,
                volleyballSettings.agentJumpVelocityMaxChange);
        }

        // provides a downward force to end the jump
        if (!(jumpingTime > 0f) && !grounded)
        {
            agentRb.AddForce(
                Vector3.down * volleyballSettings.fallingForce, ForceMode.Acceleration);
        }

        // controls the jump sequence
        if (jumpingTime > 0f)
        {
            jumpingTime -= Time.fixedDeltaTime;
        }
    }

    public override void OnActionReceived(ActionBuffers actionBuffers)
    {
        MoveAgent(actionBuffers.DiscreteActions);
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        /*
         * 'Observations' are how the agent 'sees' its environment.
         *
         * In ML-Agents, there are 3 types of observations we can use:
         *    -> 'Vectors' � "direct" information about environment (e.g. list of floats containing the position, scale, velocity, etc of objects)
         *    -> 'Raycasts' � "beams" that shoot out from the agent and detect nearby objects
         *    -> 'Visual/camera input' 
         *    
         * In this project, 'vector observations' are implemented to keep things simple.
         * The main idea is to include only the observations which are relevant for making an informed decision about how to act. 
         * 
         * 
         * The obeservations Used by the Agent have been finalized to:
         * 
         *     -> Agent's y-rotation [1 float]
         *     -> Agent's x,y,z-velocity [3 floats]
         *     -> Agent's x,y,z-normalized vector to the volley-ball (i.e. direction to the ball) [3 floats]
         *     -> Distance of the VolleyBall from the Agent from the normalized distance vector [1 float]
         *     -> Volley Ball's x,y,z-velocity [3 floats]
         * 
         *  There are a total of 11 vector observations i.e. a vector of 11 float values [1 + 3 + 3 + 1 + 3] to be used as observation
         *  
         *  This method is defined to collect all these observations which define the state of the environment as 'seen' by the Agent
         *  for it to interpret the environment & interact accordingly.
        */

        // Agent rotation (1 float)
        sensor.AddObservation(this.transform.rotation.y);

        // Vector from agent to ball (direction to ball) (3 floats)
        Vector3 toBall = new Vector3((ballRb.transform.position.x - this.transform.position.x) * agentRot,
        (ballRb.transform.position.y - this.transform.position.y),
        (ballRb.transform.position.z - this.transform.position.z) * agentRot);

        sensor.AddObservation(toBall.normalized); // include the normalized distance vector

        // Distance from the ball (1 float)
        sensor.AddObservation(toBall.magnitude); // include the distance of the Ball

        // Agent velocity (3 floats)
        sensor.AddObservation(agentRb.velocity);

        // Ball velocity (3 floats)
        sensor.AddObservation(ballRb.velocity.y);
        sensor.AddObservation(ballRb.velocity.z * agentRot);
        sensor.AddObservation(ballRb.velocity.x * agentRot);

    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        /*
         * This method is invoked for manually testing the Agent's Movements with
         * manual Player inputs.
         * 
         * Key 'A' -> Rotate left
         * Key 'D' -> Rotate right
         * Key 'W' or key 'Up Arrow' -> Move Forwards
         * Key 'S' or key 'Down Arrow' -> Move Backwards
         * key 'left Arrow' -> Move Left
         * key 'right Arrow' -> Move Right
         * key 'space bar' -> Jump
        */

        var discreteActionsOut = actionsOut.DiscreteActions;

        if (Input.GetKey(KeyCode.D))
        {
            // rotate right
            discreteActionsOut[1] = 2;
        }

        if (Input.GetKey(KeyCode.A))
        {
            // rotate left
            discreteActionsOut[1] = 1;
        }

        if (Input.GetKey(KeyCode.W) || Input.GetKey(KeyCode.UpArrow))
        {
            // forward
            discreteActionsOut[0] = 1;
        }

        if (Input.GetKey(KeyCode.S) || Input.GetKey(KeyCode.DownArrow))
        {
            // backward
            discreteActionsOut[0] = 2;
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            // move left
            discreteActionsOut[2] = 1;
        }
        if (Input.GetKey(KeyCode.RightArrow))
        {
            // move right
            discreteActionsOut[2] = 2;
        }

        // JUMP Action
        discreteActionsOut[3] = Input.GetKey(KeyCode.Space) ? 1 : 0;
    }
}
