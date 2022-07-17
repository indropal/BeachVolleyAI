using UnityEngine;

public class VolleyballController : MonoBehaviour
{
    /*
     * Determine whether the Volleyball has touched the Player's court
     * Or the Agent's Court. 
     * The Trigger setup on the individual game objects (as in the Player areas)
     * will initiate the routines setup in the 'OnTriggerEnter' method
    */

    // instantiate the environment-controller object
    public VolleyballEnvController envController;

    // declare game object place-holder variables
    public GameObject purpleGoal;
    public GameObject blueGoal;

    Collider purpleGoalCollider;
    Collider blueGoalCollider;

    void Start()
    {
        // Method is invoked when the environment is first rendered by the Unity Engine

        purpleGoalCollider = purpleGoal.GetComponent<Collider>(); // get the Collider objects of the Purple Goal
        blueGoalCollider = blueGoal.GetComponent<Collider>(); // get the Collider objects of the Blue Goal

        /*
         * Instantiate the Parent Game Object ~ this script is going to be an attribute of the Voleyball game object
         * The parent of the VolleyBall game-object is the entire VolleyBall environment
         * We instantiate the environment i.e. parent of volleyball in order to gain access to different environmental states.
        */
        envController = GetComponentInParent<VolleyballEnvController>();

    }

    void OnTriggerEnter(Collider other)
    {   
        // This method detects if the Volleyball has collided with any other game object
        // We are most interested to know whether the ball has collided with the Agent's Area, Player's Area or the Out of Bounds Area
        // In the arguments of the method 'other' - is a placeholder for the collided game object (the object which collided with the cvolleyball)
        //
        // In other words, this method handles various scenrios in which the Volleyball can be found during an Episode.

        if (other.gameObject.CompareTag("boundary"))
        {
            envController.ResolveEvent(Event.HitOutOfBounds); // volleyball ~ out of bounds
        }
        else if (other.gameObject.CompareTag("blueBoundary"))
        {
            envController.ResolveEvent(Event.HitIntoBlueArea); // ball hit into blue side
        }
        else if (other.gameObject.CompareTag("purpleBoundary"))
        {
            envController.ResolveEvent(Event.HitIntoPurpleArea); // ball hit into purple side
        }
        else if (other.gameObject.CompareTag("purpleGoal"))
        {
            envController.ResolveEvent(Event.HitPurpleGoal); // ball hit purple goal (blue side court)
        }
        else if (other.gameObject.CompareTag("blueGoal"))
        {
            envController.ResolveEvent(Event.HitBlueGoal); // ball hit blue goal (purple side court)
        }
    }

}