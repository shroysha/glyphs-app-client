using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadFriendsScene : MonoBehaviour {

	public GameObject approvedFriendPrefab, sentFriendPrefab, receivedFriendPrefab;

	public Button searchButton;
	public UserSearch userSearch;

	public Toggle friendsToggle, sentToggle, receivedToggle;
	public ScrollableList friendsList;

	public UserProfile userProfile;

	public static LoadFriendsScene INSTANCE;

	// Use this for initialization
	void Start () {
		INSTANCE = this;

		addButtonListeners ();

		friendsToggle.Select ();
		friendsToggle.isOn = true;
	}

	private void addButtonListeners() {
		friendsToggle.onValueChanged.AddListener (onFriendsToggle);
		sentToggle.onValueChanged.AddListener (onSentToggle);
		receivedToggle.onValueChanged.AddListener (onReceivedToggle);		
		searchButton.onClick.AddListener (onSearchButton);
	}
		

	private void onFriendsToggle(bool selected) {
		if (selected) {
			setListToAccepted ();
		}
	}

	private void onReceivedToggle(bool selected) {
		if (selected) {
			setListToReceived ();
		}
	}

	private void onSentToggle(bool selected) {
		if (selected) {
			setListToSent ();
		}
	}

	public void refreshList() {
		if (receivedToggle.isOn) {
			onReceivedToggle (true);
		} else if (sentToggle.isOn) {
			onSentToggle (true);
		} else {
			onFriendsToggle (true);
		}
	}

	private void onSearchButton() {
		userSearch.setVisible (true);
	}

	private void setListToAccepted() {
		friendsList.removeAllElements ();

		List<Friendship> accepted = LoggedInUser.GetLoggedInUser ().GetAcceptedFriendships ();

		foreach (Friendship friend in accepted) {
			GameObject go = Instantiate (approvedFriendPrefab);
			AcceptedFriendListItem newItem = go.GetComponent<AcceptedFriendListItem> ();
			newItem.SetFriendship (friend);
			newItem.userProfile = userProfile;

			friendsList.addElement ((ScrollableList.ListGameObject) newItem);

			Debug.LogError (friend.ToString ());
		}
	}

	private void setListToReceived() {
		friendsList.removeAllElements ();

		List<Friendship> received = LoggedInUser.GetLoggedInUser ().GetReceivedFriendships ();

		foreach (Friendship friend in received) {
			GameObject go = Instantiate (receivedFriendPrefab);
			ReceivedFriendListItem newItem = go.GetComponent<ReceivedFriendListItem> ();
			newItem.SetFriendship (friend);
			newItem.userProfile = userProfile;

			friendsList.addElement ((ScrollableList.ListGameObject)newItem);

			Debug.LogError (friend.ToString ());
		}
	}

	private void setListToSent() {
		friendsList.removeAllElements ();

		List<Friendship> sent = LoggedInUser.GetLoggedInUser ().GetSentFriendships ();

		foreach (Friendship friend in sent) {
			GameObject go = Instantiate (sentFriendPrefab);
			SentFriendListItem newItem = go.GetComponent<SentFriendListItem> ();
			newItem.SetFriendship (friend);
			newItem.userProfile = userProfile;

			friendsList.addElement ((ScrollableList.ListGameObject) newItem);

			Debug.LogError (friend.ToString ());
		}
	}
		
}
