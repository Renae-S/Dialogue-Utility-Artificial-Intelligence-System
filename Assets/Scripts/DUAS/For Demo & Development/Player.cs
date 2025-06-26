using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshObstacle))]
[RequireComponent(typeof(CharacterController))]
public class Player : MonoBehaviour
{
    private Animator animator;
    private Animation anim; // Used for dialogue emotes
    private CharacterController controller;
    private DUAS_DialogueManager dialogueManager;
    private Animator dialogueAnimator;

    public float speed = 0.5f;

    public Transform head;

    [HideInInspector]
    public bool NPCWithinRange;
    private Vector3 NPCDirection;
    private Quaternion myRotation;

    // Start is called before the first frame update
    void Start()
    {
        animator = GetComponent<Animator>();
        anim = GetComponentInChildren<Animation>();
        controller = GetComponent<CharacterController>();

        if (FindObjectOfType<DUAS_DialogueManager>())
        {
            dialogueManager = FindObjectOfType<DUAS_DialogueManager>();
            dialogueAnimator = dialogueManager.GetComponent<Animator>();
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (dialogueAnimator)
        {
            if (!dialogueAnimator.GetBool("IsOpen"))
                Move();
        }
        else
            Move();

        AnimationHandling();
    }

    private void OnTriggerStay(Collider col)
    {
        if (col.tag == "NPC")
        {
            NPCWithinRange = true;
            NPCDirection = col.transform.position - transform.position;
            myRotation = head.rotation;
        }
    }

    private void OnTriggerExit(Collider col)
    {
        if (col.tag == "NPC")
        {
            myRotation = head.rotation;
            NPCWithinRange = false;
        }
    }

    private void LateUpdate()
    {
        if (dialogueAnimator)
        {
            if (dialogueAnimator.GetBool("IsOpen") && NPCWithinRange)
            {
                Quaternion rotationAngle = Quaternion.LookRotation(NPCDirection);

                head.rotation = Quaternion.Slerp(myRotation, rotationAngle, Time.deltaTime * 1.5f);
                myRotation = head.rotation;
            }
        }
    }

    private void Move()
    {
        // Vertical and horizontal input
        float vertical = Input.GetAxis("Vertical");
        float horizontal = Input.GetAxis("Horizontal");

        // Get the forward direction of the camera
        Vector3 forward = Camera.main.transform.forward;
        forward.y = 0;
        forward.Normalize();

        Vector3 movement = horizontal * Camera.main.transform.right + vertical * forward;

        // Set the player's facing direction to the last horizontal/vertical input in accordance to the forward position of the camera
        Vector3 direction = horizontal * Camera.main.transform.right + vertical * forward;
        transform.LookAt(transform.position + direction);

        float moveH = horizontal;
        float moveV = vertical;

        // If the horizontal input is negative, make it a positive value
        if (moveH < 0)
            moveH -= (moveH * 2);

        // If the vertical input is negative, make it a positive value
        if (moveV < 0)
            moveV -= (moveV * 2);

        float move = moveH + moveV;

        // Set it so the only two speeds the player can be 
        if (moveH + moveV == 2)
            move = 1;

        controller.Move(direction * speed * Time.deltaTime);
        animator.SetFloat("Speed", move);
    }

    private void AnimationHandling()
    {
        if (dialogueAnimator)
        {
            if (dialogueAnimator.GetBool("PlayerDialogue"))
            {
                if (dialogueManager.GetCurrentDialogue().emote)
                {
                    animator.enabled = false;
                    anim.Play(dialogueManager.GetCurrentDialogue().emote.name);
                }
                else
                {
                    animator.enabled = true;
                    animator.SetBool("Interact", true);
                }
            }
            else
            {
                animator.enabled = true;
                animator.SetBool("Interact", false);
            }
        }
        else
        {
            animator.enabled = true;
            animator.SetBool("Interact", false);
        }

        if (dialogueAnimator)
        {
            if (dialogueAnimator.GetBool("AIDialogue"))
                animator.SetFloat("Speed", 0);
        }
    }
}
