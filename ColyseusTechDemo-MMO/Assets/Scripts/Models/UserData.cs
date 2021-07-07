
using LucidSightTools;
using UnityEngine;

/// <summary>
/// Data model to represent the client-side user
/// </summary>
[System.Serializable]
public class UserData
{
    /// <summary>
    /// Id of the user
    /// </summary>
    public string id;
    /// <summary>
    /// Username from account creation
    /// </summary>
    public string username;
    /// <summary>
    /// Email from account creation
    /// </summary>
    public string email;

    public string progress = "0,0";
    public string prevGrid = "0,0";
    public Vector3 position;

    public Vector2 GridAsVector2(bool getCurrentGrid = true)
    {
        string[] coords = getCurrentGrid ? progress.Split(',') : prevGrid.Split(',');

        if (coords != null && coords.Length > 1)
        {
            if (float.TryParse(coords[0], out float xVal) == false)
            {
                LSLog.LogError($"Error parsing x value for grid from {coords[0]}");
            }

            if (float.TryParse(coords[1], out float yVal) == false)
            {
                LSLog.LogError($"Error parsing y value for grid from {coords[1]}");
            }

            return new Vector2(xVal, yVal);
        }

        return Vector2.zero;
    }
}
