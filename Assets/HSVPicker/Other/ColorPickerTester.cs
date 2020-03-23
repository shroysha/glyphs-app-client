using UnityEngine;
using System.Collections;
using CP=ColorPicker;

public class ColorPickerTester : MonoBehaviour 
{

    public new Renderer renderer;
    public CP.ColorPicker picker;

	// Use this for initialization
	void Start () 
    {
        picker.onValueChanged.AddListener(color =>
        {
            renderer.material.color = color;
        });
		renderer.material.color = picker.CurrentColor;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
}
