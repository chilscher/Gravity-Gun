using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RotateTowardsPlanet : MonoBehaviour {
    //a gameobject with this script attached will be rotated towards their planet
    //mainly used for getting npcs and obstacles into the right position, then removed for regular play
    //assumes the gameobject is a direct child of a planet object

    private void Start() {
        Orient();
    }

    
    private void Orient() {
        //comment more
        Vector2 bottom = transform.up * -1;
        Vector2 planetCenter = transform.parent.GetComponent<Planet>().centerPoint;
        Vector2 towardsPlanet = (planetCenter - new Vector2(transform.position.x, transform.position.y)).normalized;
        float downAngle = Vector2.Angle(bottom, towardsPlanet); //angle in degrees
        float angle = (Mathf.Atan2(towardsPlanet.y, towardsPlanet.x) * Mathf.Rad2Deg) + 90; //in degrees
        angle -= (transform.eulerAngles.z - 360f);
        angle = angle - Mathf.CeilToInt(angle / 360f) * 360f;
        if (angle < 0) { angle += 360f; }
        float angleToRotate = angle;

        float a = (Mathf.Atan2(towardsPlanet.y, towardsPlanet.x) * Mathf.Rad2Deg) + 90; //in degrees
        Quaternion rotation = Quaternion.AngleAxis(a, Vector3.forward);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 1);

    }



}
