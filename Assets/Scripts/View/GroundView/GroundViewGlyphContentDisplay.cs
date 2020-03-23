using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using Object = System.Object;

public class GroundViewGlyphContentDisplay : MonoBehaviour, GlyphUniverse.RenderGlyphListener {

	private static readonly Vector3 HIDDEN_SCALE = new Vector3 (0.0f, 0.0f, 0.0f);
	private static readonly Vector3 VISIBLE_SCALE = new Vector3(1.0f, 1.0f, 1.0f);
	private static readonly float VISIBLE_ANIMATION_TIME = 0.2f;
	private static readonly float HIDDEN_ANIMATION_TIME = 0.1f;
	private static readonly float NUMBER_OF_ANIMATION_STEPS = 20.0f;

	private static readonly float SCREEN_PADDING = 20.0f;
	private static readonly float IMAGE_PADDING = 20.0f; // half for both sides

	public GlyphUniverse glyphUniverse;

	public RectTransform contentPanelTransform;

	public RectTransform contentContainerTransform;
	public RectTransform glyphContentImageTransform;

	public RawImage glyphContentImage;

	public Button closeButton;
	public Toggle pinToggle;

	public Button showCommentsButton;
	public CommentsOverlay commentOverlay;

	private bool isVisible = false;

	private Glyph currentGlyph;

	// Use this for initialization
	void Start () {
		interfaceWithGlyphUniverse ();
		addButtonListeners ();
	}

	private void addButtonListeners() {
		closeButton.onClick.AddListener (onCloseButton);
		pinToggle.isOn = false;
		showCommentsButton.onClick.AddListener (onShowCommentsButton);
	}

	private void onCloseButton() {
		pinToggle.isOn = false;
		hideGlyphContentDisplay ();
	}

	private void onShowCommentsButton() {
		pinToggle.isOn = true;
		commentOverlay.showCommentsForGlyph (currentGlyph);
	}

	private void interfaceWithGlyphUniverse() {
		glyphUniverse.addRenderGlyphListener (this);
	}
		
	public void startRenderringGlyph(Glyph glyph) {
		if (!pinToggle.isOn) {
			showGlyphContentDisplay ();
			setGlyphContent (glyph);
		}
	}

	public void stopRenderringGlyph() {
		if (isVisible && !pinToggle.isOn) {
			hideGlyphContentDisplay ();
		}
	}

	private void setGlyphContent(Glyph glyph) {
		currentGlyph = glyph;
		StartCoroutine (doRenderForGlyph (glyph));
	}

	private IEnumerator doRenderForGlyph(Glyph glyph) {
		Debug.Log ("Render: Loading " + ApplicationFileManager.GetLocalDownloadPath(glyph));

		if (!ApplicationFileManager.IsGlyphDownloaded (glyph)) {
			Debug.Log ("Render: Downloading"); 
			ServerCall download = new ServerCall(ServerInteract.DownloadGlyphFromFirebase(glyph));
			yield return StartCoroutine(download.call ());
			//yield return StartCoroutine (ServerInteract.downloadObjectFromAWS (glyph.getPathToContent (), LoadUserData.getLoggedInUser ().getAWSAuthToken ()));
			Debug.Log ("Render: Downloading finished"); 
		} else {
			Debug.Log ("Render: Glyph already downloaded");
		}

		string extension = ApplicationFileManager.GetFileExtension(glyph.GetPathToContent());

		if (extension.Equals (ApplicationFileManager.PNG_EXTENSION)) {
			Debug.Log ("Render: Loading PNG File");
			renderPNGGlyph (glyph);
		} else {
			Debug.Log ("Render: Unknown Render Type " + extension);
		}

		ServerCall markGlyphFound = new ServerCall (ServerInteract.INSTANCE.MarkGlyphAsFound(glyph));
		StartCoroutine (markGlyphFound.call ());

		Debug.Log ("Render: Finished");
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
			glyphContentImage.texture = null;
			return;
		}
			
		glyphContentImage.texture = texture;
		glyphContentImageTransform.sizeDelta = new Vector2(texture.width, texture.height);

		float widthScale = contentContainerTransform.rect.width / glyphContentImageTransform.sizeDelta.x;
		float heightScale = contentContainerTransform.rect.height  / glyphContentImageTransform.sizeDelta.y;

		float scale = Math.Min (widthScale, heightScale);
		glyphContentImageTransform.localScale = new Vector3 (scale, scale, scale);
	}
		
	public void showGlyphContentDisplay() {

		if (contentPanelTransform.localScale.Equals (VISIBLE_SCALE)) {
			Debug.Log("Already visible");
		} else {
			isVisible = true;
			StartCoroutine(setGlyphContentScale(VISIBLE_SCALE));
		}
	}

	public void hideGlyphContentDisplay() {

		if (contentPanelTransform.localScale.Equals (HIDDEN_SCALE)) {
			Debug.Log("Already hidden");
		} else {
			isVisible = false;
			StartCoroutine(setGlyphContentScale(HIDDEN_SCALE));
		}
	}
		
	private Object currentAnimation = null;
	private Vector3 initialScaleDifference;

	private IEnumerator setGlyphContentScale(Vector3 desiredScale) {
		Object thisAnimation = new Object ();
		currentAnimation = thisAnimation;

		initialScaleDifference = desiredScale - contentPanelTransform.localScale;
		float animationTime;
		if (desiredScale.Equals (VISIBLE_SCALE)) {
			animationTime = VISIBLE_ANIMATION_TIME;
		} else {
			animationTime = HIDDEN_ANIMATION_TIME;
		}
		float timeStep = animationTime / NUMBER_OF_ANIMATION_STEPS;
		Vector3 step = initialScaleDifference * (timeStep / animationTime);

		while (!contentPanelTransform.localScale.Equals (desiredScale)) {
			if (!ReferenceEquals (thisAnimation, currentAnimation)) {
				break;
			}

			contentPanelTransform.localScale += step;

			if (desiredScale.Equals (HIDDEN_SCALE) && contentPanelTransform.localScale.x < HIDDEN_SCALE.x) {
				contentPanelTransform.localScale = HIDDEN_SCALE;
			} else if (desiredScale.Equals (VISIBLE_SCALE) && contentPanelTransform.localScale.x > VISIBLE_SCALE.x) {
				contentPanelTransform.localScale = VISIBLE_SCALE;
			}

			yield return new WaitForSeconds (timeStep);
		}

		if(desiredScale.Equals(HIDDEN_SCALE) && ReferenceEquals (thisAnimation, currentAnimation)) {
			setGlyphContentDisplayImage (null);
		}

		yield return "Done";
	}
}
