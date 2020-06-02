using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour{

    public Planet planet;
    private bool walking = false;
    private float walkingClockwiseScalar = 1f; //1 is clockwise, -1 is counterclockwise
    public float maxWalkingSpeed = 1f;
    private float maxWalkingSpeedOnPlanet = 1f; //affected by the planet you are on!
    public float walkingAcceleration = 1f;
    [HideInInspector]
    public float currentSpeed = 0f; //positive is clockwise, negative is counterclockwise
    public bool isOnPlanet = true;
    public float rotationRate = 60f; //degrees per second
    public float rotationRate2 = 90f;
    public float fallingGravityConstant = 1f;
    public float frictionGravityConstant = 1f;

    private KeyCode leftKey = KeyCode.A;
    private KeyCode rightKey = KeyCode.D;

    public GameObject joystick;
    [HideInInspector]
    public Vector2 freeFallDirection;
    [HideInInspector]
    public float freeFallSpeed;
    
    public GameObject slowTime;

    private bool slowTimeForRotation = false;
    
    public GameObject minimapControllerGO;
    private MinimapController minimapController;
    [HideInInspector]
    public Vector2 towardsPlanet; // "down" when falling
    [HideInInspector]
    public Vector2 perpTowardsPlanet; // to the right of "down" when falling, or the "clockwise" direction tangent to the player
    
    private void Start() {
        minimapController = minimapControllerGO.GetComponent<MinimapController>();
    }

    private void Update() {
        calculateDirections();
        if (slowTimeForRotation) {
            rotatePlayerInSlowTime();
        }
        else {
            if (isOnPlanet) {
                calculateIsWalking();
                setWalkingDirection();
                setMaxWalkingSpeed();
                accelerateFromWalk();
                accelerateFromFriction();
                moveAroundPlanet(currentSpeed * Time.deltaTime);
                rotatePlayerTowardsPlanet();
            }
            else {

                fallTowardsPlanet();
                moveTowardsPlanet();
                rotatePlayerTowardsPlanet();

            }
        }
        minimapController.showPlayerMovementDirection(this);
    }

    void calculateDirections() {
        towardsPlanet = (planet.centerPoint - new Vector2(transform.position.x, transform.position.y)).normalized;
        perpTowardsPlanet = Vector2.Perpendicular(towardsPlanet);
    }
    void setMaxWalkingSpeed() {
        maxWalkingSpeedOnPlanet = maxWalkingSpeed / planet.coefficientOfFriction;
    }

    void accelerateFromWalk() {
        if (walking) {
            float speedAddition = planet.coefficientOfFriction * planet.coefficientOfFriction * walkingClockwiseScalar * (walkingAcceleration * Time.deltaTime);
            if (joystick.GetComponent<FixedJoystick>().Horizontal != 0) { //if you are using the joystick to move, the amount you accelerate by scales with how far you move the joystick
                float joystickScale = Mathf.Abs(joystick.GetComponent<FixedJoystick>().Horizontal);
                joystickScale *= 1.3f; //makes the max speed attainable even if you don't move your thumb 100% to either side
                if (joystickScale > 1) { joystickScale = 1f; }
                speedAddition *= joystickScale;
            }
            currentSpeed += speedAddition;
            if (currentSpeed >= maxWalkingSpeedOnPlanet) { currentSpeed = maxWalkingSpeedOnPlanet; }
            if (currentSpeed <= -maxWalkingSpeedOnPlanet) { currentSpeed = -maxWalkingSpeedOnPlanet; }
        }
    }

    void accelerateFromFriction() {
        if (!walking || (walking && currentSpeed * walkingClockwiseScalar < 0)) {
            float G = frictionGravityConstant;
            float M = planet.GetComponent<Rigidbody2D>().mass;
            float d = planet.GetComponent<CircleCollider2D>().radius;
            float ag = (G * M) / (d * d);
            float coefficientOfFriction = planet.coefficientOfFriction;

            float gravityMagnitude = ag;

            float frictionMagnitude = gravityMagnitude * coefficientOfFriction;
            frictionMagnitude = planet.coefficientOfFriction * planet.coefficientOfFriction * (walkingAcceleration);

            if (currentSpeed > 0) {
                currentSpeed -= frictionMagnitude * Time.deltaTime;
                if (currentSpeed < 0) {
                    currentSpeed = 0;
                }
            }
            if (currentSpeed < 0) {
                currentSpeed += frictionMagnitude * Time.deltaTime;
                if (currentSpeed > 0) {
                    currentSpeed = 0;
                }
            }
        }
    }

    void move(float add_x, float add_y) {
        float x = transform.position.x;
        float y = transform.position.y;
        x += add_x;
        y += add_y;
        Vector2 newPos = new Vector2(x, y);
        transform.position = newPos;

        minimapController.move(x, y);
    }

    void moveAroundPlanet(float distance) {
        //assumes motion clockwise. for motion counterclockwise, use a negative distance
        float planet_center_x = planet.centerPoint.x;
        float planet_center_y = planet.centerPoint.y;
        float x = transform.position.x - planet_center_x;
        float y = transform.position.y - planet_center_y;
        float r = (planet.GetComponent<CircleCollider2D>().radius * planet.transform.localScale.x) + GetComponent<CircleCollider2D>().radius;
        float s = distance * -1;
        float theta = (s / r) + Mathf.Atan(y / x);
        if (x < 0) {
            theta += Mathf.PI;
        }
        float x2 = r * Mathf.Cos(theta);
        float y2 = r * Mathf.Sin(theta);
        float del_x = (x2 - x);
        float del_y = (y2 - y);
        move(del_x, del_y);
        
    }


    void rotatePlayerTowardsPlanet() {
        //rotates the player (and its child object the main camera) towards the planet
        //rotates the full amount required immediately
        //usually used after slow time is finished, when the player's angle is supposed to continuously track the planet
        float angleToRotate = angleDifferenceToPlanet();
        float a = (Mathf.Atan2(towardsPlanet.y, towardsPlanet.x) * Mathf.Rad2Deg) + 90; //in degrees
        Quaternion rotation = Quaternion.AngleAxis(a, Vector3.forward);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 1);
    }

    private float angleDifferenceToPlanet() {
        //returns the angle difference between the player's bottom and the center of the planet vector
        Vector2 playerBottom = transform.up * -1;
        float downAngle = Vector2.Angle(playerBottom, towardsPlanet); //angle in degrees

        Vector2 direction = towardsPlanet;
        float angle = (Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg) + 90; //in degrees
        angle -= (transform.eulerAngles.z - 360f);
        angle = angle - Mathf.CeilToInt(angle / 360f) * 360f;
        if (angle < 0) {
            angle += 360f;
        }
        float angleToRotate = angle;
        return angleToRotate;
    }

    
    public void clickedPlanet(Planet p) {
        if (p!= planet) {
            planet.ignorePlayerContact = true;
            planet = p;
            if (isOnPlanet) {
                freeFallSpeed = Mathf.Abs(currentSpeed);
                if (currentSpeed != 0) {
                    freeFallDirection = Vector2.Perpendicular(transform.up * -1).normalized;
                    if (freeFallSpeed != currentSpeed) {
                        freeFallDirection *= -1f;
                    }
                }
            }
            isOnPlanet = false;
            slowTimeForRotation = true;
            slowTime.SetActive(true);
        }
    }
    void fallTowardsPlanet() {
        //net acceleration = gravity + normal + friction
        float G = fallingGravityConstant;
        float M = planet.GetComponent<Rigidbody2D>().mass;
        
        float d = planet.getDistanceToPlayerCenter();
        float ag = (G * M) / (d * d);

        float gravityMagnitude = ag;
        Vector2 gravityVector = gravityMagnitude * towardsPlanet;
        freeFallDirection = (freeFallDirection * freeFallSpeed + gravityVector).normalized;
        freeFallSpeed = (freeFallDirection * freeFallSpeed + gravityVector).magnitude;
        
    }

    void moveTowardsPlanet() {
        float move_x = freeFallDirection.x * freeFallSpeed * Time.deltaTime;
        float move_y = freeFallDirection.y * freeFallSpeed * Time.deltaTime;
        move(move_x, move_y);
    }
    

    public void touchPlanet(Planet p) {
        if (planet != p) {
            //enter slow time mode and rotate player until they are facing the right way
            slowTimeForRotation = true;
            slowTime.SetActive(true);
        }
        isOnPlanet = true;
        planet = p;
        currentSpeed = 0f;
        setSpeedOnPlanet();
        setDirectionOnPlanet();
    }
    
    void setSpeedOnPlanet() {
        Vector3 s = perpTowardsPlanet;
        Vector3 v = freeFallDirection * freeFallSpeed;
        Vector3 proj = Vector3.Project(v, s);
        currentSpeed = proj.magnitude;
    }
    
    void setDirectionOnPlanet() {
        Vector3 s = perpTowardsPlanet;
        Vector3 v = freeFallDirection * freeFallSpeed;
        Vector3 proj = Vector3.Project(v, s);
        Vector2 p = proj.normalized;
        if (p == perpTowardsPlanet) {
            //print("clockwise");
        }
        else if (p == -perpTowardsPlanet) {
            //print("counterclockwise");
            currentSpeed *= -1;
        }
        else {
            print("there was an error setting your landing movement direction");
        }

    }

    void rotatePlayerInSlowTime() {
        //rotates the player towards the planet, partially
        //ususally used right after the player targets a new planet, when the camera rotation needs to be gradual
        float angleToRotate = angleDifferenceToPlanet();
        bool rotateClockwise = false;
        if (angleToRotate > 180) {
            rotateClockwise = true;
        }
        float clockwiseScalar = 1f;
        if (rotateClockwise) {
            clockwiseScalar = -1f;
        }
        if (angleToRotate > rotationRate2 * Time.deltaTime) {
            angleToRotate = rotationRate2 * Time.deltaTime;

        }
        else {
            slowTimeForRotation = false;
            slowTime.SetActive(false);
        }
        transform.Rotate(0, 0, angleToRotate * clockwiseScalar);
    }
    

    //----------BASIC MOVEMENT AND PLANET SELECTION FUNCTIONS---------------


    void calculateIsWalking() {
        walking = false;
        if (Input.GetKey(rightKey) || Input.GetKey(leftKey)) { walking = true; }
        if (Input.GetKey(rightKey) && Input.GetKey(leftKey)) { walking = false; }
        if (joystick.GetComponent<FixedJoystick>().Horizontal != 0) { walking = true; }
    }

    void setWalkingDirection() {
        if (walking) {
            if (Input.GetKey(rightKey)) { walkingClockwiseScalar = 1f; }
            if (Input.GetKey(leftKey)) { walkingClockwiseScalar = -1f; }
            if (joystick.GetComponent<FixedJoystick>().Horizontal > 0) { walkingClockwiseScalar = 1f; }
            if (joystick.GetComponent<FixedJoystick>().Horizontal < 0) { walkingClockwiseScalar = -1f; }
        }

    }
    /*
    //player attributes
    public Vector2 velocity;
    private Vector2 netAcceleration;
    private Vector2 playerBottom; //a unit vector pointing down from the player's orientation
    private Vector2 playerTop;
    private Vector2 playerRight;
    private Vector2 playerLeft;
    private float stopMovingThreshold = 0.2f; //if the player is below this speed on a planet while not walking, their velocity drops to 0
    
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
    private float distanceToPlanetSurface;
    private float timeSpentOnPlanet;
    private Vector2 downDirection; //a unit vector in the direction of down, towards the planet
    private Vector2 leftDirection; // a unit vector tangential to the planet's surface, going counterclockwise
    private Vector2 rightDirection; //a unit vector tangenial to the planet's surface, going clockwise
    private Vector2 upDirection; //a unit vector in the direction of up, away from the planet

    //gravitational attributes -- determines how fast the player falls
    private bool useStaticGravity = false;
    private float staticGravity = 0.2f;
    public float gravitationalConstant = 1f;

    //keycode inputs
    private KeyCode leftKey = KeyCode.A;
    private KeyCode rightKey = KeyCode.D;

    //checkpoint attributes
    public Checkpoint lastCheckpoint = null;

    void Start(){
    }
    
    void Update() {
        
        calculateDirections();
        calculateIsOnPlanet();
        setTimeSpentOnPlanet();
        removeDownSpeedIfOnPlanet();

        calculateCanWalk();
        calculateIsWalking();
        setWalkingDirection();
        
        calculateNetAcceleration();
        acceleratePlayer();
        movePlayer();
        stopMovingIfSlowOnPlanet();
        rotatePlayerTowardsPlanet();
        
        printStatuses();
    }

    public void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.tag == "Planet") { changePlanet(collision.gameObject.GetComponent<Planet>());       }
    }



    //----------BASIC MOVEMENT AND PLANET SELECTION FUNCTIONS---------------
    
    void calculateCanWalk() { canWalk = isOnPlanet; }

    void calculateIsWalking() {
        isWalking = false;
        if (canWalk) { //and key is being pressed
            if (Input.GetKey(rightKey) || Input.GetKey(leftKey)) {isWalking = true;}
        }
    }

    void setWalkingDirection() {
        if (isWalking) {
            if (Input.GetKey(rightKey)) { walkingDirection = rightDirection;     }
            if (Input.GetKey(leftKey)) { walkingDirection = leftDirection;      }
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

        float frictionMagnitude = normalMagnitude * coefficientOfFriction * 0.5f; //0.5f is there to make it take longer to slow down on a planet. Emphasizes low friction
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
            //print(walkingMagnitude);
            if (velocity.normalized == walkDir) {
                if (velocity.magnitude > maxWalkingSpeed) { walkingMagnitude = 0f; } //if you are walking in your direction of motion and are above max speed, then walking does not accelerate you
                else if (((walkAccelOnPlanet * Time.deltaTime) + velocity.magnitude) > maxWalkingSpeed) { //if you are walking in your direction of motion, walking cannot accelerate you above your max walking speed
                    walkingMagnitude = (maxWalkingSpeed - velocity.magnitude) / Time.deltaTime;
                    //print("going too fast");
                }
            }
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

        //scale down normal force to tone down jitters, but not enough to make the player fly off -- scale down more on high-friction planets
        //dont scale down at all at low speeds
        if (isOnPlanet) {
            if (velocity.magnitude > planet.coefficientOfFriction) {
                float f = planet.coefficientOfFriction;
                float Nscale = 0.9f - f;
                normalVector *= Nscale;

            }
        }

        netAcceleration = gravityVector + normalVector + frictionVector + walkingVector;
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
    }

    void movePlayer(){
        //moves the player according to their current velocity
        Vector2 changeInPosition = velocity * Time.deltaTime;
        float changeDistance = changeInPosition.magnitude;
        float distanceToPlanetSurface = getDistanceToPlanetSurface();
        Vector2 pos = transform.position;
        pos += changeInPosition;
        transform.position = pos;
    }

    
    void rotatePlayerTowardsPlanet() {
        //rotates the player (and its child object the main camera) towards the planet
        bool lowSpeed = false;
        bool midSpeed = false;
        bool highSpeed = false;
        bool lowAngle = false;

        if (velocity.magnitude <= (maxWalkingSpeed * 1.1)) { lowSpeed = true;       }
        else if (velocity.magnitude <= (maxWalkingSpeed * 3)){ midSpeed = true;     }
        else { highSpeed = true;     }

        float downAngle = Vector2.Angle(playerBottom, downDirection); //angle in degrees
        if (downAngle < 1) { lowAngle = true;    }

        Vector2 direction = downDirection;
        float angle = (Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg) + 90; //in degrees
        Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);
        if (timeSpentOnPlanet > 2f) { transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Mathf.Min(1, (0.035f * timeSpentOnPlanet))); }
        else if (isOnPlanet && lowAngle) { transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 1f); }
        else if (isOnPlanet && lowSpeed) { transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 0.05f);}
        else if (isOnPlanet && midSpeed) { transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 0.06f);}
        else if (isOnPlanet && highSpeed) { transform.rotation = Quaternion.Slerp(transform.rotation, rotation, .07f);}
        else { transform.rotation = Quaternion.Slerp(transform.rotation, rotation, rotationSpeed * Time.deltaTime);}
    }
    

    public void clickedPlanet(Planet p) {
        //if p is an applicable planet, set it to be the player's down planet
        changePlanet(p);
    }

    void changePlanet(Planet newPlanet) {
        planet = newPlanet;
    }

    Vector2 getHorizontalVelocity() {
        Vector3 v = velocity;
        Vector3 down = downDirection;
        Vector3 proj = Vector3.Project(v, down);
        Vector2 downProjection = proj; //amount of velocity that is in the down direction
        Vector2 flattenedVelocity = velocity - downProjection; //amount of veloicty in the horizontal direction
        return flattenedVelocity;

    }    

    void stopMovingIfSlowOnPlanet() {
        //if the player is on a planet and going slow enough
        //and they are EITHER not walking OR walking in the opposite direction of thier motion
        //then stop them
        //"slow enough" is below a predetermined threshold and less than the planet's coefficient of friction
        if ((isOnPlanet) && ((velocity.magnitude < planet.coefficientOfFriction) || (velocity.magnitude < stopMovingThreshold))){
            if ((!isWalking) || (isWalking && velocity.normalized == -walkingDirection)){
                velocity = Vector2.zero;
            }
        }
        
    }


    private void OnTriggerEnter2D(Collider2D other) {
        if (other.gameObject.CompareTag("Checkpoint")) {
            hitCheckpoint(other.GetComponent<Checkpoint>());
        }
    }

    private void OnTriggerExit2D(Collider2D other) {
       if (other.gameObject.CompareTag("Checkpoint")) {
            leftCheckpoint(other.GetComponent<Checkpoint>());
        }
    }

    private void hitCheckpoint(Checkpoint checkpoint) {
        lastCheckpoint = checkpoint;
        checkpoint.entered();
    }

    private void leftCheckpoint(Checkpoint checkpoint) {
        checkpoint.exited();
    }

    public bool returnToLastCheckpoint() {
        //returns the player to their last checkpoint. returns the action's success or failure as a bool
        if (lastCheckpoint != null) {
            Vector2 checkpointPos = lastCheckpoint.transform.position;
            transform.position = checkpointPos; //move to the checkpoint
            velocity = Vector2.zero; //stop moving
            if (lastCheckpoint.transform.parent != null) {
                Planet newPlanet = lastCheckpoint.transform.parent.GetComponent<Planet>();
                planet = newPlanet; //set planet to be the one the checkpoint is on

                calculateDown();
                Vector2 direction = downDirection;
                float angle = (Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg) + 90; //in degrees
                Quaternion rotation = Quaternion.AngleAxis(angle, Vector3.forward);//rotate so the new planet is down
                transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 1f);

                return true;
            }
        }

        return false;
    }






    //-----------FUNCTIONS THAT GIVE INFORMATION ABOUT THE PLAYER'S RELATION TO A PLANET


    void calculateDirections() {
        calculateUp();
        calculateDown();
        calculateRight();
        calculateLeft();
        calculatePlayerDown();
        calculatePlayerUp();
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
        
        if (isOnPlanet) {
            transform.parent = planet.transform;
        }
        else {
            transform.parent = null;
        }
    }
    
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
    void setTimeSpentOnPlanet() {
        if (isOnPlanet) {timeSpentOnPlanet += Time.deltaTime;}
        else {timeSpentOnPlanet = 0f;}
    }

    void printStatuses() {
        //print(isWalking);
        //print(velocity.magnitude);
    }
    */
}
