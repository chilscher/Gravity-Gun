using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Player : MonoBehaviour{

    //player movement on the planet
    private bool walking = false;
    private float walkingClockwiseScalar = 1f; //1 is clockwise, -1 is counterclockwise
    public float maxWalkingSpeed = 1f;
    private float maxWalkingSpeedOnPlanet = 1f; //affected by the planet you are on!
    public float walkingAcceleration = 1f;
    [HideInInspector]
    public float speedOnSurface = 0f; //positive is clockwise, negative is counterclockwise
    [HideInInspector]
    public bool isOnPlanet = true;
    public float frictionGravityConstant = 1f; //increasing this increases friction on all planets
    public Planet planet;

    //player movement in free-fall
    public float fallingGravityConstant = 1f; //increasing this accelerates falling from gravity for all planets
    [HideInInspector]
    public Vector2 freeFallDirection;
    [HideInInspector]
    public float freeFallSpeed;

    //controls
    private KeyCode leftKey = KeyCode.A;
    private KeyCode rightKey = KeyCode.D;
    public GameObject joystick;

    //player rotation
    public float rotationRateInSlowTime = 90f; //degrees per second
    public GameObject stopTimeOverlay; //the semitransparent colored screen overlay that gets shown when time stops to allow the player to rotate
    private bool stopTimeForRotation = false;
    [HideInInspector]
    public Vector2 towardsPlanet; // "down" when falling
    [HideInInspector]
    public Vector2 perpTowardsPlanet; // if towardsPlanet is "down", this is "right"

    //UI elements
    public GameObject minimapControllerGO;
    private MinimapController minimapController;
    private DialogueManager dialogueManager;


    //public DialogueTrigger testDialogue;

    
    private void Start() {
        minimapController = minimapControllerGO.GetComponent<MinimapController>();
        dialogueManager = FindObjectOfType<DialogueManager>();
        //the player should land on their planet right away, to set maxWalkingSpeed
        landOnPlanet(planet);


        //testDialogue.TriggerDialogue();
    }

    private void Update() {
        calculateDirections();
        if (stopTimeForRotation) {
            rotatePlayerInStoppedTime();
        }
        else {
            if (isOnPlanet) {
                //handles movement in the surface of the planet
                calculateIsWalking();
                setWalkingDirection();
                accelerateFromWalk();
                accelerateFromFriction();
                moveAroundPlanet(speedOnSurface * Time.deltaTime);
                rotatePlayerTowardsPlanet();
            }
            else {
                //handles movement in free-fall
                fallTowardsPlanet();
                moveTowardsPlanet();
                rotatePlayerTowardsPlanet();

            }
        }
        minimapController.showPlayerMovementDirection(this);
    }

    void move(float add_x, float add_y) {
        //moves the player by add_x and add_y.
        //also moves the minimap camera
        float x = transform.position.x;
        float y = transform.position.y;
        x += add_x;
        y += add_y;
        Vector2 newPos = new Vector2(x, y);
        transform.position = newPos;

        minimapController.move(x, y);
    }


    // ---------------------------------------------------
    //FUNCTIONS THAT DEAL WITH THE PLAYER BEING ON A PLANET
    // ---------------------------------------------------

    void setMaxWalkingSpeed() {
        //sets the player's maxWalkingSpeed on the surface of a planet.
        //scales opposite of the planet's coefficientOfFriction: higher friction = lower max speed
        maxWalkingSpeedOnPlanet = maxWalkingSpeed / planet.coefficientOfFriction;
    }

    void accelerateFromWalk() {
        //accelerates the player due to walking around the surface of the planet
        //only accelerates if the player is walking
        if (walking) {
            float speedAddition = planet.coefficientOfFriction * planet.coefficientOfFriction * walkingClockwiseScalar * (walkingAcceleration * Time.deltaTime);
            if (joystick.GetComponent<FixedJoystick>().Horizontal != 0) { //if you are using the joystick to move, the amount you accelerate by scales with how far you move the joystick
                float joystickScale = Mathf.Abs(joystick.GetComponent<FixedJoystick>().Horizontal);
                joystickScale *= 1.3f; //makes the max speed attainable even if you don't move your thumb 100% to either side
                if (joystickScale > 1) { joystickScale = 1f; }
                speedAddition *= joystickScale;
            }
            speedOnSurface += speedAddition;
            if (speedOnSurface >= maxWalkingSpeedOnPlanet) { speedOnSurface = maxWalkingSpeedOnPlanet; }
            if (speedOnSurface <= -maxWalkingSpeedOnPlanet) { speedOnSurface = -maxWalkingSpeedOnPlanet; }
        }
    }

    void accelerateFromFriction() {
        //slows down the player's walking speed based on friction
        //slows the player down if they are not walking, or if their walking direction opposes their motion
        if (!walking || (walking && speedOnSurface * walkingClockwiseScalar < 0)) {
            float G = frictionGravityConstant;
            float M = planet.mass;
            float d = planet.GetComponent<CircleCollider2D>().radius;
            float gravityMagnitude = (G * M) / (d * d);
            float coefficientOfFriction = planet.coefficientOfFriction;

            float frictionMagnitude = gravityMagnitude * coefficientOfFriction;
            frictionMagnitude = planet.coefficientOfFriction * planet.coefficientOfFriction * (walkingAcceleration);

            if (speedOnSurface > 0) {
                speedOnSurface -= frictionMagnitude * Time.deltaTime;
                if (speedOnSurface < 0) { speedOnSurface = 0; }
            }
            if (speedOnSurface < 0) {
                speedOnSurface += frictionMagnitude * Time.deltaTime;
                if (speedOnSurface > 0) { speedOnSurface = 0; }
            }
        }
    }

    void moveAroundPlanet(float distance) {
        //moves the player a certain distance around the surface of the planet.
        //the distance is not linear; it is an arc length
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

    public void landOnPlanet(Planet p) {
        //all of the functions that have to be called when the player lands on the surface of a planet
        //called by planet.Update
        //sets the player's speed and direction of motion
        //rotates the player towards the planet, if they were not already facing it

        if (planet != p) {
            //enter slow time mode and rotate player until they are facing the right way
            stopTimeForRotation = true;
            stopTimeOverlay.SetActive(true);
        }
        isOnPlanet = true;
        planet = p;
        speedOnSurface = 0f;
        setSpeedOnPlanet();
        setDirectionOnPlanet();
        setMaxWalkingSpeed();
    }

    void setSpeedOnPlanet() {
        //sets the player's movement speed on the planet
        //the new speed is the component of the player's free-fall speed that is tangent to the planet's surface
        Vector3 s = perpTowardsPlanet;
        Vector3 v = freeFallDirection * freeFallSpeed;
        Vector3 proj = Vector3.Project(v, s);
        speedOnSurface = proj.magnitude;
    }

    void setDirectionOnPlanet() {
        //sets the player's movement direction at the moment they land on the planet
        //will either be clockwise or counterclockwise
        //counterclockwise sets speedOnSurface to be negative
        Vector3 s = perpTowardsPlanet;
        Vector3 v = freeFallDirection * freeFallSpeed;
        Vector3 proj = Vector3.Project(v, s);
        Vector2 p = proj.normalized;
        if (p == -perpTowardsPlanet) { speedOnSurface *= -1; }//movement is set to be counterclockwise
        else if (p != perpTowardsPlanet) { speedOnSurface = 0; }//there is no movement tangent to the surface of the planet (ex: the player was not moving when they clicked on the new planet)

    }

    void calculateIsWalking() {
        //sets the "walking" boolean based on player inputs
        //if the player is in dialogue, they cannot be walking
        walking = false;
        if (Input.GetKey(rightKey) || Input.GetKey(leftKey)) { walking = true; }
        if (Input.GetKey(rightKey) && Input.GetKey(leftKey)) { walking = false; }
        if (joystick.GetComponent<FixedJoystick>().Horizontal != 0) { walking = true; }

        if (dialogueManager.isPlayerInDialogue()) {
            walking = false;
        }
    }

    void setWalkingDirection() {
        //sets walkingClockwiseScalar based on player inputs
        if (walking) {
            if (Input.GetKey(rightKey)) { walkingClockwiseScalar = 1f; }
            if (Input.GetKey(leftKey)) { walkingClockwiseScalar = -1f; }
            if (joystick.GetComponent<FixedJoystick>().Horizontal > 0) { walkingClockwiseScalar = 1f; }
            if (joystick.GetComponent<FixedJoystick>().Horizontal < 0) { walkingClockwiseScalar = -1f; }
        }
    }


    // ---------------------------------------------------
    //FUNCTIONS THAT DEAL WITH THE PLAYER MOVING BETWEEN PLANETS
    // ---------------------------------------------------

    public void clickedPlanet(Planet p) {
        //makes the player fall toward the chosen planet
        //does nothing if the player already is targeting that planet
        //does nothing if the game is in slow-mo
        //does nothing if the player is in dialogue
        if ((p != planet) && !stopTimeForRotation && !dialogueManager.isPlayerInDialogue()) {
            planet.ignorePlayerContact = true; //the player has some time to fall away from the planet
            p.ignorePlayerContact = false; //if the player is going back to a planet they JUST left, they will not fall through it
            planet = p;
            if (isOnPlanet) {
                freeFallSpeed = Mathf.Abs(speedOnSurface);
                if (speedOnSurface != 0) {
                    freeFallDirection = Vector2.Perpendicular(transform.up * -1).normalized;
                    if (freeFallSpeed != speedOnSurface) {
                        freeFallDirection *= -1f;
                    }
                }
            }
            isOnPlanet = false;
            stopTimeForRotation = true;
            stopTimeOverlay.SetActive(true);
        }
    }
    void fallTowardsPlanet() {
        //modifes freeFallSpeed and freeFallDirection to account for gravity pulling the player towards the planet
        //used for motion off of the surface of a planet

        float G = fallingGravityConstant;
        float M = planet.mass;
        float d = planet.getDistanceToPlayerCenter();
        float gravityMagnitude = (G * M) / (d * d);
        Vector2 gravityVector = gravityMagnitude * towardsPlanet;
        freeFallDirection = (freeFallDirection * freeFallSpeed + gravityVector).normalized;
        freeFallSpeed = (freeFallDirection * freeFallSpeed + gravityVector).magnitude;

    }

    void moveTowardsPlanet() {
        //moves the player towards the planet during free-fall
        //does not change the falling speed or direction from gravity, those are changed before this function is called, in fallTowardsPlanet
        float move_x = freeFallDirection.x * freeFallSpeed * Time.deltaTime;
        float move_y = freeFallDirection.y * freeFallSpeed * Time.deltaTime;
        move(move_x, move_y);
    }



    // ---------------------------------------------------
    //FUNCTIONS THAT DEAL WITH THE PLAYER'S ROTATION, AS WELL AS CALCULATING VECTOR DIRECTIONS
    // ---------------------------------------------------
    
    void calculateDirections() {
        //sets the towardsPlanet and perpTowardsPlanet directions
        towardsPlanet = (planet.centerPoint - new Vector2(transform.position.x, transform.position.y)).normalized;
        perpTowardsPlanet = Vector2.Perpendicular(towardsPlanet);
    }

    void rotatePlayerTowardsPlanet() {
        //rotates the player (and its child object the main camera) towards the planet
        //rotates the full amount required immediately
        //usually used after stopped time is finished, when the player's angle is supposed to continuously track the planet
        float angleToRotate = angleDifferenceToPlanet();
        float a = (Mathf.Atan2(towardsPlanet.y, towardsPlanet.x) * Mathf.Rad2Deg) + 90; //in degrees
        Quaternion rotation = Quaternion.AngleAxis(a, Vector3.forward);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 1);
    }

    private float angleDifferenceToPlanet() {
        //returns the angle difference between the player's bottom and the center of the planet vector
        //playerBottom is the vector made straight through the player's feet
        //towardsPlanet is the vector made from the player to the planet core
        Vector2 playerBottom = transform.up * -1;
        float downAngle = Vector2.Angle(playerBottom, towardsPlanet); //angle in degrees
        float angle = (Mathf.Atan2(towardsPlanet.y, towardsPlanet.x) * Mathf.Rad2Deg) + 90; //in degrees
        angle -= (transform.eulerAngles.z - 360f);
        angle = angle - Mathf.CeilToInt(angle / 360f) * 360f;
        if (angle < 0) { angle += 360f; }
        float angleToRotate = angle;
        return angleToRotate;
    }
    
    void rotatePlayerInStoppedTime() {
        //rotates the player towards the planet.
        //ususally used right after the player targets a new planet, when the camera rotation needs to be gradual
        //rotation does not happen all at once. in slow time rotation is capped at (rotationRateInSlowTime) degrees/sec
        float angleToRotate = angleDifferenceToPlanet();
        bool rotateClockwise = false;
        if (angleToRotate > 180) { rotateClockwise = true; }
        float clockwiseScalar = 1f;
        if (rotateClockwise) { clockwiseScalar = -1f; }
        if (angleToRotate > rotationRateInSlowTime * Time.deltaTime) { angleToRotate = rotationRateInSlowTime * Time.deltaTime; }
        else {
            stopTimeForRotation = false;
            stopTimeOverlay.SetActive(false);
        }
        transform.Rotate(0, 0, angleToRotate * clockwiseScalar);
    }
    
}
