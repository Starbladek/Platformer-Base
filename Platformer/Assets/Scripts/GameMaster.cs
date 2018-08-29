using UnityEngine;

/*
 TODO:
 -Change ledge-handling logic to not require tagged blocks
*/

public class GameMaster : MonoBehaviour {

    //clean up static code
    public static GameMaster gameMaster;
	public static int SPRITE_BIT_RESOLUTION = 64;   //How many pixels is equivelent to 1 unit
    public static float PIXEL_SCALE = 2f;   //How "zoomed in" the camera is
	public float TIME_SCALE = 1f;   //The speed at which the game plays

	public static int JUMP_MODE = 1;

	public static float aspectRatio;
	public static float orthographicSize;

	private float deltaTime = 0.0f;	//Use for fps counter



    //Awake happens before start
	void Awake () {

        //PlayerPrefs.DeleteAll();    //Delete all player prefs (for testing purposes)

        QualitySettings.vSyncCount = 0;
        Application.targetFrameRate = 60;

        //If gameMaster doesn't exist, create one and set it to this
        if (gameMaster == null)
        {
            DontDestroyOnLoad(gameObject);
            gameMaster = this;

            //Initialize key game aspects
            //
            //
			if (PlayerPrefs.HasKey("resolutionWidth") == false || PlayerPrefs.HasKey("resolutionHeight") == false) {
				PlayerPrefs.SetInt("resolutionWidth", Screen.currentResolution.width);
				PlayerPrefs.SetInt("resolutionHeight", Screen.currentResolution.height);
			}
			Screen.SetResolution(
				PlayerPrefs.GetInt("resolutionWidth", Screen.currentResolution.width),
				PlayerPrefs.GetInt("resolutionHeight", Screen.currentResolution.height),
				(PlayerPrefs.GetInt("displayMode", 1) == 1) ? true : false
			);
			//PlayerPrefs.SetInt ("UnitySelectMonitor", 0);
            //
            //
            //Done initializing key game aspects

			aspectRatio = (float)Screen.width / (float)Screen.height;
			orthographicSize = ((float)PlayerPrefs.GetInt("resolutionHeight") / (GameMaster.PIXEL_SCALE * GameMaster.SPRITE_BIT_RESOLUTION)) / 2.0f;
        }

        //If gameMaster does exist but is not this, destroy this
        else if (gameMaster != this)
        {
            Destroy(gameObject);
        }

    }

    void Update()
    {
		Time.timeScale = TIME_SCALE;

		if (Input.GetKeyDown(KeyCode.Tab)) {
			JUMP_MODE++;
			if (JUMP_MODE > 3)
				JUMP_MODE = 1;
		}

		if (Time.timeScale != 0)
			deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
    }

	void OnGUI() {
        GUIStyle leftAlignStyle = GUI.skin.GetStyle("Label");
        leftAlignStyle.alignment = TextAnchor.UpperLeft;
        GUI.Label (new Rect (10, 10, 400, 30), "Vertical camera smoothing mode: " + JUMP_MODE.ToString() + " (press TAB to change)", leftAlignStyle);

		switch (JUMP_MODE) {
		case 1:
			GUI.Label (new Rect (10, 30, 400, 30), "Trail behind mode", leftAlignStyle);
			break;
		case 2:
			GUI.Label (new Rect (10, 30, 400, 30), "Dynamically scale with vertical velocity", leftAlignStyle);
			break;
		case 3:
			GUI.Label (new Rect (10, 30, 400, 30), "Static number mode", leftAlignStyle);
			break;
		}

		GUI.Label (new Rect (10, 50, 400, 30), "Press R to reset position", leftAlignStyle);



		//Draw framerate counter
		int w = Screen.width, h = Screen.height;
		GUIStyle style = new GUIStyle ();
		Rect rect = new Rect (0, 0, w, h * 2 / 100);
		style.alignment = TextAnchor.UpperLeft;
		style.fontSize = h * 2 / 100;
		style.normal.textColor = new Color (0.0f, 0.0f, 0.5f, 1.0f);
		float msec = deltaTime * 1000.0f;
		float fps = 1.0f / deltaTime;
		string text = string.Format ("{0:0.0} ms ({1:0.} fps)", msec, fps);
		GUI.Label (rect, text, style);
	}

}