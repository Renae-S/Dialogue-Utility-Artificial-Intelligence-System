using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

[CreateAssetMenu(fileName = "DUAS_Character", menuName = "DUAS_Character")]
public class DUAS_Character : ScriptableObject
{
    [Header("Basic Information")]
    [Tooltip("Character Name will be displayed during dialogue sequences.")] 
    public string characterName;
    [Tooltip("Default Sprite will be used for the character's portrait during dialogue sequences when other sprite is not specified in Dialogue.")]
    public Sprite defaultSprite;
    [Tooltip("Dialogue Box will be used for dialogue sequences with this character.")]
    public Sprite dialogueBox;

    [Header("Fonts")]
    [Tooltip("Name Font is used for name text during dialogue sequences with this character.")]
    public TMP_FontAsset nameFont;
    [Tooltip("Default Dialogue Font is used for dialogue text during dialogue sequences with this character when other font is not specified in Dialogue.")]
    public TMP_FontAsset defaultDialogueFont;

    [Header("Font Colors")]
    [Tooltip("Name Color is used for name text during dialogue sequences with this character.")]
    public Color nameColor = Color.white;
    [Tooltip("Default Dialogue Color is used for dialogue text during dialogue sequences with this character.")]
    public Color defaultDialogueColor = Color.white;

    [Header("Character Type (Player or NPC)")]
    [Tooltip("Set this to true if this character is the player.")]
    public bool isPlayerCharacter;
    [Tooltip("Pass in an NPC prefab with the DUAS_Agent script attached to it that represents this character.")]
    public DUAS_Agent artificialIntelligence;
}