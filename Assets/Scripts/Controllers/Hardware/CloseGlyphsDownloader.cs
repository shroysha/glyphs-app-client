using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CloseGlyphsDownloader : MonoBehaviour, HardwareController.LocationChangeListener {

	private static CloseGlyphsDownloader Instance;

	public static CloseGlyphsDownloader getInstance() {
		return Instance;
	}

	private List<GlyphDownloadListener> downloadListeners = new List<GlyphDownloadListener> ();
	private Dictionary<GPSLocation, List<Glyph>> downloadedGlyphs = new Dictionary<GPSLocation, List<Glyph>> ();

	void Awake() {
		Instance = this;
	}

	public IEnumerator onLocationChange(GPSLocation newLocation) {
		if (downloadListeners.Count != 0) {
			yield return downloadCloseGlyphs (newLocation);
		} else {
			yield return "No Download Listeners";
		}
	}

	private IEnumerator downloadCloseGlyphs(GPSLocation newLocation) {
		Debug.LogError (downloadedGlyphs.ContainsKey (newLocation));
		if(!downloadedGlyphs.ContainsKey(newLocation)) {
			downloadedGlyphs.Add (newLocation, null); // reserve key

			while (LoggedInUser.GetLoggedInUser () == null) {
				Debug.LogWarning ("GPS updated but no user logged in. Waiting to download.");
				yield return new WaitForSeconds (1.0f);
			}

			GPSLocation[] closeBounds = newLocation.calculateLatLongBoundingBox (GlyphUniverse.MAX_RENDER_DISTANCE);

			ServerCall call = new ServerCall(ServerInteract.INSTANCE.GetGlyphsInArea(closeBounds));
			yield return StartCoroutine (call.call ());

			if (call.ReturnException != null) {
				Debug.LogException (call.ReturnException);
			} else {
				List<Glyph> closeGlyphs = (List<Glyph>) call.ObjectResponse;
				if (closeGlyphs.Count == 0) {
					fireGlyphDownloadedListeners (null);
				} else {
					foreach (Glyph glyph in closeGlyphs) {
						fireGlyphDownloadedListeners (glyph);
					}

					downloadedGlyphs [newLocation] = closeGlyphs;
				}
			}
		}

		yield return "Done";
	}

	public void addGlyphDownloadedListener(GlyphDownloadListener listener) {
		downloadListeners.Add (listener);

		if (downloadedGlyphs.Count != 0) {
			Debug.LogWarning ("Already has downloaded glyphs. Immediately firing");
			foreach (List<Glyph> glyphs in downloadedGlyphs.Values) {
				if (glyphs != null) {
					foreach (Glyph glyph in glyphs) {
						StartCoroutine (listener.onDownloadedGlyph (glyph));
					}
				}
			}
		}
	}


	public void removeGlyphDownloadedListener(GlyphDownloadListener listener) {
		downloadListeners.Remove (listener);
	}

	private void fireGlyphDownloadedListeners(Glyph glyph) {
		foreach (GlyphDownloadListener listener in downloadListeners) {
			StartCoroutine (listener.onDownloadedGlyph (glyph));
		}
	}


	public interface GlyphDownloadListener {

		IEnumerator onDownloadedGlyph(Glyph newGlyph);

	}

}
