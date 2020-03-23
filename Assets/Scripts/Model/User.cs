using System;
using UnityEngine;

[Serializable] public class User : ISerializationCallbackReceiver {

	[SerializeField] private string userId = null;
	[SerializeField] protected string searchableHandle = null;
	[SerializeField] private string userAvatarLocation = null;
	[SerializeField] private string dateCreatedTimestamp = null;

	private DateTime dateCreated = DateTime.MinValue;

	public void OnAfterDeserialize () {
		if (!string.IsNullOrEmpty (dateCreatedTimestamp)) {
			dateCreated = Convert.ToDateTime (dateCreatedTimestamp);
		}
	}

	public void OnBeforeSerialize() {}

	public string GetUserId() {
		return this.userId;
	}

	public string GetSearchableHandle() {
		return this.searchableHandle;
	}
		
	public string GetUserFriendlyName() {
		if (!string.IsNullOrEmpty(this.GetSearchableHandle())) {
			return this.GetSearchableHandle();
		} else {
			return this.GetUserId();
		}
	}

	public string GetUserAvatarLocation() {
		return this.userAvatarLocation;
	}

	public DateTime GetDateCreated() {
		return this.dateCreated;
	}

	public override bool Equals (object obj) {
		if (obj == null) {
			return false;
		}

		if (obj is User) {
			User otherUser = (User) obj;
			return this.GetUserId().Equals(otherUser.GetUserId()); 
		} 

		return false;
	}

	public override string ToString() {
		return JsonUtility.ToJson (this);
	}

}
