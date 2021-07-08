using System.Collections;
using System.Collections.Generic;
using LucidSightTools;
using UnityEngine;

public class CameraController : MonoBehaviour
{
    [SerializeField]
    private Transform target = null;

    [SerializeField]
    private Transform cameraTransform = null;

    [SerializeField]
    private float followSpeed = 1.0f;

    [SerializeField]
    private float zoomSpeed = 1.0f;

    [SerializeField]
    private Vector2 minZoom;

    [SerializeField]
    private Vector2 maxZoom;

    [SerializeField]
    private float zoomMultiplier = 1.0f;

    [SerializeField]
    private float rotateSpeed = 1.0f;

    private float currentZoom = 0.0f;
    private Vector3 desiredZoom;
    private float cameraRot = 0.0f;

    private Quaternion targetRotation;

    private bool _inExit = false;

    void Awake()
    {
        desiredZoom = new Vector3(0, minZoom.x, minZoom.y);
        targetRotation = Quaternion.Euler(0, cameraRot, 0);
    }

    void LateUpdate()
    {
        if (target)
        {
            transform.position = Vector3.Lerp(transform.position, target.position, Time.deltaTime * followSpeed);
            transform.rotation = Quaternion.Lerp(transform.rotation, targetRotation, Time.deltaTime * rotateSpeed * 10);
        }

        if (cameraTransform)
        {
            cameraTransform.localPosition = Vector3.Lerp(cameraTransform.localPosition, desiredZoom, Time.deltaTime * zoomSpeed);
        }
    }

    void Update()
    {
        if (UIManager.Instance.MovementPrevented() || _inExit)
            return;

        HandleCameraInput();
    }

    private void HandleCameraInput()
    {
        if (Input.mouseScrollDelta.y != 0.0f)
        {
            currentZoom -= Input.mouseScrollDelta.y * zoomMultiplier;
            currentZoom = Mathf.Clamp01(currentZoom);
            desiredZoom = Vector3.Lerp(new Vector3(0, minZoom.x, minZoom.y), new Vector3(0, maxZoom.x, maxZoom.y), currentZoom);
        }

        if (Input.GetMouseButton(1))
        {
            cameraRot += rotateSpeed * Input.GetAxis("Mouse X");
            targetRotation = Quaternion.Euler(0, cameraRot, 0);
        }
    }

    public void SetFollow(Transform trans)
    {
        target = trans;
    }

    public void EnteredExit(Transform trans, bool snap)
    {
        _inExit = true;

        SetFollow(trans);

        desiredZoom = new Vector3(0, 0, 0);

        targetRotation = trans.rotation;

        if (snap)
        {
            transform.position = trans.position;
            transform.rotation = trans.rotation;
            cameraTransform.localPosition = desiredZoom;
        }
    }

    public void LeftExit()
    {
        desiredZoom = new Vector3(0, minZoom.x, minZoom.y);

        SetFollow(EnvironmentController.Instance.playerObject.transform);

        targetRotation = EnvironmentController.Instance.playerObject.transform.rotation;

        _inExit = false;
    }
}
