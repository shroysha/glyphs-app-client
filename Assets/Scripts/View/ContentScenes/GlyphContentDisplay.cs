using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Object = System.Object;

public class GlyphContentDisplay : MonoBehaviour {
	
	private static readonly Vector3 HIDDEN_SCALE = Vector3.zero;
	private static readonly Vector3 VISIBLE_SCALE = Vector3.one;

	private static readonly float VISIBLE_ANIMATION_TIME = 0.2f;
	private static readonly float HIDDEN_ANIMATION_TIME = 0.1f;
	private static readonly float NUMBER_OF_ANIMATION_STEPS = 20.0f;

	public RectTransform contentPanelTransform;

	public Button closeButton;

	public RawImage glyphContentImage;

	public RectTransform glyphContentImageTransform;
	public RectTransform contentContainerTransform;

	public Button showCommentsButton;
	public CommentsOverlay commentOverlay;

	public Button showSharesButton;
	public GlyphSharedWithOverlay sharedWithOverlay;

	public AppStatusPanel statusPanel;

	private Glyph currentGlyph;

	// Use this for initialization
	void Start () {
		addButtonListeners ();
	}

	private void addButtonListeners() {
		closeButton.onClick.AddListener (onCloseButton);
		showCommentsButton.onClick.AddListener (onCommentsButton);
		showSharesButton.onClick.AddListener (onShowSharesButton);
	}

	private void onCloseButton() {
		hideGlyphContentDisplay ();
	}

	private void onCommentsButton() {
		commentOverlay.showCommentsForGlyph (currentGlyph);
	}

	private void onShowSharesButton() {
		sharedWithOverlay.showSharesForGlyph (currentGlyph);
	}

	public void showGlyph(Glyph glyph) {
		currentGlyph = glyph;

		setGlyphContent (glyph);
		showGlyphContentDisplay ();
	}
		
	private void setGlyphContent(Glyph glyph) {
		StartCoroutine (doRenderForGlyph (glyph));
	}

	private IEnumerator doRenderForGlyph(Glyph glyph) {
		Debug.Log ("Render: Loading " + ApplicationFileManager.GetLocalDownloadPath(glyph));

		if (!ApplicationFileManager.IsGlyphDownloaded (glyph)) {
			Debug.Log ("Render: Downloading"); 
			ServerCall download = new ServerCall(ServerInteract.DownloadGlyphFromFirebase(glyph));
			yield return StartCoroutine(download.call ());

			if (download.ReturnException != null) {
				statusPanel.showErrorStatus (download.ReturnException.Message);
				throw download.ReturnException;
			}
			//yield return StartCoroutine (ServerInteract.downloadObjectFromAWS (glyph.getPathToContent (), LoadUserData.getLoggedInUser ().getAWSAuthToken ()));
			Debug.Log ("Render: Downloading finished"); 
		} else {
			Debug.Log ("Render: Glyph already downloaded");
		}

		string extension = ApplicationFileManager.GetFileExtension(glyph.GetPathToContent());

		if (extension.Equals (ApplicationFileManager.PNG_EXTENSION)) {
			Debug.Log ("Render: Loading PNG File");
			renderPNGGlyph (glyph);
			Debug.Log ("Render: Finished");
		} else {
			Debug.LogError ("Render: Unknown Render Type " + extension);
		}

		yield return "Done";
	}

	private void renderPNGGlyph(Glyph glyph) {
		string localPath = ApplicationFileManager.GetLocalDownloadPath (glyph);
		Debug.Log ("Render: Loading " + localPath);
		Texture2D tex = ApplicationFileManager.LoadTextureFromPNGFile (localPath);

		if (tex == null) {
			Debug.LogError ("Render: Couldn't find downloaded file");
		} else {
			Debug.Log ("Render: File loaded, setting image");
			setGlyphContentDisplayImage (tex);
		}
	}

	private void setGlyphContentDisplayImage (Texture2D texture) {
		if (texture == null) {
			return;
		}

		glyphContentImage.texture = texture;
		glyphContentImageTransform.sizeDelta = new Vector2(texture.width, texture.height);

		float widthScale = contentContainerTransform.rect.width / glyphContentImageTransform.sizeDelta.x;
		float heightScale = contentContainerTransform.rect.height / glyphContentImageTransform.sizeDelta.y;
		float scale = Mathf.Min (widthScale, heightScale);

		glyphContentImageTransform.localScale = new Vector3 (scale, scale, scale);
	}

	public void showGlyphContentDisplay() {

		if (contentPanelTransform.localScale.Equals (VISIBLE_SCALE)) {
			Debug.Log("Already visible");
		} else {
			setGlyphContentScale(VISIBLE_SCALE);
		}
	}

	public void hideGlyphContentDisplay() {

		if (contentPanelTransform.localScale.Equals (HIDDEN_SCALE)) {
			Debug.Log("Already hidden");
		} else {
			setGlyphContentScale(HIDDEN_SCALE);
		}
	}


	private Object currentAnimation = null;
	private Vector3 initialScaleDifference;

	private void setGlyphContentScale(Vector3 desiredScale) {
		contentPanelTransform.localScale = desiredScale;
	}


}
