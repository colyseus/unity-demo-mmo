using System.Collections;
using System.Collections.Generic;
using LucidSightTools;
using UnityEngine;

public class EnvironmentController : MonoBehaviour
{
    private static EnvironmentController instance;

    public static EnvironmentController Instance
    {
        get
        {
            if (instance == null)
            {
                LSLog.LogError("No EnvironmentController in scene!");
            }

            return instance;
        }
    }

    /// <summary>
    /// Is a grid area load in progress?
    /// </summary>
    public bool LoadingArea { get; private set; } = false;

    /// <summary>
    /// The currently loaded grid area
    /// </summary>
    public Area CurrentArea { get; private set; } = null;

    /// <summary>
    /// The current player object of this client
    /// </summary>
    public GameObject playerObject { get; private set; } = null;

    /// <summary>
    /// Current grid coordinates
    /// </summary>
    public Vector2 CurrentGrid { get; private set; } = Vector2.zero;
    /// <summary>
    /// Previous grid coordinates
    /// </summary>
    public Vector2 PreviousGrid { get; private set; } = Vector2.zero;

    void Awake()
    {
        instance = this;
    }

    void OnDestroy()
    {
        instance = null;
    }

    /// <summary>
    /// Begins process of loading the grid area for the newGridPos.
    /// If a current grid exists it will be unloaded.
    /// </summary>
    /// <param name="prevPos">The previous grid coordinates to transition from.</param>
    /// <param name="newGridPos">The grid coordinates to transition to.</param>
    /// <param name="immediatelyAllowExit">Should exiting be immediately allowed once the grid area is loaded?</param>
    public void TransitionArea(Vector2 prevPos, Vector2 newGridPos, bool immediatelyAllowExit)
    {
        StartCoroutine(LoadAreaRoutine(prevPos, newGridPos, immediatelyAllowExit));
    }

    IEnumerator LoadAreaRoutine(Vector2 prevPos, Vector2 pos, bool immediatelyAllowExit)
    {
        LoadingArea = true;

        CurrentGrid = pos;
        PreviousGrid = prevPos;

        ResourceRequest req = Resources.LoadAsync(string.Format("GridPositions/{0}x{1}", pos.x, pos.y));
        while (!req.isDone)
        {
            yield return new WaitForEndOfFrame();
        }

        if (req.asset != null)
        {
            AreaExit currentPlayerExit = null;
            Vector3? playerPos = null;
            
            Vector3 areaPos = Vector3.zero;
            if (CurrentArea != null)
            {
                currentPlayerExit = CurrentArea.currentPlayerExit;
                if (currentPlayerExit)
                {
                    Vector3 playersLocalPosInExit = currentPlayerExit.GetExitPlayerLocalPosition();

                    // Mirror the player's position in the previous exit
                    playersLocalPosInExit.x *= -1;
                    playersLocalPosInExit.z *= -1;
                    playerPos = playersLocalPosInExit;


                }

                Vector2 areaDiff = pos - prevPos;
                areaPos = CurrentArea.transform.localPosition + new Vector3(CurrentArea.areaDimensions.x * areaDiff.x, 0.0f, CurrentArea.areaDimensions.y * areaDiff.y);

                // Move the player object out from the grid
                playerObject.transform.SetParent(null);

                Destroy(CurrentArea.gameObject);
            }

            GameObject area = Instantiate(req.asset, transform, false) as GameObject;
            CurrentArea = area.GetComponent<Area>();
            CurrentArea.ToggleExit(immediatelyAllowExit);
            area.transform.localPosition = areaPos;

            if (playerPos != null)
            {
                PlacePlayer(PreviousGrid, CurrentGrid, (Vector3)playerPos, true);
            }
        }
        else
        {
            LSLog.LogError(string.Format("No Grid Prefab found at GridPositions/{0}x{1}", pos.x, pos.y));
        }

        UIManager.Instance.UpdateGrid(string.Format("{0}x{1}", pos.x, pos.y));

        LoadingArea = false;
    }

    /// <summary>
    /// Let the <see cref="EnvironmentController"/> know about the Player's gameobject for future reference and place them in their appropriate position
    /// </summary>
    /// <param name="playerObject"></param>
    /// <param name="playerState"></param>
    public void SetPlayerObject(GameObject playerObject, NetworkedEntityState playerState)
    {
        this.playerObject = playerObject;

        PlacePlayer(PreviousGrid, CurrentGrid, new Vector3(playerState.xPos, playerState.yPos, playerState.zPos), false);
    }

    private void PlacePlayer(Vector2 prevPos, Vector2 newGridPos, Vector3 playerPosition, bool forGridTransition)
    {
        playerObject.SetActive(false);
        Vector2 entranceDiff = prevPos - newGridPos;
        AreaExit exit = CurrentArea.GetExitByChange(entranceDiff);

        if (forGridTransition && exit != null)
        {
            // Assumes that "playerPosition" is local relative to the transition room
            Vector3 worldPos = exit.PlayerExitLocalPositionToWorldPosition(playerPosition);

            playerObject.transform.position = worldPos;

            // Snap the camera to the other side of the transition room
            NetworkedEntityFactory.Instance.cameraController.EnteredExit(exit.transitionRoom.exitCameraTarget, true);

            exit.transitionRoom.RunDecontamination(true, null);
        }
        else
        {// Assumes that "playerPosition" is the local position in the grid area
            playerObject.transform.localPosition = playerPosition;
        }

        playerObject.transform.SetParent(CurrentArea.transform);

        playerObject.SetActive(true);
    }

    public void ObjectUsed(InteractableState state, NetworkedEntity usingEntity)
    {
        if (CurrentArea != null)
        {
            Interactable interactable = CurrentArea.GetInteractableByState(state);
            if (interactable != null)
            {
                interactable.OnSuccessfulUse(usingEntity);
            }
        }
    }
}
