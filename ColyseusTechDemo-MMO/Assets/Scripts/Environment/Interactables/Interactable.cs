using System;
using System.Collections;
using System.Collections.Generic;
using Colyseus.Schema;
using LucidSightTools;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.PlayerLoop;

/// <summary>
/// The base representation of a server-interactable object.
/// An object placed in a grid position that can be interacted with by players and are linked to a schema state on the server side
/// </summary>
public class Interactable : MonoBehaviour
{
    /// <summary>
    /// This must be unique from all other interactables in the grid space, as this is how the interactable will know which schema it is linked to
    /// </summary>
    [SerializeField]
    protected string _itemID;

    /// <summary>
    /// When alerting the server of this interactable, some values will be initialized based on the serverType, namely cost and use durations
    /// </summary>
    [SerializeField]
    protected string serverType = "DEFAULT";

    /// <summary>
    /// Displayed when a player enters one of this object's <see cref="InteractableTrigger"/> colliders
    /// </summary>
    [SerializeField]
    protected string instructions;

    /// <summary>
    /// The display of the above instructions
    /// </summary>
    [SerializeField]
    protected TextMeshPro interactionInstructions;

    /// <summary>
    /// For enabling/disabling the instructions
    /// </summary>
    [SerializeField]
    protected GameObject instructionRoot;

    [SerializeField]
    private UnityEvent OnSuccessfulUseEvent;

    /// <summary>
    /// User's will press this key to initiate an interaction
    /// </summary>
    public KeyCode interactionKey = KeyCode.X;

    /// <summary>
    /// Public ID getter
    /// </summary>
    public string ID
    {
        get { return _itemID; }
    }

    /// <summary>
    /// Flag to tell local entities whether or not they can attempt to use this object
    /// </summary>
    protected bool isInUse;

    /// <summary>
    /// Array of <see cref="InteractableTrigger"/> colliders that a user can enter to initialize an interaction with this object
    /// </summary>
    [SerializeField]
    protected InteractableTrigger[] triggers;

    /// <summary>
    /// The schema state provided from the server
    /// </summary>
    protected InteractableState _state;

    public InteractableState State
    {
        get
        {
            return _state;
        }
    }

    protected virtual void Awake()
    {
        //Loop through the triggers and tell them who their owner is
        foreach (InteractableTrigger t in triggers)
        {
            t.owner = this;
        }

        //Set the instruction text to what was provided
        if (interactionInstructions)
        {
            interactionInstructions.text = instructions;
        }

        //Hide instructions by default
        instructionRoot.SetActive(false);
    }

    /// <summary>
    /// Set <see cref="isInUse"/> when the <see cref="InteractableState"/> changes
    /// </summary>
    /// <param name="inUse"></param>
    public virtual void SetInUse(bool inUse)
    {
        //Sanity check to make sure an object isn't double-used
        if (isInUse == inUse)
        {
            LSLog.LogError(string.Format("Tried to set Interactable {0}'s isInUse to {1} when it already was!", ID, isInUse));
        }

        isInUse = inUse;

        //Don't allow interaction when in use
        for (int i = 0; i < triggers.Length; ++i)
        {
            triggers[i].enabled = !isInUse;
        }
    }

    /// <summary>
    /// Override-able getter to determine if an Interactable is currently in use or not
    /// </summary>
    /// <returns></returns>
    public virtual bool InUse()
    {
        return isInUse;
    }

    /// <summary>
    /// Fired off by an <see cref="InteractableTrigger"/>. Alerts the interactable that it has a <see cref="NetworkedEntity"/> within range. Also tells the entity that it is within range of an interactable
    /// </summary>
    /// <param name="entity"></param>
    public virtual void PlayerInRange(NetworkedEntity entity)
    {
        if (InUse())
            return;

        entity.EntityNearInteractable(this);
        DisplayInRangeMessage();
    }

    /// <summary>
    /// Fired off by an <see cref="InteractableTrigger"/>. Alerts the interactable that a <see cref="NetworkedEntity"/> has exited it's range. Also tells the entity that it is no longer within range of an interactable
    /// </summary>
    /// <param name="entity"></param>
    public virtual void PlayerLeftRange(NetworkedEntity entity)
    {
        entity.EntityNearInteractable(null);
        HideInRangeMessage();
    }

    /// <summary>
    /// Sent by a <see cref="NetworkedEntity"/> when they press the <see cref="interactionKey"/> while within range
    /// </summary>
    /// <param name="entity"></param>
    public virtual void PlayerAttemptedUse(NetworkedEntity entity)
    {
        if (isInUse)
            return;

        //Hide the interaction message
        HideInRangeMessage();
        
        //Tell the server that this entity is attempting to use this interactable
        MMOManager.Instance.SendObjectInteraction(this, entity);
    }

    /// <summary>
    /// Sent by the <see cref="EnvironmentController"/> after the server sends a <see cref="ObjectUseMessage"/>
    /// </summary>
    /// <param name="entity"></param>
    public virtual void OnSuccessfulUse(NetworkedEntity entity)
    {
        OnSuccessfulUseEvent?.Invoke();
    }

    /// <summary>
    /// Overrideable in case we want an interactable to do more than just show instructions when a player enters range
    /// </summary>
    protected virtual void DisplayInRangeMessage()
    {
        if (instructionRoot)
        {
            instructionRoot.SetActive(true);
        }
    }

    /// <summary>
    /// Overrideable in case we want an interactable to do more than just hide instructions when a player exits range
    /// </summary>
    protected virtual void HideInRangeMessage()
    {
        if (instructionRoot)
        {
            instructionRoot.SetActive(false);
        }
    }

    /// <summary>
    /// Hand off the <see cref="InteractableState"/> from the server
    /// </summary>
    /// <param name="state"></param>
    public void SetState(InteractableState state)
    {
        _state = state;
        _state.OnChange += OnStateChange;
        UpdateForState();
    }

    /// <summary>
    /// Clean-up delegates
    /// </summary>
    protected virtual void OnDestroy()
    {
        if (_state != null) //This object has been given a state
        {
            _state.OnChange -= OnStateChange;
        }
    }

    /// <summary>
    /// Event handler for state changes
    /// </summary>
    /// <param name="changes"></param>
    protected virtual void OnStateChange(List<DataChange> changes)
    {
        UpdateForState();
    }

    /// <summary>
    /// Arranges the object based off of it's current state
    /// </summary>
    protected virtual void UpdateForState()
    {
        //The current in use status is not what the State indicates
        if (isInUse != State.inUse)
        {
            if (isInUse && !State.inUse)
            {
                //Was previously in use but not anymore!
                OnInteractableReset();
            }
            //Set the interactable's inUse status
            SetInUse(State.inUse);
        }
    }

    /// <summary>
    /// Triggered When an interactable was previously in use but is no longer
    /// </summary>
    protected virtual void OnInteractableReset()
    {

    }

    /// <summary>
    /// Get the server type to initialize the server provided values
    /// </summary>
    /// <returns></returns>
    public string GetServerType()
    {
        //Has not been overriden, return default!
        return string.IsNullOrEmpty(serverType) ? "DEFAULT" : serverType;
    }
}
