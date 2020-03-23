using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace UIElements.AddGlyph
{
	public class LoadAddGlyphScene : MonoBehaviour {

		private static readonly string MAPBOX_SCENE = "Scenes/AddGlyphMapBoxMinimap";
		private static readonly string PAINT_SCENE = "Scenes/PaintScene";

		private static readonly string CANNOT_ACCESS_LOCATION_ERROR = "Unable to access GPS";
		private static readonly string NO_LOGGED_IN_USER_ERROR = "No user logged in";

		public RawImage contentImage;
		public RectTransform contentAreaTransform, contentImageTransform;
		public Button backButton, saveButton;

		public Toggle publicToggle;
		public Text postTimerText;
		public Button postTimerButton;
		public DateTimePicker dateTimePicker;

		public AppStatusPanel statusPanel;

		// Use this for initialization
		void Start () {
			setImageToDrawing ();
			addButtonActionListeners ();
			SceneManager.LoadScene (MAPBOX_SCENE, LoadSceneMode.Additive);
		}
		
		private void setImageToDrawing() {	
			if (ApplicationFileManager.DoesAddGlyphFileExist()) {
				setImageTexture (ApplicationFileManager.LoadAddGlyphFileTexture());
			} 
		}

		private void setImageTexture(Texture2D texture) {
			contentImageTransform.sizeDelta = new Vector2 (texture.width, texture.height);

			float widthScale = contentAreaTransform.rect.width / contentImageTransform.sizeDelta.x;
			float heightScale = contentAreaTransform.rect.height / contentImageTransform.sizeDelta.y;

			float scale = Mathf.Min (widthScale, heightScale);

			contentImageTransform.localScale = new Vector3 (scale, scale, scale);

			contentImage.texture = texture;
		}
		
		private void addButtonActionListeners() {
			backButton.onClick.AddListener (onBackButton);
			saveButton.onClick.AddListener (onSaveButton);
			publicToggle.onValueChanged.AddListener (onPublicToggle);
			postTimerButton.onClick.AddListener (onPostTimerButton);
		}

		private void onBackButton() {
			SceneManager.LoadScene (PAINT_SCENE);
		}


		private void onSaveButton() {
			StartCoroutine (uploadGlyph ());
		}
		
		private IEnumerator uploadGlyph() {

			if (HardwareController.Instance.getLastLocation().Equals(GPSLocation.UNDEFINED)) {
				statusPanel.showErrorStatus (CANNOT_ACCESS_LOCATION_ERROR);
				yield return null;
				
			} else if (LoggedInUser.GetLoggedInUser () == null) {
				statusPanel.showErrorStatus (NO_LOGGED_IN_USER_ERROR);
				yield return null;

			} else {
				statusPanel.showLoadingStatus ("Creating Glyph");

				double latitude = HardwareController.Instance.getLastLocation().latitude;
				double longitude = HardwareController.Instance.getLastLocation().longitude;
				double altitude = HardwareController.Instance.getLastLocation().altitude;

				ServerCall uploadGlyphCall;
				if (!publicToggle.isOn) {
					uploadGlyphCall = new ServerCall (ServerInteract.INSTANCE.AddPrivateGlyph (latitude, longitude, altitude));
				} else {
					uploadGlyphCall = new ServerCall (ServerInteract.INSTANCE.AddPublicGlyph (latitude, longitude, altitude, dateTimePicker.getSelectedDateTime()));
				}

				yield return StartCoroutine (uploadGlyphCall.call ());

				if(uploadGlyphCall.ReturnException != null) {
					statusPanel.showErrorStatus(uploadGlyphCall.ReturnException.Message);
				} else {
					OwnedGlyphEvent ownedGlyph = (OwnedGlyphEvent) uploadGlyphCall.ObjectResponse;
					LoggedInUser.GetLoggedInUser ().ownedGlyphs.Add (ownedGlyph);

					statusPanel.showLoadingStatus ("Uploading Picture");

					ServerCall uploadToFirebaseCall = new ServerCall(ServerInteract.INSTANCE.UploadAddGlyphFileToFirebase(ownedGlyph.GetGlyph()));
					yield return StartCoroutine (uploadToFirebaseCall.call ());

					if (uploadToFirebaseCall.ReturnException != null) {
						statusPanel.showErrorStatus (uploadToFirebaseCall.ReturnException.Message);
					} else {
						statusPanel.showStatus ("Glyph Created!");
						ApplicationFileManager.DeleteAddGlyphFile ();

						yield return new WaitForSeconds (3);
						SceneManager.LoadScene ("Scenes/GroundViewScene");
					}
				}

			}

			yield return "Done";
		}

		private void onPublicToggle(bool selected) {
			postTimerButton.gameObject.SetActive (selected);
			postTimerText.gameObject.SetActive (selected);
		}

		private void onPostTimerButton() {
			dateTimePicker.setVisible (true);
		}
	}
}
