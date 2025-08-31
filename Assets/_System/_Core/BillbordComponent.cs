using UnityEngine;

public class BillboardComponent : MonoBehaviour
{
    [SerializeField]
    [Tooltip("The camera whose rotation will be used.")]
    private Camera _camera;

    [SerializeField]
    [Tooltip("If enabled, applies the exact rotation of the camera forward instead of its inverse.")]
    private bool _invertForward = false;

    [SerializeField]
    [Tooltip("If enabled, applies the rotation based on the Camera upwards instead of World up vector.")]
    private bool _useCameraUpwards = false;

    [SerializeField]
    [Tooltip("If enabled, rotates only once at start and then destroys this component.")]
    private bool _destroyAfterInit = false;

    private void Awake()
    {
        if (_camera == null)
            _camera = Camera.main;
    }

    private void LateUpdate()
    {
        if (_camera == null)
            return;

        FaceCamera();

        if (_destroyAfterInit)
            Destroy(this);
    }

    /// <summary>
    /// Rotates this object to face the camera.
    /// </summary>
    private void FaceCamera()
    {
        if (_camera == null)
            return;

        Vector3 forward = _invertForward
            ? _camera.transform.forward
            : -_camera.transform.forward;

        Vector3 upwards = _useCameraUpwards ? _camera.transform.up : Vector3.up;

        transform.rotation = Quaternion.LookRotation(forward, upwards);
    }


    private void OnValidate()
    {
        FaceCamera();
    }
}
