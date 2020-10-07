using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Planet : MonoBehaviour{

    public int id;
    public Vector2 centerPoint;
    public Player player;
    public bool ignorePlayerContact = false; //do not let the player touch this planet until they are at least one body distance away
    public float density = 1f;
    [HideInInspector]
    public float mass;
    public float coefficientOfFriction = 0.6f; //0 is no friction, 1 is equal to normal force
                                               //an ice planet may have 0.2, a normal planet may have 0.6, and a planet with super-friction may have 1
                                               //what would a glue planet have???



    public void Clicked() {
        //comment more
        player.ClickedPlanet(this);
    }

    private void Start() {
        //comment more
        player = GameObject.Find("Player").GetComponent<Player>();
        centerPoint = GetComponent<CircleCollider2D>().bounds.center;
        CalculateMass();
    }
    private void Update() {
        //comment more
        if (ignorePlayerContact) {
            if (GetDistanceToPlayerCenter() > ((GetComponent<CircleCollider2D>().radius * transform.lossyScale.x) + (player.GetComponent<CircleCollider2D>().radius * player.transform.lossyScale.x * 2))) {
                ignorePlayerContact = false;
            }
        }

    }
    

    private bool CheckCollisionWithPlayer() {
        //comment more
        if (GetDistanceToPlayerCenter() < ((GetComponent<CircleCollider2D>().radius * transform.lossyScale.x) + (player.GetComponent<CircleCollider2D>().radius * player.transform.lossyScale.x))) {
            return true;
        }
        return false;
    }


    public float GetDistanceToPlayerCenter() {
        //comment more
        float distance_x = player.transform.position.x - centerPoint.x;
        float distance_y = player.transform.position.y - centerPoint.y;
        float distance = Mathf.Sqrt((distance_x * distance_x) + (distance_y * distance_y));
        return distance;
    }


    public void CalculateMass() {
        //mass = density * area
        //area = 2 *pi * radius
        //radius = circlecollider radius * transform.lossyscale.x
        mass = density * 2 * Mathf.PI * GetComponent<CircleCollider2D>().radius * transform.lossyScale.x;
        //print(mass);
    }

    /*
    private void OrientObstaclesTowardPlanet() {
        foreach(Transform t in transform) {
            if (t.gameObject.tag == "Obstacle") {
                Vector2 bottom = t.up * -1;
                Vector2 planetCenter = centerPoint;
                Vector2 towardsPlanet = (planetCenter - new Vector2(t.position.x, t.position.y)).normalized;
                float downAngle = Vector2.Angle(bottom, towardsPlanet); //angle in degrees
                float angle = (Mathf.Atan2(towardsPlanet.y, towardsPlanet.x) * Mathf.Rad2Deg) + 90; //in degrees
                angle -= (t.eulerAngles.z - 360f);
                angle = angle - Mathf.CeilToInt(angle / 360f) * 360f;
                if (angle < 0) { angle += 360f; }
                float angleToRotate = angle;

                float a = (Mathf.Atan2(towardsPlanet.y, towardsPlanet.x) * Mathf.Rad2Deg) + 90; //in degrees
                Quaternion rotation = Quaternion.AngleAxis(a, Vector3.forward);
                t.rotation = Quaternion.Slerp(t.rotation, rotation, 1);
            }
        }

    }
    */
}
