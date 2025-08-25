using UnityEngine;

public class EnemyControllerComponent : MonoBehaviour, ITickable
{
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

    public bool IsActive => isActiveAndEnabled;

    private void Start()
    {
        TickManager tm = FindFirstObjectByType<TickManager>();
        tm.Register(this);
    }

    private void OnEnable()
    {
        if (_movement == null)
            Debug.LogError($"{nameof(EnemyMovementComponent)} component field is empty.");

        if (_damager == null)
            Debug.LogError($"{nameof(EnemyDamagerComponent)} component field is empty.");
    }

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
}
