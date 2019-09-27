using UnityEngine;
using System.Collections;

//This class holds where the locations that each raycast can be sent out from.
//It does not actually shoot the raycasts out itself, it simply knows where they will be shot out from.

[RequireComponent (typeof (BoxCollider2D))]
public class RaycastController : MonoBehaviour {

	public const float skinWidth = .015f;	//Width of the small layers of cushion we give the rays
	private const float dstBetweenRays = .08f;

	[HideInInspector]
	public int horizontalRayCount;
	[HideInInspector]
	public int verticalRayCount;

	[HideInInspector]
	public float horizontalRaySpacing;
	[HideInInspector]
	public float verticalRaySpacing;

	public LayerMask collisionMask;
	public new BoxCollider2D collider;

	public RaycastOrigins raycastOrigins;



	public virtual void Awake () {
		//Get access to the BoxCollider2D component
		collider = GetComponent<BoxCollider2D> ();
	}

	public virtual void Start() {
		//Get the spacing between each ray depending on the amount of rays and the width/height of the player
		CalculateRaySpacing ();
	}



	//Calculate and update the position of each ray
	public void UpdateRaycastOrigins() {

		//Get player's BoxCollider2D bounds
		Bounds bounds = collider.bounds;
		//Shrink the bounds by twice the skinWidth
		bounds.Expand (skinWidth * -2);

		//Set the key positions of the raycasts to the corners of the bounds
		raycastOrigins.bottomLeft = new Vector2 (bounds.min.x, bounds.min.y);
		raycastOrigins.bottomRight = new Vector2 (bounds.max.x, bounds.min.y);
		raycastOrigins.topLeft = new Vector2 (bounds.min.x, bounds.max.y);
		raycastOrigins.topRight = new Vector2 (bounds.max.x, bounds.max.y);

	}



	//Calculate distance between each ray
	public void CalculateRaySpacing() {

		//Get player's BoxCollider2D bounds
		Bounds bounds = collider.bounds;
		//Shrink the bounds by twice the skinWidth
		bounds.Expand (skinWidth * -2);

		float boundsWidth = bounds.size.x;
		float boundsHeight = bounds.size.y;

		//Clamp the ray counts to between 2 and very big
		horizontalRayCount = Mathf.RoundToInt (boundsHeight / dstBetweenRays);
		verticalRayCount = Mathf.RoundToInt (boundsWidth / dstBetweenRays);

		horizontalRaySpacing = bounds.size.y / (horizontalRayCount - 1);
		verticalRaySpacing = bounds.size.x / (verticalRayCount - 1);

	}



	public struct RaycastOrigins {
		public Vector2 topLeft, topRight;
		public Vector2 bottomLeft, bottomRight;
	}



}
