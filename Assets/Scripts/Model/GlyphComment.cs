using System;
using System.Collections.Generic;
using UnityEngine;
using Object = System.Object;

[Serializable] public class GlyphComment : GlyphEventInTime, ISerializationCallbackReceiver {
	
	[SerializeField] private int commentId = -1;
	[SerializeField] private string usersComment = null;
	[SerializeField] private User commentedBy = null;
	[SerializeField] private string dateCommentedTimestamp = null;

	private DateTime dateCommented = DateTime.MinValue;
	private bool hasBeenReserialized = false;

	public void OnAfterDeserialize() {
		// Populate dateCommented as soon as the object is deserialized
		if(!string.IsNullOrEmpty(dateCommentedTimestamp)) {
			dateCommented = Convert.ToDateTime (dateCommentedTimestamp);
		}
	}

	public void OnBeforeSerialize() {
		if (!hasBeenReserialized) {
			// Pass the deserialized user to LoggedInUser.GetUserFromCache to get the user singleton
			commentedBy = LoggedInUser.GetUserFromCache(commentedBy);
			hasBeenReserialized = true;
		}
	}
		
	public int GetCommentId() {
		return commentId;
	}

	public string GetUsersComment() {
		return usersComment;
	}

	public User GetUserCommentedBy() {
		return commentedBy;
	}

	public override Glyph GetGlyph() {
		return null;
	}

	public override DateTime GetDateEventOccured() {
		return dateCommented;
	}

	public override bool Equals(Object obj) {
		if (obj == null) {
			return false;
		}

		if (obj is GlyphComment) {
			GlyphComment otherComment = (GlyphComment)obj;
			return this.commentId == otherComment.commentId;
		}

		return false;
	}


	public override string ToString() {
		return JsonUtility.ToJson (this, true);
	}


	[Serializable] public class GlyphCommentList {

		[SerializeField] public List<GlyphComment> comments = null;

	}

}


