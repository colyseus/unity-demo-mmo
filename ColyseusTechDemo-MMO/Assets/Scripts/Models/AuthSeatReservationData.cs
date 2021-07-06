using System.Collections;
using System.Collections.Generic;
using Colyseus;
using UnityEngine;

[System.Serializable]
public class SeatReservationData
{
    /// <summary>
    /// The room belonging to the seat reservation
    /// </summary>
    public ColyseusRoomAvailable room;
    /// <summary>
    /// The session Id of the seat reservation
    /// </summary>
    public string sessionId;
}
