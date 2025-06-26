using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
[CreateAssetMenu(fileName = "DUAS_Need")]
public class DUAS_Need : ScriptableObject
{
    [Header("Need Name")]
    [Tooltip("The name of the need (e.g. Hunger, Entertainment, Energy, etc.).")]
    public string needName;
    [Header("Need Value")]
    [Tooltip("The value of the need (between 0 and 1).")]
    [Range(0.0f, 1.0f)]
    public float value = 1;

    [Header("Emotions Multipliers")]
    [Tooltip("Identify the emotions affected by this need's value and give each emotion affect a multiplier that will apply over time while this need is above or below a certain value.")]
    public EmotionConditionMultiplier[] emotionsAffectedMultipliers;

    private void Awake()
    {
        DUAS_Agent[] NPCs = FindObjectsOfType<DUAS_Agent>();

        if (emotionsAffectedMultipliers != null)
        {
            if (emotionsAffectedMultipliers.Length > 0 && NPCs.Length > 0)
            {
                foreach (EmotionConditionMultiplier ECM in emotionsAffectedMultipliers)
                {
                    DUAS_NeedCondition NC = ScriptableObject.CreateInstance<DUAS_NeedCondition>();
                    NC.need = this;
                    NC.emotionAffected = ECM.emotion;
                    NC.multiplier = ECM.multiplier;
                    NC.ifAboveValue = ECM.ifAboveValue;
                    NC.value = ECM.value;
                    NC.name = NC.need.name + " " + NC.emotionAffected.name;


                    foreach (DUAS_Agent NPC in NPCs)
                        NPC.conditions.Add(NC);
                }
            }
        }  
    }
}

[Serializable]
public struct EmotionConditionMultiplier
{
    public DUAS_Emotion emotion;
    [Range(-0.01f, 0.01f)]
    public float multiplier;
    public bool ifAboveValue;
    [Range(0.0f, 1.0f)]
    public float value;
}