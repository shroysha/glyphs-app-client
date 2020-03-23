using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Authentication : MonoBehaviour {
	//Just let it compile on platforms beside of iOS and Android
	//If you are just targeting for iOS and Android, you can ignore this

	private static readonly string RESTARTING_SESSION = "Restarting session...";
	private static readonly string SESSION_RESTARTED = "Session restarted";
	private static readonly string LOADING_MANUAL_LOGIN = "Loading manual login...";
	private static readonly string MANUAL_LOGIN_LOADED = "Manual login loaded";
	private static readonly string START_AUTO_LOGIN = "Starting auto login...";
	private static readonly string REAUTHORIZING_SAVED_TOKEN = "Reauthorizing saved token...";
	private static readonly string SAVED_TOKEN_SUCCESS = "Saved token success!";
	private static readonly string SAVED_TOKEN_FAILED = "Saved token failed";
	private static readonly string PROVIDER_AUTH_0 = "Provider auth0. Manual login required";
	private static readonly string AUTHORIZING_WITH_PROVIDER = "Reauthorizing with ";
	private static readonly string AUTH_0_LOGIN_PROVIDER = "auth0";

	public static string authToken;

	public Text statusText;
	// public UniWebView webView;

	private bool logoutLoaded = false;
	private bool autoLoginWorked = false;
	private static bool clearCache = false;

	IEnumerator Start() {
		Application.targetFrameRate = 60;

		// setupWebView ();
		//
		// yield return StartCoroutine(logout ());
		//
		// yield return StartCoroutine(tryAutoLogin ());
		//
		// if (!autoLoginWorked) {
		// 	loadManualLogin ();
		// } 

		yield return null;
	}
		
	// private void setupWebView() {
	// 	// webView.InsetsForScreenOreitation += InsetsForScreenOreitation;
	// 	webView.toolBarShow = true;
	//
	// 	webView.OnLoadComplete += OnLoadComplete;
	// 	webView.OnReceivedMessage += OnReceivedMessage;
	// }
	// 	

	private void setStatus(string text) {
		Debug.Log ("Login status: " + text);
		statusText.text = text;
	}

	// private IEnumerator logout() {
	// 	setStatus(RESTARTING_SESSION);
	// 	logoutLoaded = false;
	//
	// 	webView.url = ServerInteract.LOGOUT_URL;
	// 	webView.Load();
	//
	// 	while (!logoutLoaded) {
	// 		yield return null;
	// 	}
	//
	// 	if (clearCache) {
	// 		webView.CleanCache ();
	// 		webView.CleanCookie ();
	// 	}
	//
	// 	setStatus (SESSION_RESTARTED);
	//
	// 	yield return "Done";
	// }
	//
	// private IEnumerator tryAutoLogin() {
	// 	setStatus (START_AUTO_LOGIN);
	// 	autoLoginWorked = false;
	//
	// 	SavedCredentials savedCreds = readTokenAndUsernameFromFile ();
	// 	if (savedCreds != null) {
	// 		setStatus(REAUTHORIZING_SAVED_TOKEN);
	//
	// 		authToken = savedCreds.savedAuthtoken;
	//
	// 		ServerCall testSavedAuth = new ServerCall(ServerInteract.INSTANCE.SendServerInformationRequest (ServerInteract.TEST_LOGIN, null));
	// 		yield return StartCoroutine (testSavedAuth.call ());
	//
	//
	// 		if(testSavedAuth.ReturnException == null) {
	// 			
	// 			setStatus(SAVED_TOKEN_SUCCESS);
	//
	// 			authToken = savedCreds.savedAuthtoken;
	// 			StartCoroutine(startLoadUserData ());
	// 			autoLoginWorked = true;
	//
	// 		} else {
	// 			setStatus (SAVED_TOKEN_FAILED);
	//
	// 			authToken = null;
	//
	// 			if (savedCreds.savedProvider.Equals (AUTH_0_LOGIN_PROVIDER)) {
	// 				setStatus (PROVIDER_AUTH_0);
	// 				autoLoginWorked = false;
	// 			} else {
	// 				setStatus (AUTHORIZING_WITH_PROVIDER + savedCreds.savedProvider);
	// 				loadReauthorize (savedCreds.savedProvider);
	// 				autoLoginWorked = true;
	// 			}
	// 		}
	// 	} else { // create a blank file if none is there and login has failed
	// 		
	// 		autoLoginWorked = false;
	// 	}
	//
	// 	yield return "Done";
	// }

	// private void loadReauthorize(string provider) {
	// 	string authorizeurl = createAuthorizeURL (provider);
	//
	// 	webView.url = authorizeurl;
	// 	webView.Load ();
	// }

	// private void loadManualLogin() {
	// 	setStatus(LOADING_MANUAL_LOGIN);
	//
	// 	webView.url = ServerInteract.MANUALLOGIN;
	// 	webView.Load();
	//
	// 	setStatus (MANUAL_LOGIN_LOADED);
	// }

	// void OnLoadComplete(UniWebView webView, bool success, string errorMessage) {
	// 	Debug.Log ("Load complete: " + webView.url);
	// 	Debug.Log ("Load successful: " + success);
	//
	// 	if (webView.url.Equals (ServerInteract.LOGOUT_URL)) {
	// 		logoutLoaded = true;
	// 	} else if (webView.url.Equals (ServerInteract.MANUALLOGIN)) {
	// 		webView.Show();
	// 	} else if (webView.url.Contains (ServerInteract.AUTHORIZEURL)) {
	// 		webView.Show();
	// 	} 
	//
	// 	if (!success) {
	// 		Debug.Log("Something wrong in webview loading: " + errorMessage);
	// 	} 
	//
	// }
	// 	
	// void OnReceivedMessage(UniWebView webView, UniWebViewMessage message) {
	//
	// 	// You can check the message path and arguments to know which `uniwebview` link is clicked.
	// 	// UniWebView will help you to parse your link if it follows the url argument format.
	// 	// However, there is also a "rawMessage" property you could use if you need to use some other formats and want to parse it yourself.
	// 	Debug.Log("Received message from: " + webView.url);
	// 	Debug.Log ("Raw message: " + message.rawMessage);
	//
	// 	if (message.path == "success") {
	// 		string response = message.args["authToken"].Trim();
	// 		authToken = response;
	//
	// 		Debug.LogWarning("Auth token: " + authToken);
	//
	// 		//writeTokenToFile (authToken);
	// 		//LoadUserData.populateUserData(authToken);
	// 		//LoadUserData lud = new LoadUserData();
	// 		//StartCoroutine (lud.Start());
	//
	// 		StartCoroutine(startLoadUserData ());
	// 	}
	// }

	// This method will be called when the screen orientation changed. Here we return UniWebViewEdgeInsets(5,5,5,5)
	// for both situation, which means the inset is 5 point for iOS and 5 pixels for Android from all edges.
	// Note: UniWebView is using point instead of pixel in iOS. However, the `Screen.width` and `Screen.height` will give you a
	// pixel-based value. 
	// You could get a point-based screen size by using the helper methods: `UniWebViewHelper.screenHeight` and `UniWebViewHelper.screenWidth` for iOS.
	// UniWebViewEdgeInsets InsetsForScreenOreitation(UniWebView webView, UniWebViewOrientation orientation) {
	// 	int INSET = 0;
	// 	if (orientation == UniWebViewOrientation.Portrait) {
	// 		return new UniWebViewEdgeInsets(INSET,INSET,INSET,INSET);
	// 	} else {
	// 		return new UniWebViewEdgeInsets(INSET,INSET,INSET,INSET);
	// 	}
	// }

	private IEnumerator startLoadUserData() {
		HardwareController.initialize ();

		setStatus ("Downloading User Profile...");

		ServerCall waitFor = new ServerCall(ServerInteract.INSTANCE.GetLoggedInUserInfo());
		yield return StartCoroutine (waitFor.call ());

		writeTokenAndUsernameToFile (authToken, LoggedInUser.GetLoggedInUser().GetUserId());

		setStatus ("Logging into Network...");
		waitFor = new ServerCall(ServerInteract.INSTANCE.RetrieveFirebaseToken ());
		yield return StartCoroutine (waitFor.call ());

		SceneManager.LoadScene ("Scenes/GroundViewScene", LoadSceneMode.Single);
	}


	private static string createAuthorizeURL(string provider) {
		return ServerInteract.AUTHORIZEURL + "&connection=" + provider +
			"&redirect_uri=http://www.shawnsglyphproject.com/dev/functions/auth0/usersignincallback.php";
	}
		
	public static void clearUserData() {
		//clearCache = true;

		if (File.Exists (ApplicationFileManager.SAVED_CREDENTIALS_FILE)) {
			File.WriteAllText (ApplicationFileManager.SAVED_CREDENTIALS_FILE, "");
		}

		LoggedInUser.ClearLoggedInUser();
		authToken = null;
	}


	public static void writeTokenAndUsernameToFile(string authtoken, string username) {
		if (File.Exists (ApplicationFileManager.SAVED_CREDENTIALS_FILE)) {
			string writeToFile = username + "\n" + authtoken;
			File.WriteAllText (ApplicationFileManager.SAVED_CREDENTIALS_FILE, writeToFile);
		}
	}

	private static SavedCredentials readTokenAndUsernameFromFile() {

		if (File.Exists (ApplicationFileManager.SAVED_CREDENTIALS_FILE)) {
			Debug.Log ("Saved auth token file: " + ApplicationFileManager.SAVED_CREDENTIALS_FILE);

			string savedInfo = File.ReadAllText (ApplicationFileManager.SAVED_CREDENTIALS_FILE).Trim ();
			if (savedInfo.Equals ("")) {
				return null;
			}
				
			string[] split = savedInfo.Split (new string[] { "\n" }, StringSplitOptions.None);
			string savedUsername = split [0];
			string savedAuthToken = split [1];
			Debug.Log ("Saved token " + savedAuthToken);

			return new SavedCredentials (savedAuthToken, savedUsername);
		} else {
			return null;
		}
	}

	private static void createBlankSavedAuthTokenFile() {
		new FileInfo (ApplicationFileManager.SAVED_CREDENTIALS_FILE).Directory.Create ();
		File.WriteAllText (ApplicationFileManager.SAVED_CREDENTIALS_FILE, "");
	}

	private class SavedCredentials {

		public string savedAuthtoken;
		public string savedUsername;

		public string savedProvider;

		public SavedCredentials(string authtoken, string username) {
			this.savedAuthtoken = authtoken;
			this.savedUsername = username;

			string[] split2 = savedUsername.Split (new string[] { "|" }, StringSplitOptions.None);
			this.savedProvider = split2 [0];
		}

	}

}
