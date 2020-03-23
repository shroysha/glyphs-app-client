using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LoadMyGlyphsScene : MonoBehaviour {

	public ScrollableGlyphList glyphList;
	public Button addButton;

	// Use this for initialization
	void Start () {
		populateGlyphList ();
		addButtonListeners ();
	}

	private void populateGlyphList() {

		if (LoggedInUser.GetLoggedInUser() == null) {
			Debug.LogError ("No user logged in");
			return;
		}

		List<OwnedGlyphEvent> ownedGlyphs = LoggedInUser.GetLoggedInUser().ownedGlyphs;
		ownedGlyphs.Sort(new OwnedGlyphEvent.GlyphEventInTimeSorter());

		List<Glyph> glyphs = new List<Glyph> ();
		foreach (OwnedGlyphEvent ownedGlyphEvent in ownedGlyphs) {
			glyphs.Add (ownedGlyphEvent.GetGlyph());
		}

		glyphList.setGlyphs (glyphs.ToArray());

	}

	private void addButtonListeners() {
		addButton.onClick.AddListener (onAddButton);
	}

	private void onAddButton() {
		LoadTakePictureScene.setBackButtonScene ("Scenes/MyGlyphsScene");
		GeneralFunctions.switchToAddGlyphScene ();
	}

}
