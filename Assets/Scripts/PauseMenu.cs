using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PauseMenu : MonoBehaviour{

    public Player player;

    public void Start() {
        foreach (Transform t in transform) {
            t.gameObject.SetActive(false);
        }
    }

    public void PauseGame() {
        foreach(Transform t in transform) {
            t.gameObject.SetActive(true);
            player.isPaused = true;
        }
    }

    public void ResumeGame() {
        foreach (Transform t in transform) {
            t.gameObject.SetActive(false);
            player.isPaused = false;
        }
    }



}
