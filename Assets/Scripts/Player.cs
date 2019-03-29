using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour{

    public GameObject planet;
    private Vector2 downDirection; //a unit vector in the direction of down
    private float planetMass;
    public float playerMass;
    public Vector2 velocity;
    private bool isOnPlanet = false;
    private bool canWalk;
    private bool canJump;
    private float accelerationDueToGravity;
    private float distanceToPlanetSurface;

    // Start is called before the first frame update
    void Start(){
        //velocity = Vector2.zero;
        
    }

    // Update is called once per frame
    void Update() {
        calculateDown();
        calculateIsOnPlanet();
        //calculateDistanceToPlanetSurface();
        reduceSpeedIfOnPlanet();
        calculateAccelerationDueToGravity();
        modifyVelocityDueToGravity();
        movePlayer();

    }

    void calculateDown() {
        //sets downDirection to be a unit vector pointing in the downward direction
        Vector2 planetCenter = planet.transform.position;
        Vector2 playerCenter = transform.position;
        Vector2 nonUnitDown = planetCenter - playerCenter;
        nonUnitDown.Normalize();
        downDirection = nonUnitDown;
    }

    void calculateIsOnPlanet() {
        //determines if the player is on their destination planet
        Collider2D playerCollider = GetComponent<BoxCollider2D>();
        Collider2D planetCollider = planet.GetComponent<CircleCollider2D>();
        if (playerCollider.IsTouching(planetCollider)) {
            isOnPlanet = true;
        }
        else {
            isOnPlanet = false;
        }
        //Debug.Log("On planet? - " + isOnPlanet.ToString());
    }

    void reduceSpeedIfOnPlanet() {
        if (isOnPlanet) {
            velocity = Vector2.zero;
        }
    }

    void calculateAccelerationDueToGravity() {
        //calculates the acceleration due to gravity toward the specified planet
        if (isOnPlanet) {
            accelerationDueToGravity = 0f;
        }
        else {
            //will eventually vary with distance to planet, planet mass, and player mass
            accelerationDueToGravity = 0.2f;
        }
    }
    
    void modifyVelocityDueToGravity() {
        float changeInSpeed = Time.deltaTime * accelerationDueToGravity; //change in vector trajectory
        Vector2 changeInVelocity = downDirection * changeInSpeed;
        velocity += changeInVelocity;

        //Debug.Log("X Speed: " + velocity.x + "     Y Speed: " + velocity.y);
        
    }

    void movePlayer(){
        //moves the player in the direction of their velocity, unless their chosen planet is in the way
        Vector2 changeInPosition = velocity * Time.deltaTime;
        //if change in position is less than distance planet, continue
        //otherwise, if a line drawn between current position and new position intersects with the planet's surface,
        //      ???
        Vector2 pos = transform.position;
        pos += changeInPosition;
        transform.position = pos;
    }

    /*
    void calculateDistanceToPlanetSurface() {
        if (isOnPlanet) {
            distanceToPlanetSurface = 0f;
        }
        else {
            Vector2 planetCenter = planet.transform.position;
            Vector2 playerCenter = transform.position;
            Vector2 nonUnitDown = playerCenter - planetCenter;
            float planetRadius = planet.GetComponent<CircleCollider2D>().radius;
            float distanceToCenter = nonUnitDown.magnitude;
            distanceToPlanetSurface = distanceToCenter - planetRadius;

        }
    }
    */
}
