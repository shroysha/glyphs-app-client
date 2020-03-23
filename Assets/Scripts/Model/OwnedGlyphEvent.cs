using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable] public class OwnedGlyphEvent : GlyphEventInTime, ISerializationCallbackReceiver  {

	[SerializeField] private int ownedGlyphId = -1;
	[SerializeField] private string dateCreatedTimestamp = null;

	private Glyph ownedGlyph = null;
	private DateTime dateCreated = DateTime.MinValue;

	public void OnAfterDeserialize () {
		if (!string.IsNullOrEmpty (dateCreatedTimestamp)) {
			dateCreated = Convert.ToDateTime (dateCreatedTimestamp);
		}
	}

	public void OnBeforeSerialize() { 
		// Link local reference to the correct glyph in the cache
		if (ownedGlyph == null) {
			ownedGlyph = LoggedInUser.GetGlyphFromCache (ownedGlyphId);
		}
	}

	public override Glyph GetGlyph() {
		return ownedGlyph;
	}

	public override DateTime GetDateEventOccured() {
		return dateCreated;
	}

	public override string ToString() {
		return JsonUtility.ToJson (this, true);
	}

	public class GlyphEventInTimeSorter : IComparer<OwnedGlyphEvent> {
		public int Compare(OwnedGlyphEvent one, OwnedGlyphEvent two) {
			return one.GetDateEventOccured ().CompareTo (two.GetDateEventOccured ());
		}
	}

}

