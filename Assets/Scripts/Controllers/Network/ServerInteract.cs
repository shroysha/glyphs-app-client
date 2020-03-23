using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

public class ServerInteract : MonoBehaviour {

	public static ServerInteract INSTANCE;

	public Texture2D BLANK_DOWNLOAD_TEXTURE;

	void Awake() {
		DontDestroyOnLoad (this.gameObject);
		INSTANCE = this;
	}

	private static readonly string EMPTY_JSON_ARRAY = "[]";

	public static readonly string BASEURL = "http://www.shawnsglyphproject.com";
	public static readonly string FUNCTIONSLOCATION = BASEURL +  "/dev/functions";

	private static readonly string AUTH0_CLIENT_ID = "ZMBm759muZ6K5sVsg0xdgExZyyMmfidv";
	public static readonly string AUTH0_BASE_URL = "https://shroysha.auth0.com";

	public static readonly string AUTH0FUNCTIONLOCATION = FUNCTIONSLOCATION + "/auth0";	
	public static readonly string TEST_LOGIN = AUTH0FUNCTIONLOCATION + "/load-user.php";
	public static readonly string LOGOUT_URL = AUTH0FUNCTIONLOCATION + "/logout.php";
	public static readonly string MANUALLOGIN = AUTH0_BASE_URL + "/login?client=ZMBm759muZ6K5sVsg0xdgExZyyMmfidv";
	public static readonly string AUTHORIZEURL = AUTH0_BASE_URL + "/authorize?scope=openid&response_type=code&client_id=ZMBm759muZ6K5sVsg0xdgExZyyMmfidv";
	public static readonly string AUTH0_DELEGATE_URL = AUTH0_BASE_URL + "/delegation";

	public IEnumerator SendServerInformationRequest(string location, Dictionary<string, string> data) {
		// to ensure the app cannot send requests to other websites
		if (!location.StartsWith (BASEURL) && !location.StartsWith (AUTH0_BASE_URL)) {
			throw new Exception ("Error: Invalid URL");
		}

		if (Authentication.authToken == null) {
			throw new Exception ("Error: No logged in user");
		}
			
		// Create a Web Form
		WWWForm form = new WWWForm();
		// Add the authorization token to the form using the Authorization request header
		Dictionary<string, string> headers = form.headers;
		headers["Authorization"] = Authentication.authToken;

		form.AddField ("null","null");

		if (data != null) {
			foreach (string key in data.Keys) {
				form.AddField(key, data[key]);
			}
		}

		ServerCall call = new ServerCall(DownloadContent(location, form.data, headers));
		yield return StartCoroutine (call.call ());


		if (call.ReturnException != null) {
			throw call.ReturnException;
		}

		yield return call.ObjectResponse;
	}


	private static IEnumerator DownloadContent(string location, byte[] postData, Dictionary<string, string> headers) {
		Debug.Log ("Download started");

		WWW currentDownload = new WWW (location, postData, headers);
		while (!currentDownload.isDone && currentDownload.error == null) {
			yield return null;
		}

		Debug.Log ("Download finished");
		if (!string.IsNullOrEmpty(currentDownload.error)) {  // If the web request returned an error, return the error...
			Debug.LogError("Error when trying to reach " + location);

			Debug.LogError(currentDownload.error);
			Debug.LogError (currentDownload.text);

			throw new Exception(currentDownload.error.Trim());
		}
		else {	// otherwise, return the text content of the web request
			yield return currentDownload.text.Trim();
		}
	}


	public static readonly string USERFUNCTIONLOCATION = FUNCTIONSLOCATION + "/user";
	public static readonly string GET_LOGGED_IN_USER_INFO = USERFUNCTIONLOCATION + "/GetLoggedInUserInfo.php";
	public static readonly string SET_SEARCHABLE_HANDLE = USERFUNCTIONLOCATION + "/SetSearchableHandle.php";
	public static readonly string PERMANENTLY_DELETE_USER = USERFUNCTIONLOCATION + "/deleteuser.php";
	public static readonly string SEARCH_USER = USERFUNCTIONLOCATION + "/SearchUser.php";
	public static readonly string SEARCHABLE_HANDLE_TAKEN = USERFUNCTIONLOCATION + "/IsHandleTaken.php";

