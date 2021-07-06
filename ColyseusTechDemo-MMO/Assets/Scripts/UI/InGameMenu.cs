using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InGameMenu : MonoBehaviour
{
    [SerializeField]
    private AvatarCustomization avatarCustomization = null;

    public void OnBtnClose()
    {
        UIManager.Instance.ToggleInGameMenu();
    }

    public void OnBtnMainMenu()
    {
        MMOManager.Instance.ExitToMainMenu();
    }

    public void OnBtnCustomize()
    {
        NetworkedEntity entity = NetworkedEntityFactory.Instance.GetMine();
        if (entity && entity.Avatar != null)
        {
            avatarCustomization.DisplayView(entity.Avatar.Clone(), OnCustomizationSave);
        }
    }

    private void OnCustomizationSave(AvatarState avatarState)
    {
        MMOManager.NetSend("avatarUpdate", avatarState.ToNetSendObjects());
    }
}
