using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class AddGlyphMapBoxMinimap : MonoBehaviour {

	private static readonly float MINIMAP_ASPECT_RATIO = 1.0f;
	private static readonly float BOTTOM_RIGHT_SCREEN_DIM = 24.0f / Screen.width;
	private static readonly float TOP_LEFT_SCREEN_DIM = 1.0f;
	private static readonly float SCREEN_ASPECT_RATIO = (float)Screen.width / (float)Screen.height;

	private static readonly float CONSTANT_X = BOTTOM_RIGHT_SCREEN_DIM;
	private static readonly float CONSTANT_Y = BOTTOM_RIGHT_SCREEN_DIM * SCREEN_ASPECT_RATIO;
	private static readonly float SMALL_RECT_WIDTH = 0.23f;
	private static readonly float LARGE_RECT_WIDTH = 0.5f;


	private static Rect calculateSmallRect() {
		float width = SMALL_RECT_WIDTH;
		float height = calculateHeight (width);

		return new Rect (CONSTANT_X, CONSTANT_Y, width, height);
	}

	private static Rect calculateLargeRect() {
		float width = LARGE_RECT_WIDTH;
		float height = calculateHeight (width);

		return new Rect (CONSTANT_X, CONSTANT_Y, width, height);
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
			miniMapCamera.rect = calculateLargeRect();
		} else {
			miniMapCamera.rect = calculateSmallRect();
		}
	}

	private void plotGlyph(Glyph glyph) {
		GameObject newObject = Instantiate (glyphPOIObject);
		//PositionGlyphWithLocationProvider location = newObject.GetComponent<PositionGlyphWithLocationProvider> ();
		//location.setGlyph (glyph);
		plottedGlyphs.Add (glyph);
	}

}