	public IEnumerator GetLoggedInUserInfo() {
		Debug.Log("Retrieving user info for logged in user");

		ServerCall getInfo = new ServerCall(INSTANCE.SendServerInformationRequest (GET_LOGGED_IN_USER_INFO, null));
		yield return StartCoroutine (getInfo.call());
		string infoText  = getInfo.Response;

		Debug.Log ("Received " + infoText);

		LoggedInUser.ParseFullUserProfile (infoText);

		Debug.Log (LoggedInUser.GetLoggedInUser ().ToString ());

		yield return infoText;
	}
		
	public IEnumerator IsHandleTaken(string handle) {
		Debug.Log("Checking if handle is taken: " + handle );

		Dictionary<string, string> data = new Dictionary<string, string>();
		data["handle"] = "" + handle;

		ServerCall isHandleTakenCall = new ServerCall(INSTANCE.SendServerInformationRequest (SEARCHABLE_HANDLE_TAKEN, data));
		yield return StartCoroutine (isHandleTakenCall.call ());
		string infoText  = isHandleTakenCall.Response;

		Debug.Log("Received " + infoText);

		string FALSE = "0";
		if (infoText.Equals (FALSE)) {
			yield return false;
		} else {
			yield return true;
		}

	}

	public IEnumerator SetSearchableHandle(string handle) {
		Debug.Log("Setting searchable handle");

		Dictionary<string, string> data = new Dictionary<string, string>();
		data["handle"] = "" + handle;

		ServerCall setHandleCall = new ServerCall(INSTANCE.SendServerInformationRequest (SET_SEARCHABLE_HANDLE, data));
		yield return StartCoroutine (setHandleCall.call ());
		string infoText  = setHandleCall.Response;

		Debug.Log("Received " + infoText);

		LoggedInUser.GetLoggedInUser ().SetSearchableHandle(handle);

		yield return infoText;
	}
		
	public IEnumerator SearchForUser(string search) {
		Debug.Log("Searching for user: " + search);

		Dictionary<string, string> data = new Dictionary<string, string>();
		data ["search"] = search;

		ServerCall waitFor = new ServerCall(INSTANCE.SendServerInformationRequest (SEARCH_USER, data));
		yield return StartCoroutine (waitFor.call ());
		string infoText  = waitFor.Response;

		Debug.Log("Received " + infoText);

		List <User> searchedUsers = JsonUtility.FromJson<List<User>> (infoText);;
		searchedUsers.Remove((User) LoggedInUser.GetLoggedInUser());

		yield return searchedUsers;
	}

	public IEnumerator ApplyAvatarTextureToImage(User user, RawImage image) {
		Debug.Log ("Retrieving avatar for " + user.GetUserId ());
		if (ApplicationFileManager.IsAvatarDownloaded (user)) {
			Debug.Log ("Avatar already downloaded. Applying saved texture");
			image.texture = ApplicationFileManager.GetDownloadedAvatar (user);
		} else {
			Debug.Log ("Download started");

			WWW currentDownload = new WWW (user.GetUserAvatarLocation());
			while (!currentDownload.isDone && currentDownload.error == null) {
				yield return null;
			}

			Debug.Log ("Download finished");
			if (!string.IsNullOrEmpty (currentDownload.error)) {  // If the web request returned an error, return the error...
				image.texture = BLANK_DOWNLOAD_TEXTURE;
				throw new Exception(currentDownload.error);
			} else {	// otherwise, return the text content of the web request
				Texture2D downloadedTexture = currentDownload.texture;
				ApplicationFileManager.SaveAvatar (downloadedTexture, user);
				image.texture = downloadedTexture;

				yield return "Success!";
			}
		}
	}

	public IEnumerator uploadNewAvatar(Texture newAvatarTexture) {
		yield return "Coming Soon!!";
	}
		

