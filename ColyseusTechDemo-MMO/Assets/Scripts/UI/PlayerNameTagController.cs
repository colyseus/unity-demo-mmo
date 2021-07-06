using System.Collections;
using System.Collections.Generic;
using Colyseus;
using UnityEngine;

/// <summary>
/// Responsible for adding/removing player name tags.
/// Subscribes to the OnAdd and OnRemoved events of
/// the "networkedUsers" collection of the room state.
/// </summary>
public class PlayerNameTagController : MonoBehaviour
{
    [SerializeField]
    private RectTransform playerTagRoot = null;

    [SerializeField]
    private PlayerTag nameTagPrefab = null;

    [SerializeField]
    private Camera cam;

    private ColyseusRoom<RoomState> _room;
    private Dictionary<string, PlayerTag> _nameTags = new Dictionary<string, PlayerTag>();

    private void Start()
    {
        MMOManager.onRoomChanged += OnRoomChanged;
    }

    private void OnDestroy()
    {
        MMOManager.onRoomChanged -= OnRoomChanged;
    }

    private void RegisterHandlers()
    {
        _room.State.networkedUsers.OnAdd += PlayerAdded;
        _room.State.networkedUsers.OnRemove += PlayerRemoved;
    }

    private void UnRegisterHandlers()
    {
        _room.State.networkedUsers.OnAdd -= PlayerAdded;
        _room.State.networkedUsers.OnRemove -= PlayerRemoved;
    }

    /// <summary>
    /// Event handler for when the user has joined or moved to a different room
    /// </summary>
    /// <param name="room">The room state of the new room</param>
    private void OnRoomChanged(ColyseusRoom<RoomState> room)
    {
        if (_room != null)
        {
            UnRegisterHandlers();
        }

        RemoveAllTags();

        _room = room;

        RegisterHandlers();
    }

    /// <summary>
    /// Event handler for when a user is added to the room's collection of "networkedUsers"
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    private void PlayerAdded(string key, NetworkedEntityState value)
    {
        if (IsEntityMine(key))
        {
            return;
        }

        if (_nameTags.ContainsKey(key) == false)
        {
            PlayerTag nameTag = Instantiate(nameTagPrefab, new Vector3(-500, -500, 0), Quaternion.identity, playerTagRoot);

            nameTag.gameObject.name = $"Player Tag - {value.username}";

            nameTag.SetPlayerTag(value.username);

            _nameTags.Add(key, nameTag);
        }
    }

    /// <summary>
    /// Event handler for when a user is removed from the room's collection of "networkedUsers"
    /// </summary>
    /// <param name="key"></param>
    /// <param name="value"></param>
    private void PlayerRemoved(string key, NetworkedEntityState value)
    {
        if (IsEntityMine(key))
        {
            return;
        }

        if (_nameTags.ContainsKey(key))
        {
            PlayerTag nameTag = _nameTags[key];

            _nameTags.Remove(key);

            Destroy(nameTag.gameObject);
        }
    }

    private void LateUpdate()
    {
        UpdatePlayerTags();
    }

    /// <summary>
    /// Positions each name tag with its corresponding player entity
    /// </summary>
    private void UpdatePlayerTags()
    {
        foreach (KeyValuePair<string, PlayerTag> pair in _nameTags)
        {
            NetworkedEntity entity = NetworkedEntityFactory.Instance.GetEntityByID(pair.Key);

            if (entity && RectTransformUtility.ScreenPointToLocalPointInRectangle(playerTagRoot, RectTransformUtility.WorldToScreenPoint(cam, entity.transform.position), null, out Vector2 pos))
            {
                pair.Value.UpdateTag(pos, (cam.ScreenToViewportPoint(cam.WorldToViewportPoint(entity.transform.position)).z > 0) ? 1 : 0);
            }
        }
    }

    /// <summary>
    /// Checks if the given session Id matches the client's session Id
    /// </summary>
    /// <param name="sessionId"></param>
    /// <returns></returns>
    private bool IsEntityMine(string sessionId)
    {
        return string.Equals(sessionId, MMOManager.Instance.Room.SessionId);
    }

    /// <summary>
    /// Destroys all existing name tag objects and clears the collection
    /// </summary>
    private void RemoveAllTags()
    {
        foreach (KeyValuePair<string, PlayerTag> pair in _nameTags)
        {
            Destroy(pair.Value.gameObject);
        }
        
        _nameTags.Clear();
    }
}
