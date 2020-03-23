using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class UserSearch : MonoBehaviour {

	public static UserSearch INSTANCE;

	public GameObject userSearchItemPrefab;
	public GameObject friendSearchItemPrefab;

	public Button userSearchScrim;

	public InputField searchField;
	public Button clearSearchFieldButton;

	public ScrollableList searchList;

	public Button linkFacebookFriendsButton;

	public UserProfile userProfile;

	private Object currentSearchLock;
	private User[] searchedUsers;

	// Use this for initialization
	void Start () {
		INSTANCE = this;

		addButtonListeners ();
	}

	private void addButtonListeners() {
		searchField.onValueChanged.AddListener (onSearchFieldChange);
		clearSearchFieldButton.onClick.AddListener (onClearSearchButton);
		linkFacebookFriendsButton.onClick.AddListener (onLinkFacebookFriends);
		userSearchScrim.onClick.AddListener (onScrim);
	}

	private void onScrim() {
		setVisible (false);
	}

	private void onSearchFieldChange(string search) {
		if (!search.Equals (string.Empty)) {
			StartCoroutine (doSearch (search));
		} else {
			currentSearchLock = new Object ();
			searchList.removeAllElements ();
		}
	}

	private IEnumerator doSearch(string search) {

		Object searchLock = new Object ();
		currentSearchLock = searchLock;

		ServerCall call = new ServerCall (ServerInteract.INSTANCE.SearchForUser(search));
		yield return StartCoroutine (call.call ());
		User[] receivedUsers = (User[])call.ObjectResponse;

		if (!ReferenceEquals (searchLock, currentSearchLock)) {
			Debug.LogWarning ("Another search was initiated before this search returned. Not updating table");
		} else {
			searchedUsers = receivedUsers;
			refreshList ();
		}

		yield return "Done";
	}

	public void refreshList() {
		searchList.removeAllElements ();
		foreach (User user in searchedUsers) {
			ScrollableList.ListGameObject listObject;
			if (!LoggedInUser.GetLoggedInUser ().HasFriendship (user)) {
				GameObject go = Instantiate (userSearchItemPrefab);
				SearchUserNotFriendListItem newItem = go.GetComponent<SearchUserNotFriendListItem> ();
				newItem.SetSearchedUser (user);
				newItem.userProfile = userProfile;

				listObject = (ScrollableList.ListGameObject)newItem;

			} else {
				GameObject go = Instantiate (friendSearchItemPrefab);
				SearchUserIsFriendListItem newItem = go.GetComponent<SearchUserIsFriendListItem> ();
				newItem.SetSearchedUser (user);
				newItem.userProfile = userProfile;

				listObject = (ScrollableList.ListGameObject)newItem;
			}


			searchList.addElement(listObject);
		}

		searchList.sortList ();
	}

	private void onClearSearchButton() {
		if (searchField.text.Equals (string.Empty)) {
			setVisible (false);
		} else {
			currentSearchLock = new Object ();
			searchField.text = string.Empty;
		}
	}
		
	private void onLinkFacebookFriends() {
		StartCoroutine (doLinkFacebookFriends());
	}

	private IEnumerator doLinkFacebookFriends() {

		Object searchLock = new Object ();
		currentSearchLock = searchLock;

		ServerCall call = new ServerCall (ServerInteract.INSTANCE.GetLinkedFacebookFriends());
		yield return StartCoroutine (call.call ());
		User[] receivedUsers = (User[])call.ObjectResponse;

		if (!ReferenceEquals (searchLock, currentSearchLock)) {
			Debug.LogWarning ("Another search was initiated before this search returned. Not updating table");
		} else {
			searchedUsers = receivedUsers;
			refreshList ();
		}

		yield return "Done";
	}



	public void setVisible(bool visible) {
		userSearchScrim.gameObject.SetActive (visible);
		searchList.removeAllElements ();
		searchField.text = string.Empty;
	}
}
