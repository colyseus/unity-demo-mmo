using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class ColorSelector : MonoBehaviour
{
    [SerializeField]
    private UnityEvent<Color> onColorChanged = null;

    [SerializeField]
    private Slider rSlider = null;
    [SerializeField]
    private Slider gSlider = null;
    [SerializeField]
    private Slider bSlider = null;
    [SerializeField]
    private Slider aSlider = null;

    [SerializeField]
    private Image referenceImage = null;

    public void Display(Color color)
    {
        rSlider.value = color.r;
        gSlider.value = color.g;
        bSlider.value = color.b;

        if (aSlider != null)
        {
            aSlider.value = color.a;
        }

        OnSliderUpdated();
    }

    public void OnSliderUpdated()
    {
        Color color = GetColor();
        UpdateReferenceImage(color);
        onColorChanged?.Invoke(color);
    }

    private float[] GetValues()
    {
        if (aSlider == null)
        {
            return new float[] { rSlider.value, gSlider.value, bSlider.value, 1};
        }
        else
        {
            return new float[] { rSlider.value, gSlider.value, bSlider.value, aSlider.value };
        }
    }

    private void UpdateReferenceImage(Color color)
    {
        referenceImage.color = color;
    }

    public Color GetColor()
    {
        float[] color = GetValues();
        return new Color(color[0], color[1], color[2], color[3]);
    }
}
