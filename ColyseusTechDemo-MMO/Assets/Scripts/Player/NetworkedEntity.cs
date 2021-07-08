using System.Collections;
using System.Collections.Generic;
using Colyseus.Schema;
using LucidSightTools;
using UnityEngine;

public class NetworkedEntity : MonoBehaviour
{
    //Is this entity view representing the current client
    public bool isMine = false;

    /// <summary>
    /// Getter for the active session Id
    /// </summary>
    public string Id
    {
        get
        {
            return state.id;
        }
    }

    [SerializeField]
    private NetworkedEntityState state;
    private NetworkedEntityState previousState;
    private NetworkedEntityState localUpdatedState;

    [SerializeField]
    private EntityMovement movement = null;

    [SerializeField]
    private ChatDisplay chatDisplay = null;

    [SerializeField]
    private float updateTimer = 0.5f;
    private float currentUpdateTime = 0.0f;


    //Display elements
    [SerializeField]
    private AvatarDisplay avatarModel = null;
    [SerializeField]
    private Animator animator = null;

    private Vector3 prevPos = Vector3.zero;
    float currentSpeed = 0.0f;
    float walkVal = 0.0f;

    //Movement Sync
    [SerializeField]
    private float maxAngleForSnapRotation = 35f;
    public double interpolationBackTimeMs = 200f;
    public double extrapolationLimitMs = 500f;
    public float positionLerpSpeed = 5f;
    public float rotationLerpSpeed = 5f;

    private bool ignoreMovementSync = false;

    private string _chatID = "";

    public string ChatID { get { return _chatID; } }

    public int Coins { get { return state != null ? (int)state.coins : 0; } }

    public AvatarState Avatar { get { return localUpdatedState?.avatar; } }

    /// <summary>
    /// Synchronized object state
    /// </summary>
    [System.Serializable]
    private struct EntityState
    {
        public double timestamp;
        public Vector3 pos;
        public Quaternion rot;
    }

    // Clients store twenty states with "playback" information from the server. This
    // array contains the official state of this object at different times according to
    // the server.
    [SerializeField]
    private EntityState[] proxyStates = new EntityState[20];

    // Keep track of what slots are used
    private int proxyStateCount;

    public void Initialize(NetworkedEntityState initialState, bool isPlayer = false)
    {
        if (state != null)
        {// Unsubscribe from existing state events
            state.OnChange -= OnStateChange;
            state.avatar.OnChange -= AvatarOnOnChange;
        }

        isMine = isPlayer;
        state = initialState;
        previousState = state;
        movement.enabled = isPlayer;
        movement.owner = this;
        state.OnChange += OnStateChange;
        
        state.avatar.OnChange += AvatarOnOnChange;

        prevPos = transform.position;

        if (!isMine)
        {
            _chatID = initialState.chatID;
        }
        else
        {
            UIManager.Instance.UpdatePlayerInfo(this);
        }
    }

    private void AvatarOnOnChange(List<DataChange> changes)
    {
        UpdateAvatar();
    }

    public void UpdateAvatar()
    {
        avatarModel.DisplayFromState(state.avatar);
    }

    void FixedUpdate()
    {
        if (isMine)
        {
            if (currentUpdateTime >= updateTimer)
            {
                currentUpdateTime = 0.0f;
                SyncServerWithView();
                SetAnimationValues();
            }
            else
            {
                currentUpdateTime += Time.deltaTime;
            }
        }
        else
        {
            // You are not the owner, so you have to converge the object's state toward the server's state.
            ProcessViewSync();
            if (currentUpdateTime >= updateTimer)
            {
                currentUpdateTime = 0.0f;
                SetAnimationValues();
            }
            else
            {
                currentUpdateTime += Time.deltaTime;
            }
        }
    }

    private void SetAnimationValues()
    {
        //Update local speed reference
        if (prevPos != transform.localPosition)
        {
            Vector3 dist = transform.localPosition - prevPos;
            currentSpeed = dist.magnitude / Time.deltaTime;
            prevPos = transform.localPosition;
        }
        else
        {
            currentSpeed = 0.0f;
        }

        walkVal = currentSpeed / movement.moveSpeed;

        animator.SetFloat("Walk", Mathf.Clamp(walkVal, 0,1));
    }

    public void SetChatID(string chatID)
    {
        _chatID = chatID;
        SyncServerWithView();   //Let the server know we have a ChatID now
    }

