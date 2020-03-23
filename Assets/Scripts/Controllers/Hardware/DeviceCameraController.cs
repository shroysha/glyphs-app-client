using System.Collections;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class DeviceCameraController : MonoBehaviour
{
	public RawImage image;

	//public Text debug1, debug2;
	//public AspectRatioFitter imageFitter;
	// Device cameras
	private WebCamDevice frontCameraDevice;
	private WebCamDevice backCameraDevice;
	private WebCamDevice activeCameraDevice;

	private WebCamTexture frontCameraTexture;
	private WebCamTexture backCameraTexture;
	private WebCamTexture activeCameraTexture;

	// Image rotation
	private Vector3 rotationVector = new Vector3(0f, 0f, 0f);

	// Image uvRect
	private Rect defaultRect = new Rect(0f, 0f, 1f, 1f);
	private Rect fixedRect = new Rect(0f, 1f, 1f, -1f);

	// Image Parent's scale
	private Vector3 defaultScale = new Vector3(1f, 1f, 1f);
	private Vector3 fixedScale = new Vector3(1f, -1f, 1f);

	private int requestedWidth, requestedHeight;
	private Vector3 desiredScale;
	private bool isPaused = false;

	void Start()
	{
		// Check for device cameras
		if (WebCamTexture.devices.Length == 0)
		{
			Debug.Log("No devices cameras found");
			return;
		}

		requestedWidth = Screen.width;
		requestedHeight = Screen.height;

		// Get the device's cameras and create WebCamTextures with them
		frontCameraDevice = WebCamTexture.devices.Last();
		backCameraDevice = WebCamTexture.devices.First();
		frontCameraTexture = new WebCamTexture(frontCameraDevice.name);
		backCameraTexture = new WebCamTexture(backCameraDevice.name);

		frontCameraTexture.filterMode = FilterMode.Trilinear;
		backCameraTexture.filterMode = FilterMode.Trilinear;

		// Set the camera to use by default
		frontCameraTexture.Stop();
		SetActiveCamera(backCameraTexture);
	}

	// Set the device camera to use and start it
	public void SetActiveCamera(WebCamTexture cameraToUse)
	{
		if (activeCameraTexture != null)
		{
			activeCameraTexture.Stop();
		}

		//if (fixedTexture != null)
		//{
		//	fixedTexture.Stop();
		//}

		activeCameraTexture = cameraToUse;
		//fixedTexture = null;

		activeCameraDevice = WebCamTexture.devices.FirstOrDefault(device => 
			device.name == cameraToUse.deviceName);

		image.texture = activeCameraTexture;
		//image.material.mainTexture = activeCameraTexture;

		activeCameraTexture.Play();
		Debug.Log ("Started Webcam: " + activeCameraDevice.name);
	}

	// Switch between the device's front and back camera
	public void SwitchCamera()
	{
		SetActiveCamera(activeCameraTexture.Equals(frontCameraTexture) ? 
			backCameraTexture : frontCameraTexture);
	}

	// Make adjustments to image every frame to be safe, since Unity isn't 
	// guaranteed to report correct data as soon as device camera is started
	void Update()
	{
		// Skip making adjustment for incorrect camera data
		if (activeCameraTexture.width < 100)
		{
			//Debug.Log("Still waiting another frame for correct info... " + activeCameraDevice.name);
			return;
		}
			
		// Rotate image to show correct orientation 
		rotationVector.z = -activeCameraTexture.videoRotationAngle;
		image.rectTransform.localEulerAngles = rotationVector;

		// Set AspectRatioFitter's ratio
		//float videoRatio = 
		//	(float)activeCameraTexture.width / (float)activeCameraTexture.height;
		//imageFitter.aspectRatio = videoRatio;

		// Unflip if vertically flipped
		image.uvRect = 
			activeCameraTexture.videoVerticallyMirrored ? fixedRect : defaultRect;

		// Mirror front-facing camera's image horizontally to look more natural
		if (ReferenceEquals(frontCameraDevice, activeCameraDevice)) {
			desiredScale = fixedScale;
		} else {
			desiredScale = defaultScale;
		}
		//desiredScale = defaultScale;
		fitImageToScreen ();

	}

	private void fitImageToScreen() {
		

		float texWidth = activeCameraTexture.width;
		float texHeight = activeCameraTexture.height;

		// Apparently this isn't needed
		int rotAngle = activeCameraTexture.videoRotationAngle;
		if (rotAngle == 90 || rotAngle == 270) { // if rotated sideways, flip height and width
			float temp = texWidth;
			texWidth = texHeight;
			texHeight = temp;
		}


		float widthScale = requestedWidth / texWidth;
		float heightScale = requestedHeight / texHeight;

		float scaleToFitScreen;
		if (widthScale < heightScale) { // resize based on width
			scaleToFitScreen = widthScale;
		} else { // resize based on height
			scaleToFitScreen = heightScale;
		}

		//float newWidth = ratio * texWidth;
		//float newHeight = ratio * texHeight;

		//float parentScale = imageParent.GetComponent<RectTransform> ().localScale.z;

		//float posY = -(headerHeight / 2.0f) / ratio;

		//float parentZ = imageParent.GetComponent<RectTransform> ().anchoredPosition3D.z;
		//float parentScale = imageParent.GetComponent<Canvas>().scaleFactor;

		// examples: when ratio is 1, keep the original parentZ, which is 0 relative
		// when ratio is 2, z is halfed, making it ((-125) / parentScale)
		//float posZ = -(parentZ / ratio / parentScale);
		//float posZ = (parentZ / ratio - parentZ) / parentScale;

		//Debug.Log (parentZ);
		//Debug.Log (parentScale);

		if (rotAngle == 90 || rotAngle == 270) { // if rotated sideways, flip height and width
			float temp = texWidth;
			texWidth = texHeight;
			texHeight = temp;
		}
		image.GetComponent<RectTransform>().sizeDelta = new Vector2(texWidth,texHeight);
		image.GetComponent<RectTransform> ().localScale = desiredScale * scaleToFitScreen;
		//image.GetComponent<RectTransform>().sizeDelta = new Vector2(texWidth,texHeight);
		//image.GetComponent<RectTransform> ().anchoredPosition3D = new Vector3 (image.GetComponent<RectTransform> ().anchoredPosition.x, posY, posZ);
		//image.GetComponent<RectTransform> ().localScale = new Vector3 (ratio, ratio, 1.0f);
		/*
		if (fixedTexture == null) {
			fixedTexture = new WebCamTexture (activeCameraDevice.name, (int)newWidth, (int)newHeight);
			image.texture = fixedTexture;
			//image.material.color = Color.red;
			activeCameraTexture.Stop ();
			fixedTexture.Play ();
		}*/

		//texControl.Apply ();


		//debug1.text = "O: (" + texWidth + "," + texHeight + ") N: (" + image.GetComponent<RectTransform>().sizeDelta.x + "," + image.GetComponent<RectTransform>().sizeDelta.y + ")";
		//debug2.text = "S: (" + maxWidth + ", " + maxHeight + ") Trans:" + ratio;
	}

	public Texture2D takePicture() {
		Texture2D snap = new Texture2D(activeCameraTexture.width, activeCameraTexture.height);
		snap.SetPixels(activeCameraTexture.GetPixels());
		snap.Apply();

		return snap;
	}


	void OnApplicationFocus( bool hasFocus )
	{
		isPaused = !hasFocus;
		triggerPause ();
	}

	void OnApplicationPause( bool pauseStatus )
	{
		isPaused = pauseStatus;
		triggerPause ();
	}

	private void triggerPause() {
		if (isPaused) {
			stopCamera ();
		} else {
			Start ();
		}
	}

	public IEnumerator stopCamera() {
		
		Debug.Log ("Camera: Checking cameras still playing");

		Debug.Log ("Camera: Checking back camera");
		if (backCameraTexture != null) {
			Debug.Log ("Camera: Back camera not null");

			while (backCameraTexture.isPlaying) {
				Debug.Log ("Camera: Back camera still playing");
				backCameraTexture.Stop ();
				Debug.Log ("Camera: Stopping back camera");
				yield return null;
			}
		}


		Debug.Log ("Camera: Checking front camera");

		if (frontCameraTexture != null) {
			Debug.Log ("Camera: Front camera not null");

			while (frontCameraTexture.isPlaying) {
				Debug.Log ("Camera: Front camera still playing");
				frontCameraTexture.Stop ();
				Debug.Log ("Camera: Stopping front camera");
				yield return null;
			}
		}


		Debug.Log ("Camera: Checking active camera");

		if (activeCameraTexture != null) {
			Debug.Log ("Camera: Active camera not null");

			while (activeCameraTexture.isPlaying) {
				Debug.Log ("Camera: Active camera still playing");
				activeCameraTexture.Stop ();
				Debug.Log ("Camera: Stopping active camera");
				yield return null;
			}
		}

		if (activeCameraTexture != null)
		{
			image.texture = null;
		}

		Debug.Log ("Camera: Destroying cameras");
		yield return new WaitForSeconds(1.0f);
		Destroy (frontCameraTexture);
		Destroy (backCameraTexture);
		Destroy (activeCameraTexture);


		yield return new WaitForSeconds(1.0f);


		Debug.Log("Camera: Finished");

		yield return "Done";
	}

}