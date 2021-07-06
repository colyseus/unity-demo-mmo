using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ServerSetupMenu : MonoBehaviour
{
#pragma warning disable 0649
    [SerializeField]
    private InputField serverURLInput = null;
    [SerializeField]
    private InputField serverPortInput = null;
    [SerializeField]
    private Toggle secureToggle;

    [SerializeField]
    private Text logOutText = null;
#pragma warning restore 0649

    public string ServerURL
    {
        get
        {
            if (string.IsNullOrEmpty(serverURLInput.text) == false)
            {
                return serverURLInput.text;
            }

            return MMOManager.Instance.ColyseusServerAddress;
        }
    }

    public string ServerPort
    {
        get
        {
            if (string.IsNullOrEmpty(serverPortInput.text) == false)
            {
                return serverPortInput.text;
            }

            return MMOManager.Instance.ColyseusServerPort;
        }
    }

    public bool UseSecure
    {
        get
        {
            return secureToggle.isOn;
        }
    }
    
    private void Start()
    {
        serverURLInput.text = MMOManager.Instance.ColyseusServerAddress;
        serverPortInput.text = MMOManager.Instance.ColyseusServerPort;
        secureToggle.isOn = MMOManager.Instance.ColyseusUseSecure;
        
        SetUpLogOut();
    }

    /// <summary>
    /// Clears the saved log in credentials
    /// </summary>
    public void OnButtonEvent_LogOutExisting()
    {
        MMOPlayerPrefs.Clear();

        SetUpLogOut();
    }

    private void SetUpLogOut()
    {
        logOutText.gameObject.SetActive(string.IsNullOrEmpty(MMOPlayerPrefs.Email) == false && string.IsNullOrEmpty(MMOPlayerPrefs.Password) == false);
        logOutText.text = $"Log Out {MMOPlayerPrefs.Email}";
    }
}
