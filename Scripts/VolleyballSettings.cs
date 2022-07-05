using UnityEngine;

public class VolleyballSettings : MonoBehaviour
{
    /*
     * Instantiate and declare basic environmental parameters of the game
     * to define the Environment ~ Agent interaction.
    */
    public float agentRunSpeed = 2.0f;
    public float agentJumpHeight = 3.0f;
    public float agentJumpVelocity = 800.0f;
    public float agentJumpVelocityMaxChange = 10.0f;

    // Slows down strafe & backward movement
    public float speedReductionFactor = 0.8f;

    public Material blueGoalMaterial;
    public Material purpleGoalMaterial;
    public Material defaultMaterial;
    public Material penaltyMaterial;

    // This is a downward force applied when falling to make jumps look less floaty
    public float fallingForce = 200.0f;
}
