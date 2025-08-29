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

    public void Tick(float deltaTime)
    {
        _targetDirection = _target.position - transform.position;
        _targetDirection.Normalize();

        _movement.Move(_targetDirection, deltaTime);

        _damager.UpdateTimer(deltaTime);
    }

    public void SetTarget(Transform player)
    {
        _target = player;
    }

    #endregion
}
