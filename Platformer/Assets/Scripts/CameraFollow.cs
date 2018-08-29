using UnityEngine;
using System.Collections;

public class CameraFollow : MonoBehaviour {

	private Player target;

	public float lookAheadDstX;
	public float lookAheadDstY;
	public float lookSmoothTimeX;
	public float lookSmoothTimeY;

	public Vector2 focusAreaSize;
	private FocusArea focusArea;

	public float verticalOffset;

	private float currentLookAheadX;
	private float targetLookAheadX;
	private float lookAheadDirX;
	private float smoothLookVelocityX;

	private float currentLookAheadY;
	private float targetLookAheadY;
	private float smoothLookVelocityY;

	private bool lookAheadStopped;
	private float lookAheadResetTimer;
	private float lookAheadResetTimerLength = 1.5f;



	void Start () {
		
        FixAspectRatio();
		GetComponent<Camera>().orthographicSize = GameMaster.orthographicSize;
		//print(GetComponent<Camera>().orthographicSize);

		target = GameObject.FindWithTag("Player").GetComponent<Player>();
		focusArea = new FocusArea (target.controller.collider.bounds, focusAreaSize);

    }

	//Use LateUpdate here rather than Update because we want to wait for all of the player movements to be calculated
	void LateUpdate() {
		
		focusArea.Update (target.controller.collider.bounds);

		Vector2 focusPosition = focusArea.center + Vector2.up * verticalOffset;



		//HORIZONTAL SMOOTHING METHOD 1 - This method of horizontal smoothing causes the camera to move ahead of the player as long as the player is giving some sort of input

		//If the velocity of the focusArea is not zero, smooth it to its next position
		if (focusArea.velocity.x != 0) {

			//Get the direction we should look ahead
			lookAheadDirX = Mathf.Sign (focusArea.velocity.x);

			//Reset lookAheadResetTimer (because we reset it after the focusArea has not moved for two seconds)
			lookAheadResetTimer = 0;

			//If the player is holding the same direction the focusArea velocity is going AND if the player's input is not zero
			//set the targetLookAheadX to the lookAheadDirection multiplied by the lookAheadDistance,
			//and make the camera start moving that way
			if (Mathf.Sign(target.directionalInput.x) == Mathf.Sign(focusArea.velocity.x) && target.directionalInput.x != 0) {
				lookAheadStopped = false;
				targetLookAheadX = lookAheadDirX * lookAheadDstX;
			}

			//Else, if either of the above conditions are NOT true, check if lookAheadStopped is false (as in, we've been moving)
			else {
				//If lookAheadStopped is false, set it to true and mess with the targetLookAheadX
				if (!lookAheadStopped) {
					lookAheadStopped = true;
					//This line stops the camera from immediately smoothing to its furthest extent when only slightly moving out of the focusArea
					targetLookAheadX = currentLookAheadX + (lookAheadDirX * lookAheadDstX - currentLookAheadX) / 4f;
				}
			}

		}
		//Else, if the velocity IS zero, and the camera is NOT centered, wait two seconds and then set it to be centered
		else if (focusArea.velocity.x == 0 && targetLookAheadX != 0) {
			lookAheadResetTimer += Time.deltaTime;
			if (lookAheadResetTimer >= lookAheadResetTimerLength) {
				targetLookAheadX = 0;
				lookAheadResetTimer = 0;
			}
		}

		//Set the variable for how far we should be looking ahead
		currentLookAheadX = Mathf.SmoothDamp (currentLookAheadX, targetLookAheadX, ref smoothLookVelocityX, lookSmoothTimeX);
		//Add the vector to the focusArea's position
		focusPosition += Vector2.right * currentLookAheadX;



		//VERTICAL SMOOTHING METHOD 1 - This method of vertical smoothing causes the player to fall faster than the camera can move, so we wont use it
		if (GameMaster.JUMP_MODE == 1) {
			lookSmoothTimeY = 0.1f;
			focusPosition.y = Mathf.SmoothDamp (transform.position.y, focusPosition.y, ref smoothLookVelocityY, lookSmoothTimeY);
		}
		else {
			
			//VERTICAL SMOOTHING METHOD 2 - This method of vertical smoothing causes the camera to move ahead of the player so they can kinda see what they're falling into

			//If we are moving down, set the targetLookAheadY to the velocity of the focusArea, scaled up by 4
			if (focusArea.velocity.y < 0) {
				if (GameMaster.JUMP_MODE == 2) {
					lookSmoothTimeY = 0.4f;
					if (focusArea.velocity.y > -0.22f)
						//TODO: Make framerate independent.
						//At 60fps, targetLookAhead reaches ~-2.1 units per frame.
						//At 10fps, targetLookAhead reaches ~-1.2 units per frame.
						//Look into ways later I could make this framerate independent
						targetLookAheadY = focusArea.velocity.y * 10; //print (targetLookAheadY);
						//TODO: Make framerate independent.
				}
				else if (GameMaster.JUMP_MODE == 3) {
					lookSmoothTimeY = 0.4f;
					targetLookAheadY = -lookAheadDstY;
				}
			}
			//Else, if we are NOT moving down, center the vertical aspect of the camera on the player
			else {
				targetLookAheadY = 0;
			}

			//Set the variable for how far we should be looking ahead
			currentLookAheadY = Mathf.SmoothDamp (currentLookAheadY, targetLookAheadY, ref smoothLookVelocityY, lookSmoothTimeY);
			//Add the vector to the focusArea's position
			focusPosition += Vector2.up * currentLookAheadY;

		}



		//Finally apply all of the values we've calculated up to this point to the camera (also set the camera's Z position to something appropriate)
		transform.position = (Vector3)focusPosition + Vector3.forward * -10;

	}



