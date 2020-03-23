using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class LoadLoginScreen : MonoBehaviour {

	public Slider slider;

	// Use this for initialization
	void Start () {
		StartCoroutine (startSliderAnimation ());
	}

	private IEnumerator startSliderAnimation () {
		// Try to find the desired GameObject.
		// This will only find active GameObjects in the scene.

		int seconds = 2;
		float animationTime = 0f;
		while (true)
		{
			animationTime += Time.deltaTime;
			float lerpValue = animationTime / seconds;
			slider.value = Mathf.Lerp(slider.minValue, slider.maxValue, lerpValue);
			if (slider.value == slider.maxValue) {
				animationTime = 0f;
				slider.value = slider.minValue;
			}

			yield return null;
		}
	}
}
