using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPCDialogue : MonoBehaviour {
    //an NPC with the NPCDialogue script should have a circlecollider2d child to test if the player is close enough for the dialogue bubble to appear
    //they also should have a circlecollider2d of their own so the player has something to tap to interact with
    public Dialogue dialogue;
    public Planet planet;
    private bool wasPlayerInRangeLastFrame = false;
    private DialogueManager dialogueManager;
    private Player player;

    private void Start() {
        player = FindObjectOfType<Player>();
        dialogueManager = FindObjectOfType<DialogueManager>();
        transform.Find("Bubble").gameObject.SetActive(false);
        orientNPCTowardPlanet();
    }

    private void Update() {
        bool isPlayerInRangeThisFrame = isPlayerInRange();
        if (!wasPlayerInRangeLastFrame && isPlayerInRangeThisFrame) {
            showThoughtBubble();
        }
        else if (wasPlayerInRangeLastFrame && !isPlayerInRangeThisFrame) {
            hideThoughtBubble();
          
        }
        wasPlayerInRangeLastFrame = isPlayerInRangeThisFrame;

    }

    public void TriggerDialogue() {
        FindObjectOfType<DialogueManager>().StartDialogue(this);
    }

    private bool isPlayerInRange() {
        //checks to see if the player is close enough to have the NPC speech bubble pop up
        //returns false if the player is on a different planet from the NPC
        if ((player.planet != planet) || !player.isOnPlanet) {
            return false;
        }
        Vector2 center = transform.Find("Interact Range").GetComponent<CircleCollider2D>().bounds.center;
        float radius = Mathf.Abs(transform.Find("Interact Range").GetComponent<CircleCollider2D>().radius * transform.Find("Interact Range").lossyScale.x);
        Vector2 playerCenter = player.GetComponent<CircleCollider2D>().bounds.center;
        float playerRadius = player.GetComponent<CircleCollider2D>().radius * player.transform.lossyScale.x;
        float xdiff = Mathf.Abs(center.x - playerCenter.x);
        float ydiff = Mathf.Abs(center.y - playerCenter.y);
        float totaldiff = Mathf.Sqrt(xdiff * xdiff + ydiff * ydiff);
        if (totaldiff < Mathf.Abs(radius + playerRadius)) {
            return true;
        }
        return false;
    }

    private void showThoughtBubble() {
        transform.Find("Bubble").gameObject.SetActive(true);
    }

    private void hideThoughtBubble() {
        transform.Find("Bubble").gameObject.SetActive(false);
    }

    public void tappedBubble() {
        //interact with the thought bubble
        //does the same thing as interacting with the NPC
        tappedNPC();
    }
    public void tappedNPC() {
        //interact with the NPC
        //if the dialogue thought bubble is visible, start a conversation
        //if the player is moving, do not start a conversation
        if (!player.isOnPlanet || player.speedOnSurface != 0f) {
            return;
        }
        if (transform.Find("Bubble").gameObject.activeSelf) {
            //print("npc");
            TriggerDialogue();
            hideThoughtBubble();
        }
        else {
            //if the dialogueManager is already handling the current dialogue, then show the next bit of dialogue.
            if (isDialogueOngoing()) {
                dialogueManager.DisplayNextSentence();
            }
        }
    }

    private bool isDialogueOngoing() {

        if ((!transform.Find("Bubble").gameObject.activeSelf) && dialogueManager.ongoingDialogue == this) {
            return true;
        }
        return false;
    }


    private void orientNPCTowardPlanet() {
        Vector2 bottom = transform.up * -1;
        Vector2 planetCenter = planet.GetComponent<CircleCollider2D>().bounds.center;
        Vector2 towardsPlanet = (planetCenter - new Vector2(transform.position.x, transform.position.y)).normalized;
        float downAngle = Vector2.Angle(bottom, towardsPlanet); //angle in degrees
        float angle = (Mathf.Atan2(towardsPlanet.y, towardsPlanet.x) * Mathf.Rad2Deg) + 90; //in degrees
        angle -= (transform.eulerAngles.z - 360f);
        angle = angle - Mathf.CeilToInt(angle / 360f) * 360f;
        if (angle < 0) { angle += 360f; }
        float angleToRotate = angle;

        float a = (Mathf.Atan2(towardsPlanet.y, towardsPlanet.x) * Mathf.Rad2Deg) + 90; //in degrees
        Quaternion rotation = Quaternion.AngleAxis(a, Vector3.forward);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 1);

    }



}
