using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GlyphPlaceHolder : MonoBehaviour {

	private static readonly float IMAGE_PADDING = 1.0f;

	public RectTransform placeHolderTransform;
	public RectTransform imageTransform;

	public GlyphContentDisplay contentDisplay;
	public RawImage glyphImage;
	public Button showGlyphContentOverlay;

	private float placeHolderHeight = 0.0f;
	private bool imageSet = false;
	private bool imageResized = false;
	private Glyph glyph;

	// Use this for initialization
	void Start () {
		addButtonListeners ();
	}

	private void addButtonListeners() {
		showGlyphContentOverlay.onClick.AddListener (onOverlayButton);
	}

	private void onOverlayButton() {
		contentDisplay.showGlyph (glyph);
	}

	public void setGlyph(Glyph glyph) {
		this.glyph = glyph;

		StartCoroutine(setGlyphImage (glyph));
	}

	private IEnumerator setGlyphImage(Glyph glyph) {
		if (!ApplicationFileManager.IsGlyphDownloaded (glyph)) {
			ServerCall call = new ServerCall(ServerInteract.DownloadGlyphFromFirebase (glyph));
			yield return StartCoroutine(call.call ());
		}

		Texture2D texture = ApplicationFileManager.LoadTextureFromPNGFile (ApplicationFileManager.GetLocalDownloadPath (glyph));
		glyphImage.texture = texture;
		imageSet = true;

		imageTransform.sizeDelta = new Vector2 (texture.width, texture.height);

		if (placeHolderHeight != 0.0f) {
			float imageScale = (placeHolderHeight - IMAGE_PADDING * 2.0f)  / texture.height;
			imageTransform.localScale = new Vector3 (imageScale, imageScale, imageScale);

			imageResized = true;
		}

		yield return "Done";
	}

	public void setPlaceHolderHeightForImage(float height) {
		placeHolderHeight = height;

		if (imageSet && !imageResized) {
			float imageScale = (placeHolderHeight - IMAGE_PADDING * 2.0f) / glyphImage.texture.height;
			imageTransform.localScale = new Vector3 (imageScale, imageScale, imageScale);

			imageResized = true;
		}
	}

	public string getTimeSince() {
		return "Time since coming soon!";
	}

}
