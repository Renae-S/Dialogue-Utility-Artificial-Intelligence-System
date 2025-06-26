using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[CreateAssetMenu(fileName = "DUAS_Action", menuName = "DUAS_Action/DUAS_WanderAction", order = 1)]
public class DUAS_WanderAction : DUAS_Action
{
    private Vector3 target;
    [Header("Wander Limitations")]
    [Tooltip("The radius around the agent that they can find their next target position for walking around.")]
    public float radius = 5;
    [Tooltip("If true, the following Boundary Prefab will be used for it's collider size to contain the agent's movement.")]
    public bool needsBoundaries;
    [Tooltip("Boundary Prefab will be used for it's collider size to contain the agent's movement if the Needs Boundaries field is true.")]
    public GameObject boundaryPrefab;
    private GameObject boundary;

    [Header("Idle Timer")]
    [Tooltip("The length of time that the agent will be idle for before finding a new target position to walk to.")]
    public float idleTimerMax;                                // The time in seconds between walking decisions
    private bool startIdleTimer;
    private float idleTimer;                                  // The timer that decreases per frame

    private float speed;

    private Vector3 lastPos;


    public override void Awake()
    {
        DUAS_Agent NPC = FindObjectOfType<DUAS_Agent>();

        foreach (NeedMultiplier NM in needsAffectedMultipliers)
        {
            DUAS_ActionCondition AC = ScriptableObject.CreateInstance<DUAS_ActionCondition>();
            AC.action = this;
            AC.needAffected = NM.need;
            AC.multiplier = NM.multiplier;
            AC.name = AC.action.name + " " + AC.needAffected.needName;

            if (NPC)
                NPC.conditions.Add(AC);
        }

        foreach (EmotionMultiplier EM in emotionsAffectedMultipliers)
        {
            DUAS_ActionCondition AC = ScriptableObject.CreateInstance<DUAS_ActionCondition>();
            AC.action = this;
            AC.emotionAffected = EM.emotion;
            AC.multiplier = EM.multiplier;

            if (NPC)
                NPC.conditions.Add(AC);
        }
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
                urgency += 1;

            // If the need value is zero or below, make the urgency a large value
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
        withinRangeOfTarget = true;
        commitmentToAction = true;

        if (lastPos == agent.transform.position && agent.anim.IsPlaying("Walk"))
            idleTimer = 0;

        // If the idleTimer has not reached 0
        if (idleTimer > 0 && startIdleTimer)
        {
            idleTimer -= Time.deltaTime;       // Reduce the idleTimer by the change in time per frame - to go down in seconds
        }
        else if (idleTimer > 0 && !startIdleTimer)
        {
            if (!agent.anim.IsPlaying("Walk"))
                agent.anim.Play("Walk");

            agent.nav.speed = speed;
            agent.nav.SetDestination(target);
        }
        else
        {
            target = GeneratePosition(agent);

            idleTimer = idleTimerMax;      // Reset the idleTimer
            startIdleTimer = false;

        }

        // If the agent reaches its target position, start idle timer and then generate a new target position
        float distance = Vector3.Distance(agent.transform.position, target);
        if (distance <= 0.5f && !startIdleTimer)
        {
            agent.anim.Play("Idle");
            target = Vector3.zero;
            agent.nav.speed = 0;
            startIdleTimer = true;
        }

        lastPos = agent.transform.position;
    }

    // Intialises any variables in the class on entering the action
    // agent - the agent that the action belongs to
    public override void Enter(DUAS_Agent agent)
    {
        if (!boundary && needsBoundaries)
            boundary = Instantiate(boundaryPrefab, Vector3.zero, Quaternion.identity);

        agent.nav = agent.gameObject.GetComponent<NavMeshAgent>();
        agent.anim = agent.gameObject.GetComponentInChildren<Animation>();
        target = GeneratePosition(agent);
        speed = agent.nav.speed;
        agent.anim.Play("Walk");
        agent.nav.SetDestination(target);
        startIdleTimer = false;
    }

    // Resets variables that were modified on exiting the action
    // agent - the agent that the action belongs to
    public override void Exit(DUAS_Agent agent)
    {
        withinRangeOfTarget = false;
        commitmentToAction = false;
    }

    // Generates a random position
    public Vector3 GeneratePosition(DUAS_Agent agent)
    {
        // Inspired by Valkyr_x's answer: https://answers.unity.com/questions/475066/how-to-get-a-random-point-on-navmesh.html

        Vector3 randomDirection = Random.insideUnitSphere * radius;
        randomDirection += agent.transform.position;
        NavMeshHit hit;
        NavMesh.SamplePosition(randomDirection, out hit, radius, 1);
        Vector3 pos = hit.position;

        if (needsBoundaries)
        {
            if (boundary.GetComponent<Collider>().bounds.Contains(pos))
                return pos;
            else
                pos = GeneratePosition(agent);
        }

        return pos;
    }
}