using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseGame : MonoBehaviour
{
    [SerializeField] GameObject PauseScene; // 'SeriealizeField' ~ as we want variable to be private but we want it to be visible in Unity Editor

    public void Pause()
    {
        // Pause the Entire Game
        PauseScene.SetActive(true); // set the Pause Menu Canvas Scene to Active so that its visible in UI
        
        /*
         Time.timeScale : Scale or rate at which time passes. Has float values.
                          Value - 0.0f : unity environment is paused
                                  1.0f : unity environment runs at real-time
                                  0.5f : unity environment runs at 0.5 X real-time
        */
        
        Time.timeScale = 0.0f; // Game is stopped.
    }

    public void Resume()
    {
        // Resume the Game from where it was paused
        PauseScene.SetActive(false); // diable the Pause Canvas Scene

        Time.timeScale = 1.0f; // Game is unpaused & resumed
    }
}
