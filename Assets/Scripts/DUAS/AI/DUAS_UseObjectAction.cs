using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "DUAS_Action", menuName = "DUAS_Action/DUAS_UseObjectAction", order = 1)]
public class DUAS_UseObjectAction : DUAS_Action
{
    [Header("Animation Handling")]
    [Tooltip("The animation in order that the agent needs to play in order to do this action.")]
    public AnimationClip[] animationsInOrder;
    [Tooltip("The positions in order that the agent needs to be at during the animations in order to do this action.")]
    public Transform[] positionsForAnimations;
    [Tooltip("Where the agent will spawn when they stop using this object.")]
    public Transform exitActionPosition;
    [Tooltip("The NavMeshAgent's base offset that the agent needs to be at during the animations in order to do this action.")]
    public float[] baseOffsetsForAnimations;
    [Tooltip("The animation the represents this action (should be the last animation within the Animations In Order field).")]
    public AnimationClip mainAnimation;

    [Header("Speed")]
    [Tooltip("The maximum speed that the NavMeshAgent can go while walking.")]
    [Range(0.0f, 10.0f)]
    public float maxSpeed = 3.0f;

    private float distance;


    GameObject target;
    DUAS_Useable targetUseable;
    private bool animationsQueued;

    private float originalBaseOffset;

    private Vector3 lastPos;
    private Quaternion lastRot;

    Vector3 targPos = Vector3.zero;
    Quaternion targRot = Quaternion.identity;

    public override void Awake()
    {
        DUAS_Agent[] NPCs = FindObjectsOfType<DUAS_Agent>();

        if (needsAffectedMultipliers.Length > 0 && NPCs.Length > 0)
        {
            foreach (NeedMultiplier NM in needsAffectedMultipliers)
            {
                DUAS_ActionCondition AC = ScriptableObject.CreateInstance<DUAS_ActionCondition>();
                AC.action = this;
                AC.needAffected = NM.need;
                AC.multiplier = NM.multiplier;
                AC.name = AC.action.name + " " + AC.needAffected.needName;

                foreach (DUAS_Agent NPC in NPCs)
                    NPC.conditions.Add(AC);
            }
        }

        if (emotionsAffectedMultipliers.Length > 0 && NPCs.Length > 0)
        {
            foreach (EmotionMultiplier EM in emotionsAffectedMultipliers)
            {
                DUAS_ActionCondition AC = ScriptableObject.CreateInstance<DUAS_ActionCondition>();
                AC.action = this;
                AC.emotionAffected = EM.emotion;
                AC.multiplier = EM.multiplier;

                foreach (DUAS_Agent NPC in NPCs)
                    NPC.conditions.Add(AC);
            }
        }
    }

    // Sets the GameObject passed in as the target GameObject of an Action
    public override void SetGameObject(GameObject go)
    {
        target = go;
    }

    // Evaluates all of the agents needs and calculates the urgency of the need with a float - a high value mean a high importance
    // agent - the agent that has its needs evaluated
    public override float Evaluate(DUAS_Agent agent)
    {
        float finalEvaluation = 0;

        // Sum of needs urgency(i) * (recovery(i) * 10 - distance/speed * decrement(i)  

        // For every need of the agent
        foreach (DUAS_Need need in agent.needs)
        {
            float urgency = 0;
            float recovery = 0;
            float decrement = 0;
            float evaluationValue = 0;

            // Calculate urgency
            if (commitmentToAction)
                urgency += 1000000f;

            // If the need value is zero or below, make the urgency a large value and increase the agent's maximum range
            if (need.value <= 0)
                urgency = 100000;

            // If the value of the need is full then urgency is set to 0
            else if (need.value >= 1)
                urgency = 0;

            // If the value of the need neither 1 or 0, the urgency is the (1 - the value of the need) * the amount of needs the agent has
            else if (need.value > 0 && need.value < 1)
                urgency += (1 - need.value) * agent.needs.Count;

            // Calculate recovery and decrement (need gained in ten seconds)
            // For every condition of the agent
            foreach (DUAS_Condition condition in agent.conditions)
            {
                // If the condition is an action condition
                if (condition.GetType() == typeof(DUAS_ActionCondition))
                {
                    // Create an action condition of the current condition
                    DUAS_ActionCondition actionCondition = (DUAS_ActionCondition)condition;
                    // If the action condition's action is the same as this action
                    if (actionCondition.action.name == this.name)
                    {
                        // If this need is affected
                        if (need.needName == actionCondition.needAffected.needName)
                        {
                            // If the condition's multiplier is positive, calculate the recovery
                            if (actionCondition.multiplier > 0)
                            {
                                recovery = actionCondition.multiplier * 10;
                                decrement = 0;
                            }
                            // If the condition's multiplier is negative, calculate the decrement
                            else if (actionCondition.multiplier < 0)
                            {
                                recovery = 0;
                                decrement = actionCondition.multiplier * 10;
                            }
                            // If the condition's multiplier is 0, recovery and decrement is 0
                            else
                            {
                                recovery = 0;
                                decrement = 0;
                            }
                        }
                    }
                }

                // Calculate evaluation value
                evaluationValue += urgency * (recovery + decrement);
            }
            finalEvaluation += evaluationValue;
        }

        return finalEvaluation;
    }

