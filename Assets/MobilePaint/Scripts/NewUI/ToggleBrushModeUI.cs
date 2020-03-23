using UnityEngine;
using System.Collections;
using UnityEngine.UI;

namespace unitycoder_MobilePaint
{
	
	public class ToggleBrushModeUI : MonoBehaviour 
	{

		public GameObject changeColorButton;
		public RawImage brushUI;
		public Color brushUIColor;
		public Color brushDisabledColor;

		MobilePaint mobilePaint;

		void Start () 
		{
			mobilePaint = PaintManager.mobilePaint;

			if (mobilePaint==null) Debug.LogError("No MobilePaint assigned at "+transform.name,gameObject);

			GetComponent<Toggle>().onValueChanged.AddListener(delegate {this.SetMode();});
		}


		public void SetMode()
		{
			if (GetComponent<Toggle> ().isOn) {
				brushUI.color = brushUIColor;
				mobilePaint.SetDrawModeBrush ();
			} else {
				brushUI.color = brushDisabledColor;
			}

		}

	}
}