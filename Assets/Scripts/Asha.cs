using UnityEngine;

[RequireComponent(typeof(EnemyController2D))]
public class Asha : MonoBehaviour
{

    EnemyController2D enemyController;
    Animator anim;
    public Sprite exclamationPointSprite;

    float gravity = -6f;
    Vector2 velocity;
    
    public GameObject leftEye;
    public EyeBoundedArea leftEyeBounds;
    public GameObject rightEye;
    public EyeBoundedArea rightEyeBounds;

    public float patrolZoneWidth;
    public float patrolZoneHeight;
    public Vector2 patrolZoneCenter;
    public bool showPatrolZone = true;

    enum Mode { idle, wandering, alert, attackInching, attackLeaping };
    Mode aiMode;
    float timerLength;
    float timer;
    int inchLoops;
    int inchLoopsCounter;
    float chaseTimer;





    void Start()
    {
        enemyController = GetComponent<EnemyController2D>();
        anim = GetComponent<Animator>();
        leftEyeBounds = new EyeBoundedArea(leftEye);
        rightEyeBounds = new EyeBoundedArea(rightEye);
        SetAIMode(Mode.idle);
    }

    void Update()
    {
        
        BasicPhysics();
        HandleAI();
        enemyController.Move(velocity * Time.deltaTime);

        rightEyeBounds.Update();
        leftEyeBounds.Update();

        if (enemyController.collisions.left || enemyController.collisions.right)
            velocity.x = 0;
        if (enemyController.collisions.above || enemyController.collisions.below)
            velocity.y = 0;

    }



    public void BasicPhysics()
    {
        velocity.y += gravity * Time.deltaTime;
    }

    public void HandleAI()
    {

        //Check if player has entered danger zone
        if (enemyController.GetDistToPlayer() <= 3.0f && aiMode != Mode.alert && aiMode != Mode.attackInching && aiMode != Mode.attackLeaping)
            SetAIMode(Mode.alert);

        switch (aiMode)
        {

            case Mode.idle:
                timer += Time.deltaTime;
                if (timer >= timerLength)
                    SetAIMode(Mode.wandering);
                break;

            case Mode.wandering:

                //If we are in the asha inch forward animation
                if (anim.GetCurrentAnimatorStateInfo(0).IsName("Asha Inch Forward"))
                {
                    //If the animation is still playing, make the Asha move
                    if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1)
                    {
                        if (GetComponent<SpriteRenderer>().flipX == true) velocity.x = 0.8f;
                        else velocity.x = -0.8f;
                    }

                    //If the animation has finished, check if we should loop the animation, or move back to idle AI
                    else
                    {
                        //If the Asha has not completed their number of inch forward loops, increase the counter and start their pause period
                        if (inchLoopsCounter < inchLoops)
                        {
                            inchLoopsCounter++;
                            timer = 0;
                            anim.Play("Asha Idle");
                            velocity.x = 0;
                        }

                        //If the Asha is done inching forward, set their AI mode to idle
                        else
                        {
                            SetAIMode(Mode.idle);
                        }
                    }
                }
                else
                {
                    //If the Asha is done pausing, start the inching forward animation
                    if (timer >= timerLength)
                    {
                        //First, check if we left the patrol area bounds
                        if (transform.position.x < patrolZoneCenter.x - (patrolZoneWidth / 2))
                            GetComponent<SpriteRenderer>().flipX = true;
                        else if (transform.position.x > patrolZoneCenter.x + (patrolZoneWidth / 2))
                            GetComponent<SpriteRenderer>().flipX = false;

                        //If we are not in the animation, put us in the animation
                        if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Asha Inch Forward"))
                            anim.Play("Asha Inch Forward", -1, 0f);
                    }
                    //Else if the Asha is still pausing, keep them pausing
                    else
                        timer += Time.deltaTime;
                }

                break;

            case Mode.alert:
                GetComponent<SpriteRenderer>().flipX = (enemyController.player.transform.position.x > transform.position.x) ? true : false;

                timer += Time.deltaTime;
                if (timer >= timerLength || enemyController.GetDistToPlayer() <= 2.0f)
                {
                    if (enemyController.GetDistToPlayer() <= 3.0f)
                    {
                        //print("Start inching closer to the player");
                        SetAIMode(Mode.attackInching);
                    }
                    else
                    {
                        //print("Go back to wandering...");
                        SetAIMode(Mode.wandering);
                    }
                }
                break;

            case Mode.attackInching:
                //print(chaseTimer);
                GetComponent<SpriteRenderer>().flipX = (enemyController.player.transform.position.x > transform.position.x) ? true : false;

                //If the Asha is close enough to the player, make them leap at the player to attack
                if (enemyController.GetDistToPlayer() <= 2.0f)
                    SetAIMode(Mode.attackLeaping);

                //Else, move on to interest-checking portion of code
                else
                {
                    //Make the Asha retain interest as long as they're close enough to the player
                    if (enemyController.GetDistToPlayer() <= 3.0f)
                        chaseTimer = 0;
                    else
                        chaseTimer += Time.deltaTime;

                    //If the Asha loses interest in the player, put them back into idle mode
                    if (chaseTimer >= 5.0f)
                        SetAIMode(Mode.idle);

                    //Else, do their move cycle
                    else
                    {
                        if (anim.GetCurrentAnimatorStateInfo(0).IsName("Asha Inch Forward"))
                        {
                            if (anim.GetCurrentAnimatorStateInfo(0).normalizedTime < 1)
                            {
                                if (GetComponent<SpriteRenderer>().flipX == true) velocity.x = 1.0f;
                                else velocity.x = -1.0f;
                            }
                            else
                            {
                                timer = 0;
                                anim.Play("Asha Idle");
                                velocity.x = 0;
                            }
                        }
                        else
                        {
                            if (timer >= timerLength)
                            {
                                if (!anim.GetCurrentAnimatorStateInfo(0).IsName("Asha Inch Forward"))
                                    anim.Play("Asha Inch Forward", -1, 0f);
                            }
                            else
                                timer += Time.deltaTime;
                        }
                    }
                }
                break;

            case Mode.attackLeaping:
                if (enemyController.collisions.below)
                {
                    if (enemyController.GetDistToPlayer() <= 5.0f)
                    {
                        SetAIMode(Mode.attackInching);
                    }
                    else
                    {
                        SetAIMode(Mode.idle);
                    }
                }
                break;

        }

    }

    void SetAIMode(Mode aiModeToSet)
    {
        timer = 0;
        timerLength = 0;
        velocity.x = 0;
        velocity.y = 0;

        switch (aiModeToSet)
        {
            case Mode.idle:
                aiMode = Mode.idle;
                anim.Play("Asha Idle");

                timerLength = Random.Range(1.0f, 6.0f); //Set how long the Asha will be idling before starting to wander
                break;

            case Mode.wandering:
                aiMode = Mode.wandering;
                anim.Play("Asha Inch Forward");

                timerLength = 0.15f;    //Set how long the Asha will pause between each inch loop
                inchLoops = Random.Range(4, 6);
                inchLoopsCounter = 0;
                if (Random.Range(0, 10) >= 5) GetComponent<SpriteRenderer>().flipX = !GetComponent<SpriteRenderer>().flipX;
                break;

            case Mode.alert:
                aiMode = Mode.alert;
                anim.Play("Asha Idle");

                timerLength = 2.0f; //Set how long the Asha will remain alert until attacking/losing interest and wandering
                velocity.y = 1.5f;  //Pop them up, like they got startled a lil bit
                GetComponent<SpriteRenderer>().flipX = (enemyController.player.transform.position.x > transform.position.x) ? true : false;
                GameObject exclamationPointObject = GenericObject.CreateObject(new Vector3(transform.position.x, transform.position.y + 0.4f, 0), exclamationPointSprite, 0.5f);
                break;

            case Mode.attackInching:
                aiMode = Mode.attackInching;
                anim.Play("Asha Inch Forward");

                timerLength = 0.1f;    //Set how long the Asha will pause Between each inch loop
                chaseTimer = 0;
                GetComponent<SpriteRenderer>().flipX = (enemyController.player.transform.position.x > transform.position.x) ? true : false;
                break;

            case Mode.attackLeaping:
                aiMode = Mode.attackLeaping;
                anim.Play("Asha Attacking");

                velocity.x = (enemyController.player.transform.position.x > transform.position.x) ? 3.0f : -3.0f;   //Throw the Asha towards the player horizontally
                velocity.y = 2.0f;    //Lift the Asha slightly off the ground

                GetComponent<SpriteRenderer>().flipX = (enemyController.player.transform.position.x > transform.position.x) ? true : false;
                break;
        }
    }



    public void OnDrawGizmos()
    {
        if (showPatrolZone)
        {
            Gizmos.color = new Color(0, 0, 1, .25f);
            Gizmos.DrawCube(patrolZoneCenter, new Vector2(patrolZoneWidth, patrolZoneHeight));
        }
    }
    public void CenterPatrolZone()
    {
        patrolZoneCenter = transform.position;
    }

}

