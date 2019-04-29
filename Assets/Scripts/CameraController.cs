using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour{

    public float slowTimeSpeed = 0.3f;
    private float pauseTimeSpeed = 0f;
    private float fullTimeSpeed = 1f;
    private KeyCode slowTimeKey = KeyCode.Space;
    private KeyCode pauseKey = KeyCode.Escape;

    void Start(){
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

    private bool isPaused() {return Time.timeScale == pauseTimeSpeed;}
    private bool isSlowed() {return Time.timeScale == slowTimeSpeed;}
    private bool isFullSpeed() {return Time.timeScale == fullTimeSpeed;}
    private void enterSlowMode() {Time.timeScale = slowTimeSpeed;}
    private void enterPauseMode() { Time.timeScale = pauseTimeSpeed;}
    private void enterNormalMode() { Time.timeScale = fullTimeSpeed;}

    private void exitPauseMode() {
        //if the slow key is held, go to slow mode
        //if not, go to full speed
        if (Input.GetKey(slowTimeKey)) {enterSlowMode();}
        else {enterNormalMode();}
    }
}
