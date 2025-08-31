using UnityEngine;

public abstract class BaseMovementComponent : MonoBehaviour
{
    #region Fields

    public delegate void MoveDelegate(BaseMovementComponent entity, Vector3 direction, float speed);

    public MoveDelegate OnMoveStart = null;
    public MoveDelegate OnMoveUpdate = null;
    public MoveDelegate OnMoveEnd = null;

    [SerializeField]
    protected MovementData _data = null;

    protected PlanetComponent _planet;

    protected Vector3 _velocity = Vector3.zero;

    protected float _speed = 0f;
    protected float _maxSpeed = 0f;

    #endregion


    #region Lifecycle

    private void Awake()
    {
        Init();
    }

    protected virtual bool Init()
    {
        _planet = FindFirstObjectByType<PlanetComponent>();
        return true;
    }

    #endregion


    #region Public API

    public float Speed => _speed;

    public float MaxSpeed => _maxSpeed;

    public MovementData MovementData => _data;

    public abstract bool Move(Vector3 direction, float delta);

    #endregion


    #region Protected API

    /// <summary>
    /// Align entity up vector with the planet normal.
    /// </summary>
    protected void AlignWithPlanet()
    {
        Vector3 up = _planet.GetNormalAtPosition(transform.position);
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, up) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 20f);
    }

    /// <summary>
    /// Apply movement to the planet surface based on the current velocity.
    /// </summary>
    /// <param name="delta"></param>
    /// <param name="finalPosition"></param>
    protected void ApplyMovementToPlanet(float delta, out Vector3 finalPosition)
    {
        Vector3 velocity = _planet.ProjectOnSurface(_velocity, transform.position);
        Vector3 targetPosition = transform.position + velocity * delta;

        finalPosition = _planet.GetSnappedPosition(targetPosition);
        transform.position = finalPosition;
    }

    private void OnValidate()
    {
        if (_planet == null)
            _planet = FindFirstObjectByType<PlanetComponent>();

        if (_planet != null && !Application.isPlaying)
        {
                        transform.position = _planet.GetSnappedPosition(transform.position);
            AlignWithPlanet();

#if UNITY_EDITOR
            UnityEditor.EditorUtility.SetDirty(this);
#endif
        }
    }

    #endregion
}
