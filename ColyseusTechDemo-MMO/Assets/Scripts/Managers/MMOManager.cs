using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using Colyseus;
using LucidSightTools;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;

/*
 * NOTE:
 * This demo makes use of a very basic user authentication system with the intent of having
 * player persistence for unique user accounts and should NOT be used as a real
 * world example of how to implement user authentication as a whole.
 * Do NOT use any email and password combination you actually use anywhere else.
 */

public class MMOManager : ColyseusManager<MMOManager>
{
    public delegate void OnRoomChanged(ColyseusRoom<RoomState> room);
    public static event OnRoomChanged onRoomChanged;

    public static bool IsReady
    {
        get
        {
            return Instance != null;
        }
    }

    public UserData CurrentUser { get; private set; }

    /// <summary>
    ///     The current or active Room we get when joining or creating a room.
    /// </summary>
    public ColyseusRoom<RoomState> Room
    {
        get
        {
            return _room;
        }

        private set
        {
            _room = value;
        }
    }

    private ColyseusRoom<RoomState> _room;

    private bool isQuitting = false;
    //The chat room
    private ColyseusRoom<ChatRoomState> _chatRoom;

    public bool autoConnect = false;

    public RoomState currentRoomState;

    public float ServerTime
    {
        get
        {
            if (currentRoomState != null)
            {
                return currentRoomState.serverTime;
            }
            
            LSLog.Log("Asked for server time but no room yet!", LSLog.LogColor.yellow);
            return 0;
        }
    }
    protected override void Awake()
    {
        base.Awake();
    }

    protected override void Start()
    {
        base.Start();

        // In this demo auto connect was only being use when launching directly out of the "TowerScene" for quicker iteration while developing
#if UNITY_EDITOR
        if (autoConnect)
        {
            InitializeClient();
            QuickSignIn();
        }
#endif
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        UnregisterHandlers();
    } 
    /// <summary>
    /// Registers handlers for room state events as well as room messages
    /// </summary>
    private void RegisterHandlers()
    {
        if (Room != null)
        {
            Room.OnLeave += OnLeaveGridRoom;

            Room.OnStateChange += OnRoomStateChange;
            Room.State.networkedUsers.OnAdd += NetworkedUsers_OnAdd;
            Room.State.networkedUsers.OnRemove += NetworkedUsers_OnRemove;

            Room.OnMessage<ObjectUseMessage>("objectUsed", (msg) =>
            {
                StartCoroutine(AwaitObjectInteraction(msg.interactedObjectID, msg.interactingStateID));
            });
            Room.OnMessage<MovedToGridMessage>("movedToGrid", OnMovedToGrid);
        }
        else
        {
            LSLog.LogError($"Cannot register room handlers, room is null!");
        }
    }

    private void UnregisterHandlers()
    {
        if (Room != null)
        {
            Room.OnLeave -= OnLeaveGridRoom;

            Room.OnStateChange -= OnRoomStateChange;

            Room.State.networkedUsers.OnAdd -= NetworkedUsers_OnAdd;
            Room.State.networkedUsers.OnRemove -= NetworkedUsers_OnRemove;

        }
    }

    /// <summary>
    /// Message handler for when the user has begun moving to another grid room
    /// </summary>
    /// <param name="msg"></param>
    private void OnMovedToGrid(MovedToGridMessage msg)
    {
        StartCoroutine(Co_OnMovedToGrid(msg));
    }

    private IEnumerator Co_OnMovedToGrid(MovedToGridMessage msg)
    {
        // Unregister handlers to current room
        UnregisterHandlers();

        CleanUpRoom();

        // Leave current room
        Room.Leave(true);

        //Leave the current chat room
        ChatManager.Instance.LeaveChatroom();

        // Transition environment to new grid
        EnvironmentController.Instance.TransitionArea(msg.prevGridPosition, msg.newGridPosition, false);

        while (EnvironmentController.Instance.LoadingArea)
        {
            yield return null;
        }

        // Join room for the new grid
        ConsumeSeatReservation(msg.seatReservation.room, msg.seatReservation.sessionId);
    }

