using System;
using UnityEngine;

[Serializable] public class Friendship : ISerializationCallbackReceiver {

	[SerializeField] private string toUserId;
	[SerializeField] private string fromUserId;
	[SerializeField] private string dateSentTimestamp;
	[SerializeField] private string dateAcceptedTimestamp;

	private DateTime dateTimeSent = DateTime.MinValue;
	private DateTime dateTimeAccepted = DateTime.MinValue;

	private User fromUser = null;
	private User toUser = null;
	private User otherUser = null;

	public void OnAfterDeserialize() {
		if (!string.IsNullOrEmpty (dateSentTimestamp)) {
			dateTimeSent = Convert.ToDateTime (dateSentTimestamp);
		}

		if (!string.IsNullOrEmpty (dateAcceptedTimestamp)) {
			dateTimeAccepted = Convert.ToDateTime (dateAcceptedTimestamp);
		}
	}

	public void OnBeforeSerialize() {
		if (fromUser == null) {
			fromUser = LoggedInUser.GetUserFromCache (fromUserId);
		}

		if (toUser == null) {
			toUser = LoggedInUser.GetUserFromCache (toUserId);
		}

		if (otherUser == null) {
			if (fromUser is LoggedInUser) {
				otherUser = toUser;
			} else if (toUser is LoggedInUser) {
				otherUser = fromUser;
			}
		}
	}

	public bool IsPending() {
		return dateTimeAccepted.Equals(DateTime.MinValue);
	}

	public bool IsAccepted() {
		return !dateTimeAccepted.Equals(DateTime.MinValue);
	}

	public bool IsRequestSentPending() {
		return fromUser is LoggedInUser && IsPending();
	}

	public bool IsRequestReceivedPending() {
		return toUser is LoggedInUser && IsPending();
	}

	public override string ToString() {
		return JsonUtility.ToJson (this, true);
	}

	public User GetFromUser() {
		return fromUser;
	}

	public User GetToUser() {
		return toUser;
	}

	public User GetOtherUser() {
		return otherUser;
	}

}


