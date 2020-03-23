using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class SentFriendListItem : MonoBehaviour, ScrollableList.ListGameObject {

	public UserProfile userProfile;

	[HideInInspector] public UserAvatarImage avatarImage;
	[HideInInspector] public Text usernameText;
	[HideInInspector] public Button cancelButton;
	[HideInInspector] public Button menuItemButton;

	private Friendship friend;

	void Awake() {
		AssignInternalComponentReferences ();
	}

	private void AssignInternalComponentReferences() {
		avatarImage = transform.Find ("AvatarContainer/SentAvatarImage").GetComponent<UserAvatarImage> ();
		usernameText = transform.Find ("SentUsernameText").GetComponent<Text> ();
		cancelButton = transform.Find ("CancelButton").GetComponent<Button> (); 
		menuItemButton = this.GetComponent<Button> ();
	}
		
	// Use this for initialization
	void Start () {
		AddButtonListeners ();
	}

	private void AddButtonListeners () {		
		menuItemButton.onClick.AddListener (OnMenuItemButton);
		cancelButton.onClick.AddListener (OnCancelButton);
	}
		
	private void OnMenuItemButton() {
		userProfile.ShowUser (friend.GetOtherUser());
	}

	private void OnCancelButton() {
		StartCoroutine (DoCancelRequest());
	}

	private IEnumerator DoCancelRequest() {
		ServerCall call = new ServerCall (ServerInteract.INSTANCE.CancelSentFriendRequest(friend));
		yield return StartCoroutine (call.call ());

		LoadFriendsScene.INSTANCE.refreshList ();

		yield return "Done";
	}


	public void SetFriendship(Friendship friendship) {
		friend = friendship;
		avatarImage.SetUser (friend.GetOtherUser());
		usernameText.text = friend.GetOtherUser().GetUserFriendlyName ();
	}

	public GameObject GetListObject () {
		return this.gameObject;
	}

	public RectTransform GetListObjectTransform() {
		return this.gameObject.GetComponent<RectTransform> ();
	}

}
