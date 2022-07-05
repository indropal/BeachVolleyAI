using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * The Main Logic for the Environment Interaction 
*/


public enum Team
{
    Blue = 0,
    Purple = 1,
    Default = 2
}

public enum Event
{
    HitPurpleGoal = 0,
    HitBlueGoal = 1,
    HitOutOfBounds = 2,
    HitIntoBlueArea = 3,
    HitIntoPurpleArea = 4
}

public class VolleyballEnvController : MonoBehaviour
{
    int ballSpawnSide;

    VolleyballSettings volleyballSettings;

    public VolleyballAgent blueAgent;
    public VolleyballAgent purpleAgent;

    public List<VolleyballAgent> AgentsList = new List<VolleyballAgent>();
    List<Renderer> RenderersList = new List<Renderer>();

    Rigidbody blueAgentRb;
    Rigidbody purpleAgentRb;

    public GameObject ball;
    Rigidbody ballRb;

    public GameObject blueGoal;
    public GameObject purpleGoal;

    Renderer blueGoalRenderer;

    Renderer purpleGoalRenderer;

    Team lastHitter;

    private int resetTimer;
    public int MaxEnvironmentSteps;

    void Start()
    {
        /*
         * This method is invoked at the beginning when the 
         * environment gets rendered for the first time.
        */

        // Used to control agent & ball starting positions
        blueAgentRb = blueAgent.GetComponent<Rigidbody>();
        purpleAgentRb = purpleAgent.GetComponent<Rigidbody>();
        ballRb = ball.GetComponent<Rigidbody>();

        // Starting ball spawn side
        // -1 = spawn blue side, 1 = spawn purple side
        var spawnSideList = new List<int> { -1, 1 };
        ballSpawnSide = spawnSideList[Random.Range(0, 2)];

        // Render ground to visualise which agent scored
        blueGoalRenderer = blueGoal.GetComponent<Renderer>();
        purpleGoalRenderer = purpleGoal.GetComponent<Renderer>();
        RenderersList.Add(blueGoalRenderer);
        RenderersList.Add(purpleGoalRenderer);

        volleyballSettings = FindObjectOfType<VolleyballSettings>();

        ResetScene();
    }

    public void UpdateLastHitter(Team team)
    {
        // Tracks whether the Player or the Agent was the Last one to hit the volleyball
        lastHitter = team;
    }

    public void ResolveEvent(Event triggerEvent)
    {
        /*
         * This method resolves scenarios when volley ball invokes a trigger & assigns rewards respectively
         * The different Scenarios were defined in 'VolleyballController.cs' in the method -> 'OnTriggerEnter( Collide other )'
         * 
         *  Different Rewards are assigned in different scenrios for the Agent to learn adaptively.
        */

        switch (triggerEvent)
        {
            case Event.HitOutOfBounds:
                if (lastHitter == Team.Blue)
                {
                    // apply penalty to blue agent
                    blueAgent.AddReward(-0.2f);
                }
                else if (lastHitter == Team.Purple)
                {
                    // apply penalty to purple agent
                    purpleAgent.AddReward(-0.2f);
                }

                // Change the Color of the Play Area to demarcate Penalty -> turn floor to Red
                StartCoroutine(GoalScoredSwapGroundMaterial(volleyballSettings.penaltyMaterial, RenderersList, .5f));

                // Game Penalty ~ Out of Bounds -> end episode
                blueAgent.EndEpisode(); // End the Episode
                purpleAgent.EndEpisode(); // End the Episode
                ResetScene(); // Reset all params of the Environment

                break;

            case Event.HitBlueGoal:
                // blue wins
                blueAgent.AddReward(1f);
                purpleAgent.AddReward(-1f);

                // Change the Color of the Play Area -> turn floor blue 
                StartCoroutine(GoalScoredSwapGroundMaterial(volleyballSettings.blueGoalMaterial, RenderersList, .5f));

                // Goal is Scored -> end episode
                blueAgent.EndEpisode(); // End the Episode
                purpleAgent.EndEpisode(); // End the Episode
                ResetScene(); // Reset all params of the Environment

                break;

            case Event.HitPurpleGoal:
                // purple wins
                purpleAgent.AddReward(1f);
                blueAgent.AddReward(-1f);

                // Change the Color of the Play Area -> turn floor purple
                StartCoroutine(GoalScoredSwapGroundMaterial(volleyballSettings.purpleGoalMaterial, RenderersList, .5f));

                // Goal is Scored -> end episode
                blueAgent.EndEpisode(); // End the Episode
                purpleAgent.EndEpisode(); // End the Episode
                ResetScene(); // Reset all params of the Environment

                break;

            case Event.HitIntoBlueArea:
                if (lastHitter == Team.Purple)
                {
                    // The Agent is able to hit the Volleyball ~ Assign Respective Reward of 1
                    // purpleAgent.AddReward(1);
                    purpleAgent.AddReward(0.4f);
                }
                break;

            case Event.HitIntoPurpleArea:
                if (lastHitter == Team.Blue)
                {
                    // The Agent is able to hit the Volleyball ~ Assign Respective Reward of 1
                    // blueAgent.AddReward(1);
                    blueAgent.AddReward(0.4f);
                }
                break;
        }
    }

