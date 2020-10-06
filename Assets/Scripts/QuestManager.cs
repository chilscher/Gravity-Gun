using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour{


    public enum QuestType { Talk };
    
    public List<Quest> activeQuests = new List<Quest>();
    public Quest[] allQuests;

    public void Start() {
        AssignPrerequisites();
        foreach (Quest q in allQuests) {
            if (q.prerequisites.Length == 0) {
                q.StartQuest();
                activeQuests.Add(q);
            }
        }

        //firstQuest.isActive = true;
        //activeQuests.Add(firstQuest);
    }

    public void AccomplishTask(QuestType type, int param) {
        List<Quest> completedQuests = new List<Quest>();
        foreach(Quest q in activeQuests) {
            if (q.objective == type && q.param == param) {
                q.CompleteQuest();
                completedQuests.Add(q);
            }
        }

        //remove completed quests from list
        foreach(Quest comp in completedQuests) {
            activeQuests.Remove(comp);
            //activeQuests.Remove(comp);
        }
        
    }

    public Quest GetQuestFromName(string name) {
        foreach(Quest q in allQuests) {
            if (q.name == name) {
                return q;
            }
        }
        return null;
    }

    private void AssignPrerequisites() {
        foreach(Quest q in allQuests) {
            q.prerequisites = new Quest[q.prerequisiteQuestNames.Length];
            for(int i =0; i<q.prerequisiteQuestNames.Length; i++) {
                q.prerequisites[i] = GetQuestFromName(q.prerequisiteQuestNames[i]);
            }
        }
    }

    public Quest FindNPCDialogue(int npcId) {
        foreach (Quest q in activeQuests) {
            if (q.objective == QuestType.Talk) {
                if (q.param == npcId) {
                    return q;
                }
            }
        }
        return null;
    }
    

}
