using UnityEngine;

public class SpriteFlipComponent : MonoBehaviour
{
    #region Fields

    [SerializeField]
    [Range(0f, 1f)]
    [Tooltip("The minimum horizontal movement required to trigger a flip.")]
    private float _flipThreshold = 0.1f;

    private BaseMovementComponent _movement = null;

    private bool _flipped = true;

    #endregion


    #region Lifecycle

    private void Awake()
    {
        if (!this.TryGetComponentInParents<BaseMovementComponent>(out _movement))
            Debug.LogError($"{typeof(BaseMovementComponent)} component not found on {gameObject.name}.", this);
    }

    private void OnEnable()
    {
        if (_movement != null)
            _movement.OnMoveUpdate += HandleMovement;
    }

    private void OnDisable()
    {
        if (_movement != null)
            _movement.OnMoveUpdate -= HandleMovement;
    }

    #endregion


    #region Private API 

    public void Flip()
    {
        var scale = transform.localScale;
        scale.x = -scale.x;
        transform.localScale = scale;
        _flipped = !_flipped;
    }

    private void HandleMovement(BaseMovementComponent entity, Vector3 direction, float speed)
    {
        if (entity == null || Mathf.Abs(direction.x) < _flipThreshold)
            return;

        if (direction.x > 0f && _flipped)
            Flip();
        else if (direction.x < 0f && !_flipped)
            Flip();
    }

    #endregion
}