using UnityEngine;
using UnityEngine.UI;

public class AcceptedFriendListItem : MonoBehaviour, ScrollableList.ListGameObject {

	public UserProfile userProfile;

	[HideInInspector] public UserAvatarImage avatarImage;
	[HideInInspector] public Text usernameText;
	[HideInInspector] public Button moreInfoButton;
	[HideInInspector] public Button menuItemButton;

	private Friendship friend;

	void Awake() {
		AssignInternalComponentReferences ();
	}

	private void AssignInternalComponentReferences() {
		avatarImage = transform.Find ("AvatarContainer/AcceptedAvatarImage").GetComponent<UserAvatarImage> ();
		usernameText = transform.Find ("AcceptedUsernameText").GetComponent<Text> ();
		moreInfoButton = transform.Find ("MoreInfoButton").GetComponent<Button> (); 
		menuItemButton = this.GetComponent<Button> ();
	}

	// Use this for initialization
	void Start () {
		AddButtonListeners ();
	}

	private void AddButtonListeners () {
		menuItemButton.onClick.AddListener (OnMenuItemButton);
		moreInfoButton.onClick.AddListener (OnMenuItemButton);
	}
		
	private void OnMenuItemButton() {		
		userProfile.ShowUser (friend.GetOtherUser());
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