    // Updates the agents movement, needs, animation and destination
    // agent - the agent that has its movement and needs updated
    public override void UpdateAction(DUAS_Agent agent)
    {
        commitmentToAction = true;

        // If the agent is too far away, move to the target
        if (Vector3.Distance(agent.transform.position, target.transform.position) > agent.targetUseable.range)
        {
            agent.nav.SetDestination(target.transform.position);
            agent.nav.speed = maxSpeed;
            agent.anim.Play("Walk");

            distance = Vector3.Distance(agent.transform.position, target.transform.position);
        }
        // Otherwise play animation and get bonuses
        else if (!agent.currentAction.withinRangeOfTarget)
        {   
            // If the animation to be played is not an idle animation, then check if the current animation of the agent is not the animation passes in and set the animation to play if so
            if (mainAnimation.name != "Idle")
            {
                agent.anim.Stop("Walk");
                if (!targetUseable.holdable)
                    agent.transform.forward = new Vector3(target.transform.position.x - agent.transform.position.x, 0, target.transform.position.z - agent.transform.position.z);
                if (!animationsQueued)
                {
                    foreach (AnimationClip clip in animationsInOrder)
                    {
                        //Queues each of these animations to be played one after the other
                        agent.anim.CrossFadeQueued(clip.name, 0.3f, QueueMode.CompleteOthers);
                    }

                    agent.nav.velocity = Vector3.zero;
                    animationsQueued = true;
                }

                if (positionsForAnimations.Length > 0 && animationsQueued)
                {
                    for (int i = 0; i < positionsForAnimations.Length; i++)
                    {
                        if (agent.anim.IsPlaying(animationsInOrder[i].name))
                        {
                            agent.nav.baseOffset = baseOffsetsForAnimations[i];
                            targPos = new Vector3(positionsForAnimations[i].position.x, agent.transform.position.y, positionsForAnimations[i].position.z);
                            targRot = positionsForAnimations[i].rotation;
                            break;
                        }
                    }
                }

                // Set the agent's position and rotation
                if (agent.anim.IsPlaying(mainAnimation.name))
                {
                    agent.currentAction.withinRangeOfTarget = true;

                    if (!targetUseable.holdable)
                    {
                        if (positionsForAnimations.Length > 0)
                        {
                            agent.nav.baseOffset = baseOffsetsForAnimations[positionsForAnimations.Length - 1];
                            targPos = new Vector3(positionsForAnimations[positionsForAnimations.Length - 1].position.x, agent.transform.position.y, 
                                positionsForAnimations[positionsForAnimations.Length - 1].position.z);
                            targRot = positionsForAnimations[positionsForAnimations.Length - 1].rotation;
                        }
                    }
                }

                if (!targetUseable.holdable)
                {
                    if(targPos != Vector3.zero)
                        agent.transform.position = Vector3.Lerp(lastPos, targPos, Time.deltaTime * 5);
                    agent.transform.rotation = Quaternion.Lerp(lastRot, targRot, Time.deltaTime * 5);
                }
            }
        }
        else
        {
            agent.anim.Play(mainAnimation.name);
            
            // Set the agent's position and rotation
            if (positionsForAnimations.Length > 0)
            {
                agent.nav.baseOffset = baseOffsetsForAnimations[positionsForAnimations.Length - 1];
                targPos = new Vector3(positionsForAnimations[positionsForAnimations.Length - 1].position.x, agent.transform.position.y,
                                 positionsForAnimations[positionsForAnimations.Length - 1].position.z);
                targRot = positionsForAnimations[positionsForAnimations.Length - 1].rotation;
            }
            if (!targetUseable.holdable)
            {
                if (targPos != Vector3.zero)
                    agent.transform.position = Vector3.Lerp(lastPos, targPos, Time.deltaTime * 10);
                agent.transform.rotation = Quaternion.Lerp(lastRot, targRot, Time.deltaTime * 10);
            }
        }
        lastPos = agent.transform.position;
        lastRot = agent.transform.rotation;
    }

    // Intialises any variables in the class on entering the action
    // agent - the agent that the action belongs to
    public override void Enter(DUAS_Agent agent)
    {
        agent.nav = agent.gameObject.GetComponent<NavMeshAgent>();
        agent.anim = agent.gameObject.GetComponentInChildren<Animation>();
        withinRangeOfTarget = false;
        distance = Vector3.Distance(agent.transform.position, target.transform.position);
        commitmentToAction = false;
        animationsQueued = false;
        originalBaseOffset = agent.nav.baseOffset;
        targetUseable = target.GetComponent<DUAS_Useable>();
    }

    // Resets variables that were modified on exiting the action
    // agent - the agent that the action belongs to
    public override void Exit(DUAS_Agent agent)
    {
        if (agent.holdingObject)
        {
            target.transform.parent = null;
            target.transform.SetPositionAndRotation(agent.targetObjectOriginalPosition, agent.targetObjectOriginalRotation);
            agent.holdingObject = false;
        }

        if (exitActionPosition)
        {
            agent.nav.Warp(exitActionPosition.position);
            agent.transform.forward = target.transform.forward;
        }


        withinRangeOfTarget = false;
        commitmentToAction = false;
        agent.nav.baseOffset = originalBaseOffset;
    }
}
