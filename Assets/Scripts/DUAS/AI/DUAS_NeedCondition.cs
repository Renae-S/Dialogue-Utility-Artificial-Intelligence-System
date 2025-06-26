using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "DUAS_NeedCondition", menuName = "DUAS_Condition/DUAS_NeedCondition", order = 1)]
public class DUAS_NeedCondition : DUAS_Condition
{
    [Header("Need Details")]
    [Tooltip("The need that will have it's value checked if it meets the conditional value.")]
    public DUAS_Need need;

    [Header("Value Details")]
    [Tooltip("The value that will either check above or below this value (based on the If Above Value field below) and check if it meets the condition.")]
    [Range(0.0f, 1.0f)]
    public float value;
    [Tooltip("If true, the condition will check the above Value if it is of the value or higher, and if false, will check if it is of the value or lower.")]
    public bool ifAboveValue;   // If true, will check if emotion is at value or above. If false, will check if emotion is at value or below.

    // Checks whether there is a need of the agent that is the same as this condition's need and sets the action's agent commitment accordingly, returns true if the needs 
    // are the same, false otherwise
    // agent - the agent used to check if it has a need the same as this need
    public override bool CheckCondition(DUAS_Agent agent)
    {
        if (agent.GetNeed(need))
            need = agent.GetNeed(need);

        // If the need value reaches the value of this condition, then return true
        if (agent.GetNeed(need).value <= value && !ifAboveValue)
            return true;

        // If the need value reaches the value of this condition, then return true
        if (agent.GetNeed(need).value >= value && ifAboveValue)
            return true;

        // Otherwise return false
        return false;
    }

    // Updates the UI bars for the agent's emotions
    // agent - the agent that has its emotions and UI for needs updated
    public override void UpdateUI(DUAS_Agent agent)
    {
        if (agent.AICanvas)
        {
            changeInEmotionUI = agent.emotionNameBars[emotionAffected.emotionName].fillAmount;   // Adjust the changeInEmotionUI to the current value of the emotion's UI bar fill amount
            agent.GetEmotionFromName(emotionAffected.emotionName).value = changeInEmotionUI; // Set this emotion of the agent to the value of the changeInEmotionUI

            // If the agent has a emotionBar with the string name of the emotion passed in through inspector
            if (agent.emotionNameBars.ContainsKey(emotionAffected.emotionName))
            {
                changeInEmotionUI += multiplier * Time.deltaTime;      // Set value of changeInEmotionUI to be the multiplier by time passed from previous frame
                agent.emotionNameBars[emotionAffected.emotionName].fillAmount = changeInEmotionUI;   // Set value of the emotion bar UI fill amount to changeInEmotionUI
                agent.GetEmotionFromName(emotionAffected.emotionName).value = changeInEmotionUI;         // Set value of emotion in agent to changeInEmotionUI
            }
        }
        else
        {
            changeInEmotionUI = agent.GetEmotionFromName(emotionAffected.emotionName).value;

            // If the multiplier is a positive value and the agent's need value is less than full
            if (multiplier > 0 && agent.GetEmotionFromName(emotionAffected.emotionName).value < 1)
            {
                changeInEmotionUI += multiplier * Time.deltaTime;      // Set value of changeInNeedUI to be the multiplier by time passed from previous frame       
                agent.GetEmotionFromName(emotionAffected.emotionName).value = changeInEmotionUI; // Set value of need in agent to changeInNeedUI
            }

            // If the multiplier is a negative value and the agent's need value is greater than empty
            else if (multiplier < 0 && agent.GetEmotionFromName(emotionAffected.emotionName).value > 0)
            {
                changeInEmotionUI += multiplier * Time.deltaTime;      // Set value of changeInNeedUI to be the multiplier by time passed from previous frame                        
                agent.GetEmotionFromName(emotionAffected.emotionName).value = changeInEmotionUI; // Set value of need in agent to changeInNeedUI
            }
        }
    }

    // Awake allows variables to be initialised when the application begins
    public override void Awake()
    {
        changeInNeedUI = 1;
        changeInEmotionUI = 0;
    }

    // Allows for variable resetting or adjustments to condition made upon exiting the condition
    public override void Exit(DUAS_Agent agent) { }
}
