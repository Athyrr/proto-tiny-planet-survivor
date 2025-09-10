using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerControllerComponent : MonoBehaviour
{
    [SerializeField]
    private bool _useWorldSpace = true;

    [SerializeField]
    private Camera _camera;

    [SerializeField]
    private PlayerMovementComponent _movement = null;

    [SerializeField]
    private PlayerDamagerComponent _damager = null;

    [SerializeField]
    private LayerMask _pointTargetLayer = ~0;

    private GameInputs _gameInputs = null;

    private Vector2 _aimDirection = Vector2.zero;
    private Vector3 _worldAimPoint = Vector2.zero;

    private Vector3 _inputDirection = Vector3.zero;

    private PlanetComponent _planet = null;

    private void Awake()
    {
        if (_movement == null)
            Debug.LogError($"{typeof(PlayerMovementComponent)} component field is empty.");

        _planet = FindFirstObjectByType<PlanetComponent>();
    }

    private void OnEnable()
    {
        if (_gameInputs == null)
        {
            _gameInputs = new GameInputs();

            // Subscribe to the input events
            _gameInputs.Game.Move.performed += HandleMoveInput;
            _gameInputs.Game.Move.canceled += HandleMoveInput;

            _gameInputs.Game.Point.performed += HandlePointInput;
        }

        _gameInputs.Enable();
    }

    private void OnDisable()
    {
        if (_gameInputs == null)
            return;

        _gameInputs.Disable();
    }

    private void Update()
    {
        UpdateMovement(_inputDirection, Time.deltaTime);
        _damager.UpdateTimer(Time.deltaTime);
    }

    private void HandleMoveInput(InputAction.CallbackContext context)
    {
        _inputDirection = Vector2.ClampMagnitude(context.ReadValue<Vector2>(), 1f);
    }

    private void HandlePointInput(InputAction.CallbackContext context)
    {
        Ray cameraRay = _camera.ScreenPointToRay(context.ReadValue<Vector2>());

        if (Physics.Raycast(cameraRay, out RaycastHit hitInfo, Mathf.Infinity, _pointTargetLayer))
        {
            _worldAimPoint = hitInfo.point;

            //Vector3 aimDirection = GetTangentialDirection(transform.position, hitInfo.point);
            Vector3 aimDirection = _planet.ProjectOnSurface(transform.position, hitInfo.point).normalized;

            if (aimDirection.sqrMagnitude > 0.01f)
                _aimDirection = new Vector2(aimDirection.x, aimDirection.z);
        }
    }

    
    //private Vector3 GetTangentialDirection(Vector3 from, Vector3 to)
    //{
    //    Vector3 planetNormal = _planet.GetNormalAtPosition(from);
    //    Vector3 direction = (to - from);
    //    return Vector3.ProjectOnPlane(direction, planetNormal).normalized;
    //}

    private void UpdateMovement(Vector2 input, float delta)
    {
        if (_movement == null)
            return;

        Vector3 movementDirection;

        if (_useWorldSpace)
        {
            // World relative movement
            movementDirection = new Vector3(input.x, 0, input.y);

            // Project movement direction onto the planet surface
            Vector3 planetNormal = _planet.GetNormalAtPosition(transform.position);
            movementDirection = Vector3.ProjectOnPlane(movementDirection, planetNormal);
        }
        else
        {
            // Camera relative movement
            Vector3 right = _camera.transform.right;
            Vector3 forward = _camera.transform.forward;

            // Project camera vectors onto the planet surface
            Vector3 planetNormal = _planet.GetNormalAtPosition(transform.position);
            right = Vector3.ProjectOnPlane(right, planetNormal);
            forward = Vector3.ProjectOnPlane(forward, planetNormal);

            movementDirection = right * input.x + forward * input.y;

        }

        movementDirection.Normalize();
        _movement.Move(movementDirection, delta);
    }
}
