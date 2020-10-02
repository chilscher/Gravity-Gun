using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour{
    public Vector2 centerPoint;
    public Player player;
    public bool ignorePlayerContact = false; //do not let the player touch this planet until they are at least one body distance away
    public float density = 1f;
    [HideInInspector]
    public float mass;
    public float coefficientOfFriction = 0.6f; //0 is no friction, 1 is equal to normal force
                                               //an ice planet may have 0.2, a normal planet may have 0.6, and a planet with super-friction may have 1
                                               //what would a glue planet have???
    
    private void OnMouseDown() {
        //GameObject.Find("Player").GetComponent<Player>().clickedPlanet(this);
    }

    public void Clicked() {
        player.ClickedPlanet(this);
    }

    private void Start() {
        player = GameObject.Find("Player").GetComponent<Player>();
        centerPoint = GetComponent<CircleCollider2D>().bounds.center;
        CalculateMass();
    }
    private void Update() {
        if (ignorePlayerContact) {
            if (GetDistanceToPlayerCenter() > ((GetComponent<CircleCollider2D>().radius * transform.lossyScale.x) + (player.GetComponent<CircleCollider2D>().radius * player.transform.lossyScale.x * 2))) {
                ignorePlayerContact = false;
            }
        }

    }


    private bool CheckCollisionWithPlayer() {
        if (GetDistanceToPlayerCenter() < ((GetComponent<CircleCollider2D>().radius * transform.lossyScale.x) + (player.GetComponent<CircleCollider2D>().radius * player.transform.lossyScale.x))) {
            return true;
        }
        return false;
    }


    public float GetDistanceToPlayerCenter() {
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
}