	public static readonly string FRIENDSHIP_FUNCTION_LOCATION = FUNCTIONSLOCATION + "/friendships";
	public static readonly string SEND_FRIEND_REQUEST = FRIENDSHIP_FUNCTION_LOCATION + "/SendFriendRequest.php";
	public static readonly string ACCEPT_FRIEND_REQUEST = FRIENDSHIP_FUNCTION_LOCATION + "/AcceptFriendRequest.php";
	public static readonly string REJECT_FRIEND_REQUEST = FRIENDSHIP_FUNCTION_LOCATION + "/RejectFriendRequest.php";
	public static readonly string CANCEL_FRIEND_REQUEST = FRIENDSHIP_FUNCTION_LOCATION + "/CancelFriendRequest.php";
	public static readonly string GET_LINKED_FACEBOOK_FRIENDS = FRIENDSHIP_FUNCTION_LOCATION + "/GetLinkedFacebookFriends.php";


	public IEnumerator GetLinkedFacebookFriends() {
		Debug.Log("Retrieving FB Friends");

		ServerCall waitFor = new ServerCall(INSTANCE.SendServerInformationRequest (GET_LINKED_FACEBOOK_FRIENDS, null));
		yield return StartCoroutine (waitFor.call ());
		string infoText  = waitFor.Response;

		Debug.Log("Received " + infoText);

		List<User> fbFriends = JsonUtility.FromJson<List<User>> (infoText);

		yield return fbFriends;
	}

	public IEnumerator SendFriendRequest(User toUser) {
		Debug.Log("Sending friend request");

		Dictionary<string, string> data = new Dictionary<string, string>();
		data ["toUser"] = toUser.GetUserId();

		ServerCall waitFor = new ServerCall(INSTANCE.SendServerInformationRequest (SEND_FRIEND_REQUEST, data));
		yield return StartCoroutine (waitFor.call ());
		string infoText  = waitFor.Response;

		Debug.Log("Received " + infoText);

		Friendship sentFriend = JsonUtility.FromJson<Friendship> (infoText);

		LoggedInUser.GetLoggedInUser ().friends.Add (sentFriend);

		yield return sentFriend;
	}

	public IEnumerator CancelSentFriendRequest(Friendship toUser) {
		Debug.Log("Canceling sent friend request");

		Dictionary<string, string> data = new Dictionary<string, string>();
		data ["toUser"] = toUser.GetOtherUser().GetUserId();

		ServerCall waitFor = new ServerCall(INSTANCE.SendServerInformationRequest (CANCEL_FRIEND_REQUEST, data));
		yield return StartCoroutine (waitFor.call ());
		string infoText  = waitFor.Response;

		Debug.Log("Received " + infoText);

		LoggedInUser.GetLoggedInUser ().friends.Remove (toUser);

		yield return infoText;
	}

	public IEnumerator AcceptReceivedFriendRequest(Friendship existingToAccept) {
		Debug.Log("Accepting friend request");

		Dictionary<string, string> data = new Dictionary<string, string>();
		data ["fromUser"] = existingToAccept.GetOtherUser().GetUserId();

		ServerCall waitFor = new ServerCall(INSTANCE.SendServerInformationRequest (ACCEPT_FRIEND_REQUEST, data));
		yield return StartCoroutine (waitFor.call ());
		string infoText  = waitFor.Response;

		Debug.Log("Received " + infoText);

		Friendship accepted = JsonUtility.FromJson <Friendship> (infoText);

		LoggedInUser.GetLoggedInUser ().friends.Remove (existingToAccept);
		LoggedInUser.GetLoggedInUser ().friends.Add (accepted);

		yield return accepted;
	}

	public IEnumerator RejectReceivedFriendRequest(Friendship fromFriend) {
		Debug.Log("Rejecting friend request");

		Dictionary<string, string> data = new Dictionary<string, string>();
		data ["fromUser"] = fromFriend.GetOtherUser().GetUserId();

		ServerCall waitFor = new ServerCall(INSTANCE.SendServerInformationRequest (REJECT_FRIEND_REQUEST, data));
		yield return StartCoroutine (waitFor.call ());
		string infoText  = waitFor.Response;

		Debug.Log("Received " + infoText);

		LoggedInUser.GetLoggedInUser ().friends.Remove(fromFriend);

		yield return infoText;
	}


