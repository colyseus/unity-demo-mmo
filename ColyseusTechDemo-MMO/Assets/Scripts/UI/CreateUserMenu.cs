using System;
using Colyseus;
using Colyseus.Schema;
using LucidSightTools;
using UnityEngine;
using UnityEngine.UI;

public class CreateUserMenu : MonoBehaviour
{
    // Primary view components
    //====================================
    [SerializeField]
    private LobbyController lobbyController;

    [SerializeField]
    private GameObject optionsView;

    [SerializeField]
    private GameObject loginView;

    [SerializeField]
    private GameObject signUpView;

    [SerializeField]
    private GameObject backBtn;

    //====================================

    // Sign Up UI
    //====================================
    [SerializeField]
    private Text errorMsg;

    [SerializeField]
    private InputField usernameInput;

    [SerializeField]
    private InputField emailInput;

    [SerializeField]
    private InputField passwordInput;

    [SerializeField]
    private Button signUpBtn;

    private bool _attemptingSignUp = false;
    //====================================

    // Log In
    //====================================
    [SerializeField]
    private Text logInErrorMsg;

    [SerializeField]
    private InputField logInEmailInput;

    [SerializeField]
    private InputField logInPasswordInput;

    [SerializeField]
    private Button logInBtn;

    [SerializeField]
    private Toggle rememberMe;

    private bool _attemptingLogin = false;
    //====================================

    public void EnableView(bool showLogin)
    {
        gameObject.SetActive(true);

        ResetViews();

        if (showLogin)
        {
            ShowLogIn();
        }
    }

    /// <summary>
    /// Button event handler for the back button
    /// </summary>
    public void OnButtonEvent_Back()
    {
        ResetViews();
    }

    /// <summary>
    /// Button event handler for the log in view button
    /// </summary>
    public void OnButtonEvent_Login()
    {
        ShowLogIn();
    }

    /// <summary>
    /// Enables the UI for the log in process
    /// </summary>
    private void ShowLogIn()
    {
        optionsView.SetActive(false);
        loginView.SetActive(true);
        backBtn.SetActive(true);

        bool remember = MMOPlayerPrefs.RememberMe;

        if (!remember)
        {
            logInEmailInput.text = logInPasswordInput.text = "";
        }
        else
        {
            logInEmailInput.text = MMOPlayerPrefs.Email;
            logInPasswordInput.text = MMOPlayerPrefs.Password;
        }
    }

    /// <summary>
    /// Button event handler for the sign up view button
    /// </summary>
    public void OnButtonEvent_SignUp()
    {
        optionsView.SetActive(false);
        signUpView.SetActive(true);
        backBtn.SetActive(true);

        usernameInput.text = emailInput.text = passwordInput.text = "";
    }

    /// <summary>
    /// Button event handler for the sign up button.
    /// Initiates user sign up with the server. 
    /// </summary>
    /// <param name="buttonObject"></param>
    public void OnButtonEvent_SubmitSignUp(GameObject buttonObject)
    {
        errorMsg.text = "";

        UISpinner spinner = buttonObject.GetComponentInChildren<UISpinner>(true);

        _attemptingSignUp = true;
        spinner?.gameObject.SetActive(true);

        MMOManager.Instance.UserSignUp<UserAuthResponse>(usernameInput.text, emailInput.text, passwordInput.text,
            (response) =>
            {
                _attemptingSignUp = false;
                spinner?.gameObject.SetActive(false);

                ProcessUserAuth(UpdateErrorText, response);
            });
    }

    /// <summary>
    /// Button event handler for the log in button.
    /// Initiates user log in with the server.
    /// </summary>
    /// <param name="buttonObject">The game object the button component is on.</param>
    public void OnButtonEvent_SubmitLogIn(GameObject buttonObject)
    {
        MMOPlayerPrefs.RememberMe = rememberMe.isOn;

        MMOPlayerPrefs.Email = rememberMe.isOn ? logInEmailInput.text : "";
        MMOPlayerPrefs.Password = rememberMe.isOn ? logInPasswordInput.text : "";

        logInErrorMsg.text = "";

        UISpinner spinner = buttonObject.GetComponentInChildren<UISpinner>(true);

        _attemptingLogin = true;
        spinner?.gameObject.SetActive(true);

        LogUserIn(logInEmailInput.text, logInPasswordInput.text, (error) =>
        {
            _attemptingLogin = false;
            spinner?.gameObject.SetActive(false);

            UpdateErrorText(error);
        });
    }

    /// <summary>
    /// Make a request to the serve to log a user in.
    /// </summary>
    /// <param name="email"></param>
    /// <param name="password"></param>
    /// <param name="onError">Callback to execute in the event of an error occurs with the log in attempt.</param>
    public void LogUserIn(string email, string password, Action<string> onError)
    {
        MMOManager.Instance.UserLogIn<UserAuthResponse>(email, password, (response) => { ProcessUserAuth(onError, response); });
    }

    /// <summary>
    /// Update log in and sign up button interactable states.
    /// </summary>
    private void Update()
    {
        signUpBtn.interactable = CanSignUp();

        logInBtn.interactable = CanLogIn();
    }

    /// <summary>
    /// Checks if necessary inputs contain text and an attempt to sign up
    /// is not already in progress
    /// </summary>
    /// <returns></returns>
    private bool CanSignUp()
    {
        return !string.IsNullOrEmpty(usernameInput.text) && 
               !string.IsNullOrEmpty(emailInput.text) && 
               !string.IsNullOrEmpty(passwordInput.text) && 
               _attemptingSignUp == false;
    }

    /// <summary>
    /// Checks if necessary inputs contain text and an attempt to log in
    /// is not already in progress
    /// </summary>
    /// <returns></returns>
    private bool CanLogIn()
    {
        return !string.IsNullOrEmpty(logInEmailInput.text) &&
               !string.IsNullOrEmpty(logInPasswordInput.text) &&
               _attemptingLogin == false;
    }

    private void ResetViews()
    {
        optionsView.SetActive(true);
        loginView.SetActive(false);
        signUpView.SetActive(false);
        backBtn.SetActive(false);

        rememberMe.isOn = MMOPlayerPrefs.RememberMe;

        errorMsg.text = logInErrorMsg.text = "";
    }

    public void UpdateErrorText(string msg)
    {
        logInErrorMsg.text = errorMsg.text = msg;
    }

    /// <summary>
    /// Handler for log in or sign up requests responses
    /// </summary>
    /// <param name="onError">Callback function for when an error should occur that sends the error message.</param>
    /// <param name="response">Response object from the server.</param>
    private void ProcessUserAuth(Action<string> onError, RequestResponse response)
    {
        if (response.error)
        {
            onError?.Invoke(response.output);
        }
        else
        {
            UserAuthResponse userAuthResponse = (UserAuthResponse)response;

            MMOManager.Instance.SetCurrentUser(userAuthResponse);
            lobbyController.ConsumeSeatReservation(userAuthResponse);
        }
    }
}