[System.Serializable]
public class EyeBoundedArea
{

    GameObject eyeObject;
    public float boundsHalfWidth;
    public float boundsHalfHeight;
    public float eyeSize;
    Vector2 localCenter;    //Local center of the bounds (is retained even when the Asha object moves)
    float eyeMoveTimer;

    public EyeBoundedArea(GameObject eyeObject)
    {

        this.eyeObject = eyeObject;
        boundsHalfWidth = 0.02f;
        boundsHalfHeight = 0.03f;
        eyeSize = eyeObject.GetComponent<SpriteRenderer>().sprite.rect.size.x / 64f;
        localCenter = eyeObject.transform.localPosition;
        eyeMoveTimer = Random.Range(1.0f, 4.0f);

    }

    public void Update()
    {

        //Randomly decide to shift eye position
        eyeMoveTimer -= Time.deltaTime;
        if (eyeMoveTimer <= 0)
        {
            eyeMoveTimer = Random.Range(1.0f, 4.0f);
            LeanTween.moveLocal(eyeObject, new Vector2(Random.Range(localCenter.x - boundsHalfWidth, localCenter.x + boundsHalfWidth), Random.Range(localCenter.y + boundsHalfHeight, localCenter.y - boundsHalfHeight)), 0.5f).setEase(LeanTweenType.easeOutCubic);
        }

        //If the eye object leaves the bounds, put it back within the bounds
        float shiftX = 0;
        if (eyeObject.transform.localPosition.x < (localCenter.x - boundsHalfWidth))
            shiftX = (localCenter.x - boundsHalfWidth) - eyeObject.transform.localPosition.x;
        else if (eyeObject.transform.localPosition.x > (localCenter.x + boundsHalfWidth))
            shiftX = (localCenter.x + boundsHalfWidth) - eyeObject.transform.localPosition.x;

        float shiftY = 0;
        if (eyeObject.transform.localPosition.y < (localCenter.y - boundsHalfHeight))
            shiftY = (localCenter.y - boundsHalfHeight) - eyeObject.transform.localPosition.y;
        else if (eyeObject.transform.localPosition.y > (localCenter.y + boundsHalfHeight))
            shiftY = (localCenter.y + boundsHalfHeight) - eyeObject.transform.localPosition.y;

        eyeObject.transform.Translate(new Vector2(shiftX, shiftY));

    }

}