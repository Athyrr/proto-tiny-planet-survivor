using UnityEngine;

public class EnemyControllerComponent : MonoBehaviour, ITickable
{
    #region Fields

    [SerializeField]
    private EnemyMovementComponent _movement = null;

    [SerializeField]
    private EnemyDamagerComponent _damager;

    //[SerializeField]
    //private EnemyAIComponent _ai;

    //[SerializeField]
    //private EnemyDamageableComponent _damageable;

    [SerializeField]
    protected Transform _target = null;

    private TickManager _tickManager;

    private PlanetComponent _planet = null;

    private Vector3 _targetDirection = Vector3.zero;

    #endregion


    #region Lifecycle

    private void Awake()
    {
        if (_movement == null)
            _movement = GetComponent<EnemyMovementComponent>();
        if (_damager == null)
            _damager = GetComponent<EnemyDamagerComponent>();
        //if (_ai == null)
        //    _ai = GetComponent<EnemyAIComponent>();
        //if (_damageable == null)
        //    _damageable = GetComponent<EnemyDamageableComponent>();
    }

    private void Start()
    {
        TickManager tm = FindFirstObjectByType<TickManager>();
        tm.Register(this);

        var player = FindFirstObjectByType<PlayerControllerComponent>();
        if (_target == null && player != null)
            _target = player.transform;
    }

    private void OnEnable()
    {
        if (_movement == null)
            Debug.LogError($"{nameof(EnemyMovementComponent)} component field is empty.");

        if (_damager == null)
            Debug.LogError($"{nameof(EnemyDamagerComponent)} component field is empty.");
    }

    #endregion


    #region Public API

    public Vector3 Position => transform.position;

    public bool IsActive => isActiveAndEnabled;

    ///<inheritdoc cref="ITickable.Tick(float)"/>
    public void Tick(float deltaTime)
    {
        _targetDirection = _target.position - transform.position;
        _targetDirection.Normalize();

        var avoidanceForce = CalculateAvoidance();
        Vector3 computedDirection = _targetDirection + avoidanceForce;

        _movement.Move(computedDirection, deltaTime);
        _damager.UpdateTimer(deltaTime);
    }

    public void SetTarget(Transform player)
    {
        _target = player;
    }

    public void Setup(PlanetComponent planet, Transform player)
    {
        _planet = planet;
        _target = player;
    }

    #endregion


    #region Private API 

    private Vector3 CalculateAvoidance()
    {
        //@todo if low priority return

        Collider[] nearbyObjects = Physics.OverlapSphere(transform.position, _movement.MovementData.AvoidanceDetectionRadius, _movement.MovementData.AvoidanceLayer);

        Vector3 avoidanceForce = new();

        foreach (var obj in nearbyObjects)
        {
            if (obj.transform == transform)
                continue;

            Vector3 objToEnemy = transform.position - obj.transform.position;
            float distance = objToEnemy.magnitude;

            if (distance < _movement.MovementData.AvoidanceMaxDistance)
            {
                float forceField = 1 - (distance / _movement.MovementData.AvoidanceMaxDistance);
                avoidanceForce += objToEnemy * forceField;
            }
        }

        return _planet.ProjectOnSurface(avoidanceForce, transform.position);
    }

    #endregion

}
