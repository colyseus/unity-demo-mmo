using TMPro;
using UnityEngine;

public class PlayerTag : MonoBehaviour
{
    [SerializeField]
    private TextMeshProUGUI playerTag;

    [SerializeField]
    private CanvasGroup canvasGroup;

    [SerializeField]
    private RectTransform rectTransform;

    public void SetPlayerTag(string tag)
    {
        playerTag.text = tag;
    }

    public void UpdateTag(Vector2 position, float alpha)
    {
        rectTransform.anchoredPosition = position;
        canvasGroup.alpha = alpha;
    }
}
