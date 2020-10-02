using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour{
    public GameObject untouchableButton;
    //objects who will not be detected by touch will be tagged with "Ingore Touch". usually objects that are part of another interactable (children of minimap, children of dialogue bubble, etc)
    //objects that, when tapped, will not do anything, and also block taps on objects behind them, have the "Untappable" tag (joystick, minimap, etc)
    public List<GameObject> checkCircleForTouch; //objects who have a defined circlecollider, outside of which they cannot be interacted with (minimap, etc)
    public List<GameObject> checkCapsuleForTouch;

    private void Update() {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0)) {
            List<GameObject> objs = FindAllObjectCollisions(Input.mousePosition);
            GameObject o = ChooseObjectToTouch(objs);
            InteractWithObject(o);
        }
#endif

        if (Input.touchCount > 0) {
            for (int i = 0; i< Input.touches.Length; i++) {
                if (Input.touches[i].phase == TouchPhase.Began) {

                    List<GameObject> objs = FindAllObjectCollisions(Input.touches[i].position);
                    GameObject o = ChooseObjectToTouch(objs);
                    InteractWithObject(o);
                }
            }
        }
    }

    private List<GameObject> FindAllObjectCollisions(Vector2 pos) {
        //takes a screen touch position and returns all objects the touch collides with.
        //ignores any gameobject listed in ignoreTouch
        //if a gameobject is in checkCircleForTouch, the object is ignored if the touch position is outside the object's circlecollider
        List<GameObject> allTouchedObjects = new List<GameObject>(); //any object the player's touch collides with
        List<GameObject> nonIgnoredTouchedObjects = new List<GameObject>(); //eventually, will be all touched objects that are not on any ignore lists

        //find touched UI
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = pos;
        List<RaycastResult> raycastResultList = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, raycastResultList);
        for (int i = 0; i < raycastResultList.Count; i++) {
            allTouchedObjects.Add(raycastResultList[i].gameObject);
            nonIgnoredTouchedObjects.Add(raycastResultList[i].gameObject);
        }

        //find touched objects in world space
        Vector3 wp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector2 touchPos = new Vector2(wp.x, wp.y);
        if (Physics2D.OverlapPoint(touchPos) != null) {
            Collider2D[] overlaps = Physics2D.OverlapPointAll(touchPos);
            foreach(Collider2D o in overlaps) {
                allTouchedObjects.Add(o.gameObject);
                nonIgnoredTouchedObjects.Add(o.gameObject);
            }
        }

        //remove ignored objects
        //objects with the "Ingore Touch" tag are automatically ignored
        //objects in the "check circle for touch" list are ignored if the touch is outside the bounds of the object's circlecollider
        foreach (GameObject g in allTouchedObjects) {
            if (g.tag == "Ignore Touch") {
                nonIgnoredTouchedObjects.Remove(g);
            }
            else if (checkCircleForTouch.Contains(g)) {
                float xdiff = Input.mousePosition.x - g.GetComponent<CircleCollider2D>().bounds.center.x;
                float ydiff = Input.mousePosition.y - g.GetComponent<CircleCollider2D>().bounds.center.y;
                float dist = Mathf.Sqrt(xdiff * xdiff + ydiff * ydiff);
                if (!(dist <= g.GetComponent<CircleCollider2D>().radius * g.transform.lossyScale.x)) {
                    nonIgnoredTouchedObjects.Remove(g);
                }
            }
        }

        return nonIgnoredTouchedObjects;

    }

    private GameObject ChooseObjectToTouch(List<GameObject> gos) {
        //out of all of the objects the player touched, only one should have its "tapped" function called
        //this function determines which, if any, objects are to be tapped.

        //if an untappable object is touched, do nothing
        //these objects have the "Untappable" tag
        //ex: the player script handles the joystick inputs
        //ex: the player cannot click planets through the minimap
        foreach (GameObject g in gos) {
            if (g.tag == "Untappable") { return null;}
        }

        //if an NPC or their dialogue bubble is touched, return it
        foreach(GameObject g in gos) {
            if (g.tag == "NPC") {return g; }
            if (g.tag == "Thought Bubble") { return g;}
        }
        
        //if the dialogue box is touched, advance the dialogue
        foreach(GameObject g in gos) {
            if(g.tag == "Dialogue Box") {
                return g;
            }
        }
        

        //if a planet is touched, return it
        foreach (GameObject g in gos) {
            if (g.tag == "Planet") { return g; }
        }

        return null;
    }

    private void InteractWithObject(GameObject obj) {
        //"taps" an object
        if (obj == null) { return; }
        if (obj.tag == "Planet") { obj.GetComponent<Planet>().Clicked(); }
        if (obj.tag == "NPC") { obj.GetComponent<NPCDialogue>().TappedNPC(); }
        if (obj.tag == "Thought Bubble") { obj.transform.parent.GetComponent<NPCDialogue>().TappedBubble(); }
        if (obj.tag == "Dialogue Box") { FindObjectOfType<DialogueManager>().DisplayNextSentence(); }
    }

    private void DisplayText(string t) {
        //presents a text-display bar at the top of the screen, to show debug text on mobile
        GameObject.Find("Canvas").transform.Find("Debug Bar").gameObject.SetActive(true);
        Text text = GameObject.Find("Debug Text").GetComponent<Text>();
        if (text.text == "") {
            text.text += t;
        }
        else { text.text += " / " + t; }
    }
}
