using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Object = System.Object;

public class EditProfile : MonoBehaviour {

	private static readonly string HANDLE_TAKEN_ERROR = "Handle Taken";

	public Button editProfileScrim;

	public UserAvatarImage avatarImage;
	public InputField searchableHandleField;
	public Text errorText;
	public Button cancelButton, saveButton;

	public UserProfile parentUserProfile;

	private bool imageEdited = false, handleEdited = false;
	private Object uploadLock;

	// Use this for initialization
	void Start () {
		addListeners ();
		repopulateUI ();
		HideErrorText ();
	}

	private void addListeners() {
		editProfileScrim.onClick.AddListener (onCloseButton);
		cancelButton.onClick.AddListener (onCloseButton);
		saveButton.onClick.AddListener (onSaveButton);
		searchableHandleField.onValueChanged.AddListener (onSearchableHandleFieldUpdate);
	}

	private void onCloseButton() {
		SetVisible (false);
	}

	private void onSaveButton() {
		StartCoroutine (processChanges());
	}

	private void onSearchableHandleFieldUpdate(string text) {
		StartCoroutine (onHandleChanged (text));
	}

	private IEnumerator onHandleChanged(string handleText) {
		saveButton.interactable = false;
		Object myUploadLock = new Object ();
		uploadLock = myUploadLock;

		bool setHandleToDefault = handleText.Equals (LoggedInUser.GetLoggedInUser ().GetSearchableHandle());
		bool setHandleToUserID = handleText.Equals (LoggedInUser.GetLoggedInUser ().GetUserId());

		if (handleText.Equals (string.Empty) || setHandleToDefault) {
			handleEdited = false;
			HideErrorText ();
			saveButton.interactable = true;
		} else {
			Boolean handleTaken;
			if (!setHandleToUserID) {
				ServerCall handleTakenCall = new ServerCall (ServerInteract.INSTANCE.IsHandleTaken (handleText));
				yield return StartCoroutine (handleTakenCall.call ());
				handleTaken = (Boolean)handleTakenCall.ObjectResponse;
			} else {
				handleTaken = false;
			}

			Debug.Log ("Is handle taken??? " + handleTaken);

			if (handleTaken && ReferenceEquals(myUploadLock, uploadLock)) {
				ShowErrorText (HANDLE_TAKEN_ERROR);
				saveButton.interactable = false;
				handleEdited = false;
			} else {
				HideErrorText ();
				saveButton.interactable = true;
				handleEdited = true;
			}

		}

		yield return "Done";
	}

	private IEnumerator processChanges() {
		if (imageEdited) {
			ServerCall call = new ServerCall (ServerInteract.INSTANCE.uploadNewAvatar (avatarImage.texture));
			yield return StartCoroutine(call.call ());
		}

		if (handleEdited) {
			ServerCall call = new ServerCall (ServerInteract.INSTANCE.SetSearchableHandle (searchableHandleField.text));
			yield return StartCoroutine(call.call ());
		}

		SetVisible (false);

		parentUserProfile.UpdateUIElements ();
	}

	private void repopulateUI() {
		avatarImage.SetUser (LoggedInUser.GetLoggedInUser());

		searchableHandleField.text = LoggedInUser.GetLoggedInUser ().GetUserFriendlyName ();
	}

	public void SetVisible(bool visible) {
		editProfileScrim.gameObject.SetActive (visible);
	}

	private void HideErrorText() {
		errorText.gameObject.SetActive (false);
	}

	private void ShowErrorText(string text) {
		errorText.gameObject.SetActive (true);
		errorText.text = text;
	}


}
