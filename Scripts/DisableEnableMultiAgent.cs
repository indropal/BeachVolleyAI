using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class DisableEnableMultiAgent : MonoBehaviour
{

    /*
     This script is used to Enable Or Disable AI RL Agent Game player assist.
        -> Enabling this button will spawn AI Agents to Asist the Player
        -> Disabling will remove these AI Agent, leaving the player with a 1:1 vs match against the opponent AI agent
    */

    public GameObject PlayerAssistAgent;
    public GameObject OpponentAIAssistAgent;

    public Text buttonText;

    // Method definition for player interaction with button
    public void onButtonClick()
    {

        // The Player Assist RL AI Agent is active, then set it to inactive on Button click
        if (PlayerAssistAgent.activeInHierarchy == true)
        {
            PlayerAssistAgent.SetActive(false);
            buttonText.text = "Enable AI Agent Assist";
        }
        else
        {
            // If Player Assist AI is inactive before button click -> set GemeObject to Active on button toggle
            PlayerAssistAgent.SetActive(true);
            buttonText.text = "Disable AI Agent Assist";
        }

        // The Opponent Assist RL Agent is active, then set it to inactive on Button click
        if (OpponentAIAssistAgent.activeInHierarchy == true)
        {
            OpponentAIAssistAgent.SetActive(false);
        }
        else
        {
            // Opponent Assist AI is inactive prior to button click -> set GameObject to Active on button toggle
            OpponentAIAssistAgent.SetActive(true);
        }

    }

}
