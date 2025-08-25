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

    private void Awake()
    {
        if (_movement == null)
            Debug.LogError($"{typeof(PlayerMovementComponent)} component field is empty.");
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
        Ray cameraRay = Camera.main.ScreenPointToRay(context.ReadValue<Vector2>());

        if (Physics.Raycast(cameraRay, out RaycastHit hitInfo, Mathf.Infinity, _pointTargetLayer))
        {
            _worldAimPoint = hitInfo.point;

            Vector3 aimDirection = hitInfo.point - transform.position;
            if (aimDirection != Vector3.zero)
                aimDirection.Normalize();

            _aimDirection = new Vector2(aimDirection.x, aimDirection.y);
        }

        //Debug.Log("Point Input: " + _worldAimPoint);
    }

    private void UpdateMovement(Vector2 input, float delta)
    {
        if (_movement == null)
            return;

        Vector3 movementDirection;

        if (_useWorldSpace)
        {
            movementDirection = new Vector3(input.x, 0, input.y);
        }
        else
        {
            Vector3 right = _camera.transform.right;
            Vector3 forward = _camera.transform.forward;

            movementDirection = right * input.x + forward * input.y;
        }

        movementDirection.Normalize();
        _movement.Move(movementDirection, delta);
    }
}
