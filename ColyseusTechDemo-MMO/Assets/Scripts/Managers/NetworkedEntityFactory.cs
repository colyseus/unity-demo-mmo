using System.Collections;
using System.Collections.Generic;
using System.Security;
using Colyseus.Schema;
using LucidSightTools;
using UnityEngine;

public class NetworkedEntityFactory : MonoBehaviour
{
    private static NetworkedEntityFactory instance;

    public static NetworkedEntityFactory Instance
    {
        get
        {
            if (instance == null)
            {
                LSLog.LogError("No NetworkedEntityFactory in scene!");
            }
            return instance;
        }
    }

    [SerializeField]
    private GameObject entityPrefab = null;

    [SerializeField]
    private Dictionary<string, NetworkedEntity> entities = new Dictionary<string, NetworkedEntity>();

    public CameraController cameraController;

    private string _ourEntityId;

    void Awake()
    {
        instance = this;
    }

    public void SetCameraTarget(Transform target)
    {
        cameraController.SetFollow(target);
    }

    /// <summary>
    /// Instantiates a new player object setting its position and rotation as it is in the state. 
    /// </summary>
    /// <param name="state">The state for the player entity.</param>
    /// <param name="isPlayer">Will this entity belong to this client?</param>
    public void SpawnEntity(NetworkedEntityState state, bool isPlayer = false)
    {
        Vector3 position = new Vector3((float)state.xPos, (float)state.yPos, (float)state.zPos);
        Quaternion rot =  new Quaternion((float)state.xRot, (float)state.yRot, (float)state.zRot, 1.0f);

        // Spawn the entity while also making it a child object of the grid area
        GameObject newEntity = Instantiate(entityPrefab, position, rot);
        newEntity.transform.SetParent(EnvironmentController.Instance.CurrentArea.transform);
        NetworkedEntity entity = newEntity.GetComponent<NetworkedEntity>();
        entity.Initialize(state, isPlayer);
        entities.Add(state.id, entity);

        if (isPlayer)
        {
            _ourEntityId = state.id;

            SetCameraTarget(newEntity.transform);
            EnvironmentController.Instance.SetPlayerObject(newEntity, state);
            newEntity.GetComponent<EntityMovement>().cameraTransform = cameraController.transform;
        }
    }

    /// <summary>
    /// Updates this client's entity with the new state.
    /// </summary>
    /// <param name="state">The state to update this client's entity with.</param>
    /// <returns></returns>
    public bool UpdateOurEntity(NetworkedEntityState state)
    {
        if (entities.ContainsKey(_ourEntityId))
        {
            NetworkedEntity entity = entities[_ourEntityId];
            entities.Remove(_ourEntityId);

            entity.Initialize(state, true);

            _ourEntityId = state.id;

            entities.Add(_ourEntityId, entity);

            return true;
        }

        LSLog.LogError($"Missing our entity? - \"{_ourEntityId}\"");

        return false;
    }

    /// <summary>
    /// Removes the entity, keyed by session Id, from the controlled entities and
    /// destroys the player game object.
    /// </summary>
    /// <param name="id"></param>
    public void RemoveEntity(string id)
    {
        if (entities.ContainsKey(id))
        {
            NetworkedEntity entity = entities[id];
            entities.Remove(id);
            Destroy(entity.gameObject);
        }
    }

    public void HandMessages(string id, ChatQueue queue)
    {
        foreach (KeyValuePair<string, NetworkedEntity> entry in entities)
        {
            if (entry.Value.ChatID.Equals(id))
            {
                entry.Value.HandMessages(queue);
            }
        }
    }

    /// <summary>
    /// Returns the <see cref="NetworkedEntity"/> belonging to this client.
    /// </summary>
    /// <returns></returns>
    public NetworkedEntity GetMine()
    {
        foreach (KeyValuePair<string, NetworkedEntity> entry in entities)
        {
            if (entry.Value.isMine)
            {
                return entry.Value;
            }
        }

        LSLog.LogError("No entity found for user!");
        return null;
    }

    /// <summary>
    /// Returns the <see cref="NetworkedEntity"/> belonging to the given session Id if one exists.
    /// </summary>
    /// <param name="sessionId">The session Id of the desired <see cref="NetworkedEntity"/></param>
    /// <returns></returns>
    public NetworkedEntity GetEntityByID(string sessionId)
    {
        if (entities.ContainsKey(sessionId))
        {
            return entities[sessionId];
        }

        return null;
    }

    /// <summary>
    /// Clears the collection of controlled <see cref="NetworkedEntity"/>s and destroys all the linked player game objects.
    /// </summary>
    /// <param name="excludeOurs">If true the <see cref="NetworkedEntity"/> and player game object belonging to this client will not be removed and destroyed.</param>
    public void RemoveAllEntities(bool excludeOurs)
    {
        List<string> keys = new List<string>(entities.Keys);

        for (int i = keys.Count - 1; i >= 0; i--)
        {
            if (entities[keys[i]].isMine && excludeOurs)
            {
                continue;
            }

            Destroy(entities[keys[i]].gameObject);

            entities.Remove(keys[i]);
        }
    }
}
