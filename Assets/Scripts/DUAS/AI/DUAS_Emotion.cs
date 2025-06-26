using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

[Serializable]
[CreateAssetMenu(fileName = "DUAS_Emotion")]
public class DUAS_Emotion : ScriptableObject
{
    [Header("Emotion Name")]
    [Tooltip("The name of the emotion (e.g. Happiness, Anger, Sadness, etc.).")]
    public string emotionName;
    [Header("Emotion Value")]
    [Tooltip("The value of the emotion (between 0 and 1).")]
    [Range(0.0f, 1.0f)]
    public float value = 0;
}
