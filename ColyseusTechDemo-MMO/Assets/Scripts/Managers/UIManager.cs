using System.Collections;
using System.Collections.Generic;
using LucidSightTools;
using TMPro;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    private static UIManager instance;

    public static UIManager Instance
    {
        get
        {
            if (instance == null)
            {
                LSLog.LogError("No UIManager in scene!");
            }

            return instance;
        }
    }

    [SerializeField]
    private TextMeshProUGUI coinCount = null;

    [SerializeField]
    private TextMeshProUGUI gridLocation = null;

    [SerializeField]
    private InGameMenu inGameMenu = null;

    [SerializeField]
    private ChatInput chatInput = null;

    [SerializeField]
    private GameObject initialControlsDisplay = null;

    [SerializeField]
    private float controlDisplayLength = 3.0f;

    public bool InGameMenuShowing { get { return inGameMenu.gameObject.activeSelf; } }

    void Awake()
    {
        instance = this;
        inGameMenu.gameObject.SetActive(false);
    }

    //Display the controls for a bit on first launch
    IEnumerator Start()
    {
        initialControlsDisplay.SetActive(true);
        
        yield return new WaitForSeconds(controlDisplayLength);

        initialControlsDisplay.SetActive(false);
    }

    void OnDestroy()
    {
        instance = null;
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleInGameMenu();
        }
        if (Input.GetKeyDown(KeyCode.BackQuote) && !InGameMenuShowing)
        {
            chatInput.ToggleChat();
        }
    }

    public void ToggleInGameMenu()
    {
        inGameMenu.gameObject.SetActive(!inGameMenu.gameObject.activeSelf);
    }
    
    public void UpdateGrid(string grid)
    {
        gridLocation.text = grid;
    }

    public void UpdatePlayerInfo(NetworkedEntity entity)
    {
        coinCount.text = string.Format("Coins: {0}", entity.Coins);
    }

    public void ToggleChat()
    {
        chatInput.ToggleChat();
    }

    /// <summary>
    /// Check if anything in the UI should be preventing movement input
    /// </summary>
    /// <returns></returns>
    public bool MovementPrevented()
    {
        return (InGameMenuShowing || chatInput.HasFocus());
    }
}