	public static readonly string BLOCKING_FUNCTION_LOCATION = FUNCTIONSLOCATION + "/blocking";
	public static readonly string START_BLOCKING_USER = BLOCKING_FUNCTION_LOCATION + "/StartBlockingUser.php";
	public static readonly string STOP_BLOCKING_USER = BLOCKING_FUNCTION_LOCATION + "/StopBlockingUser.php";

	public IEnumerator StartBlockingUser(User user) {
		Debug.Log("Blocking user: " + user.GetUserId());

		Dictionary<string, string> data = new Dictionary<string, string>();
		data ["blockuser"] = user.GetUserId();

		ServerCall waitFor = new ServerCall(INSTANCE.SendServerInformationRequest (START_BLOCKING_USER, data));
		yield return StartCoroutine (waitFor.call ());
		string infoText  = waitFor.Response;

		Debug.Log("Received " + infoText);

		yield return infoText;
	}

	public IEnumerator StopBlockingUser(User user) {
		Debug.Log("Unblock user: " + user.GetUserId());

		Dictionary<string, string> data = new Dictionary<string, string>();
		data ["unblockuser"] = user.GetUserId();

		ServerCall waitFor = new ServerCall(INSTANCE.SendServerInformationRequest (STOP_BLOCKING_USER, data));
		yield return StartCoroutine (waitFor.call ());
		string infoText  = waitFor.Response;

		Debug.Log("Received " + infoText);

		yield return infoText;
	}

	public static readonly string GLYPHFUNCTIONLOCATION = FUNCTIONSLOCATION + "/glyph";
	public static readonly string ADD_PRIVATE_GLYPH = GLYPHFUNCTIONLOCATION + "/AddPrivateGlyph.php";
	public static readonly string ADD_PUBLIC_GLYPH = GLYPHFUNCTIONLOCATION + "/AddPublicGlyph.php";
	public static readonly string DELETE_GLYPH = GLYPHFUNCTIONLOCATION + "/DeleteGlyph.php";
	public static readonly string GLYPHS_IN_AREA = GLYPHFUNCTIONLOCATION + "/GlyphsInArea.php";
	public static readonly string MARK_AS_FOUND = GLYPHFUNCTIONLOCATION + "/MarkGlyphAsFound.php";

	public IEnumerator AddPrivateGlyph(double latitude, double longitude, double altitude ) {
		Debug.Log("Adding private glyph ");

		Dictionary<string, string> data = new Dictionary<string, string>();
		data["latitude"] = "" + latitude;
		data["longitude"] = "" + longitude;
		data["altitude"] = "" + altitude;

		IEnumerator waitFor = INSTANCE.SendServerInformationRequest (ADD_PRIVATE_GLYPH, data);
		while(waitFor.MoveNext()) {
			yield return waitFor.Current;
		}

		string infoText  = (string) waitFor.Current;
		Debug.Log("Received " + infoText);

		Glyph glyph = JsonUtility.FromJson<Glyph> (infoText);

		yield return glyph;
	}

	public IEnumerator AddPublicGlyph(double latitude, double longitude, double altitude , DateTime postDelay) {
		Debug.Log("Adding public glyph ");

		Dictionary<string, string> data = new Dictionary<string, string>();
		data["latitude"] = "" + latitude;
		data["longitude"] = "" + longitude;
		data["altitude"] = "" + altitude;
		data["postDelay"] = postDelay.ToString("yyyy-MM-dd HH:mm:ss");

		IEnumerator waitFor = INSTANCE.SendServerInformationRequest (ADD_PUBLIC_GLYPH, data);
		while(waitFor.MoveNext()) {
			yield return waitFor.Current;
		}

		string infoText  = (string) waitFor.Current;
		Debug.Log("Received " + infoText);

		Glyph glyph = JsonUtility.FromJson<Glyph> (infoText);

		yield return glyph;
	}

