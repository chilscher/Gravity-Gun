using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Quest{
    //contains data on a single quest
    //quests are added in the QuestManager script

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
        //completes a quest
        isActive = false;
        isComplete = true;
        Debug.Log("quest complete!");
    }

    public void StartQuest() {
        //starts a quest
        isActive = true;
        Debug.Log(name + " is now active! " + objective + " " + param);
    }

    public bool PrerequisitesMet() {
        //returns true if the quest's prerequisites are all completed
        foreach (Quest q in prerequisites) {
            if (!q.isComplete) {
                return false;
            }
        }
        return true;
    }
}
