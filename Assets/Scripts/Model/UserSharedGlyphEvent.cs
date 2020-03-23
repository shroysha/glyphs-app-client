using System;
using System.Collections.Generic;
using UnityEngine;
using Object = System.Object;

[Serializable] public class UserSharedGlyphEvent : GlyphEventInTime, ISerializationCallbackReceiver  {

	[SerializeField] private User userSharedWith = null;
	[SerializeField] private string dateSharedTimestamp = null;

	private DateTime dateShared = DateTime.MinValue;
	private bool hasBeenReserialized = false;


	public void OnAfterDeserialize () {
		if (!string.IsNullOrEmpty (dateSharedTimestamp)) {
			dateShared = Convert.ToDateTime (dateSharedTimestamp);
		}
	}

	public void OnBeforeSerialize() { 
		// Link local reference to the correct glyph in the cache
		if (!hasBeenReserialized) {
			userSharedWith = LoggedInUser.GetUserFromCache (userSharedWith);
			hasBeenReserialized = true;
		}
	}

	public User GetUserSharedWith() {
		return userSharedWith;
	}

	public override Glyph GetGlyph() {
		return null;
	}

	public override DateTime GetDateEventOccured() {
		return dateShared;
	}

	public override bool Equals(Object obj) {
		if (obj == null) {
			return false;
		}

		if (obj is UserSharedGlyphEvent) {
			UserSharedGlyphEvent otherShare = (UserSharedGlyphEvent)obj;
			return this.GetGlyph().Equals(otherShare.GetGlyph());
		}

		return false;
	}

	public override string ToString() {
		return JsonUtility.ToJson (this);
	}

	[Serializable] public class UserSharedGlyphEventList {

		[SerializeField] public List<UserSharedGlyphEvent> sharedWith = null;

	}

}
