using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using CP = ColorPicker;

[RequireComponent(typeof(Image))]
public class ColorImage : MonoBehaviour
{
    public CP.ColorPicker picker;

    private Image image;

    private void Awake()
    {
        image = GetComponent<Image>();
        picker.onValueChanged.AddListener(ColorChanged);
    }

    private void OnDestroy()
    {
        picker.onValueChanged.RemoveListener(ColorChanged);
    }

    private void ColorChanged(Color newColor)
    {
        image.color = newColor;
    }
}
