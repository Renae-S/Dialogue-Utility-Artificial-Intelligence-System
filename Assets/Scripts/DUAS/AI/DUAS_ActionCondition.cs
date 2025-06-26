﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// A derived class that represents a condition based on an agent's action. It is a ScriptableObject to allow it to be created and made action specific.
/// </summary>
[CreateAssetMenu(fileName = "DUAS_ActionCondition", menuName = "DUAS_Condition/DUAS_ActionCondition", order = 1)]
public class DUAS_ActionCondition : DUAS_Condition
{
    [Header("Action")]
    [Tooltip("The action that will trigger the condition to be true.")]
    public DUAS_Action action;   // The action that the condition consequences are based on

    // Checks whether the current action of the agent is the same as this condition's action and sets the action's agent commitment accordingly, returns true if the actions 
    // are the same, false otherwise
    // agent - the agent used to check if current action is this action
    public override bool CheckCondition(DUAS_Agent agent)
    {
        // If the condition's action is the agent's current action
        if (agent.currentAction && action.name == agent.currentAction.name)
        {
            // Set the action's commitmentToAction to true and returns true
            action.commitmentToAction = true;
            return true;
        }

        // Set the action's commitmentToAction to false and returns false
        action.commitmentToAction = false;
        return false;
    }

    // Updates the UI bars for the agent's needs and the actual need values if the agent is performing the action currently
    // agent - the agent that has its needs and UI for needs updated
    public override void UpdateUI(DUAS_Agent agent)
    {
        // If the agent is in range of the target of its current action
        if (agent.currentAction.withinRangeOfTarget)
        {
            if (agent.AICanvas)
            {
                changeInNeedUI = agent.needNameBars[needAffected.name].fillAmount;   // Adjust the changeInNeedUI to the current value of the need's UI bar fill amount
                agent.GetNeed(needAffected).value = changeInNeedUI; // Set this need of the agent to the value of the changeInNeedUI

                // If the agent has a needBar with the string name of the need passed in through inspector
                if (agent.needNameBars.ContainsKey(needAffected.name))
                {
                    // If the multiplier is a positive value and the agent's need value is less than full
                    if (multiplier > 0 && agent.GetNeed(needAffected).value < 1)
                    {
                        changeInNeedUI += multiplier * Time.deltaTime;      // Set value of changeInNeedUI to be the multiplier by time passed from previous frame
                        agent.needNameBars[needAffected.name].fillAmount = changeInNeedUI;   // Set value of the need bar UI fill amount to changeInNeedUI
                        agent.GetNeed(needAffected).value = changeInNeedUI; // Set value of need in agent to changeInNeedUI
                    }

                    // If the multiplier is a negative value and the agent's need value is greater than empty
                    else if (multiplier < 0 && agent.GetNeed(needAffected).value > 0)
                    {
                        changeInNeedUI += multiplier * Time.deltaTime;      // Set value of changeInNeedUI to be the multiplier by time passed from previous frame
                        agent.needNameBars[needAffected.name].fillAmount = changeInNeedUI;   // Set value of the need bar UI fill amount to changeInNeedUI
                        agent.GetNeed(needAffected).value = changeInNeedUI; // Set value of need in agent to changeInNeedUI
                    }
                }
            }
            else
            {
                changeInNeedUI = agent.GetNeed(needAffected).value;


                // If the multiplier is a positive value and the agent's need value is less than full
                if (multiplier > 0 && agent.GetNeed(needAffected).value < 1)
                {
                    changeInNeedUI += multiplier * Time.deltaTime;      // Set value of changeInNeedUI to be the multiplier by time passed from previous frame       
                    agent.GetNeed(needAffected).value = changeInNeedUI; // Set value of need in agent to changeInNeedUI
                }

                // If the multiplier is a negative value and the agent's need value is greater than empty
                else if (multiplier < 0 && agent.GetNeed(needAffected).value > 0)
                {
                    changeInNeedUI += multiplier * Time.deltaTime;      // Set value of changeInNeedUI to be the multiplier by time passed from previous frame                        
                    agent.GetNeed(needAffected).value = changeInNeedUI; // Set value of need in agent to changeInNeedUI
                }
            }

            // If the need value is full then cancel this action
            if (agent.GetNeed(needAffected).value >= 1)
                agent.actionTimer = 0;        
        }
    }

    // Awake allows variables to be initialised when the application begins
    public override void Awake()
    {
        changeInNeedUI = 1; // Needs to be set the full fillAmount value for the UI need bars
    }

    // Allows for variable resetting or adjustments to condition made upon exiting the condition
    public override void Exit(DUAS_Agent agent) { }
}

