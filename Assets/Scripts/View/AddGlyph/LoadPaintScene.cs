using System.Collections;
using System.IO;
using unitycoder_MobilePaint;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UIElements.AddGlyph
{
	public class LoadPaintScene : MonoBehaviour {

		private static readonly string GROUND_VIEW_SCENE = "Scenes/GroundViewScene";
		private static readonly string TAKE_PICTURE_SCENE = "Scenes/TakePicture";
		private static readonly string ADD_GLYPH_SCENE = "Scenes/AddGlyph";
		private static readonly string PAINT_SCENE = "Scenes/PaintScene";

		public MobilePaint canvasTexture;

		public Button backButton;
		public Button cameraButton;
		public Toggle toggleColorPickerButton;
		public GameObject picker;
		public Button saveButton;

		// Use this for initialization
		void Start () {
			addActionListeners ();
			loadSnapshot ();
		}

		private void addActionListeners() {
			backButton.onClick.AddListener (onBackButton);
			cameraButton.onClick.AddListener (onCameraButton);
			toggleColorPickerButton.onValueChanged.AddListener(toggleColorPicker);
			saveButton.onClick.AddListener(startSaveDrawing);
		}

		private void onBackButton() {
			SceneManager.LoadScene (GROUND_VIEW_SCENE);
		}

		private void onCameraButton() {
			LoadTakePictureScene.setBackButtonScene (PAINT_SCENE);
			SceneManager.LoadScene (TAKE_PICTURE_SCENE);
		}

		private void toggleColorPicker(bool stuff) {
			picker.SetActive (toggleColorPickerButton.isOn);
		}
		
		private void startSaveDrawing() {
			StartCoroutine (saveDrawing ());
		}

		private void loadSnapshot() {
			Texture2D snapshot = ApplicationFileManager.LoadTextureFromPNGFile (ApplicationFileManager.TAKE_PICTURE_FILE_PATH);
			if (snapshot != null) {
				canvasTexture.addSnapshot (snapshot);
				File.Delete (ApplicationFileManager.TAKE_PICTURE_FILE_PATH);
			} else {
				snapshot = ApplicationFileManager.LoadTextureFromPNGFile (ApplicationFileManager.ADD_GLYPH_FILE_PATH);
				if (snapshot == null)
					return;

				canvasTexture.addSnapshot (snapshot);
			}

		}

		private IEnumerator saveDrawing() {
			yield return new WaitForEndOfFrame();

			ApplicationFileManager.SavePaintTextureToAddGlyphFile (canvasTexture.GetScreenshot());
			SceneManager.LoadScene (ADD_GLYPH_SCENE);
		}

	}
}
