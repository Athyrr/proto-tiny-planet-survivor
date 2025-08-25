using UnityEngine;

public abstract class BaseMovementComponent : MonoBehaviour
{
    #region Fields

    public delegate void MoveDelegate(GameObject entity, Vector3 direction, float speed);

    public MoveDelegate OnMoveStart = null;
    public MoveDelegate OnMoveUpdate = null;
    public MoveDelegate OnMoveEnd = null;

    [SerializeField]
    protected Transform _planetCenter;

    [SerializeField]
    protected MovementData _data = null;

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
        return true;
    }

    #endregion


    #region Public API

    public float Speed => _speed;
    public float MaxSpeed => _maxSpeed;

    public MovementData MovementData => _data;

    public abstract bool Move(Vector3 direction, float delta);

    /// <summary>
    /// Set planet center
    /// </summary>
    /// <param name="center"></param>
    public void SetPlanetCenter(Transform center)
    {
        if (center == null || _planetCenter == center)
            return;

        _planetCenter = center;
    }

    #endregion


    #region Protected API

    /// <summary>
    /// Align entity up vector with the planet normal.
    /// </summary>
    protected void AlignWithPlanet()
    {
        Vector3 up = GetPlanetNormal(transform.position);
        Quaternion targetRotation = Quaternion.FromToRotation(transform.up, up) * transform.rotation;
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * 20f);
    }

    /// <summary>
    /// Calculate the normal vector from the planet center to the current position
    /// </summary>
    /// <returns></returns>
    protected Vector3 GetPlanetNormal(Vector3 position)
    {
        if (_planetCenter == null)
            return Vector3.up;

        Vector3 direction = (position - _planetCenter.position);
        return direction.magnitude > 0.001f ? direction.normalized : Vector3.up;
    }

    /// <summary>
    /// Project the given vector onto the planet surface.
    /// </summary>
    /// <param name="vector"></param>
    /// <returns></returns>
    protected Vector3 ProjectOnSphere(Vector3 vector)
    {
        Vector3 planetNormal = GetPlanetNormal(transform.position);
        return Vector3.ProjectOnPlane(vector, planetNormal);
    }

    /// <summary>
    /// Snap player position to the planet surface based on the current position.
    /// </summary>
    /// <param name="targetPosition"></param>
    protected void SnapToPlanetSurface(Vector3 targetPosition)
    {
        if (_planetCenter == null)
            return;

        Vector3 planetnormal = GetPlanetNormal(targetPosition);
        float planetRadius = _planetCenter.localScale.x * 0.5f;
        transform.position = _planetCenter.position + planetnormal * planetRadius;
    }

    /// <summary>
    /// Apply movement to the planet surface based on the current velocity.
    /// </summary>
    /// <param name="delta"></param>
    /// <param name="finalPosition"></param>
    protected void ApplyMovementToPlanet(float delta, out Vector3 finalPosition)
    {
        _velocity = ProjectOnSphere(_velocity);
        Vector3 targetPostion = transform.position + _velocity * delta;
        SnapToPlanetSurface(targetPostion);

        finalPosition = transform.position;
    }

    #endregion
}