	public IEnumerator GetGlyphsInArea(GPSLocation[] bounds) {
		Debug.Log("Retrieving glyphs in area");

		Dictionary<string, string> data = new Dictionary<string, string>();
		data["bottomLeftLatitude"] = "" + bounds[0].latitude;
		data["bottomLeftLongitude"] = "" + bounds[0].longitude;
		data["topRightLatitude"] = "" + bounds[1].latitude;
		data["topRightLongitude"] = "" + bounds[1].longitude;

		ServerCall waitFor = new ServerCall(INSTANCE.SendServerInformationRequest (GLYPHS_IN_AREA, data));
		yield return StartCoroutine (waitFor.call ());
		string infoText  = waitFor.Response;

		Debug.Log("Received " + infoText);

		if (infoText.Equals (EMPTY_JSON_ARRAY)) {
			throw new Exception ("No glyphs in area");
		} else {
			List<Glyph> glyphs = JsonUtility.FromJson<List<Glyph>> (infoText);
			yield return glyphs;
		}
	}

	public IEnumerator MarkGlyphAsFound(Glyph glyph) {
		if (LoggedInUser.GetLoggedInUser ().DidAlreadyFindGlyph (glyph)) {
			throw new Exception ("User already found glyph");
		} else {
			Dictionary<string, string> data = new Dictionary<string, string> ();
			data ["glyphid"] = "" + glyph.GetGlyphId();

			ServerCall markGlyphAsFound = new ServerCall (INSTANCE.SendServerInformationRequest (MARK_AS_FOUND, data));
			yield return StartCoroutine (markGlyphAsFound.call ());
			string infoText = markGlyphAsFound.Response;

			Debug.Log ("Received " + infoText);

			FoundGlyphEvent found = JsonUtility.FromJson<FoundGlyphEvent> (infoText);
			LoggedInUser.GetLoggedInUser().foundGlyphs.Add (found);

			yield return found;
		}
	}


	public static readonly string SHARE_GLYPH_FUNCTION_LOCATION = FUNCTIONSLOCATION + "/sharing";
	public static readonly string GET_USERS_GLYPH_IS_SHARED_WITH = SHARE_GLYPH_FUNCTION_LOCATION + "/GetUsersGlyphIsSharedWith.php";
	public static readonly string START_SHARING_GLYPH = SHARE_GLYPH_FUNCTION_LOCATION + "/StartSharingGlyph.php";
	public static readonly string STOP_SHARING_GLYPH = SHARE_GLYPH_FUNCTION_LOCATION + "/StopSharingGlyph.php";

	public IEnumerator GetUsersGlyphIsSharedWith(Glyph glyph) {
		Debug.Log("Getting shared users for: " + glyph.GetGlyphId());

		Dictionary<string, string> data = new Dictionary<string, string>();
		data ["glyphid"] = "" + glyph.GetGlyphId();

		ServerCall waitFor = new ServerCall(INSTANCE.SendServerInformationRequest (GET_USERS_GLYPH_IS_SHARED_WITH, data));
		yield return StartCoroutine (waitFor.call ());
		string infoText  = waitFor.Response;

		Debug.Log("Received " + infoText);

		UserSharedGlyphEvent.UserSharedGlyphEventList list = JsonUtility.FromJson<UserSharedGlyphEvent.UserSharedGlyphEventList>(infoText);
		yield return list;
	}

	public IEnumerator StartSharingGlyph(Glyph glyph, User user) {
		Debug.Log("Starting to share glyph with user: " + glyph.GetGlyphId() + " " + user.GetUserId());

		Dictionary<string, string> data = new Dictionary<string, string>();
		data ["glyphid"] = "" + glyph.GetGlyphId();
		data ["usersharedwith"] = user.GetUserId();

		ServerCall waitFor = new ServerCall(INSTANCE.SendServerInformationRequest (START_SHARING_GLYPH, data));
		yield return StartCoroutine (waitFor.call ());
		string infoText  = waitFor.Response;

		Debug.Log("Received " + infoText);

		SharedWithUserGlyphEvent share = JsonUtility.FromJson<SharedWithUserGlyphEvent>(infoText);
		yield return share;
	}

