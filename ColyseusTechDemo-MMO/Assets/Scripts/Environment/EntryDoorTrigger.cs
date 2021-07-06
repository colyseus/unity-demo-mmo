using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class EntryDoorTrigger : MonoBehaviour
{
    public ExitEvent<NetworkedEntity> PlayerEnteredDoorTrigger = new ExitEvent<NetworkedEntity>();
    public ExitEvent<NetworkedEntity> PlayerExitedDoorTrigger = new ExitEvent<NetworkedEntity>();

    public bool TriggerOccupied { get; private set; }

    /// <summary>
    /// Collection to keep track of entities that have recently passed through this trigger
    /// </summary>
    private Dictionary<string, Dictionary<bool, DateTime>> _entityEnterLog = new Dictionary<string, Dictionary<bool, DateTime>>();

    private void Update()
    {
        List<string> keys = new List<string>(_entityEnterLog.Keys);

        for (int i = 0; i < keys.Count; i++)
        {
            if (_entityEnterLog[keys[i]].ContainsKey(false))
            {
                // Remove log if it has expired after x seconds since the entity has left the trigger
                if ((_entityEnterLog[keys[i]][false] - _entityEnterLog[keys[i]][true]).TotalSeconds > 0 &&
                    (DateTime.Now - _entityEnterLog[keys[i]][false]).TotalSeconds >= 5)
                {
                    _entityEnterLog.Remove(keys[i]);
                }
            }
        }
    }

    public bool HasEntityPassedThrough(NetworkedEntity entity)
    {
        return _entityEnterLog.ContainsKey(entity.Id);
    }

    private void OnTriggerEnter(Collider other)
    {
        NetworkedEntity entity = other.gameObject.GetComponent<NetworkedEntity>();

        if (entity)
        {
            LogEntity(entity, true);

            PlayerEnteredDoorTrigger?.Invoke(entity);
        }
    }

    private void OnTriggerStay(Collider other)
    {
        NetworkedEntity entity = other.gameObject.GetComponent<NetworkedEntity>();

        if (entity)
        {
            TriggerOccupied = true;
        }
    }

    private void OnTriggerExit(Collider other)
    {
        NetworkedEntity entity = other.gameObject.GetComponent<NetworkedEntity>();

        if (entity)
        {
            LogEntity(entity, false);

            TriggerOccupied = false;

            PlayerExitedDoorTrigger?.Invoke(entity);
        }
    }

    private void LogEntity(NetworkedEntity entity, bool entered)
    {
        if (_entityEnterLog.ContainsKey(entity.Id))
        {
            if (_entityEnterLog[entity.Id].ContainsKey(entered))
            {
                _entityEnterLog[entity.Id][entered] = DateTime.Now;
            }
            else
            {
                _entityEnterLog[entity.Id].Add(entered, DateTime.Now);
            }
        }
        else
        {
            _entityEnterLog.Add(entity.Id, new Dictionary<bool, DateTime>() { { entered, DateTime.Now } });
        }
    }
}

public class ExitEvent<T> : UnityEvent<T>
{

}