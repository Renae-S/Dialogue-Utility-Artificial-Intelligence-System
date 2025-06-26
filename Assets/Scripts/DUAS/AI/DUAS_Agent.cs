using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;
using System;
using TMPro;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(SphereCollider))]
[RequireComponent(typeof(CapsuleCollider))]
public class DUAS_Agent : MonoBehaviour
{
    [Header("Turn On/Off Utility AI")]
    [Tooltip("If true, the agent will have needs and emotions change, altering their choice in actions, if false, they will remain idle but still allow for dialogue to occur.")]
    public bool useUtilityAI = true;

    [Header("Needs and Emotions")]
    [Tooltip("The needs that apply to this agent.")]
    public List<DUAS_Need> needs = new List<DUAS_Need>();
    [Tooltip("The emotions that apply to this agent.")]
    public List<DUAS_Emotion> emotions = new List<DUAS_Emotion>();

    [Header("UI Prefabs")]
    [Tooltip("The needs bar prefab that will be instantiated per need of this agent.")]
    public GameObject needBarPrefab;
    [Tooltip("The emotions bar prefab that will be instantiated per emotion of this agent.")]
    public GameObject emotionBarPrefab;

    [Header("Actions")]  
    [Tooltip("The actions of this agent that are not using an object (e.g. Wander, Idle, etc.).")]
    public DUAS_Action[] intrinsicActions;                           // An array of all the intrinsic actions
    public Dictionary<GameObject, DUAS_Action> actionsOnUseables;    // A dictionary of all avaliable actions on useable GameObjects
    [Tooltip("The time between agent evaluations their needs and deciding on their next best action.")]
    [Range(0.0f, 20.0f)]
    public float actionTimerMax = 10.0f;                        // The time in seconds between action decisions
    [HideInInspector]
    public float actionTimer = 0;                                   // The timer that decreases per frame
    [Tooltip("If any need is below this value, then the agent will evaluate their needs and decides on their next best action.")]
    [Range(0.0f, 1.0f)]
    public float maxNeedValueBeforeActionRequired = 0.85f;      // The fill value of the need before using objects actions are a priority

    [Header("Conditions")]
    [Tooltip("All of the conditions that apply to this agent. This will fill automatically but TimeOfDayConditions (or custom NeedConditions) should be put here.")]
    public List<DUAS_Condition> conditions = new List<DUAS_Condition>();                        // A list of all the conditions that can be met

    [HideInInspector]
    public GameObject AICanvas;
    private List<Image> nBars;                                  // An array of all the UI need bars
    [HideInInspector]
    public Dictionary<DUAS_Need, Image> needBars;               // A dictionary of all the UI need bars and their relative DUAS_Need
    [HideInInspector]
    public Dictionary<string, Image> needNameBars;           // A dictionary of all the UI need bars and their relative DUAS_Need names
    private List<Image> eBars;                                  // An array of all the UI emotion bars
    [HideInInspector]
    public Dictionary<DUAS_Emotion, Image> emotionBars;         // A dictionary of all the UI emotion bars and their relative DUAS_Emotion
    [HideInInspector]
    public Dictionary<string, Image> emotionNameBars;         // A dictionary of all the UI emotion bars and their relative DUAS_Emotion names

    private GameObject needsPanel;
    private GameObject emotionsPanel;

    private DUAS_Action best;                                   // The best possible action the agent can carry out due to it's circumstances
    [HideInInspector]
    public DUAS_Action currentAction;                           // The action we're currently carry out
    private Text currentActionText;                              // The UI text that shows the current action
    private Text currentEmotionText;                             // The UI text that shows the current emotion
    private float changeInHappiness;                            // The change in happiness value
  
    [HideInInspector]
    public GameObject sun;                                      // The directional light representing the sun (if day/night cycle included)     

    [HideInInspector]
    public GameObject targetObject;                             // The target GameObject
    [HideInInspector]
    public Vector3 targetObjectOriginalPosition;
    [HideInInspector]
    public Quaternion targetObjectOriginalRotation;
    [HideInInspector]
    public DUAS_Useable targetUseable;                          // The target Useable
    [Header("Useable Objects Range")]
    [Tooltip("The maximum range that the agent will look for useable objects.")]
    [Range(0.0f, 200.0f)]
    public float maxRange = 200.0f;                             // The max radius around the agent for finding actions it can do on objects

