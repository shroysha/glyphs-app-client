using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HardwareController : MonoBehaviour {
	
	//string 40.79639,-77.883949279
	private static readonly float DEFAULT_LATITUDE = 39.480629f;
	private static readonly float DEFAULT_LONGITUDE = -77.712664f;
	private static readonly GPSLocation DEFAULT_LOCATION = new GPSLocation (DEFAULT_LATITUDE, DEFAULT_LONGITUDE);

	// Minimum time interval between location updates, in milliseconds.
	private static readonly long ANDROID_UPDATE_FREQUENCY_MILLISECONDS = 200;
	// Minimum distance between location updates, in meters.
	private static readonly float ANDROID_UPDATE_ON_LOCATION_DELTA = 0.1f;
	private static readonly float DEVICE_ANGLE_UPDATE_HZ = 20.0f;
	private static readonly float DEVICE_ANGLE_UPDATE_FREQUENCY = 1.0f / DEVICE_ANGLE_UPDATE_HZ;

	public bool listenForLocationUpdates = true;
	public bool listenForDeviceAngleUpdates = true;

	private List<LocationChangeListener> locationListeners = new List<LocationChangeListener>();
	private List<DeviceAngleChangeListener> deviceAngleChangeListeners = new List<DeviceAngleChangeListener> ();
	private List<CompassChangeListener> compassChangeListeners = new List<CompassChangeListener> ();

	private GPSLocation lastLocation;

	private Quaternion currentDeviceAngle;
	private Quaternion lastDeviceOrientation = Quaternion.identity;
	private double lastCompassUpdateTime = 0;
	private Quaternion correction = Quaternion.identity;
	private Quaternion targetCorrection = Quaternion.identity;
	private Quaternion compassOrientation = Quaternion.identity;

	public static HardwareController Instance;

	public static void initialize() {
		if (Instance != null) {
			return;
		}

		//Debug.logger.logEnabled = false;
		if (Screen.fullScreen) {
			Screen.fullScreen = false;
		}
		GameObject hardwareController = new GameObject ("Hardware Controller");
		Instance = hardwareController.AddComponent<HardwareController> ();
		DontDestroyOnLoad (hardwareController);

		GameObject glyphDownloader = new GameObject ("Glyph Downloader");
		CloseGlyphsDownloader downloader = glyphDownloader.AddComponent<CloseGlyphsDownloader> ();
		Instance.addLocationListener (downloader);
		DontDestroyOnLoad (glyphDownloader);
	}

	// Use this for initialization
	void Start () {
		enableHardwareDevices ();
		InvokeRepeating ("UpdateDeviceDetails", 0.0f, DEVICE_ANGLE_UPDATE_FREQUENCY);
	}
		
	public GPSLocation getLastLocation() {
		return lastLocation;
	}

	private void enableHardwareDevices() {
		Debug.Log("Enabling hardware devices");

		if (listenForDeviceAngleUpdates) {
			enableCompass ();
			enableGyroscope ();
		}

		if (listenForLocationUpdates) {
			enableLocationServices ();
		}

		enableNoSleep ();

		Debug.Log("Hardware devices enabled");
	}

	private void enableCompass() {
		Input.compass.enabled = true;
	}

	private void enableGyroscope() {
		Input.gyro.enabled = true;
	}

	private void enableNoSleep() {
		Screen.sleepTimeout = SleepTimeout.NeverSleep;
	}

	private void enableLocationServices() {
		#if UNITY_EDITOR 
			enableUnityEditorLocationServices();
		#endif

		#if UNITY_ANDROID
			enableAndroidLocationServices();
		#endif

	}
		
	public void enableUnityEditorLocationServices() {
		lastLocation = DEFAULT_LOCATION;
		InvokeRepeating ("constantLocationUpdateRefresh", 0.0f, 5.0f);
	}

	public void enableAndroidLocationServices() {
		// AGGPS.RequestLocationUpdates(ANDROID_UPDATE_FREQUENCY_MILLISECONDS, ANDROID_UPDATE_ON_LOCATION_DELTA, onAndroidLocationChanged);
	}
		

	// Update is called once per frame
	void UpdateDeviceDetails () {
		if (listenForDeviceAngleUpdates && deviceAngleChangeListeners.Count != 0) {
			processDeviceAngleChange ();
		}

	}

	private void constantLocationUpdateRefresh() {
		if (listenForLocationUpdates && locationListeners.Count != 0) {
			onNewLocation (DEFAULT_LOCATION);
		}
	}

	private void processDeviceAngleChange() {
		bool deviceAngleChanged = detectDeviceAngleChange ();

		if (deviceAngleChanged) {
			fireDeviceAngleChangedListeners ();
		}
	}

	private bool detectDeviceAngleChange() {
	 	currentDeviceAngle = getCurrentDeviceAngle ();
		bool deviceOrientationChanged = !lastDeviceOrientation.Equals (currentDeviceAngle);

		if (deviceOrientationChanged) {
			lastDeviceOrientation = currentDeviceAngle;
		}

		return deviceOrientationChanged;
	}


	private Quaternion getCurrentDeviceAngle() {
		// The gyro is very effective for high frequency movements, but drifts its
		// orientation over longer periods, so we want to use the compass to correct it.
		// The iPad's compass has low time resolution, however, so we let the gyro be
		// mostly in charge here.

		// First we take the gyro's orientation and make a change of basis so it better
		// represents the orientation we'd like it to have
		Quaternion gyroOrientation = Quaternion.Euler (180, 0, 0) * Input.gyro.attitude * Quaternion.Euler(0, 0, 180);
		//Quaternion gyroOrientation = Input.gyro.attitude;

		// See if the compass has new data
		if (Input.compass.timestamp > lastCompassUpdateTime)
		{
			lastCompassUpdateTime = Input.compass.timestamp;

			// Work out an orientation based primarily on the compass
			Vector3 gravity = Input.gyro.gravity.normalized;
			Vector3 flatNorth = Input.compass.rawVector -
				Vector3.Dot(gravity, Input.compass.rawVector) * gravity;
			compassOrientation = Quaternion.Euler (180, 0, 0) * Quaternion.Inverse(Quaternion.LookRotation(flatNorth, -gravity)) * Quaternion.Euler (0, 0, 180);
			//_compassOrientation = Quaternion.Inverse(Quaternion.LookRotation(flatNorth, -gravity));
			fireCompassChangeListeners(0);
			// Calculate the target correction factor
			targetCorrection = compassOrientation * Quaternion.Inverse(gyroOrientation);
		}

		// Jump straight to the target correction if it's a long way; otherwise, slerp towards it very slowly
		if (Quaternion.Angle (correction, targetCorrection) > 45) {
			correction = targetCorrection;
		} else {
			correction = Quaternion.Slerp (correction, targetCorrection, 2.0f * Time.deltaTime);
		}
		/*
		Debug.Log ("Gyro Camera Correction: " + gyroOrientation.eulerAngles.ToString());
		Debug.Log ("Compass Camera Correction: " + compassOrientation.eulerAngles.ToString ());
		Debug.Log ("Target Camera Correction: " + targetCorrection.eulerAngles.ToString ());
		Debug.Log ("Camera Correction: " + correction.eulerAngles.ToString ());
*/
		return correction * gyroOrientation;
	}


	public void addDeviceAngleChangeListener(DeviceAngleChangeListener listener) {
		deviceAngleChangeListeners.Add (listener);
	}

	public void removeDeviceAngleChangeListener(DeviceAngleChangeListener listener) {
		deviceAngleChangeListeners.Remove (listener);
	}


	private void fireDeviceAngleChangedListeners() {
		for (int i = 0; i < deviceAngleChangeListeners.Count; i++) {
			StartCoroutine(deviceAngleChangeListeners[i].onDeviceAngleChange (currentDeviceAngle));
		}
	}

		

	public void addCompassChangeListener(CompassChangeListener listener) {
		compassChangeListeners.Add (listener);
	}

	public void removeCompassChangeListener(CompassChangeListener listener) {
		compassChangeListeners.Remove (listener);
	}


	private void fireCompassChangeListeners(int bearing) {
		for (int i = 0; i < compassChangeListeners.Count; i++) {
			StartCoroutine(compassChangeListeners[i].onCompassChange (bearing));
		}
	}

		
	private void onAndroidLocationChanged(GPSLocation location) {
		 GPSLocation newLocation = location; // = new GPSLocation (location.Latitude, location.Longitude);
		onNewLocation (newLocation);
	}

	private void onNewLocation(GPSLocation location) {
		lastLocation = location;
		fireLocationChangeListeners(location);
	}

	public void addLocationListener(LocationChangeListener listener) {
		locationListeners.Add (listener);

		if (!lastLocation.Equals(GPSLocation.UNDEFINED)) {
			Debug.LogWarning ("Can already access location. Immediately firing new location on new listener");
			StartCoroutine(listener.onLocationChange (lastLocation));
		}
	}


	public void removeLocationListener(LocationChangeListener listener) {
		bool removed = locationListeners.Remove (listener);
		Debug.LogError ("Removed " + removed);
	}

	private void fireLocationChangeListeners(GPSLocation newLocation) {
		foreach (LocationChangeListener listener in locationListeners) {
			StartCoroutine (listener.onLocationChange (newLocation));
		}
	}



	public void OnDestroy() {
		clearAllListeners ();
		disableHardwareDevices ();
	}

	private void clearAllListeners() {
		deviceAngleChangeListeners.Clear ();
		locationListeners.Clear ();
	}

	private void disableHardwareDevices() {
		disableLocationServices ();
		disableCompass ();
		disableGyroscope ();
		disableNoSleep ();
	}

	private void disableLocationServices() {
		// AGGPS.RemoveUpdates();
	}
		
	private void disableCompass() {
		Input.compass.enabled = false;
	}

	private void disableGyroscope() {
		Input.gyro.enabled = false;
	}

	private void disableNoSleep() {
		Screen.sleepTimeout = SleepTimeout.SystemSetting;
	}

	public void OnApplicationPause() {
		disableLocationServices ();
	}

	public void OnApplicationResume() {
		enableLocationServices ();
	}

	public interface DeviceAngleChangeListener {

		IEnumerator onDeviceAngleChange(Quaternion newAngle);

	}

	public interface LocationChangeListener {

		IEnumerator onLocationChange(GPSLocation newLocation);

	}

	public interface CompassChangeListener {
		
		IEnumerator onCompassChange(int bearing);

	}

}
