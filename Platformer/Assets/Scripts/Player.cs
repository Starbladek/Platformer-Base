using UnityEngine;

[RequireComponent(typeof(Controller2D))]
[RequireComponent(typeof(PlayerInput))]
public class Player : MonoBehaviour
{

    public float moveSpeed = 4f;
    public float maxJumpHeight = 2.2f;
    public float minJumpHeight = 0.5f;
    public float timeToJumpApex = 0.5f;
    float accelerationTimeGrounded = 0.05f;
    float accelerationTimeAirborne = 0.2f;

    bool crouching;
    bool sprinting;

    float gravity;
    float maxJumpVelocity;
    float minJumpVelocity;
    [HideInInspector]
    public float graceJumpTimer;
    bool checkingForGraceJump;
    [HideInInspector]
    public bool canCheckForGraceJump;

    Vector2 velocity;
    float velocityXSmoothing;

    [HideInInspector]
    public Controller2D controller;
    [HideInInspector]
    public Vector2 directionalInput;
    
    public Camera mainCamera;
    public Menu pauseMenuRef;
    Menu pauseMenu;

    Vector3 tempSpawnPos;



    void Start()
    {
        controller = GetComponent<Controller2D>();  //Get access to the Controller2D script
        gravity = -(2 * maxJumpHeight) / Mathf.Pow(timeToJumpApex, 2);
        maxJumpVelocity = Mathf.Abs(gravity) * timeToJumpApex;
        minJumpVelocity = Mathf.Sqrt(2 * Mathf.Abs(gravity) * minJumpHeight);

        tempSpawnPos = new Vector3(transform.position.x, transform.position.y, transform.position.z);
        Camera cameraClone = Instantiate(mainCamera, new Vector3(transform.position.x, transform.position.y, -20), Quaternion.identity) as Camera;  //Create camera for player
    }



    void Update()
    {

        //Check for pause
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pauseMenu == null) pauseMenu = Instantiate(pauseMenuRef);
            else Destroy(pauseMenu);
        }

        //Check for reset
        if (Input.GetKeyDown(KeyCode.R)) transform.position = tempSpawnPos;

        CalculateVelocity();
        controller.Move(velocity * Time.deltaTime, directionalInput);

        //Constantly set the player's y velocity to 0 if they are hitting the floor or ceiling, so that they don't constantly build velocity forever
        if (controller.collisions.above || controller.collisions.below || controller.collisions.hangingOnLedge)
        {
            if (controller.collisions.slidingDownMaxSlope)
            {
                velocity.y += controller.collisions.slopeNormal.y * -gravity * Time.deltaTime;
            }
            else
            {
                velocity.y = 0;
                canCheckForGraceJump = true;
                graceJumpTimer = 0;
            }
        }

        //Grace Jump stuff
        if (!controller.collisions.below && canCheckForGraceJump && !controller.collisions.hangingOnLedge)
        {
            checkingForGraceJump = true;
            canCheckForGraceJump = false;
            graceJumpTimer = 0.1f;
        }
        if (checkingForGraceJump)
        {
            graceJumpTimer -= Time.deltaTime;
            if (graceJumpTimer <= 0)
            {
                graceJumpTimer = 0;
                checkingForGraceJump = false;
            }
        }

        //Constantly set the player's x velocity to 0 if they are colliding with something from the left or the right, so they can't build velocity in either direction and be impeded when trying to turn
        if (controller.collisions.left || controller.collisions.right || controller.collisions.hangingOnLedge)
        {
            if (Input.GetKeyDown(KeyCode.Space) != true)
            {
                velocity.x = 0;
            }
        }

        //Constantly set the player's x velocity to 0 if they are climbing a slope and colliding with a ceiling
        if (controller.collisions.above && controller.collisions.climbingSlope)
        {
            velocity.x = 0;
        }
    }

    void CalculateVelocity()
    {
        float targetVelocityX = directionalInput.x * moveSpeed;
        if (sprinting) targetVelocityX *= 1.5f;
        if (crouching) targetVelocityX *= 0.1f;

        velocity.x = Mathf.SmoothDamp(velocity.x, targetVelocityX, ref velocityXSmoothing, (controller.collisions.below) ? accelerationTimeGrounded : accelerationTimeAirborne);
        velocity.y += gravity * Time.deltaTime;
    }



    //Player Input stuff

    public void SetDirectionalInput(Vector2 input)
    {
        directionalInput = input;
    }

    public void OnJumpInputDown()
    {
        //If hanging on ledge, do special checks
        if (controller.collisions.hangingOnLedge)
        {
            //If holding down on ledge, make them drop
            if (directionalInput.y == -1)
            {
                controller.collisions.hangingOnLedge = false;
                controller.collider.offset = (new Vector2(0, 0));   //Fix collider offset
            }
            //If holding away from ledge, make them leap away
            else if (controller.collisions.faceDir != directionalInput.x && directionalInput.x != 0)
            {
                controller.collisions.hangingOnLedge = false;
                velocity.y = maxJumpVelocity;
                controller.collider.offset = (new Vector2(0, 0));   //Fix collider offset
            }
            //Else, make them climb
            else
            {
                controller.collisions.hangingOnLedge = false;
                velocity.y = maxJumpVelocity;
                controller.collider.offset = (new Vector2(0, 0));   //Fix collider offset
            }
            
            //If player is pressing the direction away from the ledge, increase their velocity so they push off
            if (controller.collisions.faceDir == -1)
            {
                if (directionalInput.x == 1) velocity.x = 5;
            }
            else if (controller.collisions.faceDir == 1)
            {
                if (directionalInput.x == -1) velocity.x = -5;
            }

            canCheckForGraceJump = false;
        }

        //Else, do normal checks
        else
        {
            if (controller.collisions.below)
            {
                if (controller.collisions.slidingDownMaxSlope)
                {
                    if (directionalInput.x != -Mathf.Sign(controller.collisions.slopeNormal.x)) //not jumping against max slope
                    {
                        velocity.y = maxJumpVelocity * controller.collisions.slopeNormal.y;
                        velocity.x = maxJumpVelocity * controller.collisions.slopeNormal.x;
                    }
                }
                else
                {
                    velocity.y = maxJumpVelocity;
                    canCheckForGraceJump = false;
                }
            }

            //If we're not on the ground but we're currently checking for a grace jump, do the jump
            else
            {
                if (checkingForGraceJump)
                {
                    velocity.y = maxJumpVelocity;
                    checkingForGraceJump = false;
                    graceJumpTimer = 0;
                }
            }
        }
    }
    public void OnJumpInputUp()
    {
        if (velocity.y > minJumpVelocity)
        {
            velocity.y = minJumpVelocity;
        }
    }
    public void OnCrouchInputDown()
    {
        if (controller.collisions.below == true)
        {
            crouching = true;
        }
    }
    public void OnCrouchInputUp()
    {
        if (crouching == true)
        {
            crouching = false;
        }
    }
    public void OnSprintInputDown()
    {
        sprinting = true;
    }
    public void OnSprintInputUp()
    {
        sprinting = false;
    }

}