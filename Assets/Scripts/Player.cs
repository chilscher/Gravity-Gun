using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour{
    
    public Planet planet;
    private Vector2 downDirection; //a unit vector in the direction of down
    public float playerMass = 1f;
    public Vector2 velocity;
    private bool isOnPlanet = false;
    //private bool canWalk;
    //private bool canJump;
    private float accelerationDueToGravity;
    private float distanceToPlanetSurface;
    public float rotationSpeed = 0.7f; //how fast the player and camera rotate
    public float staticGravity = 0.2f;
    public float gravitationalConstant = 1f;
    public bool useStaticGravity = false;


    private List<Planet> planetsToIgnoreCollisions = new List<Planet>(); //after a new planet is clicked, the old planet is added to a list of planets to ignore a collision with
    private List<float> timeLeftToIgnoreCollisions = new List<float>(); //the time remaining for a planet to be ignored. when hits 0, the planet and time are popped.
    public float ignorePlanetDuration = 1f; // number of seconds that a collision with a planet is ignored for

    // Start is called before the first frame update
    void Start(){
        //velocity = Vector2.zero;
        
    }

    // Update is called once per frame
    void Update() {
        countDownIgnoreTimers();
        renewTimerInsidePlanet();
        stopIgnoringTimedOutCollisions();
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
                accelerationDueToGravity = staticGravity;
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
        //should this change so it stops the player if ANY planet is in the way?
        Vector2 changeInPosition = velocity * Time.deltaTime;
        float changeDistance = changeInPosition.magnitude;
        float distanceToPlanetSurface = getDistanceToPlanetSurface();
        if (changeDistance > distanceToPlanetSurface) {
            //do something if the planet is in the way?
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
        //rotates the player (and its child object the main camera) towards the planet
        Vector2 direction = downDirection;
        float angle = (Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg) + 90;
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime);
    }

    public void changePlanet(Planet p) {
        //add old planet on list of planets to ignore
        Planet previousPlanet = planet;
        addPlanetToIngoreList(previousPlanet);
        //if new planet is on ignore list, remove it
        Planet newPlanet = p;
        removePlanetFromIgnoreList(p);
        planet = p;
    }

    void addPlanetToIngoreList(Planet p) {
        //adds p to the list of planets to ignore collisions with, for a duration of ignorePlanetDuration
        if (arePlanetCollisionsIgnored(p) == false) {
            //do not ignore or add to list if planet is already present in list
            disableCollisionWithPlanet(p);
            planetsToIgnoreCollisions.Add(p);
            timeLeftToIgnoreCollisions.Add(ignorePlanetDuration);
        }
    }

    void countDownIgnoreTimers() {
        //reduces the timer on ignored planets
        for (int i = 0; i < timeLeftToIgnoreCollisions.Count; i++) {
            timeLeftToIgnoreCollisions[i] -= Time.deltaTime;
        }
    }

    void stopIgnoringTimedOutCollisions() {
        //removes planets and times from the ignore collision lists if their timers are down to 0
        //if the player is inside a planet, the planet/timer combo is not removed
        for (int i = 0; i < timeLeftToIgnoreCollisions.Count; i++) {
            float t = timeLeftToIgnoreCollisions[i];
            Planet p = planetsToIgnoreCollisions[i];
            if (t <= 0) {
                if (playerIsInsidePlanet(p) == false) {
                    removePlanetFromIgnoreList(p);
                }

            }
        }
    }

    void disableCollisionWithPlanet(Planet p) {
        Collider2D playerCollider = GetComponent<BoxCollider2D>();
        Collider2D planetCollider = p.GetComponent<CircleCollider2D>();
        Physics2D.IgnoreCollision(playerCollider, planetCollider, true);

    }

    void enableCollisionWithPlanet(Planet p) {
        Collider2D playerCollider = GetComponent<BoxCollider2D>();
        Collider2D planetCollider = p.GetComponent<CircleCollider2D>();
        Physics2D.IgnoreCollision(playerCollider, planetCollider, false);

    }

    bool playerIsInsidePlanet(Planet p) {
        //the player is inside the planet if any of their vertices are inside the planet's radius or if they intersect with the planet's surface
        //see if player collides with planet's surface
        BoxCollider2D playerCollider = GetComponent<BoxCollider2D>();
        CircleCollider2D planetCollider = p.GetComponent<CircleCollider2D>();
        //collisions have to be enabled to detect if the player is touching the surface
        enableCollisionWithPlanet(p);
        bool isOnSurface = (playerCollider.IsTouching(planetCollider));
        //if the collision should be disabled, disable it again
        if (arePlanetCollisionsIgnored(p)) {
            disableCollisionWithPlanet(p);
        }
        if (isOnSurface) {
            return true;
        }
        //see if player's edges are inside the planet's radius
        Vector2 planetCenter = p.transform.position;
        float colliderRadius = planetCollider.radius;
        float planetScale = p.transform.lossyScale.x; //asumes a uniform scaling
        float planetRadius = colliderRadius * planetScale;


        Vector2 playerCenter = transform.position;
        float d = Vector2.Distance(playerCenter, planetCenter);
        bool isInCenter = Vector2.Distance(playerCenter, planetCenter) < planetRadius;
        if (isInCenter) {
            return true;
        }


        return false;
    }

    void removePlanetFromIgnoreList(Planet p) {
        //Planet p can once again be collided with
        if (arePlanetCollisionsIgnored(p)) {
            for (int i = 0; i < planetsToIgnoreCollisions.Count; i++) {
                if (p == planetsToIgnoreCollisions[i]) {
                    enableCollisionWithPlanet(p);
                    planetsToIgnoreCollisions.RemoveAt(i);
                    timeLeftToIgnoreCollisions.RemoveAt(i);
                }
            }

        }
    }

    bool arePlanetCollisionsIgnored(Planet p) {
        //returns true if collisions with Planet p are being ignored
        return (planetsToIgnoreCollisions.Contains(p));
    }

    public void clickedPlanet(Planet p) {
        //if p is an applicable planet, set it to be the player's down planet
        if (playerIsInsidePlanet(p) == false) {
            changePlanet(p);
        }
    }

    void renewTimerInsidePlanet() {
        //if the player is inside one of the collision-ignored planets, reset its countdown timer to the max
        for (int i = 0; i < timeLeftToIgnoreCollisions.Count; i++) {
            Planet p = planetsToIgnoreCollisions[i];
            if (playerIsInsidePlanet(p)) {
                //if the player is still inside the planet, reset the ignore timer
                timeLeftToIgnoreCollisions[i] = ignorePlanetDuration;
            }
        }
    }
}
