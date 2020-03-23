using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadGroundViewMinimap : MonoBehaviour {

	private static readonly string MAPBOX_SCENE = "Scenes/GroundViewMapBoxMinimap";

	// Use this for initialization
	void Start () {
		SceneManager.LoadScene (MAPBOX_SCENE, LoadSceneMode.Additive);
	}

}
