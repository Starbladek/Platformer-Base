using UnityEngine;
using System.Collections;

public class SnowParticle : MonoBehaviour {

	public Sprite[] snowParticleSprites;
	public LayerMask layerMask;

	private SpriteRenderer spriteRen;
	private BoxCollider2D boxCol;

	private float speed;
	private int angle;
	private Vector2 moveVector;

	private GameObject mainCamera;



	void Start () {
		mainCamera = GameObject.FindWithTag ("MainCamera");

		spriteRen = GetComponent<SpriteRenderer> ();
		boxCol = GetComponent<BoxCollider2D> ();

		spriteRen.sprite = snowParticleSprites [Random.Range(0, 2)];
		boxCol.size = new Vector2 (spriteRen.sprite.bounds.size.x, spriteRen.sprite.bounds.size.y);

		speed = Random.Range(3.0f, 5.0f);
		angle = Random.Range (210, 230);

		moveVector.x = Mathf.Cos (Mathf.Deg2Rad * angle) * speed;
		moveVector.y = Mathf.Sin (Mathf.Deg2Rad * angle) * speed;
	}



	void Update () {
		transform.Translate (moveVector * Time.deltaTime);

		RaycastHit2D hit = Physics2D.Raycast (spriteRen.transform.position, moveVector, 0.05f, layerMask);
		Debug.DrawRay (spriteRen.transform.position, moveVector * 0.05f, Color.red);

		if (hit || transform.position.y < (mainCamera.transform.position.y - GameMaster.orthographicSize)) {
			Destroy (gameObject);
		}
	}

}
