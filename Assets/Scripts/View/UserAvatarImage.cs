using System.Collections;
using UnityEngine.UI;

public class UserAvatarImage : RawImage {

	public void SetUser(User user) {
		StartCoroutine (UpdateAvatarImage (user));
	}

	public IEnumerator UpdateAvatarImage(User user) {
		ServerCall avatarCall = new ServerCall (ServerInteract.INSTANCE.ApplyAvatarTextureToImage (user, this));
		yield return StartCoroutine (avatarCall.call ());
	}

}
