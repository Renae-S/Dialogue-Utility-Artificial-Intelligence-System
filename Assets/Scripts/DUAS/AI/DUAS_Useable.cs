using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class DUAS_Useable : MonoBehaviour
{
    [Header("Action")]
    [Tooltip("The action the agent can do on this useable.")]
    public DUAS_Action action;       // The action the agent can do on this useable
    [Tooltip("The distance of which the agent can do the action on this useable.")]
    [Range(0.0f, 25.0f)]
    public float range;         // The range the agent has to be within to interact with the useable
    [HideInInspector]
    public bool withinRange = false;

    [Header("Useable Type")]
    [Tooltip("If true, the agent will pick up the object in their hand while performing the action.")]
    public bool holdable;

    // Use this for initialization
    private void Start()
    {
        // Clone a copy of the action for this particular useable
        string nm = action.name;
        action = Instantiate(action);
        action.name = nm;

        // Set the GameObject to this action to the GameObject this is attached to
        action.SetGameObject(gameObject);
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "NPC")
            withinRange = true;
    }

    private void OnTriggerStay(Collider other)
    {
        if (other.tag == "NPC")
            withinRange = true;
    }

     private void OnTriggerExt(Collider other)
    {
        if (other.tag == "NPC")
            withinRange = false;
    }
}