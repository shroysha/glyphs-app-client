using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class CommentsOverlay : MonoBehaviour {

	public GameObject commentListItemPrefab;

	public Button commentOverlayScrim;
	public Button closeButton;

	public ScrollableList commentList;

	public InputField writeCommentField;
	public Button addCommentButton;

	public AppStatusPanel statusPanel;

	private Glyph currentGlyph;

	// Use this for initialization
	void Start () {
		addButtonListeners ();
	}

	private void addButtonListeners() {
		commentOverlayScrim.onClick.AddListener (onScrim);
		closeButton.onClick.AddListener (onScrim);
		addCommentButton.onClick.AddListener (onAddCommentButton);
	}

	private void onScrim() {
		setVisible (false);
	}

	public void showCommentsForGlyph(Glyph glyph) {
		currentGlyph = glyph;
		setVisible (true);

		StartCoroutine(refreshCommentList ());
	}

	public IEnumerator refreshCommentList() {
		ServerCall getCommentsCall = new ServerCall (ServerInteract.INSTANCE.GetCommentsOnGlyph(currentGlyph));
		yield return StartCoroutine (getCommentsCall.call());

		if (getCommentsCall.ReturnException != null) {
			throw getCommentsCall.ReturnException;
		} else {
			GlyphComment.GlyphCommentList glyphComments = (GlyphComment.GlyphCommentList)getCommentsCall.ObjectResponse;

			foreach (GlyphComment comment in glyphComments.comments) {
				GameObject go = Instantiate (commentListItemPrefab);
				CommentListItem listItem = go.GetComponent<CommentListItem> ();
				listItem.SetComment (comment, currentGlyph);

				commentList.addElement ((ScrollableList.ListGameObject)listItem);
			}
		}

		yield return "Done";
	}

	private void onAddCommentButton() {
		string newComment = writeCommentField.text.Trim();

		StartCoroutine(uploadNewComment(newComment));
	}

	private IEnumerator uploadNewComment(string newComment) {
		statusPanel.showLoadingStatus ("Uploading Comment...");
		ServerCall uploadCall = new ServerCall(ServerInteract.INSTANCE.AddCommentToGlyph(currentGlyph, newComment));
		yield return StartCoroutine (uploadCall.call ());

		if (uploadCall.ReturnException != null) {
			statusPanel.showErrorStatus (uploadCall.ReturnException.Message);
			throw uploadCall.ReturnException;
		}

		statusPanel.showStatus ("Comment Uploaded!");

		commentList.removeAllElements ();
		StartCoroutine(refreshCommentList ());

		writeCommentField.text = string.Empty;

		yield return new WaitForSeconds (1.0f);

		statusPanel.setStatusPanelVisible (false);

		yield return "Done";
	}

	public void setVisible(bool visible) {
		commentOverlayScrim.gameObject.SetActive (visible);
		commentList.removeAllElements ();
	}
}
