using System;
using System.Collections.Generic;
using UnityEngine;
using Object = System.Object;

[Serializable] public class Glyph : ISerializationCallbackReceiver {

	[SerializeField] private int glyphId;
	[SerializeField] private GPSLocation gpsLocation;
	[SerializeField] private bool isPublic;
	[SerializeField] private string postDelayTimestamp;
	[SerializeField] private string ownerUserId;

	private DateTime postDelay;
	private User owner;

	[NonSerialized] public List<GlyphComment> comments;
	[NonSerialized] public List<UserSharedGlyphEvent> sharedEvents;

	public void OnAfterDeserialize () {
		if (!string.IsNullOrEmpty (postDelayTimestamp)) {
			postDelay = Convert.ToDateTime (postDelayTimestamp);
		}
	}

	public void OnBeforeSerialize() { 
		// Link local reference to the correct glyph in the cache
		if (owner == null) {
			owner = LoggedInUser.GetUserFromCache (ownerUserId);
		}
	}

	public int GetGlyphId() {
		return glyphId;
	}

	public GPSLocation GetGpsLocation() {
		return gpsLocation;
	}

	public bool IsPublic() {
		return isPublic;
	}

	public DateTime GetPostDelay() {
		return postDelay;
	}

	public User GetOwner() {
		return owner;
	}

	public string GetPathToContent() {
		return this.GetOwner().GetUserId() + "/" + this.GetGlyphId() + "/CanvasTexture.png";
	}

	public override bool Equals(Object obj) {
		if (obj == null) {
			return false;
		}

		if (obj is Glyph) {
			Glyph otherGlyph = (Glyph) obj;
			return this.GetGlyphId () == otherGlyph.GetGlyphId ();
		}

		return false;
	}
		
	public override string ToString() {
		return JsonUtility.ToJson (this, true);
	}

	public void SetComments(List<GlyphComment> comments) {
		this.comments = comments;
	}

	public bool IsSharedWithUser(User user) {
		foreach (UserSharedGlyphEvent sharedEvent in sharedEvents) {
			if (sharedEvent.GetUserSharedWith ().Equals (user)) {
				return true;
			}
		}

		return false;
	}



}
