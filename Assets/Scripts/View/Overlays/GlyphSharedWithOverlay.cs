using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GlyphSharedWithOverlay : MonoBehaviour {

	public GameObject sharedWithItemPrefab;

	public Button sharedWithScrim;

	public InputField searchField;
	public Button clearSearchFieldButton;

	public ScrollableList searchList;

	public AppStatusPanel statusPanel;

	private User[] searchedUsers;
	private Glyph currentGlyph;

	private UserSharedGlyphEvent.UserSharedGlyphEventList sharedList;

	// Use this for initialization
	void Start () {
		addButtonListeners ();
	}

	private void addButtonListeners() {
		searchField.onValueChanged.AddListener (onSearchFieldChange);
		clearSearchFieldButton.onClick.AddListener (onClearSearchButton);
		sharedWithScrim.onClick.AddListener (onScrim);
	}

	private void onScrim() {
		setVisible (false);
	}

	private void onClearSearchButton() {
		if (searchField.text.Equals (string.Empty)) {
			setVisible (false);
		} else {
			searchField.text = string.Empty;
		}
	}
		
	private void onSearchFieldChange(string search) {
		search = search.Trim ().ToLower ();

		if (search.Equals (string.Empty)) {
			resetListToDefault ();
		} else {
			doSearch (search);
		}
	}

	private void resetListToDefault() {
		List<User> receivedUsers = new List<User> ();

		List<Friendship> friends = LoggedInUser.GetLoggedInUser ().GetAcceptedFriendships ();

		foreach (Friendship friend in friends) {
			receivedUsers.Add(friend.GetOtherUser());
		}

		searchedUsers = receivedUsers.ToArray ();
		refreshList ();
	}

	private void doSearch(string search) {
		List<User> receivedUsers = new List<User>();

		foreach (Friendship friend in LoggedInUser.GetLoggedInUser().GetAcceptedFriendships()) {
			if (friend.GetOtherUser().GetUserFriendlyName ().ToLower().Contains (search)) {
				receivedUsers.Add (friend.GetOtherUser());
			}
		}

		searchedUsers = receivedUsers.ToArray();
		refreshList ();
	}
		
	private void refreshList() {
		searchList.removeAllElements ();
		foreach (User user in searchedUsers) {
			GameObject go = Instantiate (sharedWithItemPrefab);
			GlyphSharedWithListItem newItem = go.GetComponent<GlyphSharedWithListItem> ();
			newItem.statusPanel = statusPanel;
			newItem.SetGlyphShareValues (currentGlyph, user);

			searchList.addElement ((ScrollableList.ListGameObject)newItem);
		}

	}
		
	public void showSharesForGlyph(Glyph glyph) {
		setVisible (true);
		currentGlyph = glyph;
		StartCoroutine (RetrieveUsersGlyphIsSharedWith());
	}

	public void setVisible(bool visible) {
		sharedWithScrim.gameObject.SetActive (visible);
		searchList.removeAllElements ();
		searchField.text = string.Empty;
	}

	private IEnumerator RetrieveUsersGlyphIsSharedWith() {
		ServerCall getUsersCall = new ServerCall (ServerInteract.INSTANCE.GetUsersGlyphIsSharedWith(currentGlyph));
		yield return StartCoroutine (getUsersCall.call());

		if (getUsersCall.ReturnException != null) {
			throw getUsersCall.ReturnException;
		} else {
			UserSharedGlyphEvent.UserSharedGlyphEventList userList = (UserSharedGlyphEvent.UserSharedGlyphEventList) getUsersCall.ObjectResponse;
			currentGlyph.sharedEvents = userList.sharedWith;
			resetListToDefault ();
		}

		yield return "Done";
	}
}
