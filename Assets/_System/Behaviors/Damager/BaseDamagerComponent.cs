using UnityEngine;

public abstract class BaseDamagerComponent : MonoBehaviour
{
    #region Fields

    [SerializeField]
    private DamagerData _data = null;

    protected float _damage = 0f;
    protected float _cooldown = 0f;
    protected bool _canAttack = true;

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

        return true;

    }

    #endregion


    #region Public API

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

    #endregion
}
