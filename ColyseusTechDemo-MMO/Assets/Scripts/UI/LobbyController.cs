using System;
using System.Collections;
using System.Collections.Generic;
using Colyseus;
using Colyseus.Schema;
using LucidSightTools;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LobbyController : MonoBehaviour
{
    [SerializeField]
    private GameObject connectingCover = null;

    [SerializeField]
    private CreateUserMenu createUserMenu = null;

    [SerializeField]
    private ServerSetupMenu serverMenu = null;

    public string gameSceneName = "ExampleScene";
    
    private void Awake()
    {
        connectingCover.SetActive(true);
    }

    private IEnumerator Start()
    {
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        while (!MMOManager.IsReady)
        {
            yield return new WaitForEndOfFrame();
        }

        connectingCover.SetActive(false);
        createUserMenu.gameObject.SetActive(false);
        serverMenu.gameObject.SetActive(true);
    }

    public void OnButtonEvent_Begin(GameObject buttonObject)
    {
        ColyseusSettings clonedSettings = MMOManager.Instance.CloneSettings();
        clonedSettings.colyseusServerAddress = serverMenu.ServerURL;
        clonedSettings.colyseusServerPort = serverMenu.ServerPort;
        clonedSettings.useSecureProtocol= serverMenu.UseSecure;

        MMOManager.Instance.OverrideSettings(clonedSettings);

        MMOManager.Instance.InitializeClient();

        Button button = buttonObject.GetComponent<Button>();
        UISpinner spinner = buttonObject.GetComponentInChildren<UISpinner>(true);

        button.interactable = false;
        spinner?.gameObject.SetActive(true);

        if (MMOPlayerPrefs.AccountExists)
        {
            createUserMenu.LogUserIn(MMOPlayerPrefs.Email, MMOPlayerPrefs.Password, (error) =>
            {// in case of quick-login error, take user to log in view
                serverMenu.gameObject.SetActive(false);
                createUserMenu.EnableView(true);
                createUserMenu.UpdateErrorText(error);

                button.interactable = true;
                spinner?.gameObject.SetActive(false);
            });
        }
        else
        {
            serverMenu.gameObject.SetActive(false);
            createUserMenu.EnableView(false);
        }
    }

    public void ConsumeSeatReservation(UserAuthResponse userAuthResponse)
    {
        if (userAuthResponse != null)
        {
            connectingCover.SetActive(true);

            MMOManager.Instance.StartCoroutine(Co_LoadNextSceneThenJoinRoom(userAuthResponse, gameSceneName, null));
        }
        else
        {
            LSLog.LogError($"Failed to convert response to UserAuthResponse!");
        }
    }

    private IEnumerator Co_LoadNextSceneThenJoinRoom(UserAuthResponse userAuthResponse, string scene, Action onComplete)
    {
        // Load the next scene
        yield return LoadSceneAsync(scene, onComplete);

        yield return MMOManager.Instance.LoadGridAndConsumeSeatReservation(userAuthResponse);
    }

    private IEnumerator LoadSceneAsync(string scene, Action onComplete)
    {
        Scene currScene = SceneManager.GetActiveScene();
        AsyncOperation op = SceneManager.LoadSceneAsync(scene, LoadSceneMode.Additive);
        while (op.progress <= 0.9f)
        {
            //Wait until the scene is loaded
            yield return new WaitForEndOfFrame();
        }

        op.allowSceneActivation = true;

        op = SceneManager.UnloadSceneAsync(currScene);
        while (op.progress <= 0.9f)
        {
            //Wait until the scene is unloaded
            yield return new WaitForEndOfFrame();
        }

        onComplete?.Invoke();
    }
}