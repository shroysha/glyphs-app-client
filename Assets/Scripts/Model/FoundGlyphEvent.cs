using System;
using UnityEngine;

[Serializable] public class FoundGlyphEvent : GlyphEventInTime, ISerializationCallbackReceiver {

	[SerializeField] private int foundGlyphId = -1;
	[SerializeField] private string dateFoundTimestamp = null;

	private Glyph foundGlyph = null;
	private DateTime dateFound = DateTime.MinValue;

	public void OnAfterDeserialize () {
		if (!string.IsNullOrEmpty (dateFoundTimestamp)) {
			dateFound = Convert.ToDateTime (dateFoundTimestamp);
		}
	}

	public void OnBeforeSerialize() { 
		// Link local reference to the correct glyph in the cache.
		// This method is called after the glyph cache is populated
		if (foundGlyph == null) {
			foundGlyph = LoggedInUser.GetGlyphFromCache (foundGlyphId);
		}
	}

	public override Glyph GetGlyph() {
		return foundGlyph;
	}

	public override DateTime GetDateEventOccured() {
		return dateFound;
	}

	public override string ToString() {
		return JsonUtility.ToJson (this, true);
	}

}

