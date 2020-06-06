using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour {

	//public Text nameText;
	public Text dialogueText;

	//public Animator animator;

	private Queue<string> sentences;
    /*
    [HideInInspector]
    public NPCDialogue dialogueInProgress = null;
    */

    [HideInInspector]
    public NPCDialogue ongoingDialogue;

	// Use this for initialization
	void Start () {
		sentences = new Queue<string>();
	}

	public void StartDialogue (NPCDialogue dialogueTrigger)
	{
        //animator.SetBool("IsOpen", true);

        //nameText.text = dialogue.name;
        ongoingDialogue = dialogueTrigger;
        dialogueText.transform.parent.GetComponent<Image>().enabled = true;
        dialogueText.gameObject.SetActive(true);

        //dialogueInProgress = dialogueTrigger;
        Dialogue dialogue = dialogueTrigger.dialogue;

		sentences.Clear();

		foreach (string sentence in dialogue.sentences)
		{
			sentences.Enqueue(sentence);
		}

		DisplayNextSentence();
	}

	public void DisplayNextSentence ()
	{
		if (sentences.Count == 0)
		{
			EndDialogue();
			return;
		}

		string sentence = sentences.Dequeue();
        dialogueText.text = sentence;
		//StopAllCoroutines();
		//StartCoroutine(TypeSentence(sentence));
	}

	IEnumerator TypeSentence (string sentence)
	{
		dialogueText.text = "";
		foreach (char letter in sentence.ToCharArray())
		{
			dialogueText.text += letter;
			yield return null;
		}
	}

	void EndDialogue()
	{
        //dialogueInProgress = null;
        dialogueText.transform.parent.GetComponent<Image>().enabled = false;
        dialogueText.gameObject.SetActive(false);
        //animator.SetBool("IsOpen", false);
    }

    public bool isPlayerInDialogue() {
        if (dialogueText.gameObject.activeSelf) {
            return true;
        }
        return false;
    }

}
