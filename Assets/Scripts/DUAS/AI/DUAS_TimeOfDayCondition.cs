using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[CreateAssetMenu(fileName = "DUAS_TimeOfDayCondition", menuName = "DUAS_Condition/DUAS_TimeOfDayCondition", order = 2)]
public class DUAS_TimeOfDayCondition : DUAS_Condition
{
    private GameObject sun;

    [Header("Time Of Day")]
    [Tooltip("If true, the condition will be met when the sun's X rotation is between 180 and 360.")]
    public bool night;
    [Tooltip("If true, the condition will be met when the sun's X rotation is between 0 and 180.")]
    public bool day;

    // Checks whether the sun is at a day rotation or night rotation, returns true if the bools set in the inspector are the same as the current time of day, false otherwise
    // agent - the agent used to access the sun
    public override bool CheckCondition(DUAS_Agent agent)
    {
        sun = agent.sun;

        // If the sun's rotation is below the terrain, set isNight to true
        bool isNight = false;
        if (sun.transform.eulerAngles.x <= 0.0f || sun.transform.eulerAngles.x >= 180.0f)
            isNight = true;

        // If the sun's rotation is above the terrain, set isNight to true
        bool isDay = false;
        if (sun.transform.eulerAngles.x > 0.0f && sun.transform.eulerAngles.x < 180.0f)
            isDay = true;

        // If the night and day variables match the current time of day, then return true
        if (night == isNight || day == isDay)
            return true;
       

        // Otherwise return false
        return false;
    }

    // Updates the UI bars for the agent's needs and the actual need values if the agent's currently
    // agent - the agent that has its emotions and UI for needs updated
    public override void UpdateUI(DUAS_Agent agent)
    {
        if (agent.AICanvas)
        {
            changeInNeedUI = agent.needNameBars[needAffected.name].fillAmount;   // Adjust the changeInNeedUI to the current value of the need's UI bar fill amount
            agent.GetNeed(needAffected).value = changeInNeedUI; // Set this need of the agent to the value of the changeInNeedUI
                                                                // If the agent has a needBar with the string name of the need passed in through inspector
            if (agent.needNameBars.ContainsKey(needAffected.name))
            {
                changeInNeedUI += multiplier * Time.deltaTime;      // Set value of changeInNeedUI to be the multiplier by time passed from previous frame
                agent.needNameBars[needAffected.name].fillAmount = changeInNeedUI;   // Set value of the need bar UI fill amount to changeInNeedUI
                agent.GetNeed(needAffected).value = changeInNeedUI;         // Set value of need in agent to changeInNeedUI
            }
        }
        else
        {
            changeInNeedUI = agent.GetNeed(needAffected).value;
            changeInNeedUI += multiplier * Time.deltaTime;      // Set value of changeInNeedUI to be the multiplier by time passed from previous frame
            agent.GetNeed(needAffected).value = changeInNeedUI;         // Set value of need in agent to changeInNeedUI
        }
    }

    // Awake allows variables to be initialised when the application begins
    public override void Awake()
    {
        changeInNeedUI = 1;
        changeInEmotionUI = 1;
    }

    // Allows for variable resetting or adjustments to condition made upon exiting the condition
    public override void Exit(DUAS_Agent agent) { }
}