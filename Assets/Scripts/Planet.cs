using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Planet : MonoBehaviour{

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
    
}
