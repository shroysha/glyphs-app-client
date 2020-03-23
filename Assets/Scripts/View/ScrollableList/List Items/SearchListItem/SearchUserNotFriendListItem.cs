using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SearchUserNotFriendListItem : MonoBehaviour, ScrollableList.ListGameObject {

	public UserProfile userProfile;

	[HideInInspector] public UserAvatarImage avatarImage;
	[HideInInspector] public Text usernameText;
	[HideInInspector] public Button sendRequestButton;
	[HideInInspector] public Button menuItemButton;

	private User searchedUser;

	void Awake() {
		AssignInternalComponentReferences ();
	}

	private void AssignInternalComponentReferences() {
		avatarImage = transform.Find ("AvatarContainer/SearchedAvatarImage").GetComponent<UserAvatarImage> ();
		usernameText = transform.Find ("SearchedUsernameText").GetComponent<Text> ();
		sendRequestButton = transform.Find ("SearchedAddButton").GetComponent<Button> (); 
		menuItemButton = this.GetComponent<Button> ();
	}


	// Use this for initialization
	void Start () {
		AddButtonListeners ();
	}

	private void AddButtonListeners () {
		sendRequestButton.onClick.AddListener (OnSendRequestButton);
		menuItemButton.onClick.AddListener (OnMenuItemButton);
	}

	private void OnSendRequestButton() {
		StartCoroutine (DoSendFriendRequest());
	}
		
	private void OnMenuItemButton() {
		userProfile.ShowUser (searchedUser);
	}

	private IEnumerator DoSendFriendRequest() {
		ServerCall call = new ServerCall (ServerInteract.INSTANCE.SendFriendRequest(searchedUser));
		yield return StartCoroutine (call.call ());

		LoadFriendsScene.INSTANCE.refreshList ();
		UserSearch.INSTANCE.refreshList ();

		yield return "Done";
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
