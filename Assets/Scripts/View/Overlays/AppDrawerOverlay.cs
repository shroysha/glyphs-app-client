using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class AppDrawerOverlay : MonoBehaviour {

	private static readonly string GROUND_VIEW_SCENE = "Scenes/GroundViewScene";
	private static readonly string FRIEND_SCENE = "Scenes/FriendScene";
	private static readonly string MY_GLYPHS_SCENE = "Scenes/MyGlyphsScene";
	private static readonly string FOUND_GLYPHS_SCENE = "Scenes/FoundGlyphsScene";
	private static readonly string LOGIN_SCENE = "Scenes/LoginWindow";
	private static readonly string GLYPHS_SHARED_WITH_ME_SCENE = "Scenes/SharedGlyphsScene";

	public RectTransform canvasTransform;
	public UserProfile userProfile;
	public Button appDrawerScrim;

	[HideInInspector] public RectTransform appDrawerTransform;
	[HideInInspector] public Button collapseButton;	
	[HideInInspector] public RectTransform collapseButtonTransform;
	[HideInInspector] public UserAvatarImage avatarImage;
	[HideInInspector] public Text currentHandleText;
	[HideInInspector] public Button myProfileButton;
	[HideInInspector] public Button groundViewButton; 
	[HideInInspector] public Button myGlyphsButton; 
	[HideInInspector] public Button foundGlyphsButton; 
	[HideInInspector] public Button glyphsSharedWithMeButton; 
	[HideInInspector] public Button friendsButton;
	[HideInInspector] public Button settingsButton;
	[HideInInspector] public Button logoutButton;	
	[HideInInspector] public Button showOptionsButton;
	[HideInInspector] public Button extraScrim;

	private bool manageMenuVisible = false;

	void Awake() {
		AssignInternalComponentReferences ();
	}
		
	private void AssignInternalComponentReferences() {
		appDrawerTransform = transform.GetComponent<RectTransform> ();
		collapseButton = transform.Find ("CollapseButton").GetComponent<Button>();	
		collapseButtonTransform = transform.Find ("CollapseButton").GetComponent<RectTransform> ();
		avatarImage = transform.Find ("Main/InfoPanel/UserAvatarImage").GetComponent<UserAvatarImage> ();
		currentHandleText = currentHandleText = transform.Find ("Main/InfoPanel/CurrentHandleText").GetComponent<Text> ();
		groundViewButton = transform.Find ("Main/ButtonPanel/GroundViewButton").GetComponent<Button> (); 
		myGlyphsButton = transform.Find ("Main/ButtonPanel/MyGlyphsButton").GetComponent<Button> (); 
		foundGlyphsButton = transform.Find ("Main/ButtonPanel/FoundGlyphsButton").GetComponent<Button> (); 
		glyphsSharedWithMeButton = transform.Find ("Main/ButtonPanel/GlyphsSharedWithMeButton").GetComponent<Button> (); 
		friendsButton = transform.Find ("Main/ButtonPanel/FriendsButton").GetComponent<Button> ();
		settingsButton = transform.Find ("Main/ButtonPanel/SettingsButton").GetComponent<Button> ();
		extraScrim = transform.Find ("AppDrawerExtraScrim").GetComponent<Button> ();
		showOptionsButton = transform.Find ("Main/InfoPanel/DropdownButton").GetComponent<Button> ();
		myProfileButton = transform.Find ("AppDrawerExtraScrim/DropdownButtonPanel/MyProfileButton").GetComponent<Button> ();
		logoutButton = transform.Find ("AppDrawerExtraScrim/DropdownButtonPanel/LogoutButton").GetComponent<Button> ();	
	}

	// Use this for initialization
	void Start () {
		AddButtonListeners ();
		UpdateUIElements ();
		DoSpecialResize();
	}


	private void UpdateUIElements() {
		if (LoggedInUser.GetLoggedInUser () != null) {
			avatarImage.SetUser (LoggedInUser.GetLoggedInUser());
			currentHandleText.text = LoggedInUser.GetLoggedInUser ().GetUserFriendlyName ();
		}
	}

	private void AddButtonListeners() {
		groundViewButton.onClick.AddListener (OnGroundViewButton);
		myGlyphsButton.onClick.AddListener (OnMyGlyphsButton);
		foundGlyphsButton.onClick.AddListener (OnFoundGlyphsButton);
		glyphsSharedWithMeButton.onClick.AddListener (OnGlyphsSharedWithMeButton);
		friendsButton.onClick.AddListener(OnMyFriendsButton);
		logoutButton.onClick.AddListener (OnLogoutButton);
		appDrawerScrim.onClick.AddListener (OnAppDrawerScrim);

		collapseButton.onClick.AddListener (OnCollapseButton);
		showOptionsButton.onClick.AddListener (OnShowOptionsButton);
		extraScrim.onClick.AddListener (OnExtraScrimButton);
		myProfileButton.onClick.AddListener (OnMyProfileButton);

	}

	private void DoSpecialResize() {
		float preferredWidth = canvasTransform.sizeDelta.x - collapseButtonTransform.sizeDelta.x;
		float maxWidth = 320.0f;

		float width = Mathf.Min (preferredWidth, maxWidth);

		appDrawerTransform.sizeDelta = new Vector2 (width, appDrawerTransform.sizeDelta.y);
		appDrawerTransform.anchoredPosition = new Vector2 (-width / 2, appDrawerTransform.anchoredPosition.y);
	}

	private void OnMyProfileButton() {
		userProfile.ShowUser (LoggedInUser.GetLoggedInUser ());
	}

	private void OnGroundViewButton() {
		LoadSceneIfNotActive (GROUND_VIEW_SCENE);
	}

	private void OnMyGlyphsButton() {
		LoadSceneIfNotActive (MY_GLYPHS_SCENE);
	}

	private void OnFoundGlyphsButton() {
		LoadSceneIfNotActive (FOUND_GLYPHS_SCENE);
	}

	private void OnGlyphsSharedWithMeButton() {
		LoadSceneIfNotActive (GLYPHS_SHARED_WITH_ME_SCENE);
	}

	private void OnMyFriendsButton() {
		LoadSceneIfNotActive(FRIEND_SCENE);
	}

	private void OnLogoutButton() {
		Authentication.clearUserData ();
		SceneManager.LoadScene (LOGIN_SCENE);
	}

	private void LoadSceneIfNotActive(string scene) {
		Debug.Log ("Current Scene: " + SceneManager.GetActiveScene ().name + " Next Scene: " + scene);

		if (!SceneManager.GetActiveScene ().name.Equals (scene.Replace ("Scenes/", ""))) {
			SceneManager.LoadScene (scene);
		} else {
			CollapseAppDrawer ();
		}
	}

	private void OnAppDrawerScrim() {
		CollapseAppDrawer ();
	}

	private void OnShowOptionsButton() {
		SetExtraScrimVisible (true);
	}

	private void OnExtraScrimButton() {
		SetExtraScrimVisible (false);
	}

	private void SetExtraScrimVisible (bool visible) {
		extraScrim.gameObject.SetActive (visible);
	}


	private void OnCollapseButton() {
		if (manageMenuVisible) {
			CollapseAppDrawer ();
		} else {
			ExpandAppDrawer ();
		}
	}

	public void CollapseAppDrawer() {
		StartCoroutine(DoCollapseAppDrawer ());

		appDrawerScrim.gameObject.SetActive (false);

		manageMenuVisible = false;
	}

	public void ExpandAppDrawer() {
		StartCoroutine(DoExpandAppDrawer ());

		appDrawerScrim.gameObject.SetActive (true);

		manageMenuVisible = true;
	}

	private static float ANIMATION_TIME = 0.2f;
	private static float TIME_STEP = 0.05f;

	private float NUM_OF_STEPS = ANIMATION_TIME / TIME_STEP;

	private IEnumerator DoExpandAppDrawer() {
		float currentPosX = GetHideTargetX();
		float targetPosX = GetShowTargetX ();

		float distanceStep = (targetPosX - currentPosX) / NUM_OF_STEPS;
		float steps = 0.0f;

		while (steps < NUM_OF_STEPS) {
			MoveAppDrawer (distanceStep);

			steps++;
			yield return new WaitForSeconds (TIME_STEP);
		}

		yield return "Done";
	}

	private IEnumerator DoCollapseAppDrawer() {
		float currentPosX = GetShowTargetX();
		float targetPosX = GetHideTargetX();

		float distanceStep = (targetPosX - currentPosX) / NUM_OF_STEPS;
		float steps = 0.0f;

		while (steps < NUM_OF_STEPS) {
			MoveAppDrawer (distanceStep);

			steps++;
			yield return new WaitForSeconds (TIME_STEP);
		}

		yield return "Done";

	}

	private float GetHideTargetX() {
		return -appDrawerTransform.sizeDelta.x / 2.0f;
	}

	private float GetShowTargetX () {
		return appDrawerTransform.sizeDelta.x / 2.0f;
	}

	private void MoveAppDrawer(float distanceStep) {
		appDrawerTransform.anchoredPosition = new Vector2 (appDrawerTransform.anchoredPosition.x + distanceStep, appDrawerTransform.anchoredPosition.y);
	}

	public bool IsAppDrawerVisible() {
		return manageMenuVisible;
	}
}
