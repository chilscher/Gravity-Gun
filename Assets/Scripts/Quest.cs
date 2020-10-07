using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Quest{

    //comment more
    public string name;
    public int id;
    public QuestManager.QuestType objective;
    public int param;
    public int[] prerequisiteQuestIds;
    public Dialogue npcDialogue;
    [HideInInspector]
    public bool isActive = false;
    [HideInInspector] [System.NonSerialized]
    public Quest[] prerequisites;
    [HideInInspector]
    public bool isComplete = false;

    public void CompleteQuest() {
        //comment more
        isActive = false;
        isComplete = true;
        Debug.Log("quest complete!");
    }

    public void StartQuest() {
        //comment more
        isActive = true;
        Debug.Log(name + " is now active! " + objective + " " + param);
    }

    public bool PrerequisitesMet() {
        //comment more
        foreach (Quest q in prerequisites) {
            if (!q.isComplete) {
                return false;
            }
        }
        return true;
    }
}