    [HideInInspector]
    public NavMeshAgent nav;                                    // The agent's NavMeshAgent component
    [HideInInspector]
    public Animation anim;                                      // The agent's Animation component


    private bool playerWithinRange;
    private Vector3 playerDirection;
    private Quaternion myRotation;

    [Header("Agent Body Parts")]
    [Tooltip("Pass in the hand of this agent model (will be used to pick up objects).")]
    public Transform hand;
    [Tooltip("Pass in the head of this agent model (will be used to look at the player).")]
    public Transform head;
    [HideInInspector]
    public bool holdingObject;

    private Canvas dialogueCanvas;
    [Header("Dialogue")]
    [Tooltip("Pass in every possible conversation that this agent can have.")]
    public DUAS_Conversation[] conversationPossibilities;
    private DUAS_DialogueManager dialogueManager;
    private Animator dialogueAnimator;

    private Player player;

    // Use this for initialization
    private void Start()
    {
        actionsOnUseables = new Dictionary<GameObject, DUAS_Action>();
        needBars = new Dictionary<DUAS_Need, Image>();
        needNameBars = new Dictionary<string, Image>();
        emotionBars = new Dictionary<DUAS_Emotion, Image>();
        emotionNameBars = new Dictionary<string, Image>();
        nBars = new List<Image>();
        eBars = new List<Image>();
        best = null;
        currentAction = null;
        targetObject = null;
        targetUseable = null;
        nav = GetComponent<NavMeshAgent>();
        anim = GetComponentInChildren<Animation>();
        playerWithinRange = false;

        for (int i = 0; i < needs.Count; i++)
        {
            // Clone a copy of the need for this particular agent - will call the Awake() on DUAS_Need to create the NeedConditions
            string nm = needs[i].name;
            DUAS_Need needCopy = Instantiate(needs[i]) as DUAS_Need;
            needs[i] = needCopy;
            needs[i].name = nm;
        }

        for (int i = 0; i < emotions.Count; i++)
        {
            // Clone a copy of the emotion for this particular agent 
            string nm = emotions[i].name;
            DUAS_Emotion emotionCopy = Instantiate(emotions[i]) as DUAS_Emotion;
            emotions[i] = emotionCopy;
            emotions[i].name = nm;

            if (emotions[i].emotionName == "Happiness")
                emotions[i].value = 1;
            else
                emotions[i].value = 0;
        }

        if (FindObjectOfType<DUAS_DialogueManager>())
        {
            dialogueManager = FindObjectOfType<DUAS_DialogueManager>();
            dialogueCanvas = dialogueManager.gameObject.GetComponent<Canvas>();
            dialogueAnimator = dialogueManager.GetComponent<Animator>();
        }

        if (GameObject.FindGameObjectWithTag("AI Canvas"))
        {
            AICanvas = GameObject.FindGameObjectWithTag("AI Canvas");
            currentActionText = GameObject.FindGameObjectWithTag("Current Action").GetComponent<Text>();
            currentEmotionText = GameObject.FindGameObjectWithTag("Current Emotion").GetComponent<Text>();
            needsPanel = GameObject.FindGameObjectWithTag("Needs");
            emotionsPanel = GameObject.FindGameObjectWithTag("Emotions");
        }

        foreach (DUAS_Action action in intrinsicActions)
        {
            DUAS_Action newAction = Instantiate(action);
            newAction.name = action.name;
        }

        if (GameObject.FindGameObjectWithTag("AI Canvas"))
        {
            // For every need in Needs, add the name of the need and the bar image that represents that need's value
            if (needs.Count > 0)
            {
                for (int i = 0; i < needs.Count; i++)
                {
                    GameObject needBar = Instantiate(needBarPrefab);
                    needBar.transform.SetParent(needsPanel.transform);
                    nBars.Add(needBar.transform.Find("FillBar").GetComponent<Image>());

                    needBar.GetComponentInChildren<TextMeshProUGUI>().text = needs[i].needName;

                    needs[i].value = 1;

                    needBars.Add(needs[i], nBars[i]);
                    needNameBars.Add(needs[i].name, nBars[i]);
                }
            }


            // For every emotion in emotions, add the name of the emotion and the bar image that represents that emotion's value
            if (emotions.Count > 0)
            {
                for (int i = 0; i < emotions.Count; i++)
                {
                    GameObject emotionBar = Instantiate(emotionBarPrefab);
                    emotionBar.transform.SetParent(emotionsPanel.transform);
                    eBars.Add(emotionBar.transform.Find("FillBar").GetComponent<Image>());

                    emotionBar.GetComponentInChildren<TextMeshProUGUI>().text = emotions[i].emotionName;

                    emotionBars.Add(emotions[i], eBars[i]);
                    emotionNameBars.Add(emotions[i].name, eBars[i]);
                }
            }
        } 

        // For each condition in conditions, call Awake() to initialise condition variables
        foreach (DUAS_Condition condition in conditions)
            condition.Awake();

        for (int i = 0; i < needs.Count; i++)
            needs[i].value = 1;


        if (GameObject.FindGameObjectWithTag("Sun"))
            sun = GameObject.FindGameObjectWithTag("Sun");
    }

