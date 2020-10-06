using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Quest{

    public string name;
    public QuestManager.QuestType objective;
    public int param;
    public string[] prerequisiteQuestNames;
    public Dialogue npcDialogue;
    [HideInInspector]
    public bool isActive = false;
    [HideInInspector]
    public Quest[] prerequisites;

    // Start is called before the first frame update
    void Start(){
        //Debug.Log(objective + " " + param);
    }

    // Update is called once per frame
    void Update() {
        
    }

    public void CompleteQuest() {
        isActive = false;
        Debug.Log("quest complete!");
    }

    public void StartQuest() {
        isActive = true;
        Debug.Log(name + " is now active! " + objective + " " + param);
    }
}
