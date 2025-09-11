using UnityEngine;

public abstract class BaseDamagerComponent : MonoBehaviour, IUpgradableBehavior
{
    #region Fields

    [SerializeField]
    private DamagerData _data = null;

    protected float _damage = 0f;
    protected float _cooldown = 0f;
    protected bool _canAttack = true;

    protected float _range;

    protected float _timer = 0f;

    #endregion


    #region Lifecycle

    private void Awake()
    {
        Init();
    }

    protected virtual bool Init()
    {
        if (_data == null)
            return false;

        _canAttack = true;
        _damage = _data.Damage;
        _cooldown = _data.Cooldown;
        _timer = 0f;
        _range = _data.AttackRadius;

        return true;

    }

    #endregion


    #region Public API

    public float Range => _range;

    public bool CanApplyDamage => _canAttack;

    public DamagerData Data => _data;

    public abstract bool Attack(float damage);

    public void UpdateTimer(float delta)
    {
        _timer += delta;
        if (_timer >= _cooldown)
        {
            _timer = _cooldown;

            if (!_canAttack)
                return;


            Attack(_damage);
            _timer = 0f;
        }
    }

    public abstract bool CanAttack(float damage);

    public void SetRange(float range)
    {
        if (_range != range)
            _range = range;
    }

    #endregion
}