    // Update is called once per frame - updates the needs, UI, health, current action, target, and action timer
    void Update()
    {
        if (dialogueManager)
        {
            if (!dialogueAnimator.GetBool("IsOpen"))
                dialogueManager.NPC = null;
            if (dialogueManager.NPC == null || dialogueManager.NPC.name != this.name)
            {
                if (useUtilityAI)
                    UpdateAI();
                else
                    anim.Play("Idle");
            }
            else
            {
                nav.isStopped = true;

                // Emote (animation)
                if (dialogueAnimator.GetBool("AIDialogue"))
                {
                    if (dialogueManager.GetCurrentDialogue().emote)
                        anim.Play(dialogueManager.GetCurrentDialogue().emote.name);
                    else
                        anim.Play("Talk");
                }
                else
                    anim.Play("Idle");
            }
        }
        else
        {
            if (useUtilityAI)
                UpdateAI();
            else
                anim.Play("Idle");
        }
    }

    private void UpdateAI()
    {
        nav.isStopped = false;

        // If the actionTimer has reached 0
        if (actionTimer <= 0 || currentAction == null)
        {
            best = GetBestAction();         // Find the best action
            actionTimer = actionTimerMax;   // Reset the actionTimer
        }
        actionTimer -= Time.deltaTime;      // Reduce the actionTimer by the change in time per frame - to go down in seconds

        // If it is different from what the agent is currently doing, switch the finite state machine
        if (best != currentAction)
        {
            if (currentAction)
                currentAction.Exit(this);
            currentAction = best;
            if (currentAction)
                currentAction.Enter(this);
        }


        // If the currentAction is set, update the current action and the text describing the action
        if (currentAction)
        {
            currentAction.UpdateAction(this);

            if (AICanvas)
                currentActionText.text = currentAction.name;

            HoldObject();
        }

        // Update current emotion text
        if (emotions.Count > 0 && AICanvas)
            currentEmotionText.text = GetHighestEmotion();


        if (!currentAction)
        {
            targetObject = null;
            targetUseable = null;
            anim.Play("Idle");
        }

        if (needs.Count > 0 && conditions.Count > 0)
            UpdateNeedsAndEmotions();      // Updates agent's need and emotion values including UI values

        if (emotions.Count > 0)
        {
            UpdateEmotions();
        }
    }

    private void HoldObject()
    {
        // If the current action requires a holdable useable, then hold the GameObject
        if (hand && targetObject && currentAction.withinRangeOfTarget && targetObject.transform.parent == null && targetUseable.holdable)
        {
            if (currentAction.GetType() == typeof(DUAS_UseObjectAction))
            {
                targetObjectOriginalPosition = targetObject.transform.position;
                targetObjectOriginalRotation = targetObject.transform.rotation;

                targetObject.transform.parent = hand;
                targetObject.transform.position = hand.position;
                holdingObject = true;
            }
        }
    }

