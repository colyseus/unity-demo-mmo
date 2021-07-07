using System;
using System.Collections;
using System.Collections.Generic;
using LucidSightTools;
using UnityEngine;

public class AreaExit : MonoBehaviour
{
    //This exit will change the user's grid position by this amount
    [SerializeField]
    private Vector2 exitGridChange = Vector2.zero;

    public TransitionRoom transitionRoom = null;

    [SerializeField]
    private EntryDoorTrigger entryDoorTrigger = null;

    private Area _owner;
    private float _exitCounter = 0;
    private bool _canExit = false;

    public Vector2 GridChange
    {
        get
        {
            return exitGridChange;
        }
    }
    
    private void Start()
    {
        entryDoorTrigger.PlayerEnteredDoorTrigger.AddListener(OnPlayerEnteredDoorTrigger);
        entryDoorTrigger.PlayerExitedDoorTrigger.AddListener(OnPlayerExitedDoorTrigger);
    }

    public void Initialize(Area owner)
    {
        _owner = owner;
    }

    public void ToggleExit(bool canExit)
    {
        _canExit = canExit;
    }

    public void UseExit()
    {
        Vector3 playerExitWorldPos = EnvironmentController.Instance.playerObject.transform.localPosition;

        playerExitWorldPos.z *= -1;
        playerExitWorldPos.x *= -1;

        _owner.AttemptExit(exitGridChange, playerExitWorldPos);
    }

    /// <summary>
    /// Gets the player's position local to the exit
    /// </summary>
    /// <returns></returns>
    public Vector3 GetExitPlayerLocalPosition()
    {
        return transitionRoom.transform.InverseTransformPoint(EnvironmentController.Instance.playerObject.transform
            .position);
    }

    /// <summary>
    /// Converts the position from the exit's local space to world space
    /// </summary>
    /// <param name="localPosition"></param>
    /// <returns></returns>
    public Vector3 PlayerExitLocalPositionToWorldPosition(Vector3 localPosition)
    {
        return transitionRoom.transform.TransformPoint(localPosition);
    }

    void OnTriggerEnter(Collider other)
    {
        NetworkedEntity entity = other.GetComponent<NetworkedEntity>();
        if (entity != null)
        {
            transitionRoom.AddForceField(entity);

            if (entity.isMine)
            {
                transitionRoom.forceField.gameObject.SetActive(true);
                _owner.currentPlayerExit = this;

                _exitCounter = 0;

                entryDoorTrigger.PlayerExitedDoorTrigger.RemoveListener(OnPlayerExitedDoorTrigger);
            }
            else
            {
                if (entryDoorTrigger.HasEntityPassedThrough(entity) == false)
                {
                    transitionRoom.RunDecontamination(true, null);
                }
                else
                {
                    transitionRoom.OpenDoor(true, null);
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        NetworkedEntity entity = other.GetComponent<NetworkedEntity>();
        if (entity)
        {
            transitionRoom.RemoveForceField(entity.Id);

            if (entity.isMine)
            {
                transitionRoom.StopDecontamination();

                _owner.currentPlayerExit = null;

                Invoke("EnableExitAfterPlayerLeave", 1.0f);

                NetworkedEntityFactory.Instance.cameraController.LeftExit();

                entryDoorTrigger.PlayerExitedDoorTrigger.AddListener(OnPlayerExitedDoorTrigger);
            }
        }
    }

    private void EnableExitAfterPlayerLeave()
    {
        // Will allow all exits
        _owner.ToggleExit(true);
    }

    private void OnTriggerStay(Collider other)
    {
        NetworkedEntity entity = other.GetComponent<NetworkedEntity>();

        if (entity && entity.isMine)
        {
            transitionRoom.SetForceFieldPosition(other.transform.position);

            if (_canExit)
            {
                if (_exitCounter < 3.0f)
                {
                    _exitCounter += Time.deltaTime;

                    if (_exitCounter >= 3.0)
                    {
                        transitionRoom.CloseDoor(true, () =>
                        {
                            NetworkedEntityFactory.Instance.cameraController.EnteredExit(transitionRoom.entryCameraTarget, false);

                            Invoke("UseExit", 1.0f);
                        });
                    }
                }
            }

            
        }
    }

    private void OnPlayerEnteredDoorTrigger(NetworkedEntity entity)
    {
        transitionRoom.OpenDoor(true, null);
    }

    private void OnPlayerExitedDoorTrigger(NetworkedEntity entity)
    {
        transitionRoom.CloseDoor(true, null);
    }
}