	public IEnumerator StopSharingGlyph(Glyph glyph, User user) {
		Debug.Log("Stopping share glyph with user");

		Dictionary<string, string> data = new Dictionary<string, string>();
		data ["glyphid"] = "" + glyph.GetGlyphId();
		data ["usersharedwith"] = user.GetUserId();

		ServerCall waitFor = new ServerCall(INSTANCE.SendServerInformationRequest (STOP_SHARING_GLYPH, data));
		yield return StartCoroutine (waitFor.call ());
		string infoText  = waitFor.Response;

		Debug.Log("Received " + infoText);

		yield return "Success!";
	}

	public static readonly string COMMENT_FUNCTION_LOCATION = FUNCTIONSLOCATION + "/comments";
	public static readonly string GET_COMMENTS_ON_GLYPH = COMMENT_FUNCTION_LOCATION + "/GetCommentsOnGlyph.php";
	public static readonly string ADD_COMMENT_TO_GLYPH = COMMENT_FUNCTION_LOCATION + "/AddCommentToGlyph.php";
	public static readonly string DELETE_COMMENT_FROM_GLYPH = COMMENT_FUNCTION_LOCATION + "/DeleteCommentFromGlyph.php";

	public IEnumerator GetCommentsOnGlyph(Glyph glyph) {
		Debug.Log("Getting comments on glyph " + glyph.GetGlyphId());

		Dictionary<string, string> data = new Dictionary<string, string>();
		data ["glyphid"] = "" + glyph.GetGlyphId();

		ServerCall waitFor = new ServerCall(INSTANCE.SendServerInformationRequest (GET_COMMENTS_ON_GLYPH, data));
		yield return StartCoroutine (waitFor.call ());
		string infoText  = waitFor.Response;

		Debug.Log("Received " + infoText);

		if (infoText.Equals (EMPTY_JSON_ARRAY)) {
			throw new Exception ("No comments on glyph");
		}

		GlyphComment.GlyphCommentList commentList = JsonUtility.FromJson<GlyphComment.GlyphCommentList> (infoText);

		yield return commentList;
	}

	public IEnumerator AddCommentToGlyph(Glyph glyph, string commentText) {
		Debug.Log("Adding comment to glyph");

		Dictionary<string, string> data = new Dictionary<string, string>();
		data ["glyphid"] = "" + glyph.GetGlyphId();
		data ["userComment"] = commentText;

		ServerCall waitFor = new ServerCall(INSTANCE.SendServerInformationRequest (ADD_COMMENT_TO_GLYPH, data));
		yield return StartCoroutine (waitFor.call ());
		string infoText  = waitFor.Response;

		Debug.Log("Received " + infoText);

		GlyphComment comment = JsonUtility.FromJson<GlyphComment> (infoText);

		yield return comment;
	}

	public IEnumerator DeleteCommentFromGlyph(Glyph glyph, GlyphComment comment) {
		Debug.Log("Deleting comment " + comment.GetCommentId());

		Dictionary<string, string> data = new Dictionary<string, string>();
		data ["glyphid"] = "" + glyph.GetGlyphId();
		data ["userComment"] = "" + comment.GetCommentId();

		ServerCall waitFor = new ServerCall(INSTANCE.SendServerInformationRequest (DELETE_COMMENT_FROM_GLYPH, data));
		yield return StartCoroutine (waitFor.call ());
		string infoText  = waitFor.Response;

		Debug.Log("Received " + infoText);

		yield return "Success!";
	}
		
	public static readonly string GOOGLE_CLOUD_STORAGE_URL = "gs://glyph-project-1344.appspot.com";
	// private static StorageReference storage_ref;
	//private static Firebase.Auth.FirebaseUser newUser = null;

