using System.Collections;
using System.Collections.Generic;
using LucidSightTools;
using UnityEngine;

public class Area : MonoBehaviour
{
    [SerializeField]
    private Vector2 gridPosition;

    public Vector2 areaDimensions = new Vector2(100, 100);

    [SerializeField]
    private AreaExit[] exits;

    private Interactable[] interactables;

    public AreaExit currentPlayerExit;

    void Awake()
    {
        for (int i = 0; i < exits.Length; ++i)
        {
            exits[i].Initialize(this);
        }

        interactables = GetComponentsInChildren<Interactable>();
    }

    /// <summary>
    /// Sends a message to the room on the server that a grid transition has been initiated by this client.
    /// </summary>
    /// <param name="gridChange">The grid change delta to be applied to the current grid coordinates.</param>
    /// <param name="transitionPosition">The position the client entity should be set to at the end of the transition.</param>
    public void AttemptExit(Vector2 gridChange, Vector3 transitionPosition)
    {
        MMOManager.NetSend("transitionArea", new object[]
        {
            new Vector2Obj(gridChange.x, gridChange.y),
            new Vector3Obj(transitionPosition.x, transitionPosition.y, transitionPosition.z)
        });
    }

    /// <summary>
    /// Toggle whether grid exits can initiate an area transition
    /// </summary>
    /// <param name="canExit"></param>
    public void ToggleExit(bool canExit)
    {
        for (int i = 0; i < exits.Length; i++)
        {
            exits[i].ToggleExit(canExit);
        }
    }

    /// <summary>
    /// Get the Area's exit based off of the change that it would provide
    /// </summary>
    /// <param name="change"></param>
    /// <returns></returns>
    public AreaExit GetExitByChange(Vector2 change)
    {
        for (int i = 0; i < exits.Length; ++i)
        {
            if (exits[i].GridChange.Equals(change))
            {
                return exits[i];
            }
        }

        return null;
    }

    /// <summary>
    /// Sent by the server to associate an <see cref="InteractableState"/> with it's representation in the grid space
    /// </summary>
    /// <param name="state"></param>
    /// <returns></returns>
    public Interactable GetInteractableByState(InteractableState state)
    {
        foreach (Interactable t in interactables)
        {
            if (!t.ID.Equals(state.id))
            {
                continue;
            }

            //This interactable has the correct ID but it has not yet been given a state, so correct that!
            if (t.State == null)
            {
                t.SetState(state);
            }

            return t;
        }

        LSLog.LogError("Area has no reference to an interactable with ID " + state.id + " but it was requested!");
        return null;
    }
}
