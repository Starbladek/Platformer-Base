using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;

public class Menu : MonoBehaviour {

	public GameObject mainMenuHolder;
	public GameObject optionsMenuHolder;

	public Button resumeButton;
	public Button optionsButton;
	public Button quitButton;

	public Dropdown displayModeDropdown;
	public Dropdown resolutionDropdown;
	public Button cancelButton;
	public Button applyButton;

	public Resolution[] resolutions;

	private int tempResolutionWidth;
	private int tempResolutionHeight;
	private int tempDisplayMode;



	public void Start() {
		Time.timeScale = 0f;

		//Get all possible resolutions of the user's screen
		resolutions = Screen.resolutions;

		//Setup options with resolutions
		List<string> resolutionList = new List<string> ();
		for (int i = 0; i < resolutions.Length; i++) {
			resolutionList.Add (resolutions [i].ToString());
		}
		resolutionDropdown.AddOptions (resolutionList);

		//Setup options with display modes
		List<string> displayModeList = new List<string> ();
		displayModeList.Add ("Windowed");
		displayModeList.Add ("Fullscreen");
		displayModeDropdown.AddOptions (displayModeList);

		MainMenu ();
	}

	public void OnDestroy() {
		Time.timeScale = 1f;
	}



	public void OnResumeClick() {
		Destroy(gameObject);
	}

	public void OptionsMenu() {

		//Disable main menu, enable options menu
		mainMenuHolder.SetActive (false);
		optionsMenuHolder.SetActive (true);

		//Enable options UI elements
		resolutionDropdown.onValueChanged.AddListener (delegate {
			OnResolutionChange ();
		});
		displayModeDropdown.onValueChanged.AddListener (delegate {
			OnDisplayModeChange ();
		});
		cancelButton.onClick.AddListener (delegate {
			OnCancelClick ();
		});
		applyButton.onClick.AddListener (delegate {
			OnApplyClick ();
		});

		//Disable main menu UI elements
		resumeButton.onClick.RemoveAllListeners ();
		optionsButton.onClick.RemoveAllListeners ();
		quitButton.onClick.RemoveAllListeners ();

		//Setup game options
		tempResolutionWidth = PlayerPrefs.GetInt ("resolutionWidth", Screen.currentResolution.width);
		tempResolutionHeight = PlayerPrefs.GetInt ("resolutionHeight", Screen.currentResolution.height);
		tempDisplayMode = PlayerPrefs.GetInt ("displayMode", 1);

		//Set which resolution the resolutionDropdown should start on
		for (int i = 0; i < resolutions.Length; i++) {
			if (PlayerPrefs.GetInt ("resolutionWidth", Screen.currentResolution.width) == resolutions[i].width && PlayerPrefs.GetInt ("resolutionHeight", Screen.currentResolution.height) == resolutions[i].height) {
				resolutionDropdown.value = i;
				break;
			}
		}

		//Set which display mode the displayModeDropdown should start on
		displayModeDropdown.value = PlayerPrefs.GetInt ("displayMode", 1);

	}

	public void OnQuitClick() {
		Application.Quit ();
	}



	public void MainMenu() {
		
		//Disable options menu, enable main menu
		mainMenuHolder.SetActive (true);
		optionsMenuHolder.SetActive (false);

		//Enable main menu UI elements
		resumeButton.onClick.AddListener (delegate {
			OnResumeClick ();
		});
		optionsButton.onClick.AddListener (delegate {
			OptionsMenu ();
		});
		quitButton.onClick.AddListener (delegate {
			OnQuitClick ();
		});

		//Disable options menu UI elements
		resolutionDropdown.onValueChanged.RemoveAllListeners ();
		displayModeDropdown.onValueChanged.RemoveAllListeners ();
		cancelButton.onClick.RemoveAllListeners ();
		applyButton.onClick.RemoveAllListeners ();

	}

	public void OnResolutionChange() {
		tempResolutionWidth = Screen.resolutions [resolutionDropdown.value].width;
		tempResolutionHeight = Screen.resolutions [resolutionDropdown.value].height;
	}

	public void OnDisplayModeChange() {
		tempDisplayMode = displayModeDropdown.value;
	}

	public void OnCancelClick() {
		MainMenu ();
	}

	public void OnApplyClick() {
		Screen.SetResolution (tempResolutionWidth, tempResolutionHeight, tempDisplayMode == 1 ? true : false);
		PlayerPrefs.SetInt ("resolutionWidth", tempResolutionWidth);
		PlayerPrefs.SetInt ("resolutionHeight", tempResolutionHeight);
		PlayerPrefs.SetInt ("displayMode", tempDisplayMode);

		GameObject mainCamera = GameObject.FindGameObjectWithTag ("MainCamera");
		CameraFollow mainCameraScript = (CameraFollow)mainCamera.GetComponent (typeof(CameraFollow));
		mainCameraScript.FixAspectRatio ();
	}

}
