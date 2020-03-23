using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadTakePictureScene : MonoBehaviour {

	private static readonly string PAINT_SCENE = "Scenes/PaintScene";
	private static readonly string ADD_GLYPH_SCENE = "Scenes/AddGlyph";

	private static string backButtonScene = "Scenes/GroundViewScene";

	public static void setBackButtonScene(string scene) {
		backButtonScene = scene;
	}

	public Camera sceneCamera;

	public Button backButton;
	public RawImage backButtonImage;
	public Texture backButtonCloseTexture;
	public Texture backButtonBackTexture;

	public Button switchCameraOrientationButton;
	public Button takePictureButton;
	public Button paintSceneButton;

	public GameObject uiCanvas;

	public GameObject snapshotObject;
	public GameObject cameraObject;
	public GameObject switchCameraOrientationObject;
	public GameObject takePictureObject;
	public GameObject saveObject;

	public DeviceCameraController webcam;
	public RawImage snapshotImage;
	private Texture2D currentSnapshot;

	private bool inTakePictureView = true;

	// Use this for initialization
	void Start () {
		addButtonListeners ();
		switchToTakePictureView ();
	}

	private void addButtonListeners() {
		backButton.onClick.AddListener (onBackButton);
		takePictureButton.onClick.AddListener (onTakePictureButton);
		switchCameraOrientationButton.onClick.AddListener (onSwitchCameraOrientationButton);
		paintSceneButton.onClick.AddListener (onPaintSceneButton);
	}

	private void onBackButton() {
		if (inTakePictureView) {
			SceneManager.LoadScene (backButtonScene);
		} else {
			switchToTakePictureView ();
		}
	}

	private void onTakePictureButton() {
		StartCoroutine( takeSnapshot ());
	}

	private void onSwitchCameraOrientationButton() {
		if (inTakePictureView) {
			webcam.SwitchCamera ();
		} else {
			switchToTakePictureView ();
		}
	}

	private void onSaveButton() {
		saveSnapshot ();
	}

	private void onPaintSceneButton() {
		SceneManager.LoadScene (PAINT_SCENE);
	}

	private void switchToTakePictureView() {
		inTakePictureView = true;

		takePictureObject.SetActive(true);
		switchCameraOrientationObject.SetActive(true);
		cameraObject.SetActive(true);

		saveObject.SetActive(true);
		snapshotObject.SetActive(false);

		backButtonImage.texture = backButtonCloseTexture;
	}

	private void switchToPictureEvaluationView() {
		inTakePictureView = false;

		saveObject.SetActive (true);
		snapshotObject.SetActive(true);

		takePictureObject.SetActive(false);
		switchCameraOrientationObject.SetActive(false);
		cameraObject.SetActive(false);

		backButtonImage.texture = backButtonBackTexture;

		setSnapshotImage (currentSnapshot);
	}

	private void setSnapshotImage(Texture2D snapshot) {
		snapshotImage.texture = snapshot;


		float widthScale = (float)Screen.width / (float)snapshot.width;
		float heightScale = (float)Screen.height  / (float)snapshot.height;

		float scale = Mathf.Min (widthScale, heightScale);

		snapshotImage.GetComponent<RectTransform> ().sizeDelta = new Vector2 (snapshot.width, snapshot.height);
		snapshotImage.GetComponent<RectTransform> ().localScale = new Vector3 (scale, scale, scale);
	}

	private IEnumerator takeSnapshot() {
		yield return new WaitForEndOfFrame ();

		hideUI ();

		currentSnapshot = takeSnapshotOfScreen ();

		showUI ();

		saveSnapshot ();
	}

	private Texture2D takeSnapshotOfScreen() {
		sceneCamera.Render();

		Texture2D image = new Texture2D(Screen.width, Screen.height, TextureFormat.RGB24, false);
		image.ReadPixels(new Rect(new Vector2(0.0f, 0.0f), new Vector2(Screen.width, Screen.height)),0,0); 
		image.Apply();

		return image;
	}

	private void hideUI() {
		uiCanvas.SetActive (false);
	}

	private void showUI() {
		uiCanvas.SetActive (true);
	}

	private void saveSnapshot() {
		ApplicationFileManager.SaveTextureToFile (ApplicationFileManager.TAKE_PICTURE_FILE_PATH, currentSnapshot);
		SceneManager.LoadScene (PAINT_SCENE);
	}

}
