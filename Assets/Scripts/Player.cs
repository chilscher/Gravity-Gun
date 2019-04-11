using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour{

    //player attributes
    public Vector2 velocity;
    private Vector2 netAcceleration;
    private Vector2 playerBottom; //a unit vector pointing down from the player's orientation
    private Vector2 playerTop;
    private Vector2 playerRight;
    private Vector2 playerLeft;
    
    //walking attributes
    public float maxWalkingSpeed = 2f; //1 means the player walks 1xRadius per second --uses circlecollider2d
    public float timeToMaxSpeed = 0.5f; //amount of seconds it takes to get to max speed on a neutral surface
    private bool canWalk;
    private bool isWalking;
    private Vector2 walkingDirection;

    //rotation attributes -- how fast the player and camera rotate during free fall and while on a planet
    public float rotationSpeed = 0.7f; //how fast the player and camera rotate

    //planet attributes -- this planet is the one the player is falling towards / walking on
    public Planet planet;
    private bool isOnPlanet = false;
    //private bool wasOnPlanet;
    private float distanceToPlanetSurface;
    //private bool shouldStayOnSurface;
    private Vector2 downDirection; //a unit vector in the direction of down, towards the planet
    private Vector2 leftDirection; // a unit vector tangential to the planet's surface, going counterclockwise
    private Vector2 rightDirection; //a unit vector tangenial to the planet's surface, going clockwise
    private Vector2 upDirection; //a unit vector in the direction of up, away from the planet

    //gravitational attributes -- determines how fast the player falls
    private bool useStaticGravity = false;
    private float staticGravity = 0.2f;
    public float gravitationalConstant = 1f;


    //collision attributes -- if the player can collide with certain objects
    private List<Planet> planetsToIgnoreCollisions = new List<Planet>(); //after a new planet is clicked, the old planet is added to a list of planets to ignore a collision with
 
    void Start(){
    }
    
    void Update() {
        //Debug.Log("-------- new frame --------");
        //setWasOnPlanet();
        countDownIgnoredPlanetTimers();
        renewPlanetIgnoreTimersIfApplicable();
        stopIgnoringTimedOutCollisions();
        
        calculateDirections();
        calculateIsOnPlanet();
        //Debug.Log("is player on planet?: " + isOnPlanet);
        removeDownSpeedIfOnPlanet();
        //calculateShouldStayOnSurface();

        calculateCanWalk();
        calculateIsWalking();
        setWalkingDirection();
        
        calculateNetAcceleration();
        acceleratePlayer();
        movePlayer();
        //calculateIsOnPlanet();
        //stayOnSurface();
        rotatePlayerTowardsPlanet();
        
        rotateVelocityToHorizontal();
        calculateIsOnPlanet();
        //Debug.Log("is player on planet?: " + isOnPlanet);
        //Debug.Log("is player bottom = down: " + (downDirection == playerBottom));

    }

    public void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.tag == "Planet") { changePlanet(collision.gameObject.GetComponent<Planet>());       }
    }



    //----------BASIC MOVEMENT AND PLANET SELECTION FUNCTIONS---------------
    
        /*
    void calculateShouldStayOnSurface() {
        shouldStayOnSurface = false;
        if (isOnPlanet) {
            shouldStayOnSurface = true;
        }
    }
    */
    void calculateCanWalk() { canWalk = isOnPlanet; }

    void calculateIsWalking() {
        isWalking = false;
        if (canWalk) { //and key is being pressed
            if (Input.GetKey(KeyCode.D) || Input.GetKey(KeyCode.A)) {
                isWalking = true;
            }
        }
    }

    void setWalkingDirection() {
        if (isWalking) {
            if (Input.GetKey(KeyCode.D)) { walkingDirection = rightDirection;     }
            if (Input.GetKey(KeyCode.A)) { walkingDirection = leftDirection;      }
        }
        else { walkingDirection = Vector2.zero; }

    }
    

    void calculateNetAcceleration() {
        //net acceleration = gravity + normal + friction
        float G = gravitationalConstant;
        float M = planet.GetComponent<Rigidbody2D>().mass;
        float d = getDistanceToPlanetCenter();
        float ag = (G * M) / (d * d);
        float coefficientOfFriction = planet.coefficientOfFriction;

        float gravityMagnitude = ag;
        if (useStaticGravity) { gravityMagnitude = staticGravity;     }

        float normalMagnitude = gravityMagnitude;
        if (isOnPlanet == false) { normalMagnitude = 0f;       }

        float frictionMagnitude = normalMagnitude * coefficientOfFriction;
        //frictionMagnitude = 0f;
        Vector2 frictionDirection = velocity.normalized * -1;

        //calculate walking magnitude and direction
        //is 0 unless the player is walking
        //if the player is moving faster than their walking speed in the same direction, their walking speed is 0
        //the player's walking acceleration is reduced to keep the walking speed at or below their maximum
        //if the player is walking in the opposite direction of their friction, their walking cancels the friction out
        float walkingMagnitude = 0f;
        float walkAccelOnPlanet = (maxWalkingSpeed / timeToMaxSpeed) * coefficientOfFriction;
        Vector2 walkDir = walkingDirection;
        if (isWalking) {
            walkingMagnitude = walkAccelOnPlanet; //your walking acceleration = your player's walking speed, reduced by the coefficient of friction on your planet
            //Debug.Log(walkingMagnitude);
            if (velocity.normalized == walkDir) {
                if (velocity.magnitude > maxWalkingSpeed) { walkingMagnitude = 0f; } //if you are walking in your direction of motion and are above max speed, then walking does not accelerate you
                else if (((walkAccelOnPlanet * Time.deltaTime) + velocity.magnitude) > maxWalkingSpeed) { //if you are walking in your direction of motion, walking cannot accelerate you above your max walking speed
                    walkingMagnitude = (maxWalkingSpeed - velocity.magnitude) / Time.deltaTime;
                    //Debug.Log("going too fast");
                }
            }
            /*
            else {
                Debug.Log("you are not walking on the surface of the planet");
            }
            */
        }

        if (walkingMagnitude > 0) {
            //if you are still walking after all of those calculations
            if (walkDir == (frictionDirection * -1)) { //if your walking direction opposes friction
                walkingMagnitude += frictionMagnitude;
            }
        }

        

        Vector2 gravityVector = gravityMagnitude * downDirection;
        Vector2 normalVector = normalMagnitude * downDirection * (-1);
        Vector2 frictionVector = frictionMagnitude * frictionDirection;
        Vector2 walkingVector = walkingMagnitude * walkDir;


        netAcceleration = gravityVector + normalVector + frictionVector + walkingVector;
        /*
        //after all that, if normal force is enough to elevate player off of planet, then tone it down
        if (isOnPlanet) {
            Vector3 na = netAcceleration;
            Vector3 up = upDirection;
            Vector3 proj = Vector3.Project(na, up);
            Vector2 upComponent = proj; //amount of velocity that is in the down direction
            if (upComponent.magnitude > 0) {
                //remove upComponent from net Acceleration
                netAcceleration -= upComponent;
            }

        }
        */
        /*
        if (isOnPlanet) {
            netAcceleration = frictionVector + walkingVector + gravityVector;
        }
        else {
            netAcceleration = gravityVector + normalVector + frictionVector + walkingVector;
        }
        */
    }

    void removeDownSpeedIfOnPlanet() {
        //if the player is on their planet, they immediately lose all downward momentum
        if (isOnPlanet) {
            Vector3 v = velocity;
            Vector3 down = downDirection;
            Vector3 proj = Vector3.Project(v, down);
            Vector2 downProjection = proj; //amount of velocity that is in the down direction
            Vector2 flattenedVelocity = velocity - downProjection;
            velocity = flattenedVelocity;
        }
    }

    void acceleratePlayer() {
        //applies the net acceleration to the player
        Vector2 velocityChange = Time.deltaTime * netAcceleration;
        velocity += velocityChange;
        Debug.Log(velocity.magnitude);
    }

    void movePlayer(){
        //moves the player in the direction of their velocity, unless their chosen planet is in the way
        //should this change so it stops the player if ANY planet is in the way?

        Vector2 changeInPosition = velocity * Time.deltaTime;
        float changeDistance = changeInPosition.magnitude;
        float distanceToPlanetSurface = getDistanceToPlanetSurface();
        Vector2 pos = transform.position;
        pos += changeInPosition;
        transform.position = pos;
        
    }
    /*
    void stayOnSurface() {
        //if the player should stay on their planet's surface and they are not on it, move them back down
        if (shouldStayOnSurface) {
            
            Collider2D playerCollider = GetComponent<CircleCollider2D>();
            Collider2D planetCollider = planet.GetComponent<CircleCollider2D>();
            float d = playerCollider.Distance(planetCollider).distance;
            
            bool i = (playerCollider.IsTouching(planetCollider));
            
            if (isOnPlanet == false) {

                float planetDistance = getDistanceToPlanetSurface();
                float playerDistance = GetComponent<CircleCollider2D>().radius;
                float distanceToCollision = planetDistance - playerDistance;
                //float distanceToCollision = planetDistance;
                Vector2 direction = downDirection;
                Vector2 changeInPosition = direction * distanceToCollision;
                Vector2 pos = transform.position;
                pos += changeInPosition;
                transform.position = pos;
                if (changeInPosition != Vector2.zero) {
                    Debug.Log("moved player back to planet");

                }
            }
        }
    }
    */

    
    void rotatePlayerTowardsPlanet() {
        //rotates the player (and its child object the main camera) towards the planet
        bool lowSpeed = false;
        bool midSpeed = false;
        bool highSpeed = false;
        bool lowAngle = false;
        //bool midAngle = false;
        //bool highAngle = false;

        if (velocity.magnitude <= (maxWalkingSpeed * 1.1)) { lowSpeed = true;       }
        else if (velocity.magnitude <= (maxWalkingSpeed * 3)){ midSpeed = true;     }
        else { highSpeed = true;     }

        float downAngle = Vector2.Angle(playerBottom, downDirection); //angle in degrees
        if (downAngle < 10) { lowAngle = true;    }

        Vector2 direction = downDirection;
        float angle = (Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg) + 90; //in degrees
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        if (isOnPlanet && lowSpeed && lowAngle) { transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 1);  }
        else if (isOnPlanet && lowSpeed) { transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 0.05f);     }
        if (isOnPlanet && midSpeed && lowAngle) { transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 1);  }
        else if (isOnPlanet && midSpeed) { transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 0.06f);     }
        if (isOnPlanet && highSpeed && lowAngle) { transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 1); }
        else if (isOnPlanet && highSpeed) { transform.rotation = Quaternion.Slerp(transform.rotation, rotation, .07f);     }
        /*
        if (isOnPlanet && velocity.magnitude > rotationSpeed) {
            float speedtotal = 0.15f * 100;
            float rotspeed = speedtotal / velocity.magnitude; //the faster you are going, the faster you rotate
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rotspeed * Time.deltaTime);
            //transform.rotation = Quaternion.Slerp(transform.rotation, rotation, velocity.magnitude * Time.deltaTime);

        }
        else { transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime);     }//use static rotation rate
        */
        else {
            transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime);

        }
    }
    

    public void clickedPlanet(Planet p) {
        //if p is an applicable planet, set it to be the player's down planet
        if (playerIsInsidePlanet(p) == false) { changePlanet(p);     }
        else { Debug.Log("You can't select this planet, because you are inside of it!");    }
    }

    void changePlanet(Planet p) {
        //add old planet on list of planets to ignore
        Planet previousPlanet = planet;
        addPlanetToIngoreList(previousPlanet);
        //if new planet is on ignore list, remove it
        Planet newPlanet = p;
        removePlanetFromIgnoreList(p);
        planet = p;
    }

    Vector2 getHorizontalVelocity() {
        Vector3 v = velocity;
        Vector3 down = downDirection;
        Vector3 proj = Vector3.Project(v, down);
        Vector2 downProjection = proj; //amount of velocity that is in the down direction
        Vector2 flattenedVelocity = velocity - downProjection; //amount of veloicty in the horizontal direction
        return flattenedVelocity;

    }

    void rotateVelocityToHorizontal() {
        //rotates the player's velocity vector so it is perpendicular to their down direction
        if (isOnPlanet) {
            Vector2 horizontalVelocity = getHorizontalVelocity();
            float mag = velocity.magnitude;
            Vector2 hNormal = horizontalVelocity.normalized;
            Vector2 newVelocity = hNormal * (mag);
            velocity = newVelocity;
        }

    }






    //-----------FUNCTIONS THAT GIVE INFORMATION ABOUT THE PLAYER'S RELATION TO A PLANET


    void calculateDirections() {
        calculateDown();
        calculateUp();
        calculateRight();
        calculateLeft();
        calculatePlayerUp();
        calculatePlayerDown();
        calculatePlayerRight();
        calculatePlayerLeft();
    }

    void calculatePlayerDown() { playerBottom = playerTop * -1;  }    //sets playerBottom to be a unit vector pointing to the bottom of the player object
    void calculatePlayerUp() { playerTop = transform.up;         }    //sets playerTop to be a unit vector pointing to the bottom of the player object
    void calculatePlayerRight() { playerRight = Vector2.Perpendicular(playerBottom);   }   //sets playerRight to be a unit vector pointing to the bottom of the player object
    void calculatePlayerLeft() { playerLeft = playerRight * -1;  }   //sets playerLeft to be a unit vector pointing to the bottom of the player object
    void calculateUp() { upDirection = -1 * downDirection;       }   //sets upDirection to be a unit vector pointing up, opposite of down
    void calculateDown() { downDirection = (planet.transform.position - transform.position).normalized;    }
    void calculateRight() { rightDirection = Vector2.Perpendicular(downDirection);   }   //sets rightDirection to be a unit vector pointing right, which is perpendicular to down
    void calculateLeft() { leftDirection = -1 * rightDirection;   }   //sets leftDirection to be a unit vector pointing left, which is perpendicular to down

    void calculateIsOnPlanet() {
        //determines if the player is on their destination planet
        //Collider2D playerCollider = GetComponent<BoxCollider2D>();
        Collider2D playerCollider = GetComponent<CircleCollider2D>();
        Collider2D planetCollider = planet.GetComponent<CircleCollider2D>();
        isOnPlanet = (playerCollider.IsTouching(planetCollider));
        
    }

    /*
    void setWasOnPlanet() {
        wasOnPlanet = isOnPlanet;
    }
    */
    float getDistanceToPlanetSurface() {
        if (isOnPlanet) { return 0f;     }
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

    bool playerIsInsidePlanet(Planet p) {
        //the player is inside the planet if any of their vertices are inside the planet's radius or if they intersect with the planet's surface
        //see if player collides with planet's surface -- collisions have to be enabled for this
        //BoxCollider2D playerCollider = GetComponent<BoxCollider2D>();
        Collider2D playerCollider = GetComponent<CircleCollider2D>();
        CircleCollider2D planetCollider = p.GetComponent<CircleCollider2D>();
        enableCollisionWithPlanet(p);
        bool isOnSurface = (playerCollider.IsTouching(planetCollider));
        if (planetsToIgnoreCollisions.Contains(p)) { disableCollisionWithPlanet(p); }    //if the collision should be disabled, disable it again
        if (isOnSurface) {  return true;     }

        //see if player's edges are inside the planet's radius
        Vector2 planetCenter = p.transform.position;
        float colliderRadius = planetCollider.radius;
        float planetScale = p.transform.lossyScale.x; //asumes a uniform scaling
        float planetRadius = colliderRadius * planetScale;
        Vector2 playerCenter = transform.position;
        float d = Vector2.Distance(playerCenter, planetCenter);
        bool isInCenter = Vector2.Distance(playerCenter, planetCenter) < planetRadius;
        if (isInCenter) { return true;      }

        return false;
    }





    //----------FUNCTIONS THAT ALLOW THE PLAYER TO IGNORE COLLISIONS WITH RECENTLY-SELECTED PLANETS------------

    void addPlanetToIngoreList(Planet p) {
        //adds p to the list of planets to ignore collisions with, for a duration of ignorePlanetDuration
        if (planetsToIgnoreCollisions.Contains(p) == false) {
            //do not ignore or add to list if planet is already present in list
            disableCollisionWithPlanet(p);
            planetsToIgnoreCollisions.Add(p);
            p.startIgnoreTimer();
        }
    }

    void countDownIgnoredPlanetTimers() {
        //reduces the timer on ignored planets
        foreach (Planet p in planetsToIgnoreCollisions) { p.countDownIgnoreTimer();    }
    }

    void stopIgnoringTimedOutCollisions() {
        //removes planets and times from the ignore collision lists if their timers are down to 0
        //if the player is inside a planet, the planet/timer combo is not removed

        List<Planet> planetsToRemove = new List<Planet>();
        foreach (Planet p in planetsToIgnoreCollisions) {
            if (p.hasIgnoreTimerFinished()) {
                p.clearIgnoreTimer();
                planetsToRemove.Add(p);
            }
        }
        foreach (Planet p in planetsToRemove) { removePlanetFromIgnoreList(p);   }
        planetsToRemove = new List<Planet>();
    }

    void disableCollisionWithPlanet(Planet p) {
        //Collider2D playerCollider = GetComponent<BoxCollider2D>();
        Collider2D playerCollider = GetComponent<CircleCollider2D>();
        Collider2D planetCollider = p.GetComponent<CircleCollider2D>();
        Physics2D.IgnoreCollision(playerCollider, planetCollider, true);
    }

    void enableCollisionWithPlanet(Planet p) {
        //Collider2D playerCollider = GetComponent<BoxCollider2D>();
        Collider2D playerCollider = GetComponent<CircleCollider2D>();
        Collider2D planetCollider = p.GetComponent<CircleCollider2D>();
        Physics2D.IgnoreCollision(playerCollider, planetCollider, false);
    }

    void removePlanetFromIgnoreList(Planet p) {
        //Planet p can once again be collided with
        if (planetsToIgnoreCollisions.Contains(p)) {
            enableCollisionWithPlanet(p);
            planetsToIgnoreCollisions.Remove(p);
        }
    }

    void renewPlanetIgnoreTimersIfApplicable() {
        //if the player is inside one of the collision-ignored planets, reset its countdown timer to the max
        foreach (Planet p in planetsToIgnoreCollisions) {
            if (playerIsInsidePlanet(p)) { p.renewIgnoreTimer();      }
        }
    }
}
