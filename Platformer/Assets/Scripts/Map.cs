using UnityEngine;
using System.Collections;

public class Map : MonoBehaviour {

	private float snowTimer = 0;
	public float snowTimerLength;

	private float cameraHalfWidth;
	private float cameraHalfHeight;

	private GameObject mainCamera;
	public GameObject snowParticleRef;
	private GameObject snowParticleHolder;

	//Parallax background stuff
	public Transform[] backgrounds;
	public Transform[] backgrounds2;
	private float[] parallaxScales;
	public float parallaxSmoothing;
	private Vector3 previousCameraPos;



	void Start () {
		mainCamera = GameObject.FindWithTag ("MainCamera");
		cameraHalfWidth = GameMaster.aspectRatio * GameMaster.orthographicSize;
		cameraHalfHeight = GameMaster.orthographicSize;

		snowParticleHolder = new GameObject("SnowParticleHolder");

		FillScreenWithSnow ();

		previousCameraPos = mainCamera.transform.position;
		parallaxScales = new float[backgrounds.Length];
		for (int i = 0; i < parallaxScales.Length; i++) {
			parallaxScales [i] = backgrounds [i].position.z * -1;
		}
	}



	void Update() {

		//Snow partical spawning
		snowTimer += Time.deltaTime;
		if (snowTimer >= snowTimerLength) {
			snowTimer = 0;
			float xSpawnPos;
			float ySpawnPos = mainCamera.transform.position.y + (GameMaster.orthographicSize * 1.0f);

			//Spawn left snow particle
			xSpawnPos = Random.Range (mainCamera.transform.position.x - (cameraHalfWidth * 3), mainCamera.transform.position.x - cameraHalfWidth);
			//ySpawnPos = Random.Range(mainCamera.transform.position.y - GameMaster.orthographicSize, mainCamera.transform.position.y + (GameMaster.orthographicSize * 1.5f));
			GameObject snow1 = Instantiate (snowParticleRef, new Vector3 (xSpawnPos, ySpawnPos, -5), Quaternion.identity) as GameObject;
			snow1.transform.parent = snowParticleHolder.transform;

			//Spawn center snow particle
			xSpawnPos = Random.Range (mainCamera.transform.position.x - cameraHalfWidth, mainCamera.transform.position.x + cameraHalfWidth);
			//ySpawnPos = mainCamera.transform.position.y + (GameMaster.orthographicSize * 1.5f);
			GameObject snow2 = Instantiate (snowParticleRef, new Vector3 (xSpawnPos, ySpawnPos, -5), Quaternion.identity) as GameObject;
			snow2.transform.parent = snowParticleHolder.transform;

			//Spawn right snow particle
			xSpawnPos = Random.Range (mainCamera.transform.position.x + cameraHalfWidth, mainCamera.transform.position.x + (cameraHalfWidth * 3));
			//ySpawnPos = Random.Range(mainCamera.transform.position.y - GameMaster.orthographicSize, mainCamera.transform.position.y + (GameMaster.orthographicSize * 1.5f));
			GameObject snow3 = Instantiate (snowParticleRef, new Vector3 (xSpawnPos, ySpawnPos, -5), Quaternion.identity) as GameObject;
			snow3.transform.parent = snowParticleHolder.transform;
		}

	}



	void LateUpdate() {

		for (int i = 0; i < backgrounds.Length; i++) {
			Vector3 parallax = (previousCameraPos - mainCamera.transform.position) * (parallaxScales [i] / parallaxSmoothing);
			backgrounds [i].position = new Vector3 (backgrounds [i].position.x + parallax.x, backgrounds [i].position.y + parallax.y, backgrounds [i].position.z);

			//If the center of the main background is to the right of the center of mainCamera, put the loop background to the left of main background
			if (backgrounds[i].position.x >= mainCamera.transform.position.x) {
				backgrounds2[i].position = new Vector3 (backgrounds [i].position.x - (backgrounds [i].GetComponent<SpriteRenderer> ().bounds.size.x), backgrounds[i].position.y, backgrounds[i].position.z);
				//If the center of the loop background is to the right of the center of mainCamera, put the main background where the loop background is, and put the loop background to the left of main background
				if (backgrounds2[i].position.x >= mainCamera.transform.position.x) {
					backgrounds [i].position = backgrounds2 [i].position;
					backgrounds2[i].position = new Vector3 (backgrounds [i].position.x - (backgrounds [i].GetComponent<SpriteRenderer> ().bounds.size.x), backgrounds[i].position.y, backgrounds[i].position.z);
				}
			}
			//Else, put the loop background to the right of the main background
			else {
				backgrounds2[i].position = new Vector3 (backgrounds [i].position.x + (backgrounds [i].GetComponent<SpriteRenderer> ().bounds.size.x), backgrounds[i].position.y, backgrounds[i].position.z);
				//Do similar thing to thing mentioned above
				if (backgrounds2[i].position.x < mainCamera.transform.position.x) {
					backgrounds [i].position = backgrounds2 [i].position;
					backgrounds2[i].position = new Vector3 (backgrounds [i].position.x + (backgrounds [i].GetComponent<SpriteRenderer> ().bounds.size.x), backgrounds[i].position.y, backgrounds[i].position.z);
				}
			}
		}

		previousCameraPos = mainCamera.transform.position;

	}



	void FillScreenWithSnow() {

		//Spawn left snow particles
		for (int i = 0; i < 5; i++) {
			for (int j = 0; j < 4; j++) {
				float xSpawnPos = (mainCamera.transform.position.x - (cameraHalfWidth * 3)) + ((cameraHalfWidth * 2) * (i / 5.0f)) + Random.Range(0.0f, 4.0f);
				float ySpawnPos = (mainCamera.transform.position.y + cameraHalfHeight) - ((cameraHalfHeight * 2) * (j / 4.0f)) - Random.Range(0.0f, 1.0f);
				GameObject snow = Instantiate (snowParticleRef, new Vector3 (xSpawnPos, ySpawnPos, -5), Quaternion.identity) as GameObject;
				snow.transform.parent = snowParticleHolder.transform;
			}
		}

		//Spawn center snow particles
		for (int i = 0; i < 5; i++) {
			for (int j = 0; j < 4; j++) {
				float xSpawnPos = (mainCamera.transform.position.x - cameraHalfWidth) + ((cameraHalfWidth * 2) * (i / 5.0f)) + Random.Range(0.0f, 4.0f);
				float ySpawnPos = (mainCamera.transform.position.y + cameraHalfHeight) - ((cameraHalfHeight * 2) * (j / 4.0f)) - Random.Range(0.0f, 1.0f);
				GameObject snow = Instantiate (snowParticleRef, new Vector3 (xSpawnPos, ySpawnPos, -5), Quaternion.identity) as GameObject;
				snow.transform.parent = snowParticleHolder.transform;
			}
		}

		//Spawn right snow particles
		for (int i = 0; i < 5; i++) {
			for (int j = 0; j < 4; j++) {
				float xSpawnPos = (mainCamera.transform.position.x + cameraHalfWidth) + ((cameraHalfWidth * 2) * (i / 5.0f)) + Random.Range(0.0f, 4.0f);
				float ySpawnPos = (mainCamera.transform.position.y + cameraHalfHeight) - ((cameraHalfHeight * 2) * (j / 4.0f)) - Random.Range(0.0f, 1.0f);
				GameObject snow = Instantiate (snowParticleRef, new Vector3 (xSpawnPos, ySpawnPos, -5), Quaternion.identity) as GameObject;
				snow.transform.parent = snowParticleHolder.transform;
			}
		}

	}

}
