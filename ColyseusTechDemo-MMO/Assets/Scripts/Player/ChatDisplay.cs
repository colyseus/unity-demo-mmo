using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UI;

public class ChatDisplay : MonoBehaviour
{
    [SerializeField]
    private GameObject chatViewPrefab = null;

    [SerializeField]
    private List<ChatMessage> messages = new List<ChatMessage>();

    [SerializeField]
    private Transform listRoot;

    [SerializeField]
    private FaceCamera cameraFace;

    public int maxDisplay = 1;

    private List<ChatMessageView> spawnedViews = new List<ChatMessageView>();
    private List<ChatMessageView> viewsOnDisplay = new List<ChatMessageView>();

    void Awake()
    {
        cameraFace.enabled = false;
    }

    public void HandMessages(ChatQueue queue)
    {
        List<ChatMessage> compareList = new List<ChatMessage>();
        queue.chatMessages.ForEach((message) =>
        {
            compareList.Add(message);
        });

        compareList.Sort(MessageSort);
        if (!messages.Equals(compareList))
        {
            messages = compareList;
            HandleNewMessages();
            cameraFace.enabled = messages.Count > 0;
        }
    }

    private void HandleNewMessages()
    {
        List<ChatMessageView> viewsStillInUse = new List<ChatMessageView>();
        int count = Math.Min(maxDisplay, messages.Count);
        for (int i = 0; i <count; ++i)
        {
            ChatMessageView view = null;
            if (MessageDisplaying(messages[i], out view))
            {
                viewsStillInUse.Add(view);
            }
            else
            {
                viewsOnDisplay.Add(view);
                viewsStillInUse.Add(view);
            }

            view.transform.SetSiblingIndex(i);
        }

        List<ChatMessageView> viewsToRemove = new List<ChatMessageView>();
        foreach (ChatMessageView oldView in viewsOnDisplay)
        {
            if (!viewsStillInUse.Contains(oldView)) //We aren't still using this view, discard it
            {
                oldView.UnsetMessage();
                viewsToRemove.Add(oldView);
            }
        }

        foreach (ChatMessageView oldView in viewsToRemove)
        {
            viewsOnDisplay.Remove(oldView);
        }
    }

    private bool MessageDisplaying(ChatMessage message, out ChatMessageView viewInUse)
    {
        for(int i = 0; i < viewsOnDisplay.Count; ++i)
        {
            if (viewsOnDisplay[i].IsMessage(message))
            {
                viewInUse = viewsOnDisplay[i];
                return true;
            }
        }

        viewInUse = SpawnOrRecycleView();
        viewInUse.SetMessage(message);
        return false;
    }

    private ChatMessageView SpawnOrRecycleView()
    {
        ChatMessageView view = null;
        for (int i = 0; i < spawnedViews.Count; ++i)
        {
            if (!spawnedViews[i].gameObject.activeSelf)
            {
                view = spawnedViews[i];
            }
        }

        if (view == null)
        {
            GameObject newObject = Instantiate(chatViewPrefab, listRoot, false);
            view = newObject.GetComponent<ChatMessageView>();
            spawnedViews.Add(view);
        }
        return view;
    }

    private int MessageSort(ChatMessage x, ChatMessage y)
    {
        return x.timestamp.CompareTo(y.timestamp);
    }
}
