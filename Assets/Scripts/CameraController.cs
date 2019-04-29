using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour{

    public float slowTimeSpeed = 0.5f;
    private KeyCode slowTimeKey = KeyCode.Space;

    void Start(){
    }
    
    void Update(){
        if (Input.GetKeyDown(slowTimeKey)) {
            Time.timeScale = slowTimeSpeed;
        }
        if (Input.GetKeyUp(slowTimeKey)) {
            Time.timeScale = 1f;
        }
    }
}
