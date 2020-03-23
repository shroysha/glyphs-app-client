using System.Collections.Generic;
using UnityEngine;

public class LoadSharedGlyphsScene : MonoBehaviour {

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
			
		List<SharedWithUserGlyphEvent> glyphShares = LoggedInUser.GetLoggedInUser().sharedWithUserGlyphs;
		Glyph[] glyphs = new Glyph[glyphShares.Count];

		for (int i = 0; i < glyphShares.Count; i++) {
			glyphs [i] = glyphShares [i].GetGlyph();
		}

		glyphList.setGlyphs (glyphs);
	}

}
