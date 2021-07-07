using System.Collections;
using System.Collections.Generic;
using Colyseus;
using Colyseus.Schema;
using LucidSightTools;
using UnityEngine;

public class ChatManager : MonoBehaviour
{
    public float messageShowTime;

    private static ChatManager instance;

    public static ChatManager Instance
    {
        get
        {
            if (instance == null)
            {
                LSLog.LogError("No ChatManager in scene!");
            }
            return instance;
        }
    }

    private ColyseusRoom<ChatRoomState> chatRoom;

    void Awake()
    {
        instance = this;
    }

    /// <summary>
    /// Hand the manager the current ChatRoom 
    /// </summary>
    /// <param name="room"></param>
    public void SetRoom(ColyseusRoom<ChatRoomState> room)
    {
        chatRoom = room;
        RegisterForMessages();
        ConnectIDs();
    }

    private void RegisterForMessages()
    {
        if (chatRoom != null)
        {
            chatRoom.OnStateChange += ChatRoomOnOnStateChange;
        }
    }

    private void UnregisterForMessages()
    {
        if (chatRoom != null)
        {
            chatRoom.OnStateChange -= ChatRoomOnOnStateChange;
        }
    }

    //Chat room ID and MMO Room ID will be different, need to connect those values
    private void ConnectIDs()
    {
        NetworkedEntity entity = NetworkedEntityFactory.Instance.GetMine();
        if (entity && chatRoom != null)
        {
            entity.SetChatID(chatRoom.SessionId);
        }
    }

    private void ChatRoomOnOnStateChange(ChatRoomState state, bool isfirststate)
    {
        //We have at least 1 message
        if (state.chatQueue.Count > 0)
        {
            HandleMessages(state.chatQueue);
        }
    }

    private void HandleMessages(MapSchema<ChatQueue> chatQueue)
    {
        chatQueue.ForEach((clientID, queue) => { NetworkedEntityFactory.Instance.HandMessages(clientID, queue); });
    }

    public void SendChat(string message)
    {
        chatRoom?.Send("sendChat", new ChatMessage()
        {
            message =  message
        });
    }

    void OnApplicationQuit()
    {
        LeaveChatroom();
    }

    public void LeaveChatroom()
    {
        UnregisterForMessages();
        chatRoom?.Leave(true);
    }
}
