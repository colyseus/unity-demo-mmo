/// <summary>
/// Base server response object.
/// </summary>
[System.Serializable]
public class RequestResponse
{
    public string rawResponse;
    /// <summary>
    /// Did the request result with an error?
    /// </summary>
    public bool error;
    /// <summary>
    /// Server response data. Will be an error message when <see cref="error"/> is true.
    /// </summary>
    public string output = "Some error occurred :(";
}

/// <summary>
/// Server response object when creating a new account or signing into an existing account.
/// </summary>
[System.Serializable]
public class UserAuthResponse : RequestResponse
{
    /// <summary>
    /// Response data that contains user data as well room seat reservation data.
    /// </summary>
    public new UserAuthData output;
}