using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[System.Serializable]
[CreateAssetMenu(fileName = "DUAS_Dialogue")]
public class DUAS_Dialogue : ScriptableObject
{
    [Header("Character")]
    [Tooltip("The character speaking this dialogue.")]
    public DUAS_Character character;

    [Header("Sentences in this dialogue")]
    [Tooltip("Separated sections of the dialogue text (will be displayed one at a time.")]
    [TextArea(3, 10)]
    public string[] sentences;

    [Header("Teletype Speed")]
    [Tooltip("The speed the text will teletype at.")]
    [Range(0.0f, 0.1f)]
    public float speed = 0.025f;

    [Header("Font Options")]
    [Tooltip("If true, will use the font put into the Font field below.")]
    public bool useNonDefaultFont;
    [Tooltip("If the Use Non-Default Font field above is true, the font of this dialogue will be the font in this field.")]
    public TMP_FontAsset font;
    [Tooltip("If true, will use the font color put in the textColor field below.")]
    public bool useNonCharacterDialogueColor;
    [Tooltip("If the Use Non Character Dialogue Color field above is true, the text color of this dialogue will be the color in this field.")]
    public Color textColor = Color.white;

    [Header("Character Portrait")]
    [Tooltip("An expression sprite that will be displayed instead of the defaullt character portrait.")]
    public Sprite expression;

    [Header("Animation")]
    [Tooltip("An expression sprite that will be displayed instead of the defaullt character portrait.")]
    public AnimationClip emote;

    [Header("Audio")]
    [Tooltip("Plays this clips at the beginning of this dialogue.")]
    public AudioClip dialogueSound;

    [Header("Emotional Impact")]
    [Tooltip("If true, this will trigger the emotions below to be impacted by this dialogue by the value below (is += to the emotion value).")]
    public bool affectsEmotion;
    [Tooltip("If the Effects Emotion field above is true, these emotions will be impacted by this dialogue by the value below (is += to the emotion value).")]
    public DUAS_Emotion[] emotionsAffected;
    [Tooltip("If the Effects Emotion field above is true, these emotions listed above will be impacted by this dialogue by the value below (is += to the emotion value).")]
    [Range(-1.0f, 1.0f)]
    public float emotionEffectValue;

    [Header("Branching Dialogue")]
    [Tooltip("If true, this will display the choice buttons with the choice conversations listed in the Choice Conversations section below.")]
    public bool isDialogueChoice;
    [Tooltip("A list of 3 conversation choices the player can make.")]
    public DUAS_Conversation[] choices;

}