using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DialogueManager : MonoBehaviour {

	//public Text nameText;
	public GameObject dialogueText;
    public GameObject bubbleBackground;
    public int characterLimitPerLine;
	//public Animator animator;

	private Queue<string> sentences;
    private bool typingSentence = false;
    private string currentSentence;
    /*
    [HideInInspector]
    public NPCDialogue dialogueInProgress = null;
    */

    [HideInInspector]
    public NPCDialogue ongoingDialogue;

	// Use this for initialization
	void Start () {
		sentences = new Queue<string>();
        showDialogueBox(false);
        //transform.Find("Bubble Background").gameObject.SetActive(false);
        //transform.Find("Dialogue Text").gameObject.SetActive(false);
    }

	public void StartDialogue (NPCDialogue dialogueTrigger)
	{
        //animator.SetBool("IsOpen", true);

        //nameText.text = dialogue.name;
        ongoingDialogue = dialogueTrigger;
        showDialogueBox(true);
        //dialogueText.transform.parent.Find("Bubble Background").gameObject.SetActive(true);
        //dialogueText.gameObject.SetActive(true);

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

        //dialogueText.GetComponent<Text>().text = sentence;
        if (typingSentence) {
            StopAllCoroutines();
            dialogueText.GetComponent<Text>().text = currentSentence;
            typingSentence = false;
        }
        else {
            if (sentences.Count == 0) {
                EndDialogue();
                return;
            }

            string sentence = formatTextForBox(sentences.Dequeue());
            StopAllCoroutines();
            StartCoroutine(TypeSentence(sentence));
        }
	}

	IEnumerator TypeSentence (string sentence)
	{
        typingSentence = true;
        currentSentence = sentence;
        dialogueText.GetComponent<Text>().text = "";
		foreach (char letter in sentence.ToCharArray())
		{
            dialogueText.GetComponent<Text>().text += letter;
            if (dialogueText.GetComponent<Text>().text == sentence) {
                typingSentence = false;
            }
			yield return null;
		}
	}

	void EndDialogue()
	{
        //dialogueInProgress = null;
        showDialogueBox(false);
        //animator.SetBool("IsOpen", false);
    }

    public bool isPlayerInDialogue() {
        if (dialogueText.gameObject.activeSelf) {
            return true;
        }
        return false;
    }

    private void showDialogueBox(bool b) {

        bubbleBackground.SetActive(b);
        dialogueText.SetActive(b);
    }

    private string formatTextForBox(string sentence) {
        //takes a sentence and adds new line characters
        //this is done so that when TypeSentence is filling in the text box, it doesn't get halfway through a word before starting a new line
        //the sentence cannot have any new line characters in it already
        //return sentence;
        string s = "";

        string[] words = sentence.Split(' ');
        string line = words[0];
        bool firstline = true;
        foreach (string word in words) {
            if (!firstline) {
                /*
                if (word.Contains("\n")) {
                    print("found one");
                    string[] l = word.Split('\n');
                    line += (" " + l[0]);
                    s += (line + "\n");
                    line = l[1];
                }
                */
                if (line.Length + word.Length <= characterLimitPerLine) {
                    line += (" " + word);
                }
                else {
                    s += (line + "\n");
                    line = "";
                    line += word;
                }
            }
            else {
                firstline = false;
            }
        }
        s += line;
        
        
        return s;
    }

}