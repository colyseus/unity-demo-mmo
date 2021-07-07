using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AvatarDisplay : MonoBehaviour
{
    [SerializeField]
    private Renderer[] shirtRenderers = null;

    [SerializeField]
    private Renderer[] legRenderers = null;

    [SerializeField]
    private Renderer[] skinRenderers = null;

    [SerializeField]
    private Renderer[] hatRenderers = null;

    [SerializeField]
    private GameObject hatRoot = null;

    public void SetHatEnabled(bool val)
    {
        hatRoot.SetActive(val);
    }

    public void SetHat(bool enabled, string color)
    {
        SetHatEnabled(enabled);
        if (enabled)
        {
            ColorRenderers(hatRenderers, color);
        }
    }

    public void SetSkinTone(string color)
    {
        ColorRenderers(skinRenderers, color);
    }

    public void SetShirtColor(string color)
    {
        ColorRenderers(shirtRenderers, color);
    }

    public void SetPantsColor(string color)
    {
        ColorRenderers(legRenderers, color);
    }

    public void DisplayFromState(AvatarState state)
    {
        SetHat(state.hatChoice != 0, state.hatColor);
        SetSkinTone(state.skinColor);
        SetShirtColor(state.shirtColor);
        SetPantsColor(state.pantsColor);
    }

    private void ColorRenderers(Renderer[] renderers, string colorHex)
    {
        Color color = renderers[0].material.color;  //Default in case we cant parse
        if (ColorUtility.TryParseHtmlString(colorHex, out color))
        {
            for (int i = 0; i < renderers.Length; ++i)
            {
                renderers[i].material.SetColor("_Color",color);
            }
        }
    }
}
