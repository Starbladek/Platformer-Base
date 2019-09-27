using UnityEngine;

public class Controller2D : RaycastController
{
    public float maxSlopeAngle = 50f;

    public CollisionInfo collisions;
    [HideInInspector]
    public Vector2 playerInput;

    float ledgeCooldownTimer = 0;

    public override void Start()
    {
        base.Start();
        collisions.faceDir = 1;
        MoveToFloor(new Vector2(collider.bounds.min.x + (collider.bounds.size.x / 2), collider.bounds.min.y), 0);
    }

    public void Move(Vector2 moveAmount, bool standingOnPlatform)
    {
        Move(moveAmount, Vector2.zero, standingOnPlatform);
    }

    public void Move(Vector2 moveAmount, Vector2 input, bool standingOnPlatform = false)
    {
        UpdateRaycastOrigins();

        collisions.Reset();
        collisions.moveAmountOld = moveAmount;
        playerInput = input;

        if (moveAmount.y < 0) DescendSlope(ref moveAmount); //If we are moving down, check to descend slope
        if (moveAmount.x != 0 && collisions.hangingOnLedge == false) collisions.faceDir = (int)Mathf.Sign(moveAmount.x);
        HorizontalCollisions(ref moveAmount);
        if (moveAmount.y != 0) VerticalCollisions(ref moveAmount);

        if (collisions.hangingOnLedge) return;  //If we're on a ledge, don't apply the calculated movement

        transform.Translate(moveAmount);

        //Miscellaneous stuff
        if (standingOnPlatform) collisions.below = true;

        if (ledgeCooldownTimer > 0) ledgeCooldownTimer -= Time.deltaTime;
        else ledgeCooldownTimer = 0;
    }

    void HorizontalCollisions(ref Vector2 moveAmount)
    {
        float directionX = collisions.faceDir;
        float rayLength = Mathf.Abs(moveAmount.x) + skinWidth;

        if (Mathf.Abs(moveAmount.x) < skinWidth)
            rayLength = 2 * skinWidth;

        //Ledge stuff
        float ledgeHangPosY = 0;    //Used to set the player's Y position to the ledge's Y position
        float ledgeHangPosX = 0;    //Used to set the player's X position to the ledge's X position

        for (int i = 0; i < horizontalRayCount; i++)
        {
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength * 3, Color.red);

            if (hit)
            {
                //If the thing we hit is a through platform OR we are literally inside of what we are hitting, skip this ray
                if (hit.collider.tag == "Through" || hit.distance == 0)
                    continue;

                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

                //If this is the bottomLeft/bottomRight ray AND we are not hitting a steep slope, climb the slope
                if (i == 0 && slopeAngle <= maxSlopeAngle)
                {
                    //Handle suddenly climbing a slope immediately after having been descending a slope
                    if (collisions.descendingSlope)
                    {
                        collisions.descendingSlope = false;
                        moveAmount = collisions.moveAmountOld;
                    }

                    //Handle snapping the player to the slope in order to start climbing
                    float distanceToSlopeStart = 0;
                    if (slopeAngle != collisions.slopeAngleOld)
                    {
                        distanceToSlopeStart = hit.distance - skinWidth;
                        moveAmount.x -= distanceToSlopeStart * directionX;
                    }

                    ClimbSlope(ref moveAmount, slopeAngle, hit.normal);
                    moveAmount.x += distanceToSlopeStart * directionX;
                }

                //If we are not climbing slope OR we are hitting a steep slope/wall, detect wall collisions as normal
                if (!collisions.climbingSlope || slopeAngle > maxSlopeAngle)
                {
                    moveAmount.x = (hit.distance - skinWidth) * directionX;
                    rayLength = hit.distance;

                    //Handle colliding with wall while climbing slope
                    if (collisions.climbingSlope)
                        moveAmount.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(moveAmount.x);

                    //Ledge thing
                    if (hit.collider.tag == "Ledge")
                        collisions.numLedgeRayHits++;

                    collisions.left = directionX == -1;
                    collisions.right = directionX == 1;
                }

                if (hit.collider.tag == "Ledge" && ledgeCooldownTimer <= 0)
                {
                    ledgeHangPosX = (collisions.left) ? (hit.collider.bounds.max.x + 0.5f) : (hit.collider.bounds.min.x - 0.5f);  //If the player is colliding the right side of the ledge, set them to the right of it, else set them to the left of it
                    ledgeHangPosY = hit.collider.bounds.max.y - 0.5f;
                }
            }
        }

