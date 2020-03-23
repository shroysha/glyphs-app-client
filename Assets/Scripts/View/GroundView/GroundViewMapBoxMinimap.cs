using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GroundViewMapBoxMinimap : MonoBehaviour, CloseGlyphsDownloader.GlyphDownloadListener {

	private static readonly float MINIMAP_ASPECT_RATIO = 1.0f;
	private static readonly float BOTTOM_RIGHT_SCREEN_DIM = 0.0f;
	private static readonly float TOP_LEFT_SCREEN_DIM = 1.0f;

	private static readonly float CONSTANT_Y = BOTTOM_RIGHT_SCREEN_DIM;
	private static readonly float SMALL_RECT_WIDTH = 0.45f;
	private static readonly float LARGE_RECT_WIDTH = 0.8f;

	private static readonly float SCREEN_ASPECT_RATIO = (float)Screen.width / (float)Screen.height;

	private static Rect calculateSmallRect() {
		float width = SMALL_RECT_WIDTH;
		float x=  TOP_LEFT_SCREEN_DIM - width;
		float height = calculateHeight (width);

		return new Rect (x, CONSTANT_Y, width, height);
	}

	private static Rect calculateLargeRect() {
		float width = LARGE_RECT_WIDTH;
		float x = TOP_LEFT_SCREEN_DIM - width;
		float height = calculateHeight (width);

		return new Rect (x, CONSTANT_Y, width, height);
	}

	private static float calculateHeight(float width) {
		return width * SCREEN_ASPECT_RATIO * MINIMAP_ASPECT_RATIO;
	}
		

	public GameObject glyphPOIObject;
	public Camera miniMapCamera;
	public Button switchRectButton;
	private List<Glyph> plottedGlyphs = new List<Glyph>();

	// Use this for initialization
	void Start () {
		setMinimapRect (calculateSmallRect());
		addButtonListeners ();
		loadGlyphs ();
		interfaceWithDownloadListener ();
	}

	private void setMinimapRect(Rect rect) {
		Debug.Log ("Setting minimap rect to: " + rect.ToString ());
		miniMapCamera.rect = rect;
	}

	private void addButtonListeners() {
		switchRectButton.onClick.AddListener (onSwitchRectButton);
	}

	private void onSwitchRectButton() {
		if (miniMapCamera.rect.Equals(calculateSmallRect())) {
			setMinimapRect(calculateLargeRect());
		} else {
			setMinimapRect(calculateSmallRect());
		}
	}

	private void loadGlyphs() {
		if (LoggedInUser.GetLoggedInUser () == null) {
			return;
		}

		foreach (OwnedGlyphEvent glyph in LoggedInUser.GetLoggedInUser ().ownedGlyphs) {
			plotGlyph (glyph.GetGlyph());
		}

	}

	private void interfaceWithDownloadListener() {
		if (LoggedInUser.GetLoggedInUser () != null) {
			CloseGlyphsDownloader.getInstance ().addGlyphDownloadedListener (this);
		}
	}

	public IEnumerator onDownloadedGlyph(Glyph glyph) {
		if (glyph != null && !plottedGlyphs.Contains (glyph)) {
			plotGlyph (glyph);
		}

		yield return "Done";
	}

	private void plotGlyph(Glyph glyph) {
		GameObject newObject = Instantiate (glyphPOIObject);
		//PositionGlyphWithLocationProvider location = newObject.GetComponent<PositionGlyphWithLocationProvider> ();
		//location.setGlyph (glyph);
		plottedGlyphs.Add (glyph);
	}

	public void OnDestroy() {
		CloseGlyphsDownloader.getInstance ().removeGlyphDownloadedListener (this);
	}

}
