using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ChatMessageView : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI messageContents;

    private ChatMessage currentMessage = null;

    public bool IsMessage(ChatMessage proposedMessage)
    {
        if (currentMessage != null)
        {
            return proposedMessage.timestamp.Equals(currentMessage.timestamp);
        }
        else
        {
            return false;
        }
    }

    public void SetMessage(ChatMessage message)
    {
        currentMessage = message;
        messageContents.text = currentMessage.message;
        gameObject.SetActive(true);
        StartCoroutine(TriggerDirty());
    }

    IEnumerator TriggerDirty()
    {
        messageContents.gameObject.SetActive(false);
        yield return new WaitForEndOfFrame();
        messageContents.gameObject.SetActive(true);
    }

    public void UnsetMessage()
    {
        currentMessage = null;
        gameObject.SetActive(false);
    }
}
