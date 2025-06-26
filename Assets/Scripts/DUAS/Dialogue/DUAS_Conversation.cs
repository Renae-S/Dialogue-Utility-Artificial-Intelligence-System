using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "DUAS_Conversation")]
public class DUAS_Conversation : ScriptableObject
{
    [Header("Player Choice Button Display")]
    [Tooltip("If this conversation is a result of a player dialogue choice, give this a name to be displayed on the choice button.")]
    public string choiceTitle;

    [Header("NPC Conversations Only")]
    [Tooltip("If the NPC's highest emotion is this emotion, then this conversation is more likely to be triggered.")]
    public DUAS_Emotion emotionToTriggerThisConversation;

    [Header("Dialogue")]
    [Tooltip("Plays dialogues in this order.")]
    public DUAS_Dialogue[] dialogues;
}
