using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NPC : MonoBehaviour {
    //an NPC with the NPCDialogue script should have a circlecollider2d child to test if the player is close enough for the dialogue bubble to appear
    //they also should have a circlecollider2d of their own so the player has something to tap to interact with
    public int npcId;

    public Dialogue normalDialogue;
    private bool wasPlayerInRangeLastFrame = false;
    private DialogueManager dialogueManager;
    private Player player;

    [HideInInspector] [System.NonSerialized]
    public Quest quest;


    private void Start() {
        //set some variables and hide the thought bubbles
        player = FindObjectOfType<Player>();
        dialogueManager = FindObjectOfType<DialogueManager>();
        transform.Find("Normal Bubble").gameObject.SetActive(false);
        transform.Find("Quest Bubble").gameObject.SetActive(false);
    }

    private void Update() {
        //if player entered or exited the conversation range, show/hide the relevant thought bubble
        bool isPlayerInRangeThisFrame = IsPlayerInRange();
        if (!wasPlayerInRangeLastFrame && isPlayerInRangeThisFrame) {
            ShowThoughtBubble();
        }
        else if (wasPlayerInRangeLastFrame && !isPlayerInRangeThisFrame) {
            HideThoughtBubble();
          
        }
        wasPlayerInRangeLastFrame = isPlayerInRangeThisFrame;

    }

    private bool IsPlayerInRange() {
        //checks to see if the player is close enough to have the NPC speech bubble pop up
        //returns false if the player is on a different planet from the NPC
        //assumes the NPC is a direct child of its planet
        if ((player.planet != transform.parent.GetComponent<Planet>()) || !player.isOnPlanet) {
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

    private void ShowThoughtBubble() {
        //if this npc is part of an active quest, show the quest thought bubble. otherwise show the normal dialogue thought bubble
        quest = FindObjectOfType<QuestManager>().FindNPCDialogue(npcId);
        if (quest == null) {
            ShowNormalBubble();
        }
        else {
            ShowQuestBubble();
        }
    }

    private void ShowQuestBubble() {
        //shows the quest thought bubble and hides the normal one
        transform.Find("Normal Bubble").gameObject.SetActive(false);
        transform.Find("Quest Bubble").gameObject.SetActive(true);

    }

    private void ShowNormalBubble() {
        //shows the normal thought bubble and hides the quest one
        transform.Find("Normal Bubble").gameObject.SetActive(true);
        transform.Find("Quest Bubble").gameObject.SetActive(false);
    }
    
    private void HideThoughtBubble() {
        //hides both the normal and quest thought bubbles
        transform.Find("Normal Bubble").gameObject.SetActive(false);
        transform.Find("Quest Bubble").gameObject.SetActive(false);
    }

    public void TappedBubble() {
        //interact with the thought bubble
        //does the same thing as interacting with the NPC
        TappedNPC();
    }
    public void TappedNPC() {
        //interact with the NPC
        //if the dialogue thought bubble is visible, start a conversation
        //if the player is moving, do not start a conversation
        if (!player.isOnPlanet || player.speedOnSurface != 0f) {
            return;
        }
        if (transform.Find("Quest Bubble").gameObject.activeSelf) {
            HideThoughtBubble();
            FindObjectOfType<DialogueManager>().StartQuestDialogue(this);
        }
        else if (transform.Find("Normal Bubble").gameObject.activeSelf) {
            HideThoughtBubble();
            FindObjectOfType<DialogueManager>().StartNormalDialogue(this);
        }
        else {
            //if the dialogueManager is already handling the current dialogue, then show the next bit of dialogue.
            if (IsDialogueOngoing()) {
                dialogueManager.DisplayNextSentence();
            }
        }
    }

    private bool IsDialogueOngoing() {
        //if the dialogue manager is handling a conversation with this npc, return true
        if ((!transform.Find("Normal Bubble").gameObject.activeSelf) && (!transform.Find("Quest Bubble").gameObject.activeSelf) && dialogueManager.npc == this) {
            return true;
        }
        return false;
    }

    public void AccomplishDialogueTask() {
        //tell the quest manager you have just finished a dialogue task
        FindObjectOfType<QuestManager>().AccomplishTask(QuestManager.QuestType.Talk, npcId);
    }

    public void EndDialogue() {
        //called when the player ends a conversation, and shows the dialogue thought bubble again
        if (IsPlayerInRange()) {
            ShowThoughtBubble();
        }
    }

}
