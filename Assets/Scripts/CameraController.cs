using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class CameraController : MonoBehaviour{
    public GameObject untouchableButton;

    private void Update() {
#if UNITY_EDITOR
        if (Input.GetMouseButtonDown(0) && !isMouseOverUI()) {
            
            Vector3 wp = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            Vector2 touchPos = new Vector2(wp.x, wp.y);
            if (Physics2D.OverlapPoint(touchPos) != null) {
                if (Physics2D.OverlapPoint(touchPos).gameObject.tag == "Planet") {
                    Physics2D.OverlapPoint(touchPos).GetComponent<Planet>().clicked();
                }
            }
        }
#endif

        if (Input.touchCount > 0) {
            for (int i = 0; i< Input.touches.Length; i++) {
                if (Input.touches[i].phase == TouchPhase.Began) {
                    if (checkIfPlanetTouched(Input.touches[i])) {
                        Vector3 wp = Camera.main.ScreenToWorldPoint(Input.touches[i].position);
                        Vector2 touchPos = new Vector2(wp.x, wp.y);
                        if (Physics2D.OverlapPoint(touchPos) != null) {
                            if (Physics2D.OverlapPoint(touchPos).gameObject.tag == "Planet") {
                                Physics2D.OverlapPoint(touchPos).GetComponent<Planet>().clicked();
                            }
                        }
                    }
                }
            }
        }
    }
    /*
    private void displayText(string t) {
        Text text = GameObject.Find("Debug Text").GetComponent<Text>();
        if (text.text == "") {
            text.text += t;
        }
        else { text.text += " / " + t; }
    }
    */

    private bool checkIfPlanetTouched(Touch touch) {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = touch.position;
        List<RaycastResult> raycastResultList = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, raycastResultList);
        for (int i = 0; i < raycastResultList.Count; i++) {
            if (raycastResultList[i].gameObject.transform.parent.name == "Canvas") {
                return false;
            }
        }
        Vector3 wp = Camera.main.ScreenToWorldPoint(touch.position);
        Vector2 touchPos = new Vector2(wp.x, wp.y);
        if (Physics2D.OverlapPoint(touchPos) != null) {
            if (Physics2D.OverlapPoint(touchPos).gameObject.tag == "Planet") {
                return true;
            }
        }
        return false;
    }




    private bool isMouseOverUI() {
        PointerEventData pointerEventData = new PointerEventData(EventSystem.current);
        pointerEventData.position = Input.mousePosition;
        List<RaycastResult> raycastResultList = new List<RaycastResult>();
        EventSystem.current.RaycastAll(pointerEventData, raycastResultList);
        for (int i = 0; i < raycastResultList.Count; i++) {
            if (raycastResultList[i].gameObject.transform.parent.name == "Canvas") {
                return true;
            }
        }
        return false;
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
