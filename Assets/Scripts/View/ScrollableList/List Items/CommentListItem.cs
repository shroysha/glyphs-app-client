using UnityEngine;
using UnityEngine.UI;

public class CommentListItem : MonoBehaviour, ScrollableList.ListGameObject {

	[HideInInspector] public UserAvatarImage avatarImage;
	[HideInInspector] public Text commentText;
	[HideInInspector] public Button actionButton;

	private Glyph parentGlyph;
	private GlyphComment comment;

	void Awake() {
		AssignInternalComponentReferences ();
	}

	private void AssignInternalComponentReferences() {
		avatarImage = transform.Find ("CommentUserAvatar").GetComponent<UserAvatarImage> ();
		commentText = transform.Find ("CommentText").GetComponent<Text> ();
		actionButton = transform.Find ("CommentActionButton").GetComponent<Button> (); 
	}

	public void SetComment(GlyphComment comment, Glyph glyph) {
		parentGlyph = glyph;
		this.comment = comment;

		RepopulateUI ();
	}

	private void RepopulateUI() {
		avatarImage.SetUser (comment.GetUserCommentedBy ());
		commentText.text = comment.GetUsersComment();
	}

	public GameObject GetListObject() {
		return this.gameObject;
	}

	public RectTransform GetListObjectTransform() {
		return this.gameObject.GetComponent<RectTransform> ();
	}

}
