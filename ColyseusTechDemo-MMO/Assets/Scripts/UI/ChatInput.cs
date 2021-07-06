using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatInput : MonoBehaviour
{
    [SerializeField]
    private TMP_InputField chatInput;

    [SerializeField]
    private Button sendButton;

    public void ToggleChat()
    {
        gameObject.SetActive(!gameObject.activeSelf);
        if (gameObject.activeSelf)
        {
            chatInput.Select();
        }
        else
        {
            ClearText();
        }
    }

    public void InputFieldChanged()
    {
        sendButton.interactable = chatInput.text.Length > 0;
    }

    public void OnSend()
    {
        if (chatInput.text.Length > 0)
        {
            ChatManager.Instance.SendChat(chatInput.text);
            ClearText();
            ToggleChat();   //Maybe dont want to close by default
        }
    }

    private void ClearText()
    {
        chatInput.text = "";
    }

    public bool HasFocus()
    {
        return chatInput.isFocused;
    }
}
