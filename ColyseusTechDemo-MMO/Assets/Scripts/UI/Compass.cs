using UnityEngine;

/// <summary>
/// The compass always points to the "North" or Z-forward on the world axis
/// </summary>
public class Compass : MonoBehaviour
{
    [SerializeField]
    private Transform targetCameraRoot = null;

    [SerializeField]
    private Transform compassCameraRoot = null;

    [SerializeField]
    private Transform compassTransform = null;

    [SerializeField]
    private float rotateSpeed = 5.0f;

    private void Start()
    {
        // Ensure the compass transform's forward rotation is pointing in the Z-forward on the world axis
        compassTransform.rotation = Quaternion.identity;
    }

    private void Update()
    {
        // Update the camera root rotation to match the rotation of the target camera
        compassCameraRoot.rotation = Quaternion.Lerp(compassCameraRoot.rotation, targetCameraRoot.rotation, Time.deltaTime * rotateSpeed);
    }
}
