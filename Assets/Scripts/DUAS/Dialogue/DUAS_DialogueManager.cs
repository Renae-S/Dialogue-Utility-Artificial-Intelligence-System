using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class DUAS_DialogueManager : MonoBehaviour
{
    private Canvas dialogueCanvas;
    private Queue<DUAS_Dialogue> dialogues;
    private Queue<string> sentences;
    private DUAS_Dialogue currentDialogue;

    private Image UIPortraitPlayer;
    private Image UIPortraitNPC;
    private Image UIDialogueBoxPlayer;
    private Image UIDialogueBoxNPC;
    private TextMeshProUGUI UINamePlayer;
    private TextMeshProUGUI UINameNPC;
    private TextMeshProUGUI UIDialoguePlayer;
    private TextMeshProUGUI UIDialogueNPC;

    [Header("Defaults")]
    [Tooltip("Uses this font if the character and dialogue do not have a specified font.")]
    public TMP_FontAsset defaultFont;
    [Tooltip("Uses this color if the character does not have a dialogue box.")]
    public Color defaultDialogueBoxColor; // No set box
    [Tooltip("Uses this color for the dialogue boxes (allows for transparency and color tints).")]
    public Color dialogueBoxColor; // Set box

    private AudioSource audioSource;

    private GameObject UIChoicesPanel;

    private Animator animator;

    protected bool typingInProgress = false;

    protected Player player;
    [HideInInspector]
    public DUAS_Agent NPC;

    private bool first = true;



    // Start is called before the first frame update and is used for variable initialisation
    void Start()
    {
        dialogueCanvas = GetComponent<Canvas>();
        animator = GetComponent<Animator>();
        dialogues = new Queue<DUAS_Dialogue>();
        sentences = new Queue<string>();
        audioSource = GetComponent<AudioSource>();

        if (!defaultFont)
            Debug.LogWarning("No default font for DialogueManager has been set.");


        Transform[] children = GetComponentsInChildren<Transform>();

        foreach (Transform child in children)
        {
            if (child.tag == "Left")
            {
                Transform[] leftChildren = child.GetComponentsInChildren<Transform>();
                foreach (Transform t in leftChildren)
                {
                    switch (t.tag)
                    {
                        case "Portrait":
                            UIPortraitPlayer = t.gameObject.GetComponent<Image>();
                            break;
                        case "Dialogue Box":
                            UIDialogueBoxPlayer = t.gameObject.GetComponent<Image>();
                            break;
                        case "Name":
                            UINamePlayer = t.gameObject.GetComponent<TextMeshProUGUI>();
                            break;
                        case "Dialogue":
                            UIDialoguePlayer = t.gameObject.GetComponent<TextMeshProUGUI>();
                            break;
                    }
                }
            }
            else if (child.tag == "Right")
            {
                Transform[] rightChildren = child.GetComponentsInChildren<Transform>();
                foreach (Transform t in rightChildren)
                {
                    switch (t.tag)
                    {
                        case "Portrait":
                            UIPortraitNPC = t.gameObject.GetComponent<Image>();
                            break;
                        case "Dialogue Box":
                            UIDialogueBoxNPC = t.gameObject.GetComponent<Image>();
                            break;
                        case "Name":
                            UINameNPC = t.gameObject.GetComponent<TextMeshProUGUI>();
                            break;
                        case "Dialogue":
                            UIDialogueNPC = t.gameObject.GetComponent<TextMeshProUGUI>();
                            break;
                    }
                }
            }
            else if (child.tag == "Choices")
                UIChoicesPanel = child.gameObject;

        }
    }

    // Update is called once per frame
    private void Update()
    {
        if (!animator.GetBool("IsOpen") && !first)
        {
            first = true;
        }

        // If the teletype is done, the dialogue box is open and the player interacts 
        if (!typingInProgress && animator.GetBool("IsOpen") && Input.GetButtonUp("Submit") && !animator.GetBool("Choices"))
        {
            DisplayNextSentence(sentences, dialogues);    // Immediately overwrites the sentence text and displays next sentence
        }

    }

    // Takes an array of dialogues that make up the conversation and a player which will be disabled while the dialogue is in progress
    public void StartConveration(DUAS_Conversation conversation, Player p)
    {
        player = p;

        dialogues.Clear();      // Clear the previous conversation

        // For each dialogue in the conversation
        foreach (DUAS_Dialogue dialogue in conversation.dialogues)
        {
            dialogues.Enqueue(dialogue);    // Add to queue of dialogues to be displayed
        }

        StartDialogue(dialogues);
    }

    // Takes a queue of dialogues that is used to create a queue of strings being the sentences that make up the dialogue
    public void StartDialogue(Queue<DUAS_Dialogue> convoDialogues)
    {
        // Open the dialogue box
        animator.SetBool("IsOpen", true);
        // Set current dialogue to the first dialogue in the queue
        currentDialogue = convoDialogues.Dequeue();
        // Clear the previous sentences from the previous dialogue
        sentences.Clear();

        // For each sentence in the current dialogue
        foreach (string sentence in currentDialogue.sentences)
        {
            sentences.Enqueue(sentence);    // Add to queue of strings to be displayed
        }

        if (first)
        {
            DisplayNextSentence(sentences, dialogues);
            first = false;
        }


        if (currentDialogue.isDialogueChoice)
        {
            animator.SetBool("Choices", true);
            SetUpButtons();
        }
        else
        {
            animator.SetBool("Choices", false);
        }

        if (!currentDialogue.character.isPlayerCharacter)
        {
            NPC = currentDialogue.character.artificialIntelligence;
        }

        DUAS_Agent[] agents = FindObjectsOfType<DUAS_Agent>();
        foreach (DUAS_Agent agent in agents)
        {
            if (agent.name == NPC.name)
                NPC = agent;
        }

        if (currentDialogue.affectsEmotion)
        {
            foreach (DUAS_Emotion emotion in currentDialogue.emotionsAffected)
            {
                foreach (DUAS_Emotion agentEmotion in NPC.emotions)
                {
                    if (emotion.emotionName == agentEmotion.emotionName)
                    {
                        if ((agentEmotion.value + currentDialogue.emotionEffectValue) < 0)
                            agentEmotion.value = 0;
                        else if ((agentEmotion.value + currentDialogue.emotionEffectValue) > 1)
                            agentEmotion.value = 1;
                        else
                            agentEmotion.value += currentDialogue.emotionEffectValue;
                    }
                }
            }
        }
    }

    private void SetUpButtons()
    {
        Button[] buttons = GetComponentsInChildren<Button>();

        for (int i = 0; i < buttons.Length; i++)
        {
            buttons[i].GetComponentInChildren<TextMeshProUGUI>().text = currentDialogue.choices[i].choiceTitle;
        }
    }

    // Takes a queue of strings that are going to be displayed in the dialogue box and the rest of the dialogue queue to start the next dialogue when ready
    public void DisplayNextSentence(Queue<string> sentences, Queue<DUAS_Dialogue> convoDialogues)
    {
        // If all the sentences in the queue have been displayed
        if (sentences.Count == 0)
        {
            // If all the dialogues in the queue have been displayed
            if (convoDialogues.Count == 0)
            {
                // The conversation has finished and the dialogue box can be closed
                EndConversation();
                return;
            }
            // Start the next dialogue from the queue
            StartDialogue(convoDialogues);
        }

        // If there are still sentences to be displayed
        if (sentences.Count > 0)
        {
            if (currentDialogue.character.isPlayerCharacter)
            {
                animator.SetBool("PlayerDialogue", true);
                animator.SetBool("AIDialogue", false);

                if (currentDialogue.expression)
                    UIPortraitPlayer.sprite = currentDialogue.expression;
                else
                    UIPortraitPlayer.sprite = currentDialogue.character.defaultSprite;

                if (currentDialogue.character.dialogueBox)
                {
                    UIDialogueBoxPlayer.sprite = currentDialogue.character.dialogueBox;
                    UIDialogueBoxPlayer.color = dialogueBoxColor;
                }
                else
                {
                    UIDialogueBoxPlayer.sprite = null;
                    UIDialogueBoxPlayer.color = defaultDialogueBoxColor;
                }

                if (currentDialogue.character.nameFont)
                    UINamePlayer.font = currentDialogue.character.nameFont;
                else
                    UINamePlayer.font = defaultFont;

                UINamePlayer.text = currentDialogue.character.characterName;
                UINamePlayer.color = currentDialogue.character.nameColor;

                if (currentDialogue.useNonCharacterDialogueColor)
                    UIDialoguePlayer.color = currentDialogue.textColor;
                else
                    UIDialoguePlayer.color = currentDialogue.character.defaultDialogueColor;

                if (currentDialogue.useNonDefaultFont)
                    UIDialoguePlayer.font = currentDialogue.font;
                else
                    UIDialoguePlayer.font = currentDialogue.character.defaultDialogueFont;
        

                // Dequeue the queue of sentences and display sentence and start the teletyping
                string sentence = sentences.Dequeue();
                UIDialoguePlayer.text = sentence;
                UIDialoguePlayer.ForceMeshUpdate(true);
                typingInProgress = true;
                StartCoroutine(Teletype(sentence, UIDialoguePlayer));
            }

            else
            {
                animator.SetBool("AIDialogue", true);
                animator.SetBool("PlayerDialogue", false);

                if (currentDialogue.expression)
                    UIPortraitNPC.sprite = currentDialogue.expression;
                else
                    UIPortraitNPC.sprite = currentDialogue.character.defaultSprite;

                if (currentDialogue.character.dialogueBox)
                {
                    UIDialogueBoxNPC.sprite = currentDialogue.character.dialogueBox;
                    UIDialogueBoxNPC.color = dialogueBoxColor;
                }
                else
                {
                    UIDialogueBoxNPC.sprite = null;
                    UIDialogueBoxNPC.color = defaultDialogueBoxColor;
                }

                if (currentDialogue.character.nameFont)
                    UINameNPC.font = currentDialogue.character.nameFont;
                else
                    UINameNPC.font = defaultFont;

                UINameNPC.text = currentDialogue.character.characterName;
                UINameNPC.color = currentDialogue.character.nameColor;

                // Dialogue color
                if (currentDialogue.useNonCharacterDialogueColor)
                    UIDialogueNPC.color = currentDialogue.textColor;
                else
                    UIDialogueNPC.color = currentDialogue.character.defaultDialogueColor;

                // Dialogue font
                if (currentDialogue.useNonDefaultFont)
                    UIDialogueNPC.font = currentDialogue.font;
                else
                    UIDialogueNPC.font = currentDialogue.character.defaultDialogueFont;
                   

                // Dequeue the queue of sentences and display sentence and start the teletyping
                string sentence = sentences.Dequeue();
                UIDialogueNPC.text = sentence;
                UIDialogueNPC.ForceMeshUpdate(true);
                typingInProgress = true;
                StartCoroutine(Teletype(sentence, UIDialogueNPC));
            }

            if (currentDialogue.dialogueSound && audioSource.clip != currentDialogue.dialogueSound)
            {
                audioSource.clip = currentDialogue.dialogueSound;
                audioSource.Play();
            }
        }
    }


    public void ChoiceOne()
    {
        if (currentDialogue.choices.Length > 0)
            StartConveration(currentDialogue.choices[0], player);
        DisplayNextSentence(sentences, dialogues);    // Immediately overwrites the sentence text and displays next sentence
    }

    public void ChoiceTwo()
    {
        if (currentDialogue.choices.Length > 0)
            StartConveration(currentDialogue.choices[1], player);
        DisplayNextSentence(sentences, dialogues);    // Immediately overwrites the sentence text and displays next sentence
    }

    public void ChoiceThree()
    {
        if (currentDialogue.choices.Length > 0)
            StartConveration(currentDialogue.choices[2], player);
        DisplayNextSentence(sentences, dialogues);    // Immediately overwrites the sentence text and displays next sentence
    }

    // EnConversation closes the dialogue box and enables them to move again
    public void EndConversation()
    {
        animator.SetBool("PlayerDialogue", false);
        animator.SetBool("AIDialogue", false);
        animator.SetBool("IsOpen", false);
    }

    // Teletype allows for a string sentence to be passed in and print out one letter at a time 
    protected IEnumerator Teletype(string sentence, TextMeshProUGUI UIDialogue)
    {
        int totalVisibleCharacters = UIDialogue.text.Length;
        int counter = 0;

        // While there is teletyping in progress
        while (typingInProgress)
        {
            int visibleCount = counter;
            
            if (!Input.GetButton("Submit") || visibleCount == 0)
            {
                visibleCount = counter;
            }
            else
            {
                visibleCount = totalVisibleCharacters;
            }

            UIDialogue.maxVisibleCharacters = visibleCount;

            // Once the last character is revealed
            if (visibleCount >= totalVisibleCharacters)
            {
                typingInProgress = false;
                break;
            }

            counter += 1;

            yield return new WaitForSeconds(currentDialogue.speed);
        }
    }

    public DUAS_Dialogue GetCurrentDialogue()
    {
        return currentDialogue;
    }
}