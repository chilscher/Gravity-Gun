using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MinimapController : MonoBehaviour{
    public GameObject minimapCamera;
    public GameObject movementArrow;
    //public GameObject minimap;

    public void move(float x, float y) {
        Vector3 minimapCameraPos = new Vector3(x, y, -10);
        minimapCamera.transform.position = minimapCameraPos;
    }

    public void showPlayerMovementDirection(Player player) {
        movementArrow.SetActive(true);
        if (player.isOnPlanet) {
            if (player.currentSpeed > 0) {
                rotateArrow(player.perpTowardsPlanet);
            }
            else if (player.currentSpeed < 0) {
                rotateArrow(-player.perpTowardsPlanet);

            }
            else {
                movementArrow.SetActive(false);
            }
        }
        else {
            rotateArrow(player.freeFallDirection);
        }
        
    }


    private void rotateArrow(Vector2 direction) {

        float a = Vector2.SignedAngle(new Vector2(1, 0), direction); //in degrees
        Quaternion rotation = Quaternion.AngleAxis(a, Vector3.forward);
        movementArrow.transform.rotation = Quaternion.Slerp(movementArrow.transform.rotation, rotation, 1);
    }
}
