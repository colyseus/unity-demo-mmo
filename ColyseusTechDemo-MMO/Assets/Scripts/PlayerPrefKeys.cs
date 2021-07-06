using UnityEngine;

/// <summary>
/// Class to simplify interaction with Unity's PlayerPrefs system for saving/retrieving log in credentials 
/// </summary>
public class MMOPlayerPrefs
{
    static string RememberMeKey = "TD4-MMO-RememberMe";
    static string EmailKey = "TD4-MMO-RememberEmail";
    static string PasswordKey = "TD4-MMO-RememberPassword";

    public static string Email
    {
        get
        {
            return PlayerPrefs.GetString(EmailKey, "");
        }

        set
        {
            PlayerPrefs.SetString(EmailKey, value);
        }
    }

    public static string Password
    {
        get
        {
            return PlayerPrefs.GetString(PasswordKey, "");
        }

        set
        {
            PlayerPrefs.SetString(PasswordKey, value);
        }
    }

    public static bool RememberMe
    {
        get
        {
            return PlayerPrefs.GetInt(RememberMeKey, 0) == 1;
        }

        set
        {
            PlayerPrefs.SetInt(RememberMeKey, value ? 1 : 0);
        }
    }

    public static bool AccountExists
    {
        get
        {
            return string.IsNullOrEmpty(Email) == false && string.IsNullOrEmpty(Password) == false;
        }
    }

    public static void Clear()
    {
        RememberMe = false;
        Email = "";
        Password = "";
    }
}
