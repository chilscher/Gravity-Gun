using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuestManager : MonoBehaviour{


    public enum QuestType { Talk, VisitPlanet };
    
    [HideInInspector] [System.NonSerialized]
    public List<Quest> activeQuests = new List<Quest>();
    public Quest[] allQuests;

    public void Start() {
        //comment more
        AssignPrerequisites();
        foreach (Quest q in allQuests) {
            if (q.prerequisites.Length == 0) {
                q.StartQuest();
                activeQuests.Add(q);
            }
        }
    }

    public void AccomplishTask(QuestType type, int param) {
        //comment more
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
        //comment more
        foreach (Quest q in allQuests) {
            if (q.id == id) {
                return q;
            }
        }
        return null;
    }

    private void AssignPrerequisites() {
        //comment more
        foreach (Quest q in allQuests) {
            q.prerequisites = new Quest[q.prerequisiteQuestIds.Length];
            for(int i =0; i<q.prerequisiteQuestIds.Length; i++) {
                q.prerequisites[i] = GetQuestFromID(q.prerequisiteQuestIds[i]);
            }
        }
    }

    public Quest FindNPCDialogue(int npcId) {
        //comment more
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
        //comment more
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
