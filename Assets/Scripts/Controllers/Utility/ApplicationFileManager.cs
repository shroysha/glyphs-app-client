using System;
using System.IO;
using UnityEngine;

public class ApplicationFileManager {
	
	public static readonly string SAVED_CREDENTIALS_FILE = Application.persistentDataPath + Path.DirectorySeparatorChar + "savedtoken.txt";

	public static readonly string PNG_EXTENSION = "PNG";

	public static readonly string TAKE_PICTURE_FILE_NAME = "Snapshot.png";
	public static readonly string TAKE_PICTURE_FILE_PATH = Application.persistentDataPath + Path.DirectorySeparatorChar + TAKE_PICTURE_FILE_NAME;

	public static readonly string ADD_GLYPH_FILE_NAME = "CanvasTexture.png";
	public static readonly string ADD_GLYPH_FILE_PATH = Application.persistentDataPath + Path.DirectorySeparatorChar + ADD_GLYPH_FILE_NAME;

	public static readonly string AVATARS_LOCATION = Application.persistentDataPath + Path.DirectorySeparatorChar + "Avatars" + Path.DirectorySeparatorChar;

	public static bool DoesAddGlyphFileExist () {
		return File.Exists (ADD_GLYPH_FILE_PATH);
	}

	public static Texture2D LoadAddGlyphFileTexture() {
		return LoadTextureFromPNGFile (ADD_GLYPH_FILE_PATH);
	}

	public static Texture2D LoadTextureFromPNGFile(string filePath) {
		Texture2D tex = null;
		byte[] fileData;

		if (File.Exists(filePath))     {
			fileData = File.ReadAllBytes(filePath);
			tex = new Texture2D(2, 2);
			tex.LoadImage(fileData); //..this will auto-resize the texture dimensions.
		}
		return tex;
	}

	public static void DeleteAddGlyphFile() {
		File.Delete (ADD_GLYPH_FILE_PATH);
	}

	public static void SavePaintTextureToAddGlyphFile(Texture2D texture) {
		SaveTextureToFile (ADD_GLYPH_FILE_PATH, texture);
	}

	public static void SaveTextureToFile(string filePath, Texture2D texture){		
		var bytes = texture.EncodeToPNG();
		File.WriteAllBytes(filePath, bytes);
	}


	public static bool IsGlyphDownloaded(Glyph glyph) {
		return File.Exists(GetLocalDownloadPath(glyph));
	}

	public static string GetLocalDownloadPath(Glyph glyph) {
		return Application.persistentDataPath + Path.DirectorySeparatorChar + glyph.GetPathToContent();
	}

	public static string GetFileExtension(string filePath) {
		string[] split = filePath.Split (new string[] { "." }, StringSplitOptions.None);
		return split [split.Length - 1].Trim().ToUpper();
	}

	public static void SaveAvatar(Texture2D avatar, User user) {
		CreateParentDirectories (GetLocalAvatarDownloadLocation (user));
		SaveTextureToFile (GetLocalAvatarDownloadLocation (user), avatar);
	}

	private static void CreateParentDirectories(string fullpath) {
		new FileInfo (fullpath).Directory.Create ();
	}

	public static bool IsAvatarDownloaded(User user) {
		return File.Exists(GetLocalAvatarDownloadLocation(user));
	}

	public static string GetLocalAvatarDownloadLocation(User user) {
		return AVATARS_LOCATION + user.GetUserId() + ".png";
	}

	public static Texture2D GetDownloadedAvatar(User user) {
		return LoadTextureFromPNGFile(GetLocalAvatarDownloadLocation(user));
	}
		
}


