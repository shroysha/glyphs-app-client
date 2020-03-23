using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GlyphSharedWithListItem : MonoBehaviour, ScrollableList.ListGameObject {

	public AppStatusPanel statusPanel;

	[HideInInspector] public UserAvatarImage avatarImage;
	[HideInInspector] public Text usernameText;
	[HideInInspector] public Toggle shareGlyphToggle;

	private User searchedUser;
	private Glyph sharedGlyph;

	void Awake() {
		AssignInternalComponentReferences ();
	}

	private void AssignInternalComponentReferences() {
		avatarImage = transform.Find ("SharedAvatar/SharedAvatarImage").GetComponent<UserAvatarImage> ();
		usernameText = transform.Find ("SharedUsernameText").GetComponent<Text> ();
		shareGlyphToggle = transform.Find ("IsSharedToggle").GetComponent<Toggle> (); 
	}

	// Use this for initialization
	void Start () {
		AddButtonListeners ();
	}

	private void AddButtonListeners () {
		shareGlyphToggle.onValueChanged.AddListener (OnShareGlyphToggle);
	}

	private void OnShareGlyphToggle(bool selected) {
		if (selected && !sharedGlyph.IsSharedWithUser (searchedUser)) {
			StartCoroutine (DoShareGlyphWithUser (sharedGlyph, searchedUser));
		} else if (!selected && sharedGlyph.IsSharedWithUser (searchedUser)) {
			StartCoroutine (DoStopShareGlyphWithUser (sharedGlyph, searchedUser));
		}
	}


	private IEnumerator DoShareGlyphWithUser(Glyph glyph, User user) {
		statusPanel.showStatus ("Sharing Glyph...");

		ServerCall call = new ServerCall (ServerInteract.INSTANCE.StartSharingGlyph(glyph, user));
		yield return StartCoroutine (call.call ());

		if (call.ReturnException != null) {
			statusPanel.showErrorStatus (call.ReturnException.Message);
			throw call.ReturnException;
		}

		statusPanel.showStatus ("Glyph Shared!");

		yield return new WaitForSeconds (1.0f);

		statusPanel.setStatusPanelVisible (false);

		yield return "Done";
	}

	private IEnumerator DoStopShareGlyphWithUser(Glyph glyph, User user) {
		statusPanel.showStatus ("Unsharing Glyph...");

		ServerCall call = new ServerCall (ServerInteract.INSTANCE.StopSharingGlyph(glyph, user));
		yield return StartCoroutine (call.call ());

		if (call.ReturnException != null) {
			statusPanel.showErrorStatus (call.ReturnException.Message);
			throw call.ReturnException;
		}

		statusPanel.showStatus ("Glyph Unshared!");

		yield return new WaitForSeconds (1.0f);

		statusPanel.setStatusPanelVisible (false);

		yield return "Done";
	}

	public void SetGlyphShareValues(Glyph glyph, User user) {
		sharedGlyph = glyph;
		searchedUser = user;

		avatarImage.SetUser (searchedUser);
		usernameText.text = searchedUser.GetUserFriendlyName ();
		shareGlyphToggle.isOn = glyph.IsSharedWithUser (user);
	}

	public GameObject GetListObject () {
		return this.gameObject;
	}

	public RectTransform GetListObjectTransform() {
		return this.gameObject.GetComponent<RectTransform> ();
	}

}
