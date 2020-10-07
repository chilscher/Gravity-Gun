using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour{
    //contains all the quests in the game, and also contains functions that start and stop those quests

    public enum QuestType { Talk, VisitPlanet }; //all of the tasks a quest may have
    [HideInInspector] [System.NonSerialized]
    public List<Quest> activeQuests = new List<Quest>(); //all active quests
    public Quest[] allQuests; //all quests - quests are added to this list from the inspector

    public void Start() {
        //at the start of the game, go through every quest and assign its prerequisites
        AssignPrerequisites();
        foreach (Quest q in allQuests) {
            if (q.prerequisites.Length == 0) {
                q.StartQuest();
                activeQuests.Add(q);
            }
        }
    }

    public void AccomplishTask(QuestType type, int param) {
        //called by various other scripts when the player accomplishes a task
        //goes through each quest that requires that task and completes that quest
        List<Quest> completedQuests = new List<Quest>();
        foreach(Quest q in activeQuests) {
            if (q.objective == type && q.param == param) {
                q.CompleteQuest();
                completedQuests.Add(q);
            }
        }

        //remove completed quests from list
        foreach(Quest comp in completedQuests) {
            //get all quests that have the completed quest as a requirement
            List<Quest> newQuests = GetAllQuestsWithPrerequisite(comp);

            foreach(Quest q in newQuests) {
                if (q.PrerequisitesMet()) {
                    q.StartQuest();
                    activeQuests.Add(q);
                }
            }
            activeQuests.Remove(comp);
        }
    }

    public Quest GetQuestFromID(int id) {
        //gets the Quest that has a particular id
        foreach (Quest q in allQuests) {
            if (q.id == id) {
                return q;
            }
        }
        return null;
    }

    private void AssignPrerequisites() {
        //iterates through each Quest, and assigns its prerequisites from a list of quest ids
        foreach (Quest q in allQuests) {
            q.prerequisites = new Quest[q.prerequisiteQuestIds.Length];
            for(int i =0; i<q.prerequisiteQuestIds.Length; i++) {
                q.prerequisites[i] = GetQuestFromID(q.prerequisiteQuestIds[i]);
            }
        }
    }

    public Quest FindNPCDialogue(int npcId) {
        //returns a quest that requires dialogue with a specific npc
        foreach (Quest q in activeQuests) {
            if (q.objective == QuestType.Talk) {
                if (q.param == npcId) {
                    return q;
                }
            }
        }
        return null;
    }

    public List<Quest> GetAllQuestsWithPrerequisite(Quest prereq) {
        //gets a list of all Quests that have the specified Quest as a prereq
        List<Quest> quests = new List<Quest>();
        foreach(Quest q in allQuests) {
            foreach(Quest p in q.prerequisites) {
                if (p == prereq) {
                    quests.Add(q);
                }
            }
        }
        return quests;
    }
}
