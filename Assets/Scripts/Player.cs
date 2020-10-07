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


    //hitting an obstacle mid-fall, including walking on it
    private bool hitObstacleSide = false;
    private bool hitObstacleTop = false;
    private bool fallingOffObstacle = false;
    private BoxCollider2D obstacle;

    //comment more
    public bool isPaused = false;
    private bool stopTimeForShoot = false;
    public float stopTimeShootDuration = 1f;
    private float shootTimeRemaining;

    
    private void Start() {
        minimapController = minimapControllerGO.GetComponent<MinimapController>();
        dialogueManager = FindObjectOfType<DialogueManager>();
        //the player should land on their planet right away, to set maxWalkingSpeed
        LandOnPlanet(planet);
    }

    private void Update() {
        if (!isPaused) {
            CalculateDirections();
            if (stopTimeForShoot) {
                ShootTowardsPlanetInStoppedTime();
            }
            else if (stopTimeForRotation) {
                RotatePlayerInStoppedTime();
            }
            else {
                if (isOnPlanet) {
                    //handles movement on the surface of the planet
                    CalculateIsWalking();
                    SetWalkingDirection();
                    AccelerateFromWalk();
                    AccelerateFromFriction();
                    MoveAroundPlanet(speedOnSurface * Time.deltaTime);
                    RotatePlayerTowardsPlanet();
                }
                else if (hitObstacleTop) {
                    //handles movement on the surface of an obstacle
                    CalculateIsWalking();
                    SetWalkingDirection();
                    AccelerateFromWalk();
                    AccelerateFromFriction();
                    MoveAroundObstacle(speedOnSurface * Time.deltaTime);
                    RotatePlayerTowardsPlanet();
                    CheckForFallingOffObstacle();
                }
                else if (hitObstacleSide) {
                    //handles movement falling down the side of an obstacle
                    FallTowardsPlanet();
                    MoveTowardsPlanet();
                    MovePlayerToEdgeOfObstacle(obstacle);

                    RotatePlayerTowardsPlanet();
                    CheckForCollisions();
                }

                else {
                    //handles movement in free-fall
                    FallTowardsPlanet();
                    MoveTowardsPlanet();
                    RotatePlayerTowardsPlanet();
                    CheckForCollisions();
                }
            }
            minimapController.ShowPlayerMovementDirection(this);
        }

    }

    void Move(float add_x, float add_y) {
        //moves the player by add_x and add_y.
        //also moves the minimap camera
        float x = transform.position.x;
        float y = transform.position.y;
        x += add_x;
        y += add_y;
        Vector2 newPos = new Vector2(x, y);
        transform.position = newPos;

        //float dist = Mathf.Sqrt((add_x * add_x) + (add_y * add_y));
        //print(dist);

        minimapController.Move(x, y);
    }


    // ---------------------------------------------------
    //FUNCTIONS THAT DEAL WITH THE PLAYER BEING ON A PLANET
    // ---------------------------------------------------

    void SetMaxWalkingSpeed() {
        //sets the player's maxWalkingSpeed on the surface of a planet.
        //scales opposite of the planet's coefficientOfFriction: higher friction = lower max speed
        maxWalkingSpeedOnPlanet = maxWalkingSpeed / planet.coefficientOfFriction;
    }

    void AccelerateFromWalk() {
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

    void AccelerateFromFriction() {
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

    void MoveAroundPlanet(float distance) {
        //moves the player a certain distance around the surface of the planet.
        //the distance is not linear; it is an arc length
        //assumes motion clockwise. for motion counterclockwise, use a negative distance
        float planet_center_x = planet.centerPoint.x;
        float planet_center_y = planet.centerPoint.y;
        float x = transform.position.x - planet_center_x;
        float y = transform.position.y - planet_center_y;
        float planetRad = planet.GetComponent<CircleCollider2D>().radius * planet.transform.localScale.x;
        if (planet.transform.parent != null && planet.transform.parent.tag == "Planet") { planetRad *= planet.transform.parent.localScale.x; }
        float r = (planetRad) + GetComponent<CircleCollider2D>().radius;
        float s = distance * -1;
        float theta = (s / r) + Mathf.Atan(y / x);
        if (x < 0) {
            theta += Mathf.PI;
        }
        float x2 = r * Mathf.Cos(theta);
        float y2 = r * Mathf.Sin(theta);
        float del_x = (x2 - x);
        float del_y = (y2 - y);
        Move(del_x, del_y);

        //print(checkIfPlayerHitObstacleOnPlanet());
        BoxCollider2D obstacleHit = CheckIfPlayerHitObstacleOnPlanet();
        if (obstacleHit != null) {
            MovePlayerToEdgeOfObstacle(obstacleHit);
            speedOnSurface = 0f;
        }
    }



    public void LandOnPlanet(Planet p) {
        //all of the functions that have to be called when the player lands on the surface of a planet
        //called by checkForCollisions
        //sets the player's speed and direction of motion
        //rotates the player towards the planet, if they were not already facing it

        if (planet != p) {
            //enter slow time mode and rotate player until they are facing the right way
            stopTimeForRotation = true;
            stopTimeOverlay.SetActive(true);
            //stop animations during slow time
            GetComponent<Animator>().enabled = false;
        }
        isOnPlanet = true;
        planet = p;
        speedOnSurface = 0f;
        CalculateDirections();
        SetSpeedOnPlanet();
        SetDirectionOnPlanet();
        SetMaxWalkingSpeed();

        hitObstacleSide = false;
        hitObstacleTop = false;
        fallingOffObstacle = false;

        //set animator conditions
        GetComponent<Animator>().SetBool("IsFalling", false);

        FindObjectOfType<QuestManager>().AccomplishTask(QuestManager.QuestType.VisitPlanet, planet.id);
    }

    void SetSpeedOnPlanet() {
        //sets the player's movement speed on the planet
        //the new speed is the component of the player's free-fall speed that is tangent to the planet's surface
        Vector3 s = perpTowardsPlanet;
        Vector3 v = freeFallDirection * freeFallSpeed;
        Vector3 proj = Vector3.Project(v, s);
        speedOnSurface = proj.magnitude;
    }

    void SetDirectionOnPlanet() {
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

    void CalculateIsWalking() {
        //sets the "walking" boolean based on player inputs
        //if the player is in dialogue, they cannot be walking
        walking = false;
        if (Input.GetKey(rightKey) || Input.GetKey(leftKey)) { walking = true; }
        if (Input.GetKey(rightKey) && Input.GetKey(leftKey)) { walking = false; }
        if (joystick.GetComponent<FixedJoystick>().Horizontal != 0) { walking = true; }

        if (dialogueManager.IsPlayerInDialogue()) {
            walking = false;
        }
        
        //animate the player as walking
        GetComponent<Animator>().SetBool("IsWalking", walking);

    }

    void SetWalkingDirection() {
        //sets walkingClockwiseScalar based on player inputs
        if (walking) {
            if (Input.GetKey(rightKey)) { walkingClockwiseScalar = 1f; }
            if (Input.GetKey(leftKey)) { walkingClockwiseScalar = -1f; }
            if (joystick.GetComponent<FixedJoystick>().Horizontal > 0) { walkingClockwiseScalar = 1f; }
            if (joystick.GetComponent<FixedJoystick>().Horizontal < 0) { walkingClockwiseScalar = -1f; }
            
            //make the player face left or right
            Vector3 scale = transform.localScale;
            scale.x = walkingClockwiseScalar;
            transform.localScale = scale;
        }
    }


    // ---------------------------------------------------
    //FUNCTIONS THAT DEAL WITH THE PLAYER MOVING BETWEEN PLANETS
    // ---------------------------------------------------

    public void ClickedPlanet(Planet p) {
        //makes the player fall toward the chosen planet
        //does nothing if the player already is targeting that planet
        //does nothing if the game is in slow-mo
        //does nothing if the player is in dialogue
        if ((p != planet) && !stopTimeForRotation && !dialogueManager.IsPlayerInDialogue() && !isPaused) {
            if (IsTooCloseToObstacle()) {
                print("you are too close to an obstacle to take off!");
            }
            else {
                planet.ignorePlayerContact = true; //the player has some time to fall away from the planet
                p.ignorePlayerContact = false; //if the player is going back to a planet they JUST left, they will not fall through it
                SetUpShoot(planet, p);
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
                //stopTimeForRotation = true;
                isOnPlanet = false;
                //stop all player animations in stopped time
                GetComponent<Animator>().enabled = false;
                //make stop the player from walking
                GetComponent<Animator>().SetBool("IsWalking", false);
            }

        }
    }
    void FallTowardsPlanet() {
        //modifes freeFallSpeed and freeFallDirection to account for gravity pulling the player towards the planet
        //used for motion off of the surface of a planet

        float G = fallingGravityConstant;
        float M = planet.mass;
        float d = planet.GetDistanceToPlayerCenter();
        float gravityMagnitude = (G * M) / (d * d);
        Vector2 gravityVector = gravityMagnitude * towardsPlanet;
        freeFallDirection = (freeFallDirection * freeFallSpeed + gravityVector).normalized;
        freeFallSpeed = (freeFallDirection * freeFallSpeed + gravityVector).magnitude;

    }

    void MoveTowardsPlanet() {
        //moves the player towards the planet during free-fall
        //does not change the falling speed or direction from gravity, those are changed before this function is called, in fallTowardsPlanet
        float move_x = freeFallDirection.x * freeFallSpeed * Time.deltaTime;
        float move_y = freeFallDirection.y * freeFallSpeed * Time.deltaTime;
        Move(move_x, move_y);
    }



    // ---------------------------------------------------
    //FUNCTIONS THAT DEAL WITH THE PLAYER'S ROTATION, AS WELL AS CALCULATING VECTOR DIRECTIONS
    // ---------------------------------------------------
    
    void CalculateDirections() {
        //sets the towardsPlanet and perpTowardsPlanet directions
        towardsPlanet = (planet.centerPoint - new Vector2(transform.position.x, transform.position.y)).normalized;
        perpTowardsPlanet = Vector2.Perpendicular(towardsPlanet);
    }

    void RotatePlayerTowardsPlanet() {
        //rotates the player (and its child object the main camera) towards the planet
        //rotates the full amount required immediately
        //usually used after stopped time is finished, when the player's angle is supposed to continuously track the planet
        float angleToRotate = AngleDifferenceToPlanet();
        float a = (Mathf.Atan2(towardsPlanet.y, towardsPlanet.x) * Mathf.Rad2Deg) + 90; //in degrees
        Quaternion rotation = Quaternion.AngleAxis(a, Vector3.forward);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, 1);
    }

    private float AngleDifferenceToPlanet() {
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
    
    void RotatePlayerInStoppedTime() {
        //rotates the player towards the planet.
        //ususally used right after the player targets a new planet, when the camera rotation needs to be gradual
        //rotation does not happen all at once. in slow time rotation is capped at (rotationRateInSlowTime) degrees/sec
        float angleToRotate = AngleDifferenceToPlanet();
        bool rotateClockwise = false;
        if (angleToRotate > 180) { rotateClockwise = true; }
        float clockwiseScalar = 1f;
        if (rotateClockwise) { clockwiseScalar = -1f; }
        if (angleToRotate > rotationRateInSlowTime * Time.deltaTime) { angleToRotate = rotationRateInSlowTime * Time.deltaTime; }
        else {
            stopTimeForRotation = false;
            stopTimeOverlay.SetActive(false);

            //once time-stop is over, allow the animator to animate the player again
            GetComponent<Animator>().enabled = true;

            //set animator conditions
            if (isOnPlanet) {
                GetComponent<Animator>().SetBool("IsFalling", false);
            }
            else {
                GetComponent<Animator>().SetBool("IsFalling", true);
            }
            
        }
        transform.Rotate(0, 0, angleToRotate * clockwiseScalar);
    }
    
    private void ShootTowardsPlanetInStoppedTime() {
        //comment more

        //if still in the moving-gun phase, move the gun
        //if after that, then shoot the gun

        shootTimeRemaining -= Time.deltaTime;
        if (shootTimeRemaining <= 0) {
            stopTimeForRotation = true;
            stopTimeForShoot = false;

            transform.Find("Arm with Gun").gameObject.SetActive(false);
        }
    }

    private void SetUpShoot(Planet oldPlanet, Planet newPlanet) {
        //comment more
        stopTimeForShoot = true;
        shootTimeRemaining = stopTimeShootDuration;
        stopTimeOverlay.SetActive(true);

        //flip player sprite to face new planet
        Vector2 towardsOldPlanet = (oldPlanet.centerPoint - new Vector2(transform.position.x, transform.position.y)).normalized; //normalized, points toward old planet
        Vector2 perpTowardsOldPlanet = Vector2.Perpendicular(towardsOldPlanet);
        Vector2 newPlanetVector = (newPlanet.centerPoint - new Vector2(transform.position.x, transform.position.y)); //points to the new planet
        Vector3 proj = Vector3.Project(newPlanetVector, perpTowardsOldPlanet); //projection of the new planet vector onto the perpendicular one
        Vector2 p = proj.normalized; //will be either equal to or opposite the perpendicular vector

        int dir = 1; //by default, face the player clockwise
        if (p == -perpTowardsOldPlanet) { dir = -1; }//if planet is to the left, face player counterclockwise
        
        //apply new direction
        Vector3 scale = transform.localScale;
        scale.x = dir;
        transform.localScale = scale;

        //figure out total angle that gun needs to move
        //figure out how much time we have to move the gun
        //set all those as variables
        transform.Find("Arm with Gun").gameObject.SetActive(true);


        float a = (Mathf.Atan2(newPlanetVector.y, newPlanetVector.x) * Mathf.Rad2Deg) + 90; //in degrees
        if (a > 180) { a = 180 - (a - 180); }//turn all angles into under 180
        Quaternion rotation = Quaternion.AngleAxis(a, Vector3.forward);
        transform.Find("Arm with Gun").rotation = Quaternion.Slerp(transform.rotation, rotation, 1);
    }



    Vector2[] GetCorners(BoxCollider2D box) {
        //gets the corners in world space of the box collider
        //in order, goes top left, top right, bottom left, bottom right
        float width = box.size.x;
        float height = box.size.y;
        Vector2 tll = new Vector2(-width, height) * 0.5f;
        Vector2 trl = new Vector2(width, height) * 0.5f;
        Vector2 bll = new Vector2(-width, -height) * 0.5f;
        Vector2 brl = new Vector2(width, -height) * 0.5f;
        Vector2 tlw = box.transform.TransformPoint(tll); //top-left in world space
        Vector2 trw = box.transform.TransformPoint(trl); //top-right in world space
        Vector2 blw = box.transform.TransformPoint(bll); //bottom-left in world space
        Vector2 brw = box.transform.TransformPoint(brl); //bottom-right in world space
        Vector2[] result = new Vector2[4];
        result[0] = tlw;
        result[1] = trw;
        result[2] = blw;
        result[3] = brw;
        return result;
    }

    Vector2 FindIntersectionPoint(Vector2 planetCenter, float radiusOfPlayerSide, Vector2 boxTopCorner, Vector2 boxBottomCorner, Vector2 playerSidePoint) {
        //finds the point that the player's left or right edge should be set to when they collide with an obstacle while walking on a planet
        //specifically, draws a circle around the planet with a defined radius, and sees where that circle intersects a line that goes through boxTopConer and boxBottomCorner
        //this intersection will occur at two points. We want to take the point closer to boxTopCorner
        //equation of a circle: r^2 = (x - h)^2 + (y - k)^2, where h is planet_center.x, and k is planet_center.y, and r is the radius of the circle
        //equation of a line: y = mx+b, where m is the line's slope and b is the y-intercept

        //if x1 and y1 are the bottom corner of the box, and x2 and y2 are the top corner (it makes no difference either way)...
        float x1 = boxBottomCorner.x;
        float x2 = boxTopCorner.x;
        float y1 = boxBottomCorner.y;
        float y2 = boxTopCorner.y;
        //y2-y2 = (m(x2)+b) - (m(x1)+b), or y2 - y1 = m (x2 - x1), or m = (y2 - y1) / (x2 - x1)
        float m = (y2 - y1) / (x2 - x1); //slope of line
        //y1 = m(x1) + b, or b = y1 - m(x1)
        float b = y1 - (m * x1); //y-intercept of line

        //the intersection point on the equation of the circle is then defined as r^2 = (x - h)^2 + (y - k)^2 with y being equal to mx+b
        //which can also be written as r^2 = (x - h)^2 + ((mx+b) - k)^2
        //factoring this out gives r^2 = x^2 - 2xh + h^2 + m^2x^2 + 2mbx - 2mkx + (b-k)^2
        //if A = ((m^2) + 1), and B = 2m(b-k) - 2h, then r^2 = Ax^2 + Bx + (h^2 + (b - k)^2)
        //if C = (h^2 + (b - k)^2) - r^2, then Ax^2 + Bx + C = 0, where A, B, and C are all constants
        //this is a simple quadratic equation, where the solution is: x= (-B += sqrt(B^2 - 4AC)) / 2A
        float h = planetCenter.x;
        float k = planetCenter.y;
        float r = radiusOfPlayerSide;
        float A = (m * m) + 1f;
        float B = 2 * m * (b - k) - 2 * h;
        float C = (h * h) + (b - k) * (b - k) - (r * r);
        float sqrtPart = Mathf.Sqrt((B * B) - (4 * A * C));
        float solX1 = (sqrtPart - B) / (2 * A); //the x-component of the first intersection point
        float solX2 = (-sqrtPart - B) / (2 * A); //the x-component of the second intersection point
        float solY1 = (m * solX1) + b;
        float solY2 = (m * solX2) + b;
        Vector2 sol1 = new Vector2(solX1, solY1);
        Vector2 sol2 = new Vector2(solX2, solY2);

        //find which solution is closer to the player's side point - fine to do unless the player is traveling all the way around the planet in a single frame
        //this situation is already problematic because it means the player would not even be inside the boxcollider at all at that point, so none of this code would be running
        float dist1 = Vector2.Distance(sol1, playerSidePoint);
        float dist2 = Vector2.Distance(sol2, playerSidePoint);
        if (dist1 < dist2) {
            return sol1;
        }
        else {
            return sol2;
        }
    }

    void MovePlayerTouchingPoint(Vector2 point, bool rightSide) {
        //takes the right bound of the player's circleCollider and moves the player so that it touches point
        //if rightSide is false, then moves to fit the left circlecollider bound
        
        //Vector2 cent = GetComponent<CircleCollider2D>().bounds.center;
        float rad = GetComponent<CircleCollider2D>().radius * transform.lossyScale.x;
        Vector2 dist = perpTowardsPlanet * rad;
        Vector2 leftDist = -dist;
        Vector2 rightDist = dist;

        Vector2 leftPos = point + leftDist;
        Vector2 rightPos = point + rightDist;
        //Vector2 leftEdge = cent + (-perpTowardsPlanet * rad);
        //Vector2 rightEdge = cent + (perpTowardsPlanet * rad);
        if (!rightSide) {
            transform.position = rightPos;
        }
        else {
            transform.position = leftPos;
        }
        
    }


    BoxCollider2D CheckIfPlayerHitObstacleOnPlanet() {
        //if the player is on a planet, checks all of the children objects with the tag "Obstacle"
        //if the player's left or right collision points are inside the obstacle, return the BoxCollider2D component

        Vector2 cent = GetComponent<CircleCollider2D>().bounds.center;
        Vector2 leftDir = -perpTowardsPlanet;
        float rad = GetComponent<CircleCollider2D>().radius * transform.lossyScale.x;

        Vector2 leftEdge = cent + (leftDir * rad);
        Vector2 rightEdge = cent + (perpTowardsPlanet * rad);
        
        foreach(Transform child in planet.transform) {
            if (child.tag == "Obstacle") {
                //print("obstacle");
                BoxCollider2D ob = child.GetComponent<BoxCollider2D>();
                if (ob.OverlapPoint(leftEdge)) {
                    return ob;
                }
                else if (ob.OverlapPoint(rightEdge)) {
                    return ob;
                }
            }
        }
        return null;
    }

    void MovePlayerToEdgeOfObstacle(BoxCollider2D obstacle) {
        //moves the player so they are positioned directly next to obstacle
        //assumes the player is overlapping the edge of the obstacle on one of the player's sides
        //also assumes the player is not entirely inside the obstacle

        Vector2 cent = GetComponent<CircleCollider2D>().bounds.center;
        Vector2 leftDir = -perpTowardsPlanet;
        float rad = GetComponent<CircleCollider2D>().radius * transform.lossyScale.x;

        Vector2 leftEdge = cent + (leftDir * rad);
        Vector2 rightEdge = cent + (perpTowardsPlanet * rad);

        bool collidedOnLeftOfPlayer = false;
        bool collidedOnRightOfPlayer = false;
        if (obstacle.OverlapPoint(leftEdge)) {
            collidedOnLeftOfPlayer = true;
        }
        else if (obstacle.OverlapPoint(rightEdge)) {
            collidedOnRightOfPlayer = true;
        }
        if (collidedOnLeftOfPlayer || collidedOnRightOfPlayer) {
            //find height of the player's left/right edge from the center of the planet
            float playerEdgeRadius = Vector2.Distance(leftEdge, planet.centerPoint);
            //get corners of boxcollider next to player
            Vector2[] corners = GetCorners(obstacle); //[0] is top-left corner, [1] is top-right, [2] is bottom-left, [3] is bottom-right
            //assuming the boxcollider is oriented so the bottom is facing the planet...
            Vector2 topCorner;
            Vector2 bottomCorner;
            Vector2 touchedSide;
            
            if (!PlayerFacingDown()) {
                bool temp = collidedOnLeftOfPlayer;
                collidedOnLeftOfPlayer = collidedOnRightOfPlayer;
                collidedOnRightOfPlayer = temp;
            }

            if (collidedOnLeftOfPlayer) {
                //collided on player's left, meaning on the obstacle's right, assuming player is facing down towards planet
                topCorner = corners[1];
                bottomCorner = corners[3];
                touchedSide = leftEdge;
            }
            else {
                //collided on player's right, meaning on the obstacle's left, assuming player is facing down towards planet
                topCorner = corners[0];
                bottomCorner = corners[2];
                touchedSide = rightEdge;
            }

            Vector2 point = FindIntersectionPoint(planet.centerPoint, playerEdgeRadius, topCorner, bottomCorner, touchedSide);
            MovePlayerTouchingPoint(point, collidedOnRightOfPlayer);

        }
    }

    bool PlayerFacingDown() {
        //returns true if the bottom of the player is closer to the planet than the top of the player
        Vector2 pc = planet.centerPoint;
        Vector2[] points = GetPlayerCircleEdgePositions();
        Vector2 top = points[1];
        Vector2 bottom = points[0];

        float topDistX = top.x - pc.x;
        float topDistY = top.y - pc.y;
        float botDistX = bottom.x - pc.x;
        float botDistY = bottom.y - pc.y;
        float topDist = Mathf.Sqrt((topDistX * topDistX) + (topDistY * topDistY));
        float botDist = Mathf.Sqrt((botDistX * botDistX) + (botDistY * botDistY));
        return (botDist < topDist);

    }
    
    void CheckForCollisions() {
        //checks the 8 main directions, in local coordinates, around the player for collisions
        //top-left, top-mid, top-right, right-mid, bottom-right, bottom-mid, bottom-left, left-mid
        //specifically, any planets or obstacles are added to a list of collided objects

        //determine points to test
        Vector2[] playerEdges = GetPlayerCircleEdgePositions();

        //check all 8 positions for collisions with planet and obstacle colliders
        List<GameObject> collisions = new List<GameObject>();
        foreach (Vector2 position in playerEdges) {
            if (Physics2D.OverlapPoint(position) != null) {
                Collider2D[] overlaps = Physics2D.OverlapPointAll(position);
                foreach (Collider2D o in overlaps) {
                    if (o.gameObject.tag == "Planet" || o.gameObject.tag == "Obstacle") {
                        if (!collisions.Contains(o.gameObject)) {
                            collisions.Add(o.gameObject);
                        }
                    }
                }
            }
        }

        //if any of the collisions is a planet, land on that planet
        foreach (GameObject go in collisions) {
            if (go.tag == "Planet") {
                if (!go.GetComponent<Planet>().ignorePlayerContact && !isOnPlanet) {
                    LandOnPlanet(go.GetComponent<Planet>());
                    return;
                }
            }
        }
        //if any of the collisions is an obstacle, land on that obstacle
        foreach (GameObject go in collisions) {
            if (go.tag == "Obstacle") {
                //if (!hitObstacleSide && !hitObstacleTop && !go.transform.parent.GetComponent<Planet>().ignorePlayerContact) {
                if (!hitObstacleSide && !hitObstacleTop) {
                    if (!(fallingOffObstacle && obstacle == go.GetComponent<BoxCollider2D>())) {
                        go.transform.parent.GetComponent<Planet>().ignorePlayerContact = false;
                        HitObstacleInFall(go);
                    }
                }
                else if (hitObstacleSide && obstacle != go.GetComponent<BoxCollider2D>()) {
                    go.transform.parent.GetComponent<Planet>().ignorePlayerContact = false;
                    HitObstacleInFall(go);
                }
                return;
            }
        }

    }

    void HitObstacleInFall(GameObject obstacle) {
        //called by checkForCollisions, when the player collides with an obstacle mid-fall
        //they will land on the obstacle if their collision was with the top of the obstacle
        //they will fall towards the planet if their collision was with the side of the obstacle
        //specifically, if the point of contact is closer to the left or right edges of the obstacle, the player should fall
        //if the point of contact is closer to the top, the player should land on the obstacle

        //get collision point
        Vector2[] playerEdges = GetPlayerCircleEdgePositions();
        Vector2 collisionPoint = Vector2.zero;
        foreach(Vector2 point in playerEdges) {
            if (obstacle.GetComponent<BoxCollider2D>().OverlapPoint(point)) {
                collisionPoint = point;
            }
        }

        //get edges of obstacle boxCollider
        Vector2[] corners = GetCorners(obstacle.GetComponent<BoxCollider2D>()); //[0] is top-left corner, [1] is top-right, [2] is bottom-left, [3] is bottom-right

        float leftDistance = FindDistanceToLine(collisionPoint, corners[0], corners[2]);
        float topDistance = FindDistanceToLine(collisionPoint, corners[0], corners[1]);
        float rightDistance = FindDistanceToLine(collisionPoint, corners[1], corners[3]);

        if (leftDistance < topDistance && leftDistance < rightDistance) {
            HitObstacleOnSide(obstacle);
            print("hit on left side!");
        }
        else if (rightDistance < topDistance && rightDistance < leftDistance) {
            HitObstacleOnSide(obstacle);
            print("hit on right side!");
        }
        else{
            HitObstacleOnTop(obstacle);
            print("hit on top!");
        }


    }

    
    void HitObstacleOnSide(GameObject obstacle) {
        //makes the player hit the side of an obstacle
        hitObstacleSide = true;
        this.obstacle = obstacle.GetComponent<BoxCollider2D>();
        if (obstacle.transform.parent.GetComponent<Planet>() != planet) {
            stopTimeForRotation = true;
            stopTimeOverlay.SetActive(true);
            
            planet = obstacle.transform.parent.GetComponent<Planet>();
        }

        //keep them on the edge of the obstacle
        MovePlayerToEdgeOfObstacle(obstacle.GetComponent<BoxCollider2D>());
        
        //halt all momentum - probably change this soon!
        freeFallSpeed = 0f;
    }

    void HitObstacleOnTop(GameObject obstacle) {
        //makes the player land on top of an obstacle
        hitObstacleTop = true;
        this.obstacle = obstacle.GetComponent<BoxCollider2D>();

        if (obstacle.transform.parent.GetComponent<Planet>() != planet) {
            stopTimeForRotation = true;
            stopTimeOverlay.SetActive(true);

            planet = obstacle.transform.parent.GetComponent<Planet>();
        }

        SetSpeedOnPlanet();
        SetDirectionOnPlanet();
    }

    Vector2[] GetPlayerSquareEdgePositions() {
        //returns a list of 8 points around the player's center
        //if you draw a square around the player, there will be 4 points on the vertices of that square, and 4 points on the midpoints of the edges of the square
        Vector2 cent = GetComponent<CircleCollider2D>().bounds.center;
        Vector2 downDir = transform.up * -1;
        Vector2 upDir = transform.up;
        Vector2 leftDir = -perpTowardsPlanet;
        Vector2 rightDir = perpTowardsPlanet;

        Vector2 downAmt = downDir * GetComponent<CircleCollider2D>().radius * transform.lossyScale.x;
        Vector2 upAmt = upDir * GetComponent<CircleCollider2D>().radius * transform.lossyScale.x;
        Vector2 leftAmt = leftDir * GetComponent<CircleCollider2D>().radius * transform.lossyScale.x;
        Vector2 rightAmt = rightDir * GetComponent<CircleCollider2D>().radius * transform.lossyScale.x;

        Vector2 bottomPos = cent + downAmt;
        Vector2 topPos = cent + upAmt;
        Vector2 leftPos = cent + leftAmt;
        Vector2 rightPos = cent + rightAmt;
        Vector2 topLeftPos = cent + upAmt + leftAmt;
        Vector2 topRightPos = cent + upAmt + rightAmt;
        Vector2 bottomLeftPos = cent + downAmt + leftAmt;
        Vector2 bottomRightPos = cent + downAmt + rightAmt;

        Vector2[] positions = new Vector2[8];
        positions[0] = bottomPos;
        positions[1] = topPos;
        positions[2] = leftPos;
        positions[3] = rightPos;
        positions[4] = topLeftPos;
        positions[5] = topRightPos;
        positions[6] = bottomLeftPos;
        positions[7] = bottomRightPos;

        return positions;
    }

    Vector2[] GetPlayerCircleEdgePositions() {
        //returns a list of 8 points around the player's center
        //the points at 0, 30, 45, 60, 90, etc
        Vector2 cent = GetComponent<CircleCollider2D>().bounds.center;
        Vector2 downDir = transform.up * -1;
        Vector2 upDir = transform.up;
        Vector2 leftDir = -perpTowardsPlanet;
        Vector2 rightDir = perpTowardsPlanet;

        Vector2 downAmt = downDir * GetComponent<CircleCollider2D>().radius * transform.lossyScale.x;
        Vector2 upAmt = upDir * GetComponent<CircleCollider2D>().radius * transform.lossyScale.x;
        Vector2 leftAmt = leftDir * GetComponent<CircleCollider2D>().radius * transform.lossyScale.x;
        Vector2 rightAmt = rightDir * GetComponent<CircleCollider2D>().radius * transform.lossyScale.x;

        Vector2 s = cent + downAmt;
        Vector2 n = cent + upAmt;
        Vector2 w = cent + leftAmt;
        Vector2 e = cent + rightAmt;

        float r2o2 = Mathf.Sqrt(2) / 2; //root 2 /2, used for 45 deg angles
        Vector2 nw = cent + (upAmt + leftAmt) * r2o2;
        Vector2 ne = cent + (upAmt + rightAmt) * r2o2;
        Vector2 se = cent + (downAmt + leftAmt) * r2o2;
        Vector2 sw = cent + (downAmt + rightAmt) * r2o2;

        float r3o2 = Mathf.Sqrt(3) / 2; //root 3 /2, used for 
        float half = 1f / 2f;
        Vector2 nne = cent + (upAmt * r3o2) + (rightAmt * half);
        Vector2 ene = cent + (upAmt * half) + (rightAmt * r3o2);
        Vector2 nnw = cent + (upAmt * r3o2) + (leftAmt * half);
        Vector2 wnw = cent + (upAmt * half) + (leftAmt * r3o2);
        Vector2 sse = cent + (downAmt * r3o2) + (rightAmt * half);
        Vector2 ese = cent + (downAmt * half) + (rightAmt * r3o2);
        Vector2 ssw = cent + (downAmt * r3o2) + (leftAmt * half);
        Vector2 wsw = cent + (downAmt * half) + (leftAmt * r3o2);

        Vector2[] positions = new Vector2[16];
        positions[0] = s;
        positions[1] = n;
        positions[2] = w;
        positions[3] = e;
        positions[4] = ne;
        positions[5] = nw;
        positions[6] = se;
        positions[7] = sw;
        positions[8] = nne;
        positions[9] = ene;
        positions[10] = nnw;
        positions[11] = wnw;
        positions[12] = sse;
        positions[13] = ese;
        positions[14] = ssw;
        positions[15] = wsw;

        return positions;
    }


    private float FindDistanceToLine(Vector2 solitaryPoint, Vector2 lineEndpoint1, Vector2 lineEndpoint2) {
        //finds the distance from a point to a line
        //used in hitObstacleInFall
        //gets the shortest distance that can be drawn from the solitaryPoint to a line that connects lineEndpoint1 and lineEndpoint2
        //uses code found at http://csharphelper.com/blog/2016/09/find-the-shortest-distance-between-a-point-and-a-line-segment-in-c/
        //will return a point inside the line, or one of the endpoints

        float dx = lineEndpoint2.x - lineEndpoint1.x; //the x length of the line
        float dy = lineEndpoint2.y - lineEndpoint1.y; //the y length of the line
        float t = ((solitaryPoint.x - lineEndpoint1.x) * dx + (solitaryPoint.y - lineEndpoint1.y) * dy) / (dx * dx + dy * dy);
        Vector2 closest = new Vector2(lineEndpoint1.x + t * dx, lineEndpoint1.y + t * dy);

        //that is the closest point on the line, assuming the line is infinite
        //if the "closest" point is not on the line segment defined by the endpoints, then move it to one of the endpoints
        float x1c = closest.x - lineEndpoint1.x;
        float y1c = closest.y - lineEndpoint1.y;
        float x2c = closest.x - lineEndpoint2.x;
        float y2c = closest.y - lineEndpoint2.y;

        //calculate the distances between the 3 points: the "closest" point and the two line endpoints
        float lineLength = Mathf.Abs(Mathf.Sqrt((dx * dx) + (dy * dy)));
        float distFromEndpoint1 = Mathf.Abs(Mathf.Sqrt((x1c * x1c) + (y1c * y1c)));
        float distFromEndpoint2 = Mathf.Abs(Mathf.Sqrt((x2c * x2c) + (y2c * y2c)));
        
        //if the "closest" point is further than either endpoint
        if (distFromEndpoint1 > lineLength || distFromEndpoint2 > lineLength) {
            //find which endpoint the "closest" point is closer to
            //replace the "closest" point with that endpoint
            if (distFromEndpoint1 > distFromEndpoint2) {
                closest = lineEndpoint2;
            }
            else {
                closest = lineEndpoint1;
            }
        }
        
        //return the distance to the closest point
        dx = solitaryPoint.x - closest.x;
        dy = solitaryPoint.y - closest.y;
        return Mathf.Sqrt(dx * dx + dy * dy);
    }

    void MoveAroundObstacle(float distance) {
        //moves the player a certain distance around the top of an obstacle
        //the distance is not linear; it is an arc length
        //assumes motion clockwise. for motion counterclockwise, use a negative distance
        //same as MoveAroundPlanet, but uses the obstacle's radius from the center of the planet instead

        float planet_center_x = planet.centerPoint.x;
        float planet_center_y = planet.centerPoint.y;
        float x = transform.position.x - planet_center_x;
        float y = transform.position.y - planet_center_y;

        Vector2[] corners = GetCorners(obstacle);
        float rad = FindDistanceToLine(planet.centerPoint, corners[0], corners[1]); //finds the distance from the planet center to the top of the obstacle
        float r = rad + GetComponent<CircleCollider2D>().radius;
        float s = distance * -1;
        float theta = (s / r) + Mathf.Atan(y / x);
        if (x < 0) {
            theta += Mathf.PI;
        }
        float x2 = r * Mathf.Cos(theta);
        float y2 = r * Mathf.Sin(theta);
        float del_x = (x2 - x);
        float del_y = (y2 - y);
        Move(del_x, del_y);
        
        BoxCollider2D obstacleHit = CheckIfPlayerHitObstacleOnPlanet();
        if (obstacleHit != null) {
            MovePlayerToEdgeOfObstacle(obstacleHit);
            speedOnSurface = 0f;
        }
        
    }

    void CheckForFallingOffObstacle() {
        //checks the 8 main directions in a square around the player to see if they are still on top of their obstacle

        //determine points to test
        Vector2[] playerEdges = GetPlayerSquareEdgePositions();

        //check if any point on the player's edges is inside the obstacle
        bool isIn = false;
        foreach(Vector2 point in playerEdges) {
            if (obstacle.bounds.Contains(point)) {
                isIn = true;
            }
        }
        //if none is, make the player fall towards their planet
        if (!isIn) { FallOffObstacle(); }
    }

    void FallOffObstacle() {
        //run when the player walks off the edge of an obstacle.
        //they should start falling towards their planet
        hitObstacleTop = false;
        fallingOffObstacle = true;
        freeFallSpeed = Mathf.Abs(speedOnSurface);
        if (speedOnSurface != 0) {
            freeFallDirection = Vector2.Perpendicular(transform.up * -1).normalized;
            if (freeFallSpeed != speedOnSurface) {
                freeFallDirection *= -1f;
            }
        }
    }

    bool IsTooCloseToObstacle() {
        //returns true if the player is too close to an obstacle on their current planet to be able to select another planet

        float minScalar = 1.2f;
        foreach(Transform t in planet.transform) {
            if (t.gameObject.tag == "Obstacle") {
                BoxCollider2D coll = t.GetComponent<BoxCollider2D>();
                Vector2[] corners = GetCorners(coll); //in order, goes top left, top right, bottom left, bottom right
                float dist1 = FindDistanceToLine(transform.position, corners[0], corners[2]); //distance to the obstacle's left edge
                float dist2 = FindDistanceToLine(transform.position, corners[1], corners[3]); //distance to the obstacle's right edge

                if ((dist1 < GetComponent<CircleCollider2D>().radius * minScalar) || (dist2 < GetComponent<CircleCollider2D>().radius * minScalar)) {
                    return true;
                }
            }
        }
        return false;
    }
}
