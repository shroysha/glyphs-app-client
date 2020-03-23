using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;

[Serializable] public class LoggedInUser : User {

	private static LoggedInUser INSTANCE;

	public static LoggedInUser GetLoggedInUser() {
		return INSTANCE;
	}

	public static void ClearLoggedInUser() {
		INSTANCE = null;
	}

	public static void ParseFullUserProfile(string userProfileText) {
		userProfileText = Regex.Unescape (userProfileText);

		INSTANCE = JsonUtility.FromJson<LoggedInUser> (userProfileText);
		ReplaceLoggedInUserInCache (INSTANCE);

		Debug.Log (INSTANCE.ToString ());
	}
		
	public static Glyph GetGlyphFromCache(int searchFor) {
		foreach (Glyph cachedGlyph in INSTANCE.glyphCache) {
			if(cachedGlyph.GetGlyphId() == searchFor) {
				return cachedGlyph;
			}
		}

		return null;
	}

	public static User GetUserFromCache(string searchFor) {
		foreach (User cachedUser in INSTANCE.userCache) {
			if(cachedUser.GetUserId().Equals(searchFor)) {
				return cachedUser;
			}
		}

		throw new Exception ("User not found in cache: " + searchFor);
	}

	public static User GetUserFromCache(User searchFor) {
		foreach (User cachedUser in INSTANCE.userCache) {
			if(cachedUser.Equals(searchFor)) {
				return cachedUser;
			}
		}

		INSTANCE.userCache.Add (searchFor);

		return searchFor;
	}

	public static void ReplaceLoggedInUserInCache(LoggedInUser liu) {
		INSTANCE.userCache.Remove (liu);
		INSTANCE.userCache.Add (liu);
	}

	[SerializeField] public List<Friendship> friends = new List<Friendship> ();
	[SerializeField] public List<OwnedGlyphEvent> ownedGlyphs = new List<OwnedGlyphEvent> ();
	[SerializeField] public List<FoundGlyphEvent> foundGlyphs = new List<FoundGlyphEvent> ();
	[SerializeField] public List<SharedWithUserGlyphEvent> sharedWithUserGlyphs = new List<SharedWithUserGlyphEvent> ();
	[SerializeField] public List<Glyph> glyphCache = new List<Glyph> ();
	[SerializeField] public List<User> userCache = new List<User> ();


	public void SetSearchableHandle(string handle) {
		this.searchableHandle = handle;
	}

	public bool HasFriendship(User user) {
		return GetFriendshipWithUser (user) != null;
	}

	public Friendship GetFriendshipWithUser(User user) {
		foreach (Friendship friend in friends) {
			if (friend.GetOtherUser().Equals (user)) {
				return friend;
			}
		}

		return null;
	}

	public List<Friendship> GetAcceptedFriendships() {
		List<Friendship> accepted = new List<Friendship> ();

		foreach (Friendship friend in friends) {
			if (friend.IsAccepted ()) {
				accepted.Add (friend);
			}
		}

		return accepted;
	}

	public List<Friendship> GetSentFriendships() {
		List<Friendship> sent = new List<Friendship> ();

		foreach (Friendship friend in friends) {
			if (friend.IsRequestSentPending()) {
				sent.Add (friend);
			}
		}

		return sent;
	}

	public List<Friendship> GetReceivedFriendships() {
		List<Friendship> received = new List<Friendship> ();

		foreach (Friendship friend in friends) {
			if (friend.IsRequestReceivedPending()) {
				received.Add (friend);
			}
		}

		return received;
	}

	public bool DidAlreadyFindGlyph(Glyph glyph) {
		foreach(FoundGlyphEvent foundGlyphEvent in foundGlyphs) {
			if(foundGlyphEvent.GetGlyph().Equals(glyph)) {
				return true;
			}
		}

		return false;
	}

	public override string ToString() {
		return JsonUtility.ToJson (this, true);
	}

}