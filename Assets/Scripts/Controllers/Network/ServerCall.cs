using System;
using System.Collections;
using UnityEngine;

public class ServerCall {

	public static readonly string UNAUTHORIZED_RESPONSE1 = "401: Unauthorized";
	public static readonly string UNAUTHORIZED_RESPONSE2 = "401 Unauthorized";

	public string Response = null;
	public object ObjectResponse = null;
	public Exception ReturnException = null;

	private IEnumerator serverCall;

	public ServerCall(IEnumerator serverCall) {
		this.serverCall = serverCall;
	}

	public IEnumerator call() {
		bool active = true;
		while (active) {
			bool hasNext = false;
			try {
				hasNext = serverCall.MoveNext();
				active = true;
			} catch (Exception ex) {
				ReturnException = ex;
				Debug.LogException (ex);
				active = false;
			}

			if (hasNext == false) {
				active = false;
			}

			yield return serverCall.Current;
		}

		ObjectResponse = serverCall.Current;

		if (serverCall.Current is string) {
			Response = (string)serverCall.Current;		
			Debug.Log ("Set string response to " + Response);
		}

		if (ReturnException != null) {
			ObjectResponse = ReturnException;
		}

		yield return "Done";
	}

	/*
	public IEnumerator call() {
		bool active = true;
		while (active) {
			bool hasNext = false;

			hasNext = serverCall.MoveNext();

			if (hasNext == false) {
				active = false;
			}

			yield return serverCall.Current;
		}
			
		ObjectResponse = serverCall.Current;

		if (serverCall.Current is string) {
			Response = (string)serverCall.Current;		
			Debug.Log ("Set string response to " + Response);
		}

		if (serverCall.Current is ServerInteract.ServerException) {
			ReturnException = serverCall.Current;	
			Debug.LogWarning ("Caught exception in return: " + ((ServerInteract.ServerException) ReturnException).Message);
		}

		yield return "Done";
	}*/


}
