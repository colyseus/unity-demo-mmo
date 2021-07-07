using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EntityMovement : MonoBehaviour
{
    [SerializeField]
    private CharacterController _controller;

    public float moveSpeed = 1.0f;
    public float SprintSpeed
    {
        get
        {
            return moveSpeed * 2f;
        }
    }

    private bool isSprinting = false;
    private bool isForcingRotation = false;
    public float rotationSpeed = 1.0f;

    public Transform cameraTransform;

    //Cached movement
    Vector3 moveVec = Vector3.zero;
    float rotationAngle = 0.0f;

    private Interactable currentInteractable;

    public NetworkedEntity owner;

    void Update()
    {
        if (UIManager.Instance.MovementPrevented())  //Dont accept input while user is typing in the chat box
            return;

        HandleInput();
    }

    void HandleInput()
    {
        isSprinting = Input.GetKey(KeyCode.LeftShift);

        if (Input.GetKey(KeyCode.Q) || Input.GetKey(KeyCode.E))
        {
            isForcingRotation = true;
            rotationAngle = (Input.GetKey(KeyCode.Q) ? -1.0f : 1.0f) * rotationSpeed;
            _controller.transform.Rotate(Vector3.up, rotationAngle / 2.0f);
        }
        else
        {
            isForcingRotation = false;
        }

        if (Input.GetAxis("Horizontal") != 0.0f || Input.GetAxis("Vertical") != 0.0f)
        {
            Vector3 lateralVec = cameraTransform != null ? cameraTransform.right : _controller.transform.right;
            lateralVec.y = 0;
            
            Vector3 forwardVec = cameraTransform != null ? cameraTransform.forward : _controller.transform.forward;
            forwardVec.y = 0;

            if (!isForcingRotation)
            {
                //Attempt to rotate to face forward
                Quaternion desRot = Quaternion.LookRotation(forwardVec, Vector3.up);
                _controller.transform.rotation = Quaternion.Lerp(_controller.transform.rotation, desRot,
                    rotationSpeed * Time.deltaTime);
            }

            moveVec = (forwardVec * Input.GetAxis("Vertical") * Time.deltaTime) + (lateralVec * Input.GetAxis("Horizontal") * Time.deltaTime);
            _controller.Move(moveVec * (isSprinting ? SprintSpeed : moveSpeed));
        }

        if (currentInteractable != null)
        {
            if (Input.GetKeyDown(currentInteractable.interactionKey))
            {
                currentInteractable.PlayerAttemptedUse(owner);
            }
        }
    }

    public void SetCurrentInteractable(Interactable interactable)
    {
        currentInteractable = interactable;
    }
}