	public IEnumerator RetrieveFirebaseToken() {
		// FirebaseApp.DefaultInstance.SetEditorDatabaseUrl("https://glyph-project-1344.firebaseio.com");
		// FirebaseApp.DefaultInstance.SetEditorP12FileName("Glyph Project-53a06b80ee55.p12");
		// FirebaseApp.DefaultInstance.SetEditorServiceAccountEmail("firebase-adminsdk-efn19@glyph-project-1344.iam.gserviceaccount.com");
		// FirebaseApp.DefaultInstance.SetEditorP12Password("notasecret");
		// FirebaseApp.DefaultInstance.SetEditorAuthUserId ("facebook|1751101561581447");

		if (LoggedInUser.GetLoggedInUser() == null) {
			throw new Exception ("No logged in user");
		}

		Debug.Log ("Getting firebase access token");

		string granttype = "urn:ietf:params:oauth:grant-type:jwt-bearer";
		string apitype = "firebase";

		// Create a Web Form
		WWWForm form = new WWWForm ();
		// Add the authorization token to the form using the Authorization request header
		Dictionary<string, string> headers = form.headers;

		headers ["Authorization"] = Authentication.authToken;

		form.AddField ("null", "null");

		/* Sample POST:
		 * POST https://shroysha.auth0.com/delegation
			Content-Type: 'application/json'
			{
			  "client_id":   "ZMBm759muZ6K5sVsg0xdgExZyyMmfidv",
			  "grant_type":  "urn:ietf:params:oauth:grant-type:jwt-bearer",
			  "id_token":    "{YOUR_ID_TOKEN}",
			  "target":      "ZMBm759muZ6K5sVsg0xdgExZyyMmfidv",
			  "api_type":    "firebase",
			}
		 */
		Dictionary<string, string> data = new Dictionary<string, string> ();
		data ["client_id"] = AUTH0_CLIENT_ID;
		data ["grant_type"] = granttype;
		data ["id_token"] = Authentication.authToken;
		data ["target"] = AUTH0_CLIENT_ID;
		data ["api_type"] = apitype;

		if (data != null) {
			foreach (string key in data.Keys) {
				form.AddField (key, data [key]);
			}
		}

		IEnumerator waitFor = DownloadContent (AUTH0_DELEGATE_URL, form.data, headers);
		while (waitFor.MoveNext ()) {
			yield return waitFor.Current;
		}

		string response = ((string)waitFor.Current).Trim ();

		Auth0FirebaseCredentials auth0Creds = JsonUtility.FromJson<Auth0FirebaseCredentials> (response);

		Debug.Log (response);
		Debug.Log (auth0Creds.id_token);
		/*
		Firebase.Auth.FirebaseAuth auth = Firebase.Auth.FirebaseAuth.DefaultInstance;
		bool wait = true;

		auth.SignInWithCustomTokenAsync (auth0Creds.id_token).ContinueWith (task => {
			if (task.IsCanceled) {
				Debug.LogError ("SignInWithCustomTokenAsync was canceled.");
				wait = false;
				return;
			}
			if (task.IsFaulted) {
				Debug.LogError ("SignInWithCustomTokenAsync encountered an error: " + task.Exception);
				wait = false;
				return;
			}

			newUser = task.Result;

			Debug.LogFormat ("User signed in successfully: {0} ({1})",
				newUser.DisplayName, newUser.UserId);
		});

		while (newUser == null && wait) {
			yield return null;
		}

		if (newUser == null) {
			yield return "N/A";
		} else {			
			CreateFirebaseStorageReference ();
			yield return "Success";
		}
*/

		CreateFirebaseStorageReference ();

		yield return "Done";
	}

	private static void CreateFirebaseStorageReference() {
		// FirebaseStorage storage = FirebaseStorage.DefaultInstance;

		// Create a storage reference from our storage service
		// storage_ref = storage.GetReferenceFromUrl(GOOGLE_CLOUD_STORAGE_URL);
	}

