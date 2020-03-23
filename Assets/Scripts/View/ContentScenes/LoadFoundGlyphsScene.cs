using System.Collections.Generic;
using UnityEngine;

public class LoadFoundGlyphsScene : MonoBehaviour {

	public ScrollableGlyphList glyphList;
	public GlyphContentDisplay glyphContentDisplay;

	// Use this for initialization
	void Start () {
		populateGlyphList ();
		glyphContentDisplay.showSharesButton.gameObject.SetActive (false);
	}

	private void populateGlyphList() {

		if (LoggedInUser.GetLoggedInUser () == null) {
			Debug.LogError ("No user logged in");
			return;
		}

		List<FoundGlyphEvent> foundGlyphs = LoggedInUser.GetLoggedInUser().foundGlyphs;
		foundGlyphs.Sort ();

		Glyph[] glyphs = new Glyph[foundGlyphs.Count];
		int i = 0;
		foreach (FoundGlyphEvent found in foundGlyphs) {
			glyphs [i] = foundGlyphs [i].GetGlyph();
			i++;
		}

		glyphList.setGlyphs (glyphs);
	}

}
