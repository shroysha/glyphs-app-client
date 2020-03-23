using UnityEngine;
using UnityEngine.UI;

public class DateSeparator : MonoBehaviour {

	public static readonly float HEIGHT = 48.0f;

	public RectTransform separatorTransform;

	public Text dateText;

	// Use this for initialization
	void Start () {
		
	}

	public void setDateText(string date) {
		dateText.text = date;
	}
}