    /// <summary>
    /// Removes all entities from the scene except the entity for this client
    /// </summary>
    private void CleanUpRoom()
    {
        NetworkedEntityFactory.Instance.RemoveAllEntities(true);
    }

    private void OnLeaveGridRoom(NativeWebSocket.WebSocketCloseCode code)
    {
        // We have left the current grid room
    }

    /// <summary>
    /// Callback for when a networked entity has been removed from the room state's collection of networked entities/users
    /// </summary>
    /// <param name="key">The sessionId of the networked entity that got removed</param>
    /// <param name="value">The <see cref="NetworkedEntityState"/> of the user that was removed</param>
    private void NetworkedUsers_OnRemove(string key, NetworkedEntityState value)
    {
        NetworkedEntityFactory.Instance.RemoveEntity(value.id);
    }

    /// <summary>
    /// Callback for when a networked entity has been added to the room state's collection of networked entities/users
    /// </summary>
    /// <param name="key">The sessionId of the networked entity that got added</param>
    /// <param name="value">The <see cref="NetworkedEntityState"/> of the user that was added</param>
    private void NetworkedUsers_OnAdd(string key, NetworkedEntityState value)
    {
        // TODO: subscribe to NetworkedEntityState OnChange event for updating entities
        StartCoroutine(WaitThenSpawnPlayer(value.id));

        //if (value.id.Equals(_room.SessionId))
        //{
        //    JoinChatRoom();
        //}
    }

    /// <summary>
    /// Event handler when the room receives its first state
    /// </summary>
    /// <param name="state"></param>
    /// <param name="isfirststate"></param>
    private void OnRoomStateChange(RoomState state, bool isfirststate)
    {
        if (isfirststate)
        {
            //LSLog.LogImportant($"On Room State Changed - First State!", LSLog.LogColor.yellow);
        }
    }

#if UNITY_EDITOR
    /// <summary>
    /// To aid in development with quicker test iterations, it will automatically sign you in
    /// provided you have gone through normal log in procedure and have selected the "remember me" feature
    /// </summary>
    private void QuickSignIn()
    {
        string email = MMOPlayerPrefs.Email;
        string password = MMOPlayerPrefs.Password;

        if (string.IsNullOrEmpty(email) || string.IsNullOrEmpty(password))
        {
            LSLog.LogError($"Quick Sign In Failed - Missing email and/or password!");
            return;
        }

        UserLogIn<UserAuthResponse>(email, password, (response) =>
        {
            UserAuthResponse userAuthResponse = (UserAuthResponse)response;

            SetCurrentUser(userAuthResponse);

            StartCoroutine(LoadGridAndConsumeSeatReservation(userAuthResponse));
        });
    }
#endif

    private async void JoinChatRoom()
    {
        ColyseusRoom<ChatRoomState> chatRoom = await client.JoinOrCreate<ChatRoomState>("chat_room", new Dictionary<string, object>() { { "roomID", Room.Id }, {"messageLifetime", ChatManager.Instance.messageShowTime} });
        ChatManager.Instance.SetRoom(chatRoom);
    }

    private IEnumerator WaitThenSpawnPlayer(string entityID)
    {
        while (!Room.State.networkedUsers.ContainsKey(entityID))
        {
            //Wait until the room has a state for this ID (may take a frame or two, prevent race conditions)
            yield return new WaitForEndOfFrame();
        }

        bool isOurs = entityID.Equals(Room.SessionId);
        NetworkedEntityState entityState = Room.State.networkedUsers[entityID];

        if (isOurs == false || (isOurs && !EnvironmentController.Instance.playerObject))
        {
            NetworkedEntityFactory.Instance.SpawnEntity(entityState, isOurs);
        }
        else
        {// Update our existing entity

            if (NetworkedEntityFactory.Instance.UpdateOurEntity(entityState) == false)
            {// Spawn a new entity for us since something went wrong attempting to update our existing one
                NetworkedEntityFactory.Instance.SpawnEntity(entityState, true);
            }
        }
    }

