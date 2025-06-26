using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

/// <summary>
/// An abstract class that represents an action of an agent. It is a ScriptableObject to allow it and derived actions to be created and made action specific.
/// </summary>
public abstract class DUAS_Action : ScriptableObject
{
    [HideInInspector]
    public bool withinRangeOfTarget;                // Whether the Agent is within the range of the usable or not
    [HideInInspector]
    public bool commitmentToAction;                 // Whether the Agent is currently performing this particular action or not

    [Header("Need Multipliers")]
    [Tooltip("Identify the needs affected by this action and give each need affect a multiplier that will apply over time while this is the current action.")]
    public NeedMultiplier[] needsAffectedMultipliers;
    
    [Header("Emotions Multipliers")]
    [Tooltip("Identify the emotions affected by this action and give each emotion affect a multiplier that will apply over time while this is the current action.")]
    public EmotionMultiplier[] emotionsAffectedMultipliers;

    public abstract void Awake();

    public abstract float Evaluate(DUAS_Agent agent);    // Evaluates all of the agents needs and calculates the urgency of the need with a float - a high value mean a high importance
                                                    // agent - the agent that has its needs evaluated
    public abstract void UpdateAction(DUAS_Agent agent); // Updates the agents movement, needs, animation and destination
                                                    // agent - the agent that has its movement and needs updated
    public abstract void Enter(DUAS_Agent agent);        // Intialises any variables in the class on entering the action
                                                    // agent - the agent that the action belongs to
    public abstract void Exit(DUAS_Agent agent);         // Resets variables that were modified on exiting the action 
                                                    // agent - the agent that the action belongs to

    public virtual void SetGameObject(GameObject go) { }    // Sets the GameObject passed in as the target GameObject of an Action
}

[Serializable]
public struct NeedMultiplier
{
    public DUAS_Need need;
    [Range(-0.1f, 0.1f)]
    public float multiplier;
}

[Serializable]
public struct EmotionMultiplier
{
    public DUAS_Emotion emotion;
    [Range(-0.1f, 0.1f)]
    public float multiplier;
}