	void OnDrawGizmos() {
		Gizmos.color = new Color (1, 0, 0, .5f);
		Gizmos.DrawCube (focusArea.center, focusAreaSize);
	}



	struct FocusArea {
		public Vector2 center;
		public Vector2 velocity;
		float left, right;
		float top, bottom;

		//targetBounds is the size of the player's collider box that we're following, size is how big the focus box should actually be
		public FocusArea(Bounds targetBounds, Vector2 size) {
			left = targetBounds.center.x - size.x/2;
			right = targetBounds.center.x + size.x/2;
			bottom = targetBounds.min.y;
			top = targetBounds.min.y + size.y;

			center = new Vector2((left+right)/2, (top+bottom)/2);
			velocity = Vector2.zero;
		}

		public void Update(Bounds targetBounds) {

			//Getting the X shifts
			float shiftX = 0;
			//If player's box collider is now to the left of the focus area (that is, the player moved left out of the focus area),
			//set shiftX to the difference between the left of the player's box collider and the left of the focus area
			if (targetBounds.min.x < left) {
				shiftX = targetBounds.min.x - left;
			}
			//Same thing basically for the right side
			else if (targetBounds.max.x > right) {
				shiftX = targetBounds.max.x - right;
			}
			left += shiftX;
			right += shiftX;

			//Getting the Y shifts
			float shiftY = 0;
			//If player's box collider is now under of the focus area (that is, the player moved down out of the focus area),
			//set shiftY to the difference between the bottom of the player's box collider and the bottom of the focus area
			if (targetBounds.min.y < bottom) {
				shiftY = targetBounds.min.y - bottom;
			}
			else if (targetBounds.max.y > top) {
				shiftY = targetBounds.max.y - top;
			}
			top += shiftY;
			bottom += shiftY;

			//Set new center and velocity
			center = new Vector2((left+right)/2, (top+bottom)/2);
			velocity = new Vector2 (shiftX, shiftY);

		}
	}



	public void FixAspectRatio()
	{
		
		// set the desired aspect ratio (the values in this example are
		// hard-coded for 16:9, but you could make them into public
		// variables instead so you can set them at design time)
		float targetaspect = 16.0f / 9.0f;

		// determine the game window's current aspect ratio
		float windowaspect = (float)PlayerPrefs.GetInt("resolutionWidth") / (float)PlayerPrefs.GetInt("resolutionHeight");

		// current viewport height should be scaled by this amount
		float scaleheight = windowaspect / targetaspect;

		// obtain camera component so we can modify its viewport
		Camera camera = GetComponent<Camera>();

		// if scaled height is less than current height, add letterbox
		if (scaleheight < 1.0f)
		{
			Rect rect = camera.rect;

			rect.width = 1.0f;
			rect.height = scaleheight;
			rect.x = 0;
			rect.y = (1.0f - scaleheight) / 2.0f;

			camera.rect = rect;
		}
		else // add pillarbox
		{
			float scalewidth = 1.0f / scaleheight;

			Rect rect = camera.rect;

			rect.width = scalewidth;
			rect.height = 1.0f;
			rect.x = (1.0f - scalewidth) / 2.0f;
			rect.y = 0;

			camera.rect = rect;
		}

		GetComponent<Camera>().orthographicSize = ((float)PlayerPrefs.GetInt("resolutionHeight") / (GameMaster.PIXEL_SCALE * GameMaster.SPRITE_BIT_RESOLUTION)) / 2.0f;

	}

}