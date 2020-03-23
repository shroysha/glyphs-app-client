using UnityEngine;
using UnityEngine.UI;

public class SearchUserIsFriendListItem : MonoBehaviour, ScrollableList.ListGameObject {

	public UserProfile userProfile;

	[HideInInspector] public UserAvatarImage avatarImage;
	[HideInInspector] public Text usernameText;
	[HideInInspector] public Button viewProfileButton;
	[HideInInspector] public Button menuItemButton;

	private User searchedUser;

	void Awake() {
		AssignInternalComponentReferences ();
	}

	private void AssignInternalComponentReferences() {
		avatarImage = transform.Find ("AvatarContainer/SearchedAvatarImage").GetComponent<UserAvatarImage> ();
		usernameText = transform.Find ("SearchedUsernameText").GetComponent<Text> ();
		viewProfileButton = transform.Find ("SearchedViewButton").GetComponent<Button> (); 
		menuItemButton = this.GetComponent<Button> ();
	}

	// Use this for initialization
	void Start () {
		AddButtonListeners ();
	}

	private void AddButtonListeners () {
		viewProfileButton.onClick.AddListener (OnMenuItemButton);
		menuItemButton.onClick.AddListener (OnMenuItemButton);
	}
		
	private void OnMenuItemButton() {
		userProfile.ShowUser (searchedUser);
	}

	public void SetSearchedUser(User user) {
		searchedUser = user;
		avatarImage.SetUser (searchedUser);
		usernameText.text = searchedUser.GetUserFriendlyName ();
	}
		
	public GameObject GetListObject () {
		return this.gameObject;
	}

	public RectTransform GetListObjectTransform() {
		return this.gameObject.GetComponent<RectTransform> ();
	}


}