        if (collisions.hangingOnLedge == false && ledgeCooldownTimer <= 0 && collisions.numLedgeRayHits >= horizontalRayCount * 0.9f)
            SnapToLedge(ledgeHangPosX, ledgeHangPosY);
    }

    void VerticalCollisions(ref Vector2 moveAmount)
    {
        float directionY = Mathf.Sign(moveAmount.y);
        float rayLength = Mathf.Abs(moveAmount.y) + skinWidth;

        for (int i = 0; i < verticalRayCount; i++)
        {

            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;
            rayOrigin += Vector2.right * (verticalRaySpacing * i + moveAmount.x);
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask);

            Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength * 3, Color.red);

            if (hit)
            {
                //Handle Through platform stuff
                if (hit.collider.tag == "Through")
                {
                    //If we're traveling up through a Through platform, skip this ray
                    if (directionY == 1 || hit.distance == 0)
                        continue;
                    //If we're falling through a Through platform, skip this ray
                    if (collisions.fallingThroughPlatform)
                        continue;
                    //If we're on a Through platform and the player presses DOWN and SPACE, make them fall through
                    if (playerInput.y == -1 && Input.GetKeyDown(KeyCode.Space))
                    {
                        collisions.fallingThroughPlatform = true;
                        GetComponent<Player>().canCheckForGraceJump = false;
                        Invoke("ResetFallingThroughPlatform", .1f);
                        continue;
                    }
                }

                moveAmount.y = (hit.distance - skinWidth) * directionY;
                rayLength = hit.distance;   //POTENTIALLY REMOVE THIS

                //Handle hitting a ceiling while moving up a slope
                if (collisions.climbingSlope)
                    moveAmount.x = moveAmount.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(moveAmount.x);

                collisions.below = directionY == -1;
                collisions.above = directionY == 1;
            }
        }

        //Handle changes in angle when climbing slope
        if (collisions.climbingSlope)
        {
            float directionX = Mathf.Sign(moveAmount.x);
            rayLength = Mathf.Abs(moveAmount.x) + skinWidth;
            Vector2 rayOrigin = ((directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) + Vector2.up * moveAmount.y;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);
            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (slopeAngle != collisions.slopeAngle)
                {
                    moveAmount.x = (hit.distance - skinWidth) * directionX;
                    collisions.slopeAngle = slopeAngle;
                    collisions.slopeNormal = hit.normal;
                }
            }
        }
    }

    void ClimbSlope(ref Vector2 moveAmount, float slopeAngle, Vector2 slopeNormal)
    {
        float moveDistance = Mathf.Abs(moveAmount.x);
        float climbmoveAmountY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

        //If our moveAmount is less than the climbing moveAmount (this is true as long as the player doesn't jump on the slope)
        if (moveAmount.y <= climbmoveAmountY)
        {
            moveAmount.y = climbmoveAmountY;
            moveAmount.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(moveAmount.x);
            collisions.below = true;
            collisions.climbingSlope = true;
            collisions.slopeAngle = slopeAngle;
            collisions.slopeNormal = slopeNormal;
        }
        else
        {
            collisions.climbingSlope = true;
            collisions.slopeAngle = slopeAngle;
            collisions.slopeNormal = slopeNormal;
            //print("This printed because a motherfucker jumped while climbing a slope");
        }
    }

    void DescendSlope(ref Vector2 moveAmount)
    {

        RaycastHit2D maxSlopeHitLeft = Physics2D.Raycast(raycastOrigins.bottomLeft, Vector2.down, Mathf.Abs(moveAmount.y) + skinWidth, collisionMask);
        RaycastHit2D maxSlopeHitRight = Physics2D.Raycast(raycastOrigins.bottomRight, Vector2.down, Mathf.Abs(moveAmount.y) + skinWidth, collisionMask);
        if (maxSlopeHitLeft ^ maxSlopeHitRight)
        {
            SlideDownMaxSlope(maxSlopeHitLeft, ref moveAmount);
            SlideDownMaxSlope(maxSlopeHitRight, ref moveAmount);
        }

        //If we are not sliding down any steep slopes, descend them as usual
        if (!collisions.slidingDownMaxSlope)
        {
            float directionX = Mathf.Sign(moveAmount.x);
            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);

            //If our bottomRight/bottomLeft raycast hit something
            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

                //If we are not on a flat surface AND we are not on a steep slope
                if (slopeAngle != 0 && slopeAngle <= maxSlopeAngle)
                {
                    //If the normal of the slope is equal to the direction we are moving
                    if (Mathf.Sign(hit.normal.x) == directionX)
                    {
                        //If the distance to the slope is less than (???), snap the player to the slope
                        if (hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(moveAmount.x))
                        {
                            float moveDistance = Mathf.Abs(moveAmount.x);
                            float descendVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

                            moveAmount.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(moveAmount.x);
                            moveAmount.y -= descendVelocityY;

                            collisions.slopeAngle = slopeAngle;
                            collisions.descendingSlope = true;
                            collisions.below = true;
                            collisions.slopeNormal = hit.normal;
                        }
                    }
                }
            }
        }
    }

    void SlideDownMaxSlope(RaycastHit2D hit, ref Vector2 moveAmount)
    {
        if (hit)
        {
            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
            if (slopeAngle > maxSlopeAngle)
            {
                moveAmount.x = Mathf.Sign(hit.normal.x) * (Mathf.Abs(moveAmount.y) - hit.distance) / Mathf.Tan(slopeAngle * Mathf.Deg2Rad);
                collisions.slopeAngle = slopeAngle;
                collisions.slidingDownMaxSlope = true;
                collisions.slopeNormal = hit.normal;
            }
        }
    }

    void MoveToFloor(Vector2 rayOrigin, float xOffset)
    {
        RaycastHit2D findFloor = Physics2D.Raycast(rayOrigin, Vector2.down, Mathf.Infinity, collisionMask);
        float distanceToFloor = -(findFloor.distance - skinWidth);
        transform.Translate(new Vector3(xOffset, distanceToFloor, 0));
        collisions.below = true;
    }

    void SnapToLedge(float ledgeHangPosX, float ledgeHangPosY)
    {
        collisions.hangingOnLedge = true;
        transform.position = new Vector3(ledgeHangPosX, ledgeHangPosY, transform.position.z);
        ledgeCooldownTimer = 0.25f;
    }

    void ResetFallingThroughPlatform()
    {
        collisions.fallingThroughPlatform = false;
    }

    void OnGUI()
    {
        GUI.Label(new Rect(Screen.width * 0.75f, 10, 300, 30), "Collisions");
        GUI.Label(new Rect(Screen.width * 0.75f, 30, 300, 30), "Above: " + collisions.above);
        GUI.Label(new Rect(Screen.width * 0.75f, 45, 300, 30), "Below: " + collisions.below);
        GUI.Label(new Rect(Screen.width * 0.75f, 60, 300, 30), "Left: " + collisions.left);
        GUI.Label(new Rect(Screen.width * 0.75f, 75, 300, 30), "Right: " + collisions.right);
        GUI.Label(new Rect(Screen.width * 0.75f, 95, 300, 30), "Climbing Slope: " + collisions.climbingSlope);
        GUI.Label(new Rect(Screen.width * 0.75f, 110, 300, 30), "Descending Slope: " + collisions.descendingSlope);
        GUI.Label(new Rect(Screen.width * 0.75f, 125, 300, 30), "Slope angle: " + collisions.slopeAngle.ToString());
        GUI.Label(new Rect(Screen.width * 0.75f, 150, 300, 30), "Ledge timer: " + ledgeCooldownTimer.ToString());
        GUI.Label(new Rect(Screen.width * 0.75f, 165, 300, 30), "Grace Jump timer: " + GetComponent<Player>().graceJumpTimer.ToString());
    }

    public struct CollisionInfo
    {
        public bool above, below;
        public bool left, right;

        public bool climbingSlope;
        public bool descendingSlope;
        public bool slidingDownMaxSlope;

        public float slopeAngle, slopeAngleOld;
        public Vector2 slopeNormal;
        public Vector2 moveAmountOld;
        public int faceDir;
        public bool fallingThroughPlatform;

        public float numLedgeRayHits;
        public bool hangingOnLedge;

        public void Reset()
        {
            above = below = false;
            left = right = false;

            climbingSlope = false;
            descendingSlope = false;
            slidingDownMaxSlope = false;

            slopeNormal = Vector2.zero;
            slopeAngleOld = slopeAngle;
            slopeAngle = 0;

            numLedgeRayHits = 0;
        }
    }
}