    /// <summary>
    /// Send this entity's position and rotation values to the server to be synced with all other clients.
    /// </summary>
    private void SyncServerWithView()
    {
        previousState = state.Clone();

        //Copy Transform to State (round position to fix floating point issues with state compare)
        state.xPos = (float)System.Math.Round((decimal)transform.localPosition.x, 4);
        state.yPos = (float)System.Math.Round((decimal)transform.localPosition.y, 4);
        state.zPos = (float)System.Math.Round((decimal)transform.localPosition.z, 4);

        state.xRot = transform.rotation.x;
        state.yRot = transform.rotation.y;
        state.zRot = transform.rotation.z;
        state.wRot = transform.rotation.w;

        if (!state.chatID.Equals(_chatID))
        {
            state.chatID = _chatID;
        }

        ////No need to update again if last sent state == current view modified state
        if (localUpdatedState != null)
        {
            //TODO: Uses reflection so might be slow, replace with defined compare to improve speed
            List<NetworkedEntityChanges> changesLocal = NetworkedEntityChanges.Compare(localUpdatedState, state);
            if (changesLocal.Count == 0 || (changesLocal.Count == 1 && changesLocal[0].Name == "timestamp"))
            {
                return;
            }
        }


        //TODO: Uses reflection so might be slow, replace with defined compare to improve speed
        List<NetworkedEntityChanges> changes = NetworkedEntityChanges.Compare(previousState, state);

        //Transform has been update locally, push changes
        if (changes.Count > 0)
        {
            //Create Change Set Array for NetSend
            object[] changeSet = new object[(changes.Count * 2) + 1];
            changeSet[0] = state.id;
            int saveIndex = 1;
            for (int i = 0; i < changes.Count; i++)
            {
                changeSet[saveIndex] = changes[i].Name;
                changeSet[saveIndex + 1] = changes[i].NewValue;
                saveIndex += 2;
            }
            localUpdatedState = state.Clone();
            MMOManager.NetSend("entityUpdate", changeSet);
        }
    }

    private void OnStateChange(List<DataChange> changes)
    {
        //If not mine Sync
        if (!isMine)
        {
            SyncViewWithServer();
        }
        else
        {
            bool userInfoChanged = false;
            //Check for coin change
            changes.ForEach((change) =>
            {
                if (change.Field.Equals("coins"))
                {
                    userInfoChanged = true;
                }
            });

            if (userInfoChanged)
            {
                UIManager.Instance.UpdatePlayerInfo(this);
            }
        }
    }

    /// <summary>
    /// Synchronize this entity with the current position and rotation values from the state
    /// </summary>
    private void SyncViewWithServer()
    {
        // Network player, receive data
        Vector3 pos = new Vector3((float)state.xPos, (float)state.yPos, (float)state.zPos);
        Quaternion rot = new Quaternion((float)state.xRot, (float)state.yRot, (float)state.zRot, (float) state.wRot);

        // Shift the buffer sideways, deleting state 20
        for (int i = proxyStates.Length - 1; i >= 1; i--)
        {
            proxyStates[i] = proxyStates[i - 1];
        }

        // Record current state in slot 0
        EntityState newState = new EntityState() { timestamp = state.timestamp }; //Make sure timestamp is in ms
                                                                                  //newState.timestamp = state.timestamp;

        newState.pos = pos;
        newState.rot = rot;
        proxyStates[0] = newState;


        // Update used slot count, however never exceed the buffer size
        // Slots aren't actually freed so this just makes sure the buffer is
        // filled up and that uninitalized slots aren't used.
        proxyStateCount = Mathf.Min(proxyStateCount + 1, proxyStates.Length);

        // Check if states are in order
        if (proxyStates[0].timestamp < proxyStates[1].timestamp)
        {
#if UNITY_EDITOR
            LSLog.Log("Timestamp inconsistent: " + proxyStates[0].timestamp + " should be greater than " + proxyStates[1].timestamp, LSLog.LogColor.yellow);
#endif
        }

        _chatID = state.chatID;
    }

    /// <summary>
    /// Lerp this entity's position and rotation towards latest 
    /// </summary>
    protected virtual void ProcessViewSync()
    {
        if (ignoreMovementSync)
        {
            //Don't lerp this object right now
            return;
        }

        // This is the target playback time of this body
        double interpolationTime = MMOManager.Instance.ServerTime - interpolationBackTimeMs;

        // Use interpolation if the target playback time is present in the buffer
        if (proxyStates[0].timestamp > interpolationTime)
        {
            // The longer the time since last update add a delta factor to the lerp speed to get there quicker
            float deltaFactor = (MMOManager.Instance.ServerTime > proxyStates[0].timestamp) ?
                (float)(MMOManager.Instance.ServerTime - proxyStates[0].timestamp) * 0.2f : 0f;

            transform.localPosition = Vector3.Distance(transform.localPosition, proxyStates[0].pos) < 5 ? Vector3.Lerp(transform.localPosition, proxyStates[0].pos, Time.deltaTime * (positionLerpSpeed + deltaFactor)) : proxyStates[0].pos;

            if (Mathf.Abs(Quaternion.Angle(transform.rotation, proxyStates[0].rot)) > maxAngleForSnapRotation)
                transform.rotation = proxyStates[0].rot;
            else  
                transform.rotation = Quaternion.Slerp(transform.rotation, proxyStates[0].rot, Time.deltaTime * (rotationLerpSpeed + deltaFactor));
        }
        // Use extrapolation (If we did not get a packet in the last "X" ms and object had velocity)
        else
        {
            EntityState latest = proxyStates[0];

            float extrapolationLength = (float)(interpolationTime - latest.timestamp);
            // Don't extrapolate for more than 500 ms, you would need to do that carefully
            if (extrapolationLength < extrapolationLimitMs / 1000f)
            {
                transform.localPosition = latest.pos;
                transform.localRotation = latest.rot;
            }
        }
    }

    public void HandMessages(ChatQueue queue)
    {
        chatDisplay.HandMessages(queue);
    }

    public void EntityNearInteractable(Interactable interactable)
    {
        movement.SetCurrentInteractable(interactable);
    }

    public void SetMovementEnabled(bool val)
    {
        if(isMine)
            movement.enabled = val;
    }

    public void SetIgnoreMovementSync(bool ignore)
    {
        ignoreMovementSync = ignore;
    }
}
