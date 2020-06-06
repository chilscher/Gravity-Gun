using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour{
    public GameObject untouchableButton;
    public List<GameObject> ignoreTouch; //objects who will not be detected by touch, usually for objects that are a part of another iteractable (children of minimap, etc)
    public List<GameObject> checkCircleForTouch; //objects who have a defined circlecollider, outside of which they cannot be interacted with (minimap, etc)
    public List<GameObject> untappable; //objects that, when tapped, will not do anything, but also objects behind them cannot be tapped (joystick, minimap, etc)

    private void Update() {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0)) {
            List<GameObject> objs = findAllObjectCollisions(Input.mousePosition);
            GameObject o = chooseObjectToTouch(objs);
            interactWithObject(o);
        }
#endif

        if (Input.touchCount > 0) {
            for (int i = 0; i< Input.touches.Length; i++) {
                if (Input.touches[i].phase == TouchPhase.Began) {

                    List<GameObject> objs = findAllObjectCollisions(Input.touches[i].position);
                    GameObject o = chooseObjectToTouch(objs);
                    interactWithObject(o);
                }
            }
        }
    }

    private List<GameObject> findAllObjectCollisions(Vector2 pos) {
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
        foreach (GameObject g in allTouchedObjects) {
            if (ignoreTouch.Contains(g)) {
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
        /*
        foreach(GameObject go in nonIgnoredTouchedObjects) {
            displayText(go.name);
        }
        */
        return nonIgnoredTouchedObjects;

    }

    private GameObject chooseObjectToTouch(List<GameObject> gos) {
        //out of all of the objects the player touched, only one should have its "tapped" function called
        //this function determines which, if any, objects are to be tapped.

        //if an untappable object is touched, do nothing
        //ex: the player script handles the joystick inputs
        //ex: the player cannot click planets through the minimap
        foreach (GameObject g in gos) {
            if (untappable.Contains(g)) { return null;}
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

    private void interactWithObject(GameObject obj) {
        //"taps" an object
        if (obj == null) { return; }
        if (obj.tag == "Planet") { obj.GetComponent<Planet>().clicked(); }
        if (obj.tag == "NPC") { obj.GetComponent<NPCDialogue>().tappedNPC(); }
        if (obj.tag == "Thought Bubble") { obj.transform.parent.GetComponent<NPCDialogue>().tappedBubble(); }
        if (obj.tag == "Dialogue Box") { FindObjectOfType<DialogueManager>().DisplayNextSentence(); }
    }

    private void displayText(string t) {
        //presents a text-display bar at the top of the screen, to show debug text on mobile
        GameObject.Find("Canvas").transform.Find("Debug Bar").gameObject.SetActive(true);
        Text text = GameObject.Find("Debug Text").GetComponent<Text>();
        if (text.text == "") {
            text.text += t;
        }
        else { text.text += " / " + t; }
    }
    
    

    /*
    public float slowTimeSpeed = 0.3f;
    private float pauseTimeSpeed = 0f;
    private float fullTimeSpeed = 1f;
    private KeyCode slowTimeKey = KeyCode.Space;
    private KeyCode pauseKey = KeyCode.Escape;
    public Player player;

    public GameObject pauseMenuCanvas;


    void Start() {
        //pauseMenuObjects.Add(returnToCheckpointButton);
        hidePauseMenu();
    }
    
    void Update(){
        //if slow time button is held, your are in slow mode
        //if pause button is pressed, the game pauses. pause supercedes slowmo

        if (Input.GetKeyDown(pauseKey)) {
            if (isPaused()) {exitPauseMode();}
            else {enterPauseMode();}
        }
        if (Input.GetKeyDown(slowTimeKey)) {
            if (!isPaused()) {enterSlowMode();}
        }
        if (Input.GetKeyUp(slowTimeKey)) {
            if (!isPaused()) {enterNormalMode();}
        }
    }

    //functions for setting the play, paused, and slowmo states
    private bool isPaused() {return Time.timeScale == pauseTimeSpeed;}
    private bool isSlowed() {return Time.timeScale == slowTimeSpeed;}
    private bool isFullSpeed() {return Time.timeScale == fullTimeSpeed;}
    private void enterSlowMode() {Time.timeScale = slowTimeSpeed;}
    private void enterPauseMode() {
        Time.timeScale = pauseTimeSpeed;
        showPauseMenu();
    }
    private void enterNormalMode() { Time.timeScale = fullTimeSpeed;}

    private void exitPauseMode() {
        //if the slow key is held, go to slow mode
        //if not, go to full speed
        if (Input.GetKey(slowTimeKey)) {enterSlowMode();}
        else {enterNormalMode();}
        hidePauseMenu();
    }

    private void showPauseMenu() {
        pauseMenuCanvas.SetActive(true);
    }

    private void hidePauseMenu() {
        pauseMenuCanvas.SetActive(false);
    }




    //functions for interacting with UI elements

    public void clickedContinueButton() {
        exitPauseMode();
    }

    public void clickedCheckpointButton() {
        bool success = player.returnToLastCheckpoint();
        if (success) { exitPauseMode(); }
    }

    public void clickedRestartButton() {
        exitPauseMode();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
    }

    public void clickedQuitButton() {
        exitPauseMode();
        Application.Quit();
    }
    */
}
