using System;
using System.Collections;
using System.Collections.Generic;
using LucidSightTools;
using UnityEngine;
using UnityEngine.UI;

public class AvatarCustomization : MonoBehaviour
{
    [SerializeField]
    private AvatarDisplay display = null;

    [SerializeField]
    private ColorSelector skinColorSelector = null;
    [SerializeField]
    private Color defaultSkinColor = Color.magenta;

    [SerializeField]
    private ColorSelector shirtColorSelector = null;
    [SerializeField]
    private Color defaultShirtColor = Color.magenta;

    [SerializeField]
    private ColorSelector pantsColorSelector = null;
    [SerializeField]
    private Color defaultPantsColor = Color.magenta;

    [SerializeField]
    private Toggle hatToggle = null;
    [SerializeField]
    private ColorSelector hatColorSelector = null;
    [SerializeField]
    private Color defaultHatColor = Color.magenta;

    private Action<AvatarState> onSave;
    private AvatarState avatar;

    public void DisplayView(AvatarState initializingState, Action<AvatarState> onSaveAction)
    {
        avatar = initializingState;
        onSave = onSaveAction;
        gameObject.SetActive(true);
    }

    public void CloseView(bool save)
    {
        if (save)
        {
            SetAvatarValues();
            onSave.Invoke(avatar);
        }

        gameObject.SetActive(false);
    }

    public void OnBtnClose()
    {
        CloseView(false);
    }

    void OnEnable()
    {
        InitializeControls();
    }
    private void InitializeControls()
    {
        skinColorSelector.Display(HexToColor(avatar.skinColor, defaultSkinColor));
        shirtColorSelector.Display(HexToColor(avatar.shirtColor, defaultShirtColor));
        pantsColorSelector.Display(HexToColor(avatar.pantsColor, defaultPantsColor));
        hatColorSelector.Display(HexToColor(avatar.hatColor, defaultHatColor));
        hatToggle.isOn = avatar.hatChoice != 0;
    }

    private string ColorToHex(Color color)
    {
        return string.Format("#{0}",ColorUtility.ToHtmlStringRGBA(color));
    }

    private Color HexToColor(string hex, Color defaultColor)
    {
        Color parsedColor = defaultColor;
        if (!ColorUtility.TryParseHtmlString(hex, out parsedColor))
        {
            LSLog.Log("Failed to parse " + hex + " for color, will use default!");
            return defaultColor;
        }

        return parsedColor;
    }

    //Callbacks for the UI
    public void OnHatEnableChange(bool val)
    {
        display.SetHatEnabled(val);
    }

    public void HatColorChange(Color color)
    {
        display.SetHat(hatToggle.isOn, ColorToHex(color));
    }

    public void SkinColorChange(Color color)
    {
        display.SetSkinTone(ColorToHex(color));
    }

    public void ShirtColorChange(Color color)
    {
        display.SetShirtColor(ColorToHex(color));
    }

    public void PantsColorChange(Color color)
    {
        display.SetPantsColor(ColorToHex(color));
    }

    public void SaveCustomization()
    {
        CloseView(true);
    }

    private void SetAvatarValues()
    {
        avatar.skinColor = ColorToHex(skinColorSelector.GetColor());
        avatar.shirtColor = ColorToHex(shirtColorSelector.GetColor());
        avatar.pantsColor = ColorToHex(pantsColorSelector.GetColor());
        avatar.hatColor = ColorToHex(hatColorSelector.GetColor());
        avatar.hatChoice = hatToggle.isOn ? 1 : 0;
    }

    public void CancelCustomization()
    {
        CloseView(false);
    }
}
