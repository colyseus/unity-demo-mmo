using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Colyseus;

[System.Serializable]
public class EntityMessage
{
    public string entityID;
}

public class ObjectUseMessage
{
    public string interactedObjectID;
    public string interactingStateID;
}

[System.Serializable]
public class MovedToGridMessage
{
    public Vector2 newGridPosition;
    public Vector2 prevGridPosition;
    public SeatReservationData seatReservation;
}