using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class CameraController : MonoBehaviour{

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
}
