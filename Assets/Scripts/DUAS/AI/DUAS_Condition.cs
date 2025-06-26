using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DUAS_Condition", menuName = "DUAS_Condition")]
public abstract class DUAS_Condition : ScriptableObject
{
    [Header("Multiplier Value")]
    [Tooltip("The value that will += to the need and emotions affected by this condition.")]
    [Range(-0.01f, 0.01f)]
    public float multiplier;

    [Header("Affected Need and Emotion")]
    [Tooltip("The need affected by this condition that will have the multiplier += to its value.")]
    public DUAS_Need needAffected;
    [Tooltip("The emotion affected by this condition that will have the multiplier += to its value.")]
    public DUAS_Emotion emotionAffected;

    [HideInInspector]
    public float changeInNeedUI;
    [HideInInspector]
    public float changeInEmotionUI;


    public abstract bool CheckCondition(DUAS_Agent agent);    // Checks whether the current action of the agent is the same as this condition's action and sets the action's agent commitment accordingly, 
                                                         // returns true if the actions are the same, false otherwise
                                                         // agent - the agent used to check if current action is this action
    public abstract void UpdateUI(DUAS_Agent agent);          // Updates the UI bars for the agent's needs and the actual need values if the agent is performing the action currently
                                                         // agent - the agent that has its needs and UI for needs updated
    public abstract void Awake();                        // Awake allows variables to be initialised when the application begins
    public abstract void Exit(DUAS_Agent agent);              // Allows for variable resetting or adjustments to condition made upon exiting the condition
}