    private IEnumerator AwaitObjectInteraction(string objectID, string entityID)
    {
        while (!Room.State.interactableItems.ContainsKey(objectID))
        {
            //Wait for the room to be aware of the object
            yield return new WaitForEndOfFrame();
        }

        NetworkedEntity entity = NetworkedEntityFactory.Instance.GetEntityByID(entityID);
        EnvironmentController.Instance.ObjectUsed(Room.State.interactableItems[objectID], entity);
    }

    /// <summary>
    ///     Send an action and message object to the room.
    /// </summary>
    /// <param name="action">The action to take</param>
    /// <param name="message">The message object to pass along to the room</param>
    public static void NetSend(string action, object message = null)
    {
        if (Instance.Room == null)
        {
            LSLog.LogError($"Error: Not in room for action {action} msg {message}");
            return;
        }

        _ = message == null ? Instance.Room.Send(action) : Instance.Room.Send(action, message);
    }

    /// <summary>
    /// Sends a message to the room on the server that an <see cref="Interactable"/> has been used
    /// </summary>
    /// <param name="interactable">The <see cref="Interactable"/> used.</param>
    /// <param name="entity">The entity that has used the <see cref="Interactable"/>.</param>
    public void SendObjectInteraction(Interactable interactable, NetworkedEntity entity)
    {
        //LSLog.Log("Sending object interaction for ID " + interactable.ID);
        NetSend("objectInteracted", new object[] {interactable.ID, interactable.GetServerType()});
    }

    protected override void OnApplicationQuit()
    {
        base.OnApplicationQuit();
        Room?.Leave(true);
    }

    /// <summary>
    /// Sends the provided user info to the server to attempt the creation of a new account.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="RequestResponse"/> we expect to receive from the server.</typeparam>
    /// <param name="username">Username of the new account</param>
    /// <param name="email">Email of the new account</param>
    /// <param name="password">Password of the new account</param>  
    /// <param name="onComplete">Callback to execute when the request has completed</param>
    public void UserSignUp<T>(string username, string email, string password, Action<RequestResponse> onComplete) where T : RequestResponse
    {
        WWWForm form = new WWWForm();

        form.AddField("username", username);
        form.AddField("email", email);
        form.AddField("password", password);
        StartCoroutine(Co_ServerRequest<T>("POST", "users/signup", form, onComplete));
    }

    /// <summary>
    /// Send the provided user info to the server to attempt user login.
    /// </summary>
    /// <typeparam name="T">The type of <see cref="RequestResponse"/> we expect to receive from the server.</typeparam>
    /// <param name="email">Email of the user account</param>
    /// <param name="password">Password of the user account</param>
    /// <param name="onComplete">Callback to execute when the request has completed</param>
    public void UserLogIn<T>(string email, string password, Action<RequestResponse> onComplete) where T : RequestResponse
    {
        WWWForm form = new WWWForm();

        form.AddField("email", email);
        form.AddField("password", password);

        StartCoroutine(Co_ServerRequest<T>("POST", "users/login", form, onComplete));
    }

    /// <summary>
    /// Set the current user to the one held in the <see cref="UserAuthResponse"/>
    /// </summary>
    /// <param name="userAuthResponse"></param>
    public void SetCurrentUser(UserAuthResponse userAuthResponse)
    {
        CurrentUser = userAuthResponse.output.user;
    }

