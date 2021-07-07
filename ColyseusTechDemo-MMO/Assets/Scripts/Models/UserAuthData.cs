using Colyseus;

/// <summary>
/// Server response data when a user has created a new account or logged in
/// </summary>
[System.Serializable]
public class UserAuthData
{
    public SeatReservationData seatReservation;
    
    /// <summary>
    /// User account data
    /// </summary>
    public UserData user;
}
