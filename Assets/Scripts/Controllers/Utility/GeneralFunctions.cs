using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GeneralFunctions {


	public static string timeSince(DateTime departure) {
		DateTime arrival = DateTime.Now;

		TimeSpan travelTime = arrival - departure;  
		Debug.Log("travelTime: " + travelTime );  

		int years = travelTime.Days / 365; //wrong but near enough
		int weeks = travelTime.Days / 7;
		string output = "";

		if (years > 0) {
			output = "" + years + " years ago";  
		} else if (weeks > 0) {
			output = "" + weeks + " weeks ago";
		} else if (travelTime.Days > 0) {
			output = "" + travelTime.Days + " days ago"; 
		} else if (travelTime.Hours > 0) {
			output = "" + travelTime.Hours + " hours ago";
		} else {
			output = "" + travelTime.Milliseconds + " milliseconds ago";
		}

		Debug.Log(output);

		return output;
	}


	public static void switchToAddGlyphScene() {
		//if (ApplicationFileManager.addGlyphFileExists()) {
		//	SceneManager.LoadScene ("Scenes/AddGlyph");
		//} else {
		ApplicationFileManager.DeleteAddGlyphFile();
			SceneManager.LoadScene ("Scenes/TakePicture");
		//}
	}
}