    private void LateUpdate()
    {
        Quaternion rotationAngle = Quaternion.LookRotation(playerDirection);

        if ((playerWithinRange) && (currentAction == null || currentAction.GetType() == typeof(DUAS_WanderAction)))
        {
            head.rotation = Quaternion.Slerp(myRotation, rotationAngle, Time.deltaTime * 1.5f);

            myRotation = head.rotation;

        }
        else if ((!playerWithinRange) && (currentAction == null || currentAction.GetType() == typeof(DUAS_WanderAction)))
        {
            head.rotation = Quaternion.Slerp(myRotation, transform.rotation, Time.deltaTime * 3);

            myRotation = head.rotation;
        }
    }


    private DUAS_Conversation EvaluateConversations()
    {
        List<DUAS_Conversation> suitableConversations = new List<DUAS_Conversation>();
        List<DUAS_Conversation> otherConversations = new List<DUAS_Conversation>();
        DUAS_Conversation bestFitConversation;

        if (conversationPossibilities.Length > 1)
        {
            foreach (DUAS_Conversation conversation in conversationPossibilities)
            {
                if (conversation.emotionToTriggerThisConversation.emotionName == GetHighestEmotion())
                    suitableConversations.Add(conversation);
                else
                    otherConversations.Add(conversation);
            }
            if (suitableConversations.Count > 1)
                bestFitConversation = suitableConversations[UnityEngine.Random.Range(0, suitableConversations.Count)];
            else
                bestFitConversation = suitableConversations[0];
        }
        else if (conversationPossibilities.Length == 1)
            bestFitConversation = conversationPossibilities[0];
        else
        {
            bestFitConversation = null;
            Debug.LogWarning("There are no conversation possibilities for this Agent.");
        }
    

        return bestFitConversation;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "Player")
        {
            if (!player)
                player = other.GetComponent<Player>();

            playerWithinRange = true;

            playerDirection = player.transform.position - transform.position;

            Debug.DrawRay(head.transform.position, playerDirection, Color.red);

            myRotation = head.rotation;

            if (dialogueCanvas)
            {
                if (Input.GetButtonUp("Submit") && player.NPCWithinRange && (currentAction == null || currentAction.GetType() == typeof(DUAS_WanderAction)))
                {
                    // If the player interacts with the trigger and the dialogue box exists
                    if (dialogueAnimator != null)
                    {
                        // Check if the dialogue is not currently open
                        if (dialogueAnimator.GetBool("IsOpen") == false)
                        {
                            // If so, trigger dialogue
                            DUAS_Conversation conversation = EvaluateConversations();
                            //DUAS_Conversation conversation = conversationPossibilities[0];

                            dialogueManager.StartConveration(conversation, player);
                        }
                    }
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            myRotation = head.rotation;
            playerWithinRange = false;
        }
    }

    // Returns the name of the highest emotion value of the agent
    string GetHighestEmotion()
    {
        DUAS_Emotion highestEmotion = null;
        float emotionValue = 0;

        float bestValue = 0;

        // For every emotion of the agent, check it and determine the highest value emotion and return the generic name of the highest value emotion
        foreach (DUAS_Emotion emotion in emotions)
        {
            emotionValue = emotion.value;
            if (highestEmotion == null || emotionValue > bestValue)
            {
                highestEmotion = emotion;
                bestValue = emotionValue;
            }
        }

        return highestEmotion.name;
    }

    private void UpdateEmotions()
    {
        if (AICanvas)
        {
            // For each of the emotion bars of the agent
            for (int i = 0; i < eBars.Count; i++)
            {
                eBars[i].fillAmount = emotions[i].value;


                if (emotions[i].name == "Happiness")
                {
                    float happinessValue = UpdateHappiness();

                    if ((emotions[i].value + happinessValue) < 0)
                        emotions[i].value = 0;
                    else if ((emotions[i].value + happinessValue) > 1)
                        emotions[i].value = 1;
                    else
                        emotions[i].value += happinessValue;
                }
            }
        }
        else
        {
            foreach (DUAS_Emotion emotion in emotions)
            {
                if (emotion.emotionName == "Happiness")
                {
                    float happinessValue = UpdateHappiness();

                    if ((emotion.value + happinessValue) < 0)
                        emotion.value = 0;
                    else if ((emotion.value + happinessValue) > 1)
                        emotion.value = 1;
                    else
                        emotion.value += happinessValue;
                }
            }
        }
    }

    private float UpdateHappiness()
    {
        int NumOfHighEmotions = 0;
        int NumOfLowEmotions = 0;

        // For each of the emotion bars of the agent
        foreach (DUAS_Emotion emotion in emotions)
        {
            if (emotion.emotionName != "Happiness")
            {
                // If the bar is high, then increment the number of high emotion bars 
                if (emotion.value >= 0.8f)
                    NumOfHighEmotions++;
                else if (emotion.value <= 0.2f)
                    NumOfLowEmotions++;
            }
        }

        // Apply the changes of the agents health according to the number of low need bars multiplied by a negative multiplier and the change in time
        changeInHappiness = ((NumOfHighEmotions * -0.01f) + (NumOfLowEmotions * 0.01f)) * Time.deltaTime;

        return changeInHappiness;
    } 

    // Returns the action (within the agent's radius) with the best evaluation value 
    DUAS_Action GetBestAction()
    {
        for (int i = 0; i < needs.Count; i++)
        {
            if (needs[i].value <= maxNeedValueBeforeActionRequired)
            {
                // Clear the previous actions from the last decision
                actionsOnUseables.Clear();

                // Physics overlapsphere and check every useable around the agent
                Collider[] items = Physics.OverlapSphere(transform.position, maxRange);

                // For every collider hit in the agents range
                foreach (Collider col in items)
                {
                    // If the GameObject is a useable
                    DUAS_Useable useable = col.GetComponent<DUAS_Useable>();
                    if (useable)
                    {
                        if (col.gameObject.tag == "Useable")
                        {
                            // Add the GameObject and the action associated to the actionsOnUseables dictionary
                            actionsOnUseables.Add(col.gameObject, col.gameObject.GetComponent<DUAS_Useable>().action);
                        }
                    }
                }
                break;
            }
        }

        
        DUAS_Action bestObjectAction = null;
        DUAS_Action bestIntrinsicAction = null;
        float bestValue = -99999999;
        float intrinsicValue = bestValue;
        float objectValue = bestValue;



        // For every intrisic action of the agent, evaluate it and determine the highest value action and keep it set as the bestIntrinsicAction variable
        foreach (DUAS_Action intrinsicAction in intrinsicActions)
        {
            intrinsicValue = intrinsicAction.Evaluate(this);
            if (intrinsicAction == null || intrinsicValue > bestValue)
            {
                bestIntrinsicAction = intrinsicAction;
                bestValue = intrinsicValue;
            }
        }

        // For every useable action of the agent, evaluate it and determine the highest value action and keep it set as the bestObjectAction variable
        // Set the target object to the object that carries this action
        foreach (KeyValuePair<GameObject, DUAS_Action> a in actionsOnUseables)
        {
            objectValue = a.Value.Evaluate(this);
            if (bestObjectAction == null || objectValue > bestValue)
            {
                bestObjectAction = a.Value;
                bestValue = objectValue;
                targetObject = a.Key;
                targetUseable = targetObject.GetComponent<DUAS_Useable>();
            }
        }

        // Determine the action with the highest value out of the object and intrinsic actions, and return the action with the higher value
        return intrinsicValue > objectValue ? bestIntrinsicAction : bestObjectAction;
    }

    // Checks every condition in the agent and if the condition is true, it updates the need values associated with the condition
    void UpdateNeedsAndEmotions()
    {
        // For every condition in the agent
        foreach (DUAS_Condition condition in conditions)
        {
            // If the condition is not currently met, then exit the condition
            if (!condition.CheckCondition(this))
                condition.Exit(this);

            // If the condition is currently met, then update the condition needs
            if (condition.CheckCondition(this))
                condition.UpdateUI(this);
        }
    }

    public DUAS_Need GetNeed(DUAS_Need need)
    {
        foreach (DUAS_Need n in needs)
        {
            if (n.needName == need.needName)
                return n;
        }

        return null;
    }

    public DUAS_Emotion GetEmotion(DUAS_Emotion emotion)
    {
        foreach (DUAS_Emotion e in emotions)
        {
            if (e.emotionName == emotion.emotionName)
                return emotion;
        }

        return null;
    }


    public DUAS_Emotion GetEmotionFromName(string emotionName)
    {
        foreach (DUAS_Emotion e in emotions)
        {
            if (e.emotionName == emotionName)
                return e;
        }

        return null;
    }
}