	public IEnumerator UploadAddGlyphFileToFirebase(Glyph glyph) { // returns path to glyph file on aws

		string firebaseLoc = glyph.GetPathToContent();

		bool callFinished = false;

		// Firebase.Storage.StorageReference firebaseUploadRef = storage_ref.Child (firebaseLoc);
		/*firebaseUploadRef.PutFileAsync(ApplicationFileManager.ADD_GLYPH_FILE_PATH)
			.ContinueWith ((Task<Firebase.Storage.StorageMetadata> task) => {
				if (task.IsFaulted || task.IsCanceled) {
					Debug.LogException(task.Exception);
					throw task.Exception;
					// Uh-oh, an error occurred!
				} else {
					// Metadata contains file metadata such as size, content-type, and download URL.
					Firebase.Storage.StorageMetadata metadata = task.Result;
					string download_url = metadata.DownloadUrl.ToString();
					Debug.Log("Finished uploading...");
					Debug.Log("download url = " + download_url);
					callFinished = true;
				}
			});*/
		

		while (!callFinished) {
			yield return null;
		}

		yield return firebaseLoc;
	}

	public static IEnumerator DownloadGlyphFromFirebase(Glyph glyph) {

		string path = glyph.GetPathToContent ();
		bool commandFinished = false;
		Debug.Log("Downloading content for " + glyph.GetGlyphId());

		// Create local filesystem URL
		string local_url = ApplicationFileManager.GetLocalDownloadPath(glyph);

		// Firebase.Storage.StorageReference glyph_fb_reference = storage_ref.Child (path);
		// Download to the local filesystem
		FileInfo local_file = new FileInfo(local_url);
		local_file.Directory.Create ();

		Debug.Log ("Path: " + path);
		// glyph_fb_reference.GetFileAsync(local_url).ContinueWith(task => {
		// 	if (!task.IsFaulted && !task.IsCanceled) {
		// 		Debug.Log("File downloaded.");
		// 		commandFinished = true;
		// 	} else {
		// 		Debug.LogError("error downloading file: " + task.Exception);
		// 	}
		// });

		while (!commandFinished) {
			yield return null;
		}

		yield return "Done";
	}

	[Serializable] private class Auth0FirebaseCredentials {

		/* Example:
		 * {"expires_in":3600,
		 * "id_token":"eyJ0eXAiOiJKV1QiLCJhbGciOiJSUzI1NiIsImtpZCI6IjdkNGY4OWMxNGI0ZmFmNGMxZDQ5YjM4MTIzMjViZDc2MzAyODhhMjcifQ.eyJ1aWQiOiJmYWNlYm9va3wxNzUxMTAxNTYxNTgxNDQ3IiwiaWF0IjoxNTE4OTc5ODY5LCJleHAiOjE1MTg5ODM0NjksImF1ZCI6Imh0dHBzOi8vaWRlbnRpdHl0b29sa2l0Lmdvb2dsZWFwaXMuY29tL2dvb2dsZS5pZGVudGl0eS5pZGVudGl0eXRvb2xraXQudjEuSWRlbnRpdHlUb29sa2l0IiwiaXNzIjoiZmlyZWJhc2UtYWRtaW5zZGstZWZuMTlAZ2x5cGgtcHJvamVjdC0xMzQ0LmlhbS5nc2VydmljZWFjY291bnQuY29tIiwic3ViIjoiZmlyZWJhc2UtYWRtaW5zZGstZWZuMTlAZ2x5cGgtcHJvamVjdC0xMzQ0LmlhbS5nc2VydmljZWFjY291bnQuY29tIn0.V29IjEtW0OVSGhU8yB-VKTJwfoOmVDK271MFeMB7NRKSE-BmClYTDWzgRKM0DcfoHXCJROthR7gfB9TFQ5JFTbWp7jzFxR0vZGKQJbN8ssYwCiV5bBkKoTAiRP5UK07FwoxJAahhU3ce3YXGePkXq_Dbdf2VHFhIbXZ3ubmL2lutMXwplsI1IKUCEsq37uR3eMBXQXOibbhUcAcywQtC5CGamPwttCSP0ExAHl10oUOkIj0HgvZMmJfgKgyMZVUlIu6enCkFdUXTyuXb7enpO9aE1gwON5ek-uLVsADwvufojUJIlt-UHgmGAMMISyBQefpwo0_XWmdf2Qrji1JHwA"
		 * }
		 */

		[SerializeField] public int expires_in;
		[SerializeField] public string id_token;

	}
}
