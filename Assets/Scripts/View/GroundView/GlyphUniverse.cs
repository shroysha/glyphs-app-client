using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GlyphUniverse : MonoBehaviour, HardwareController.LocationChangeListener, HardwareController.DeviceAngleChangeListener, CloseGlyphsDownloader.GlyphDownloadListener{

	private static readonly string GROUND_VIEW_SCENE = "Scenes/GroundViewScene";

	private static readonly string WAITING_FOR_GPS_STATUS = "Waiting for GPS...";
	private static readonly string DOWNLOADING_CLOSE_GLYPHS_STATUS = "Downloading Close Glyphs...";
	private static readonly string NO_CLOSE_GLYPHS_STATUS = "There are no visible glyphs around you. \nCreate one!";
	private static readonly string NO_USER_LOGGED_IN_STATUS = "No user logged in";

	private static readonly float M_TO_UNITY_SCALAR = 1.0f; // ___m = 1 Unity point
	public static readonly float MAX_RENDER_DISTANCE = 100.0f; // render glyphs ___m around player
	private static readonly float RENDERINGANGLE = 10.0f; // render within ___ degrees of center
	private static readonly float CLOSEST_SPHERE_RENDER_DISTANCE = 0.5f * M_TO_UNITY_SCALAR; // ___ m away
	private static readonly float FARTHEST_SPHERE_RENDER_DISTANCE = 75.0f * M_TO_UNITY_SCALAR; // ___ m away

	public GameObject glyphSpherePrefab;

	public Camera playerCamera;
	public Button addButton;
	public AppStatusPanel statusPanel;

	private GPSLocation currentPlayerLocation;
	private List<GlyphSphere> glyphSpheres = new List<GlyphSphere>();
	private GlyphSphere sphereToRender;
	private List<RenderGlyphListener> renderGlyphListeners = new List<RenderGlyphListener>();

	private List<GlyphSphere> activeSpheres;

	// Use this for initialization
	void Start () {
		interfaceWithHardwareController ();
		statusPanel.showStatus (WAITING_FOR_GPS_STATUS);
		addButton.onClick.AddListener (onAddButton);
	}

	private void onAddButton() {
		LoadTakePictureScene.setBackButtonScene (GROUND_VIEW_SCENE);
		GeneralFunctions.switchToAddGlyphScene ();
	}

	private void interfaceWithHardwareController() {
		HardwareController.Instance.addLocationListener (this);
		HardwareController.Instance.addDeviceAngleChangeListener (this);
		CloseGlyphsDownloader.getInstance ().addGlyphDownloadedListener (this);
	}

	public IEnumerator onLocationChange(GPSLocation newDeviceLocation) {
		setNewPlayerLocation (newDeviceLocation);

		reevaluateGlyphUniverseByLocation ();

		yield return "Done";
	}

	private void setNewPlayerLocation(GPSLocation newLocation) {
		bool firstRun = false;
		if (currentPlayerLocation.Equals(GPSLocation.UNDEFINED)) {
			firstRun = true;
		}

		if (!newLocation.Equals(currentPlayerLocation)) {
			currentPlayerLocation = newLocation;
			if (LoggedInUser.GetLoggedInUser () != null) {
				if (firstRun) {
					loadOwnedGlyphSpheres ();
				}
			} else {
				statusPanel.showStatus (NO_USER_LOGGED_IN_STATUS);
			}
		}

	}
		
	private void loadOwnedGlyphSpheres() {

		foreach (OwnedGlyphEvent glyph in LoggedInUser.GetLoggedInUser().ownedGlyphs) {
			GameObject go = Instantiate (glyphSpherePrefab);
			GlyphSphere sphere = go.GetComponent<GlyphSphere> ();
			sphere.setGlyph (glyph.GetGlyph());
			glyphSpheres.Add(sphere);
		}

	}

	public IEnumerator onDownloadedGlyph(Glyph newGlyph) {
		// if doesnt contain glyph, create a new glyph sphere for it
		if (newGlyph != null) {

			bool contains = false;
			foreach (GlyphSphere sphere in glyphSpheres) {
				if (sphere.getGlyph ().Equals (newGlyph)) {
					contains = true;
					break;
				}
			}

			if (!contains) {
				GameObject go = Instantiate (glyphSpherePrefab);
				GlyphSphere sphere = go.GetComponent<GlyphSphere> ();
				sphere.setGlyph (newGlyph);
				glyphSpheres.Add(sphere);

				glyphSpheres.Add (sphere);
			}

			reevaluateGlyphUniverseByLocation ();
		}

		yield return "Done";
	}

	public IEnumerator onDeviceAngleChange(Quaternion newDeviceAngle) {
		setNewPlayerCameraAngle (newDeviceAngle);

		reevaluateGlyphUniverseByAngle ();

		yield return "Done";
	}

	private void setNewPlayerCameraAngle(Quaternion newAngle) {
		playerCamera.transform.rotation = newAngle;
	}

	private void reevaluateGlyphUniverseByLocation() {

		activeSpheres = enableOnlyCloseGlyphs (); // done by comparing glyph lat, long rather than sphere pos

		if (LoggedInUser.GetLoggedInUser () == null) {
			statusPanel.showStatus (NO_USER_LOGGED_IN_STATUS);
		} else if (activeSpheres.Count == 0) {
			statusPanel.showStatus (NO_CLOSE_GLYPHS_STATUS);
		} else {
			statusPanel.setStatusPanelVisible (false);
		}

		replotGlyphSpheres ();

		pushGlyphsThatAreTooCloseAway ();

		reevaluateGlyphUniverseByAngle ();
	}


	private List<GlyphSphere> enableOnlyCloseGlyphs() {
		GPSLocation[] closeBounds = currentPlayerLocation.calculateLatLongBoundingBox (MAX_RENDER_DISTANCE);

		List<GlyphSphere> activeSpheres = new List<GlyphSphere> ();
		for (int i = 0; i < glyphSpheres.Count; i++) {
			GlyphSphere sphere = glyphSpheres [i];
			GPSLocation glyphLocation = glyphSpheres [i].getGlyph ().GetGpsLocation();
			bool glyphSphereIsInsideBounds = glyphLocation.isInsideBounds(closeBounds);
			Debug.LogError ("Bounds : " + sphere.getGlyph ().GetGlyphId() + " " + glyphSphereIsInsideBounds);
			sphere.setSphereActive (glyphSphereIsInsideBounds);

			if (sphere.isSphereActive ()) {
				activeSpheres.Add (sphere);
			}
		}

		return activeSpheres;
	}
		

	private void replotGlyphSpheres() {
		// camera always stays at 0,0,0. we want to reposition the glyphs around it.
		foreach (GlyphSphere sphere in activeSpheres) {
			Vector3 newSpherePosition = getNewGlyphSpherePosition (sphere);
			sphere.repositionGlyphSphere (newSpherePosition);
		}
	}

	private Vector3 getNewGlyphSpherePosition(GlyphSphere glyphSphere) {

		GPSLocation glyphLocation = glyphSphere.getGlyph ().GetGpsLocation();
		Vector2 offsetInM = currentPlayerLocation.calculatePolarizedOffsetInMeters (glyphLocation);

		float unityPositionZ = offsetInM.x * M_TO_UNITY_SCALAR; //latitude
		float unityPositionX = offsetInM.y * M_TO_UNITY_SCALAR; //longitude
		float unityPositionY = 0.0f; //altitude

		return new Vector3 (unityPositionX, unityPositionY, unityPositionZ);
	}


	private void pushGlyphsThatAreTooCloseAway() {

		foreach(GlyphSphere sphere in activeSpheres) {

			float distanceToGlyph = Vector3.Distance (playerCamera.transform.position, sphere.getGlyphSpherePosition());

			if (distanceToGlyph < CLOSEST_SPHERE_RENDER_DISTANCE) { // if glyph is within closest area, extrapoluate farther
				// we need to push the glyph to the closestDistance while keeping the same angle
				Debug.Log ("Need to push Glyph " + sphere.getGlyph().GetGlyphId());
			} else {
				// else, return the sphere to it's original position if needed
				Debug.Log ("Not Pushing Glyph " + sphere.getGlyph ().GetGlyphId());
			}

		}
	}


	private void reevaluateGlyphUniverseByAngle() {
		if (activeSpheres == null) {
			return;
		}

		renderClosestGlyphInRenderArea ();
	}

	private void renderClosestGlyphInRenderArea() {

		if (playerCamera == null) {
			return;
		}

		GlyphSphere closestGlyphInRenderArea = null;
		Vector3 cameraPosition = playerCamera.transform.position;
		float closestGlyphDistance = FARTHEST_SPHERE_RENDER_DISTANCE;

		foreach (GlyphSphere sphere in activeSpheres) {
			Debug.Log ("*******************************************************");
			Debug.LogWarning ("*******************************************************");
			Debug.LogError ("*******************************************************");

			Debug.Log ("Checking if  " + sphere.getGlyph().GetGlyphId() + " inside render bounds");
			float distanceToGlyph = Vector3.Distance (playerCamera.transform.position, sphere.getGlyphSpherePosition());

			if (CLOSEST_SPHERE_RENDER_DISTANCE <= distanceToGlyph && distanceToGlyph <= FARTHEST_SPHERE_RENDER_DISTANCE) {
				Debug.Log ("Glyph  " + sphere.getGlyph().GetGlyphId() + " inside render bounds");
				sphere.setSphereColor(Color.blue);

				Debug.Log ("Checking if " + sphere.getGlyph().GetGlyphId() + " is in viewing angle...");

				if (isWithinRenderAngle (sphere)) {
					Debug.Log ("Glyph " + sphere.getGlyph().GetGlyphId() + " in viewing angle");

					Debug.Log ("Checking if " + sphere.getGlyph().GetGlyphId() + " closest glyph");

					if (distanceToGlyph < closestGlyphDistance) {
						Debug.Log ("Glyph " + sphere.getGlyph().GetGlyphId() + " qualified as closest glyph");
						closestGlyphDistance = distanceToGlyph;
						closestGlyphInRenderArea = sphere;
					} else {
						if(closestGlyphInRenderArea != null)
							Debug.LogWarning ("Glyph " + closestGlyphInRenderArea.getGlyph().GetGlyphId() + " closer than " + sphere.getGlyph().GetGlyphId());
					}
				} else {
					Debug.LogWarning ("Glyph " + sphere.getGlyph().GetGlyphId() + " not in viewing angle");
				}

			} else {
				Debug.LogWarning ("Glyph " + sphere.getGlyph().GetGlyphId() + " not inside render bounds");
				sphere.setSphereColor (Color.red);
			}

		}

		if (closestGlyphInRenderArea == null) {
			Debug.Log ("Render: No closest glyph");
			sphereToRender = null;
			fireStopRenderring ();
		} else if (closestGlyphInRenderArea != sphereToRender) {
			sphereToRender = closestGlyphInRenderArea;
			fireStartRenderring (sphereToRender.getGlyph());
		} else {
			Debug.Log ("Render: Glyph already renderred");
		}

	}

	private bool isWithinRenderAngle(GlyphSphere sphere) {

		Vector3 targetDir = sphere.getGlyphSpherePosition() - playerCamera.transform.position;
		Vector3 forward = playerCamera.transform.forward;

		float angle = Vector3.Angle (forward, targetDir);
		Debug.Log ("Angle to Glyph " + sphere.getGlyph().GetGlyphId() +  " is:" + angle);

		return angle <= RENDERINGANGLE;
	}

	public void addRenderGlyphListener(RenderGlyphListener listener) {
		renderGlyphListeners.Add (listener);
	}

	public void removeRenderGlyphListener(RenderGlyphListener listener) {
		renderGlyphListeners.Remove (listener);
	}

	private void fireStartRenderring(Glyph glyph) {
		foreach(RenderGlyphListener listener in renderGlyphListeners) {
			listener.startRenderringGlyph (glyph);
		}
	}

	private void fireStopRenderring() {
		foreach(RenderGlyphListener listener in renderGlyphListeners) {
			listener.stopRenderringGlyph ();
		}
	}

	public void OnDestroy() {
		HardwareController.Instance.removeLocationListener (this);
		HardwareController.Instance.removeDeviceAngleChangeListener (this);
		CloseGlyphsDownloader.getInstance ().removeGlyphDownloadedListener (this);
		clearRenderGlyphListeners ();
	}

	public void clearRenderGlyphListeners() {
		renderGlyphListeners.Clear ();
	}

	public interface RenderGlyphListener {

		void startRenderringGlyph(Glyph sphere);
		void stopRenderringGlyph();

	}

}
