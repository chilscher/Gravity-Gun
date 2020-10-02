using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class MinimapController : MonoBehaviour{
    public GameObject minimapCamera;
    public GameObject movementArrow;

    public void Move(float x, float y) {
        //moves the minimap to position x, y
        Vector3 minimapCameraPos = new Vector3(x, y, -10);
        minimapCamera.transform.position = minimapCameraPos;
    }

    public void ShowPlayerMovementDirection(Player player) {
        //controls the rotation of the arrow on the minimap, which indicates the player's movement direction
        //if the player is not moving, the arrow is hidden
        movementArrow.SetActive(true);
        if (player.isOnPlanet) {
            if (player.speedOnSurface > 0) { RotateArrow(player.perpTowardsPlanet); }
            else if (player.speedOnSurface < 0) { RotateArrow(-player.perpTowardsPlanet);}
            else { movementArrow.SetActive(false); }
        }
        else { RotateArrow(player.freeFallDirection); }
    }

    private void RotateArrow(Vector2 direction) {
        //rotates the arrow on the minimap indicating the player's movement direction.
        //used by showPlayerMovementDirection
        float a = Vector2.SignedAngle(new Vector2(1, 0), direction); //in degrees
        Quaternion rotation = Quaternion.AngleAxis(a, Vector3.forward);
        movementArrow.transform.rotation = Quaternion.Slerp(movementArrow.transform.rotation, rotation, 1);
    }
}