    /// <summary>
    /// Coroutine for asynchronously sending requests to the server
    /// </summary>
    /// <typeparam name="T">The type of <see cref="RequestResponse"/> we expect to receive from the server.</typeparam>
    /// <param name="method">The method to use: POST, GET, etc.</param>
    /// <param name="url">The sub url for the route we are sending the request to</param>
    /// <param name="form">Form data</param>sd
    /// <param name="onComplete">Callback to execute when the request has completed</param>
    /// <returns></returns>
    private IEnumerator Co_ServerRequest<T>(string method, string url, WWWForm form, Action<RequestResponse> onComplete) where T : RequestResponse
    {
        string fullURL = $"{_colyseusSettings.WebRequestEndpoint}/{url}";

        UnityWebRequest request = null;

        switch (method)
        {
            case "POST":
                request = UnityWebRequest.Post(fullURL, form);
                break;
            //case "GET":
            //    break;
            default:
                LSLog.LogImportant($"Unsupported Server Request Type - {method}", LSLog.LogColor.yellow);

                onComplete?.Invoke(new RequestResponse() { error = true, output = $"Unsupported Server Request Type - {method}" });

                yield break;
        }

        if (request == null)
        {
            onComplete?.Invoke(new RequestResponse() { error = true, output = $"Error making web request!" });
            yield break;
        }

        UnityWebRequestAsyncOperation op = request.SendWebRequest();

        while (op.isDone == false)
        {
            yield return 0;
        }

        RequestResponse response = null;

        try
        {
            response = string.IsNullOrEmpty(request.error)
                ? JsonUtility.FromJson<T>(request.downloadHandler.text)
                : JsonUtility.FromJson<RequestResponse>(request.downloadHandler.text);

            response.rawResponse = request.downloadHandler.text;

        }
        catch (System.Exception err)
        {
            response = new RequestResponse() { error = true, output = $"{err.Message}" };
        }

        onComplete?.Invoke(response);
    }

    /// <summary>
    /// Consumes a seat reservation in order to join a room.
    /// </summary>
    /// <typeparam name="T">The room schema</typeparam>
    /// <param name="room">The room to consume the seat reservation for</param>
    /// <param name="sessionId">The session Id of the seat reservation</param>
    /// <returns></returns>
    public async void ConsumeSeatReservation(ColyseusRoomAvailable room, string sessionId)
    {
        try
        {
            ColyseusMatchMakeResponse response = new ColyseusMatchMakeResponse() { room = room, sessionId = sessionId };

            Room = await client.ConsumeSeatReservation<RoomState>(response);

            onRoomChanged?.Invoke(Room);

            currentRoomState = Room.State;
            JoinChatRoom();
            RegisterHandlers();
        }
        catch (System.Exception error)
        {
            LSLog.LogError($"Error attempting to consume seat reservation - {error.Message + error.StackTrace}");
        }
    }

    /// <summary>
    /// Loads the grid area for the player's progress and then consumes a seat reservation
    /// in order to join the room.
    /// </summary>
    /// <param name="userAuthResponse">The response expected from the server after sign up or log in that contains user and seat reservation data.</param>
    /// <returns></returns>
    public IEnumerator LoadGridAndConsumeSeatReservation(UserAuthResponse userAuthResponse)
    {
        Vector2 playerPrevGrid = CurrentUser.GridAsVector2(false);
        Vector2 playerProgress = CurrentUser.GridAsVector2();

        // Load the grid the player is currently in
        EnvironmentController.Instance.TransitionArea(playerPrevGrid, playerProgress, true);

        // Wait for grid load to complete
        while (EnvironmentController.Instance.LoadingArea)
        {
            yield return null;
        }

        // Finally join the room by consuming the seat reservation
        ConsumeSeatReservation(userAuthResponse.output.seatReservation.room, userAuthResponse.output.seatReservation.sessionId);
    }

    public void ExitToMainMenu()
    {
        if (isQuitting)
            return;

        isQuitting = true;
        Room.Leave(true);
        ChatManager.Instance.LeaveChatroom();
        StartCoroutine(LoadMainSceneAsync(() =>
        {
            isQuitting = false;
        }));
    }

    private IEnumerator LoadMainSceneAsync(Action onComplete)
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(0, LoadSceneMode.Single);
        while (op.progress <= 0.9f)
        {
            //Wait until the scene is loaded
            yield return new WaitForEndOfFrame();
        }

        op.allowSceneActivation = true;
        
        onComplete?.Invoke();
    }
}
