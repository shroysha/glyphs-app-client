using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GroundViewUIController : MonoBehaviour {

	private static readonly float HEADER_SHOW_TIME = 5.0f; // how long to show the header on recent touches

	public RectTransform headerTransform;

	public Button touchListener;

	public Button backButton;
	public Button addButton;

	private Coroutine showHeaderCoroutine;
	private Object lockObject = new Object ();

	// Use this for initialization
	void Start () {
		addButtonListeners ();

		StartCoroutine (showHeaderForFiveSeconds());
	}

	private void addButtonListeners() {
		addButton.onClick.AddListener (onAddButton);
		backButton.onClick.AddListener (onBackButton);
		touchListener.onClick.AddListener (onTouchListener);
	}

	private void onAddButton() {
		GeneralFunctions.switchToAddGlyphScene ();
	}

	private void onBackButton() {
		SceneManager.LoadScene ("Scenes/MainMenu");
	}

	private void onTouchListener() {
		StartCoroutine (showHeaderForFiveSeconds());
	}

	private IEnumerator showHeaderForFiveSeconds() {
		if (ReferenceEquals(lockObject, null)) {
			Debug.Log ("Lock Object null");
			StartCoroutine(showHeader());
		}

		Object thisInumeratorLockObject = new Object ();
		lockObject = thisInumeratorLockObject;

		yield return new WaitForSeconds (HEADER_SHOW_TIME);

		if (ReferenceEquals(lockObject, thisInumeratorLockObject)) { // no other touches were detected
			StartCoroutine(hideHeader());
			lockObject = null;
		}

		yield return "Done!";
	}
		
	private IEnumerator showHeader() {

		float currentPosY = hideTargetY();
		float targetPosY = showTargetY ();

		float distanceStep = (targetPosY - currentPosY) / NUM_OF_STEPS;
		float steps = 0.0f;

		while (steps < NUM_OF_STEPS) {
			moveHeader (distanceStep);

			steps++;
			yield return new WaitForSeconds (TIME_STEP);
		}

		yield return "Done";
	}

	private IEnumerator hideHeader() {

		float currentPosY = showTargetY();
		float targetPosY = hideTargetY();

		float distanceStep = (targetPosY - currentPosY) / NUM_OF_STEPS;
		float steps = 0.0f;

		while (steps < NUM_OF_STEPS) {
			moveHeader (distanceStep);

			steps++;
			yield return new WaitForSeconds (TIME_STEP);
		}

		yield return "Done";
	}


	private static float ANIMATION_TIME = 0.3f;
	private static float TIME_STEP = 0.05f;

	private float NUM_OF_STEPS = ANIMATION_TIME / TIME_STEP;


	private float hideTargetY() {
		return headerTransform.sizeDelta.y / 2.0f;
	}

	private float showTargetY () {
		return -headerTransform.sizeDelta.y / 2.0f;
	}

	private void moveHeader(float distanceStep) {
		headerTransform.anchoredPosition = new Vector2 (headerTransform.anchoredPosition.x, headerTransform.anchoredPosition.y + distanceStep);
	}


}
