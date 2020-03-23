using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UserProfile : MonoBehaviour {

	private static readonly string CLOSE_TEXT = "Is Self";
	private static readonly string SEND_FRIEND_REQUEST_TEXT = "Send Friend Request";
	private static readonly string CANCEL_FRIEND_REQUEST_TEXT = "Cancel Friend Request";
	private static readonly string ACCEPT_FRIEND_REQUEST_TEXT = "Accept Friend Request";
	private static readonly string REJECT_FRIEND_REQUEST_TEXT = "Reject Friend Request";
	private static readonly string REMOVE_FRIEND_TEXT = "Delete Friend";
	private static readonly string BLOCK_USER_TEXT = "Block User";

	public Button userProfileScrim;

	public UserAvatarImage avatarImage;
	public Text handleText;

	public Button editButton, closeButton, optionsButton;

	public Button extraScrim;
	public Button firstOptionButton, secondOptionButton, thirdOptionButton;
	public Text firstOptionButtonText, secondOptionButtonText, thirdOptionButtonText;

	public EditProfile editProfile;

	private User currentUser;

	private Dictionary<Button, string> isSelfMapping = new Dictionary<Button, string>(); 
	private Dictionary<Button, string> notFriendOptionMapping = new Dictionary<Button, string>(); 
	private Dictionary<Button, string> sentOptionMapping = new Dictionary<Button, string>();
	private Dictionary<Button, string> receivedOptionMapping = new Dictionary<Button, string>(); 
	private Dictionary<Button, string> friendOptionMapping = new Dictionary<Button, string>();

	private Dictionary<Button, string> currentOptionMapping;

	void Start() {
		CreateOptionButtonMappings ();
		AddButtonListeners ();
	}

	private void CreateOptionButtonMappings() {
		isSelfMapping.Add (firstOptionButton, CLOSE_TEXT);		

		notFriendOptionMapping.Add (firstOptionButton, SEND_FRIEND_REQUEST_TEXT);
		notFriendOptionMapping.Add (secondOptionButton, BLOCK_USER_TEXT);

		sentOptionMapping.Add (firstOptionButton, CANCEL_FRIEND_REQUEST_TEXT);
		sentOptionMapping.Add (secondOptionButton, BLOCK_USER_TEXT);

		receivedOptionMapping.Add (firstOptionButton, ACCEPT_FRIEND_REQUEST_TEXT);
		receivedOptionMapping.Add (secondOptionButton, REJECT_FRIEND_REQUEST_TEXT);
		receivedOptionMapping.Add (thirdOptionButton, BLOCK_USER_TEXT);

		friendOptionMapping.Add (firstOptionButton, REMOVE_FRIEND_TEXT);
		friendOptionMapping.Add (secondOptionButton, BLOCK_USER_TEXT);
	}

	private void AddButtonListeners() {
		userProfileScrim.onClick.AddListener (OnCloseButton);
		closeButton.onClick.AddListener (OnCloseButton);

		editButton.onClick.AddListener (OnEditButton);
		optionsButton.onClick.AddListener (OnShowOptionsButton);

		extraScrim.onClick.AddListener (OnExtraScrim);
		firstOptionButton.onClick.AddListener (OnFirstOptionButton);
		secondOptionButton.onClick.AddListener (OnSecondOptionButton);
		editProfile.saveButton.onClick.AddListener (UpdateUIElements);
	}

	private void OnCloseButton() {
		SetVisible (false);
	}

	private void OnEditButton() {
		editProfile.SetVisible (true);
	}

	private void OnShowOptionsButton() {
		extraScrim.gameObject.SetActive (true);
		RepopulateActionList ();
	}

	private void OnExtraScrim() {
		extraScrim.gameObject.SetActive (false);
	}

	private void OnFirstOptionButton() {

	}

	private void OnSecondOptionButton() {

	}

	public void ShowUser(User newUser) {
		currentUser = newUser;

		SetVisible (true);
		UpdateUIElements ();
	}

	public void UpdateUIElements() {
		avatarImage.SetUser (currentUser);
		handleText.text = currentUser.GetUserFriendlyName ();
	}

	private void RepopulateActionList() {
		currentOptionMapping = GetCorrectButtonMapping ();

		ApplyButtonMapping (firstOptionButton, firstOptionButtonText);
		ApplyButtonMapping (secondOptionButton, secondOptionButtonText);
		ApplyButtonMapping (thirdOptionButton, thirdOptionButtonText);
	}

	private void ApplyButtonMapping(Button button, Text textField) {
		if (currentOptionMapping.ContainsKey (button)) {
			string text;
			currentOptionMapping.TryGetValue (button, out text);

			button.gameObject.SetActive (true);
			textField.text = text;
		} else {
			button.gameObject.SetActive (false);
		}
	}

	private Dictionary<Button, string> GetCorrectButtonMapping() {
		if (currentUser is LoggedInUser) {
			return isSelfMapping;
		}

		Friendship friendshipWithUser = LoggedInUser.GetLoggedInUser ().GetFriendshipWithUser (currentUser);

		if (friendshipWithUser == null) {
			return notFriendOptionMapping;
		} else if (friendshipWithUser.IsAccepted ()) {
			return friendOptionMapping;
		} else if (friendshipWithUser.IsRequestSentPending ()) {
			return receivedOptionMapping;
		} else if (friendshipWithUser.IsRequestReceivedPending ()) {
			return sentOptionMapping;
		}  else {
			return null;
		}
	}

	public void SetVisible(bool visible) {
		if (visible) {
			userProfileScrim.gameObject.transform.localScale = Vector3.one;
		} else {
			userProfileScrim.gameObject.transform.localScale = Vector3.zero;
			extraScrim.gameObject.SetActive (false);
		}
	}
}
