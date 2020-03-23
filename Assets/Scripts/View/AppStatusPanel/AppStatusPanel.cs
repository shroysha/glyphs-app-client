using UnityEngine;
using UnityEngine.UI;

public class AppStatusPanel : MonoBehaviour {

	public GameObject statusPanelScrimObject;

	public Text statusText;
	public GameObject loadingSpinner;

	public void showStatus(string text) {
		setStatusPanelVisible (true);
		statusText.text = text;
		setLoadingSpinnerVisible (false);
	}

	public void showLoadingStatus(string text) {
		setStatusPanelVisible (true);
		statusText.text = text;
		setLoadingSpinnerVisible (true);
	}

	public void showErrorStatus(string text) {
		setStatusPanelVisible (true);
		statusText.text = text;
		setLoadingSpinnerVisible (true);
	}

	public void setStatusPanelVisible(bool visible) {
		statusPanelScrimObject.SetActive (visible);
	}

	private void setLoadingSpinnerVisible(bool visible) {
		loadingSpinner.SetActive (visible);
	}
}