    IEnumerator GoalScoredSwapGroundMaterial(Material mat, List<Renderer> rendererList, float time)
    {
        /*
         * Changes the color of the Ground / Player Court for a moment.
         * The side which wins gets the respective color changed to.
         * 
         * This method returns an enumerator with the aprorpriate 'material' / color & Time for which the 
         * the color change is to be applied.
         * 
         * parameters : 'mat' -> The material / color which is to be applied
         *              'time' -> time for which the color cahnge is to be applied
         *              'rendererList' -> List of game objects, whose color is to be changed.
        */

        // Chenge the Ground color to apporpriate material Color
        foreach (var renderer in rendererList)
        {
            renderer.material = mat;
        }

        // Wait for a couple of seconds (2 seconds) to display the color change
        yield return new WaitForSeconds(time);
        
        // Reset the changed color back to the original color
        foreach (var renderer in rendererList)
        {
            renderer.material = volleyballSettings.defaultMaterial;
        }

    }

    void FixedUpdate()
    {
        /*
        * This method is called at every Timestep i.e. at every instance when the Frame of the Game is updated.
        * 
        * The frequency (in seconds) of updates is defined in 'FixedDeltaTime' parameter in 'ProjectSettingsOverride.cs'
        * This method controls the maximum number of updates in the environment as well.
        */

        resetTimer += 1;

        // Engage the Environment-Reset routine if the number of TimeSteps exceeds 'MaxEnvironmentSteps'
        if (resetTimer >= MaxEnvironmentSteps && MaxEnvironmentSteps > 0)
        {
            // Interupt the Action / Interaction Dynamics of both the Agents
            blueAgent.EpisodeInterrupted();
            purpleAgent.EpisodeInterrupted();

            // Reset the Environment
            ResetScene();
        }

    }

    public void ResetScene()
    {
        /*
         * This method resets the Environemnt & Agent parameters i.e. beginning a new Episode
         * from the first Timestep. It resets all the Agent parameters & Env. parameters & respawns the 
         * Volleyball.
        */

        resetTimer = 0; // re-initilize the tracker / timer of environment

        lastHitter = Team.Default; // reset last hit interaction with the volleyball

        // Reset each of the Player / Agent positions at the beginning of the Episode
        foreach (var agent in AgentsList)
        {
            // randomise starting positions and rotations of the players
            var randomPosX = Random.Range(-2f, 2f);
            var randomPosZ = Random.Range(-2f, 2f);
            var randomPosY = Random.Range(0.5f, 3.75f); // depends on jump height
            var randomRot = Random.Range(-45f, 45f);

            // reset the Agent's position
            agent.transform.localPosition = new Vector3(randomPosX, randomPosY, randomPosZ);
            
            //reset the Agent's orientation
            agent.transform.eulerAngles = new Vector3(0, randomRot, 0);

            // reset the Velocity of the Agent
            agent.GetComponent<Rigidbody>().velocity = default(Vector3);
        }

        // reset the state & location-spawn of the volleyball
        ResetBall();

    }

    void ResetBall()
    {
        /*
         * This method resets the orientation & position of the volleyball
         * It re-spawns the ball with random positional attributes
        */

        var randomPosX = Random.Range(-2f, 2f);
        var randomPosZ = Random.Range(6f, 10f);
        var randomPosY = Random.Range(6f, 8f);

        // -1 = spawn blue side, 1 = spawn purple side
        ballSpawnSide = -1 * ballSpawnSide; // set out the opposite Spawn side from the last Episode

        // respawn the Volley Ball on the appropriate side
        if (ballSpawnSide == -1)
        {
            ball.transform.localPosition = new Vector3(randomPosX, randomPosY, randomPosZ);
        }
        else if (ballSpawnSide == 1)
        {
            ball.transform.localPosition = new Vector3(randomPosX, randomPosY, -1 * randomPosZ);
        }

        // reset all the Velocity attirbutes of the Volleyball to 0
        ballRb.angularVelocity = Vector3.zero;
        ballRb.velocity = Vector3.zero;
    }
}
