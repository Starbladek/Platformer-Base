using UnityEngine;
using System.Collections;

public class EnemyController2D : RaycastController
{

    public Player player;
    public CollisionInfo collisions;



    public override void Start()
    {
        base.Start();
        MoveToFloor(new Vector2(collider.bounds.min.x + (collider.bounds.size.x / 2), collider.bounds.min.y), 0);
    }

    public void Move(Vector2 velocity)
    {

        UpdateRaycastOrigins();

        collisions.Reset();
        collisions.velocityOld = velocity;

        if (velocity.x != 0)
            collisions.faceDir = (int)Mathf.Sign(velocity.x);

        if (velocity.y < 0) DescendSlope(ref velocity);
        HorizontalCollisions(ref velocity);
        VerticalCollisions(ref velocity);

        transform.Translate(velocity);

    }







    void HorizontalCollisions(ref Vector2 velocity)
    {

        float directionX = collisions.faceDir;  //Set directionX to -1 if velocity is negative, or 1 if velocity is zero or positive
        float rayLength = Mathf.Abs(velocity.x) + skinWidth;    //How far the rays extend out is determined by how fast the player is moving

        if (Mathf.Abs(velocity.x) < skinWidth)
            rayLength = 2 * skinWidth;

        //Loop through every ray
        for (int i = 0; i < horizontalRayCount; i++)
        {

            Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight;    //If we're moving left, put the rays on the left side, else put them on the right side
            rayOrigin += Vector2.up * (horizontalRaySpacing * i);   //Move the ray up depending on which number ray we're on
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);  //Actually send the ray out to detect if anything on the collisionMask layer was detected (in this case, the collisionMask layer is set to Obstacles)

            Debug.DrawRay(rayOrigin, Vector2.right * directionX * rayLength * 3, Color.red);    //Draw the ray

            //If the ray detected an Obstacle
            if (hit)
            {

                //If we are literally inside of a wall, just move on to the next ray and effectively stop doing horizontal collisions
                if (hit.distance == 0)
                    continue;

                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);   //Get the angle of what we are hitting (for example, if we are running into a wall, the angle returned would be 90)

                //If this is the bottomLeft/bottomRight ray AND the slopeAngle is less than the maxClimbAngle, climb slope
                if (i == 0 && slopeAngle <= 90)
                {

                    //Handle suddenly climbing a slope immediately after having been descending a slope
                    if (collisions.descendingSlope)
                    {
                        collisions.descendingSlope = false;
                        velocity = collisions.velocityOld;
                    }

                    //Handle snapping the player to the slope in order to start climbing
                    float distanceToSlopeStart = 0;
                    if (slopeAngle != collisions.slopeAngleOld)
                    {
                        distanceToSlopeStart = hit.distance - skinWidth;
                        velocity.x -= distanceToSlopeStart * directionX;
                    }

                    ClimbSlope(ref velocity, slopeAngle, hit.normal);
                    velocity.x += distanceToSlopeStart * directionX;

                }

                //If we are not climbing a slope, detect horizontal collisions as usual
                if (!collisions.climbingSlope || slopeAngle > 90)
                //if (slopeAngle > 90)
                {

                    velocity.x = (hit.distance - skinWidth) * directionX;   //Horizontal velocity equals the distance to what it hit (so if a player is moving extremely fast in one direction, it won't fly through the obstacle)
                    rayLength = hit.distance;   //Set the rayLength to the distance to what it hit because we want each ray to have its own independent length

                    //Handle colliding with wall while climbing slope
                    if (collisions.climbingSlope)
                    {
                        velocity.y = Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x);
                        //collisions.below = true;
                    }

                    collisions.left = (directionX == -1);   //If we are moving left, set collisions.left to true
                    collisions.right = (directionX == 1);   //If we are moving right, set collisions.right to true

                }

            }

        }

    }







    void VerticalCollisions(ref Vector2 velocity)
    {

        float directionY = Mathf.Sign(velocity.y);  //directionY is 1 if player is moving up, -1 if player is moving down
        float rayLength = Mathf.Abs(velocity.y) + skinWidth;    //Our velocity determines our velocity

        for (int i = 0; i < verticalRayCount; i++)
        {

            Vector2 rayOrigin = (directionY == -1) ? raycastOrigins.bottomLeft : raycastOrigins.topLeft;    //Set the origins of these rays to the top or the bottom of the bounds depending on player direction
            rayOrigin += Vector2.right * (verticalRaySpacing * i + velocity.x); //Set the position of the current ray based on which iteration of the loop we're on, as well as the player's x velocity
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.up * directionY, rayLength, collisionMask); //Actually shoot the ray out and see if we're hitting anything on the Obstacle collisionMask

            Debug.DrawRay(rayOrigin, Vector2.up * directionY * rayLength * 3, Color.red);   //Draw the ray

            if (hit)
            {

                //Actually handling the hit
                velocity.y = (hit.distance - skinWidth) * directionY;   //Move the player directly on top of the platform
                rayLength = hit.distance;

                //Handle hitting a ceiling while moving up a slope
                if (collisions.climbingSlope)
                    velocity.x = velocity.y / Mathf.Tan(collisions.slopeAngle * Mathf.Deg2Rad) * Mathf.Sign(velocity.x);

                collisions.above = directionY == 1;     //If we are moving up, set collisions.above to true
                collisions.below = directionY == -1;    //If we are moving down, set collisions.below to true

            }

        }



        //Handle changes in angle when climbing slope
        if (collisions.climbingSlope)
        {
            float directionX = Mathf.Sign(velocity.x);
            rayLength = Mathf.Abs(velocity.x) + skinWidth;
            Vector2 rayOrigin = ((directionX == -1) ? raycastOrigins.bottomLeft : raycastOrigins.bottomRight) + Vector2.up * velocity.y;
            RaycastHit2D hit = Physics2D.Raycast(rayOrigin, Vector2.right * directionX, rayLength, collisionMask);
            if (hit)
            {
                float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);
                if (slopeAngle != collisions.slopeAngle)
                {
                    velocity.x = (hit.distance - skinWidth) * directionX;
                    collisions.slopeAngle = slopeAngle;
                    collisions.slopeNormal = hit.normal;
                }
            }
        }

    }







    void ClimbSlope(ref Vector2 velocity, float slopeAngle, Vector2 slopeNormal)
    {

        //Distance up the slope we're moving
        float moveDistance = Mathf.Abs(velocity.x);

        float climbVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

        //If our velocity is less than the climbing velocity (this is true as long as the player doesn't jump on the slope)
        if (velocity.y <= climbVelocityY)
        {
            velocity.y = climbVelocityY;
            velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
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
        }

    }



    void DescendSlope(ref Vector2 velocity)
    {

        float directionX = Mathf.Sign(velocity.x);
        Vector2 rayOrigin = (directionX == -1) ? raycastOrigins.bottomRight : raycastOrigins.bottomLeft;
        RaycastHit2D hit = Physics2D.Raycast(rayOrigin, -Vector2.up, Mathf.Infinity, collisionMask);

        //If our raycast hit something
        if (hit)
        {

            float slopeAngle = Vector2.Angle(hit.normal, Vector2.up);

            //If we are not on a flat surface
            if (slopeAngle != 0)
            {

                //If the X of the slope is angled in the direction we are moving
                if (Mathf.Sign(hit.normal.x) == directionX)
                {

                    if (hit.distance - skinWidth <= Mathf.Tan(slopeAngle * Mathf.Deg2Rad) * Mathf.Abs(velocity.x))
                    {

                        float moveDistance = Mathf.Abs(velocity.x);
                        float descendVelocityY = Mathf.Sin(slopeAngle * Mathf.Deg2Rad) * moveDistance;

                        velocity.x = Mathf.Cos(slopeAngle * Mathf.Deg2Rad) * moveDistance * Mathf.Sign(velocity.x);
                        velocity.y -= descendVelocityY;

                        collisions.slopeAngle = slopeAngle;
                        collisions.descendingSlope = true;
                        collisions.below = true;
                        collisions.slopeNormal = hit.normal;

                    }

                }

            }

        }

    }



    void MoveToFloor(Vector2 rayOrigin, float xOffset)
    {
        RaycastHit2D findFloor = Physics2D.Raycast(rayOrigin, Vector2.down, Mathf.Infinity, collisionMask);
        float distanceToFloor = -(findFloor.distance - skinWidth);
        transform.Translate(new Vector3(xOffset, distanceToFloor, 0));
    }



    public float GetDistToPlayer()
    {
        float distToPlayer = 0;
        Vector2 playerPos = new Vector2(player.transform.position.x, player.transform.position.y);
        Vector2 enemyPos = new Vector2(transform.position.x, transform.position.y);
        distToPlayer = Mathf.Sqrt(Mathf.Pow((playerPos.x - enemyPos.x), 2.0f) + Mathf.Pow((playerPos.y - enemyPos.y), 2.0f));
        //print(distToPlayer);
        return distToPlayer;
    }



    public struct CollisionInfo
    {
        public bool above, below;
        public bool left, right;
        public int faceDir;

        public bool climbingSlope;
        public bool descendingSlope;

        public float slopeAngle, slopeAngleOld;
        public Vector2 slopeNormal;

        public Vector2 velocityOld;

        public void Reset()
        {
            above = below = false;
            left = right = false;

            climbingSlope = false;
            descendingSlope = false;
            slopeAngleOld = slopeAngle;
            slopeAngle = 0;
            slopeNormal = Vector2.zero;
        }
    }

}
