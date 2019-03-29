using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour{

    public GameObject planet;
    private Vector2 downDirection; //a unit vector in the direction of down
    public float playerMass = 1f;
    public Vector2 velocity;
    private bool isOnPlanet = false;
    //private bool canWalk;
    //private bool canJump;
    private float accelerationDueToGravity;
    private float distanceToPlanetSurface;
    public float rotationSpeed = 5f;
    public float gravity = 0.2f; //temporary, remove when gravity based on mass and distance is calculated
    public float gravitationalConstant = 1f;
    public bool useStaticGravity = false;

    // Start is called before the first frame update
    void Start(){
        //velocity = Vector2.zero;
        
    }

    // Update is called once per frame
    void Update() {
        calculateDown();
        calculateIsOnPlanet();
        reduceSpeedIfOnPlanet();
        calculateAccelerationDueToGravity();
        modifyVelocityDueToGravity();
        movePlayer();
        rotatePlayerTowardsPlanet();

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
        isOnPlanet = (playerCollider.IsTouching(planetCollider));
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
            if (useStaticGravity) {
                accelerationDueToGravity = gravity;
            }
            else {
                //force = GMm/(d^2), acceleration = GM/(d^2)
                float G = gravitationalConstant;
                float M = planet.GetComponent<Rigidbody2D>().mass;
                float d = getDistanceToPlanetCenter();
                float a = (G * M) / (d * d);
                accelerationDueToGravity = a;
                //Debug.Log("Gravity is: " + accelerationDueToGravity);
            }
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
        float changeDistance = changeInPosition.magnitude;
        float distanceToPlanetSurface = getDistanceToPlanetSurface();
        if (changeDistance > distanceToPlanetSurface) {
            //if the planet is in the way
            //  scale down the change in position so that its length equals distanceToPlanetSurface
        }
        Vector2 pos = transform.position;
        pos += changeInPosition;
        transform.position = pos;
    }

    
    float getDistanceToPlanetSurface() {
        if (isOnPlanet) {
            return 0f;
        }
        else {
            float distanceToCenter = getDistanceToPlanetCenter();
            float planetRadius = planet.GetComponent<CircleCollider2D>().radius;
            return (distanceToCenter - planetRadius);
        }
    }

    float getDistanceToPlanetCenter() {
        Vector2 planetCenter = planet.transform.position;
        Vector2 playerCenter = transform.position;
        Vector2 vectorToCenter = playerCenter - planetCenter;
        float distanceToCenter = vectorToCenter.magnitude;
        return distanceToCenter;
    }
    
    void rotatePlayerTowardsPlanet() {
        Vector2 direction = downDirection;
        float angle = (Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg) + 90;
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime);
    }
}
