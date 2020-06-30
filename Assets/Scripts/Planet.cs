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

    public void clicked() {
        player.clickedPlanet(this);
    }

    private void Start() {
        player = GameObject.Find("Player").GetComponent<Player>();
        centerPoint = GetComponent<CircleCollider2D>().bounds.center;
        calculateMass();
    }
    private void Update() {
        if (ignorePlayerContact) {
            if (getDistanceToPlayerCenter() > ((GetComponent<CircleCollider2D>().radius * transform.lossyScale.x) + (player.GetComponent<CircleCollider2D>().radius * player.transform.lossyScale.x * 2))) {
                ignorePlayerContact = false;
            }
        }
        /*
        if (ignorePlayerContact) {
            if (getDistanceToPlayerCenter() > ((GetComponent<CircleCollider2D>().radius * transform.lossyScale.x) + (player.GetComponent<CircleCollider2D>().radius * player.transform.lossyScale.x * 2))) {
                ignorePlayerContact = false;
            }
        }

        if (!player.isOnPlanet && !ignorePlayerContact) {
            if (checkCollisionWithPlayer()) {
                player.landOnPlanet(this);
            }
        }
        */
    }


    private bool checkCollisionWithPlayer() {

        if (getDistanceToPlayerCenter() < ((GetComponent<CircleCollider2D>().radius * transform.lossyScale.x) + (player.GetComponent<CircleCollider2D>().radius * player.transform.lossyScale.x))) {
            return true;
        }
        return false;
    }


    public float getDistanceToPlayerCenter() {
        float distance_x = player.transform.position.x - centerPoint.x;
        float distance_y = player.transform.position.y - centerPoint.y;
        float distance = Mathf.Sqrt((distance_x * distance_x) + (distance_y * distance_y));
        return distance;
    }


    public void calculateMass() {
        //mass = density * area
        //area = 2 *pi * radius
        //radius = circlecollider radius * transform.lossyscale.x
        mass = density * 2 * Mathf.PI * GetComponent<CircleCollider2D>().radius * transform.lossyScale.x;
        //print(mass);
    }
    /*
    private Player player;
    public float rotationSpeed = 1f; //the speed that the planet rotates around its own center
    public bool rotateClockwise = false;
    public float coefficientOfFriction = 0.6f; //0 is no friction, 1 is equal to normal force
    public float orbitalSpeed = 1f; //if this planet has a parent planet, this is how fast it moves around the parent
    public bool orbitClockwise = false;
    private bool hasParentPlanet;
    private float radiusFromParent;
    
    void Start() {
        player = GameObject.FindObjectOfType<Player>();
        if (rotateClockwise) { rotationSpeed = -rotationSpeed; }
        checkForParentPlanet();
        setRadiusFromParent();
    }
    
    void Update() {
        rotateSelf();
        orbitAroundParent();
    }

    void OnMouseDown() {player.clickedPlanet(this);}

    void rotateSelf() {transform.Rotate(Vector3.forward * rotationSpeed * Time.deltaTime);}
    
    void checkForParentPlanet() {
        hasParentPlanet = false;
        if (transform.parent != null) {
            //GameObject parent = transform.parent.GetComponent<GameObject>();
            if (transform.parent.tag == "Planet") {
                hasParentPlanet = true;
            }
        }
    }

    void orbitAroundParent() {
        if (hasParentPlanet) {
            //move player in direction of rotation
            Vector2 oldPos = transform.position;
            Vector2 towardsPlanet = (transform.parent.position - transform.position).normalized;
            Vector2 clockwiseDir = Vector2.Perpendicular(towardsPlanet);
            Vector2 counterclockwiseDir = -clockwiseDir;
            Vector2 directionOfMotion = clockwiseDir;
            if (!orbitClockwise) { directionOfMotion = counterclockwiseDir; }
            float orbitalDistance = orbitalSpeed * Time.deltaTime;
            Vector2 motion = orbitalDistance * directionOfMotion;
            Vector2 movedPos = oldPos + motion;

            //maintain distance from parent
            Vector2 parentPos = transform.parent.transform.position;
            float newDistanceFromParent = (parentPos - movedPos).magnitude;
            float radialDifference = newDistanceFromParent - radiusFromParent;
            Vector2 vectorToMoveBackTowardsPlanet = radialDifference * towardsPlanet;
            Vector2 movedPosMaintainRadius = movedPos + vectorToMoveBackTowardsPlanet;
            transform.position = movedPosMaintainRadius;
        }
    }
    
    void setRadiusFromParent() {
        if (hasParentPlanet) {
            Vector2 selfCenter = transform.position;
            Vector2 parentCenter = transform.parent.transform.position;
            Vector2 vectorToCenter = parentCenter - selfCenter;
            float distanceToCenter = vectorToCenter.magnitude;
            radiusFromParent = distanceToCenter;
        }
    
    }
    */

}
