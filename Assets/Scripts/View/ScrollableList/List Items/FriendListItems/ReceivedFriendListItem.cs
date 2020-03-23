using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class ReceivedFriendListItem : MonoBehaviour, ScrollableList.ListGameObject {

	public UserProfile userProfile;

	[HideInInspector] public UserAvatarImage avatarImage;
	[HideInInspector] public Text usernameText;
	[HideInInspector] public Button acceptRequestButton, rejectRequestButton;
	[HideInInspector] public Button menuItemButton;

	private Friendship friend;

	void Awake() {
		AssignInternalComponentReferences ();
	}

	private void AssignInternalComponentReferences() {
		avatarImage = transform.Find ("AvatarContainer/ReceivedAvatarImage").GetComponent<UserAvatarImage> ();
		usernameText = transform.Find ("ReceivedUsernameText").GetComponent<Text> ();
		acceptRequestButton = transform.Find ("ApproveButton").GetComponent<Button> (); 
		rejectRequestButton = transform.Find ("RejectButton").GetComponent<Button> (); 
		menuItemButton = this.GetComponent<Button> ();
	}

	// Use this for initialization
	void Start () {
		AddButtonListeners ();
	}

	private void AddButtonListeners () {
		menuItemButton.onClick.AddListener (OnMenuItemButton);
		acceptRequestButton.onClick.AddListener (OnAcceptRequestButton);
		rejectRequestButton.onClick.AddListener (OnRejectRequestButton);
	}
		
	private void OnMenuItemButton() {
		userProfile.ShowUser (friend.GetOtherUser());
	}

	private void OnAcceptRequestButton() {
		StartCoroutine (DoAcceptFriendRequest());
	}

	private void OnRejectRequestButton() {
		StartCoroutine (DoRejectFriendRequest());
	}

	private IEnumerator DoAcceptFriendRequest() {
		ServerCall call = new ServerCall (ServerInteract.INSTANCE.AcceptReceivedFriendRequest(friend));
		yield return StartCoroutine (call.call ());

		LoadFriendsScene.INSTANCE.refreshList ();

		yield return "Done";
	}

	private IEnumerator DoRejectFriendRequest() {
		ServerCall call = new ServerCall (ServerInteract.INSTANCE.RejectReceivedFriendRequest(friend));
		yield return StartCoroutine (call.call ());

		LoadFriendsScene.INSTANCE.refreshList ();

		yield return "Done";
	}

	public void SetFriendship(Friendship friendship) {
		if (avatarImage == null) {
			Debug.LogError ("Uh oh");
			return;
		}

		friend = friendship;
		avatarImage.SetUser (friendship.GetOtherUser ());
		usernameText.text = friend.GetOtherUser().GetUserFriendlyName ();
	}
		
	public GameObject GetListObject () {
		return this.gameObject;
	}

	public RectTransform GetListObjectTransform() {
		return this.gameObject.GetComponent<RectTransform> ();
	}

}
