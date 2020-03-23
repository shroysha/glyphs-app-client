using System;
using UnityEngine;
using Object = System.Object;

[Serializable] public class SharedWithUserGlyphEvent : GlyphEventInTime, ISerializationCallbackReceiver {

	[SerializeField] private int sharedGlyphId = -1;
	[SerializeField] private string dateSharedTimestamp = null;

	private Glyph sharedGlyph = null;
	private DateTime dateShared = DateTime.MinValue;

	public void OnAfterDeserialize () {
		if (!string.IsNullOrEmpty (dateSharedTimestamp)) {
			dateShared = Convert.ToDateTime (dateSharedTimestamp);
		}
	}

	public void OnBeforeSerialize() { 
		// Link local reference to the correct glyph in the cache
		if (sharedGlyph == null) {
			sharedGlyph = LoggedInUser.GetGlyphFromCache (sharedGlyphId);
		}
	}

	public override Glyph GetGlyph() {
		return sharedGlyph;
	}

	public override DateTime GetDateEventOccured() {
		return dateShared;
	}

	public override bool Equals(Object obj) {
		if (obj == null) {
			return false;
		}

		if (obj is SharedWithUserGlyphEvent) {
			SharedWithUserGlyphEvent otherShare = (SharedWithUserGlyphEvent)obj;
			return this.sharedGlyph.Equals(otherShare.sharedGlyph);
		}

		return false;
	}

	public override string ToString() {
		return JsonUtility.ToJson (this);
	}

}
