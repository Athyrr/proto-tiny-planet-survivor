using System.Runtime.CompilerServices;
using UnityEngine;

public abstract class BaseDamageableComponent : MonoBehaviour
{
    public delegate void HealthChangeDelegate(float previousHealth, float previousMaxHealth, BaseDamageableComponent damageable);
    public HealthChangeDelegate OnHealthChange;

    [SerializeField]
    protected DamageableData _data = null;

    protected bool _isInvicible = false;

    protected float _health = 0f;

    protected float _maxHealth = 0f;

    public DamageableData DamageableData => _data;
    public bool IsInvicible => _isInvicible;

    private void Awake()
    {
        if (_data == null)
        {
            Debug.LogError($"{nameof(DamageableData)} is not assigned in {nameof(BaseDamageableComponent)}.");
            return;
        }
        _maxHealth = _data.MaxHealth;
        _health = _maxHealth;
        _isInvicible = false;
    }


    public abstract bool TakeDamage(BaseDamagerComponent source, float amount);
    public abstract bool Heal(float amount);

    public bool SetInvicible(bool invicible)
    {
        if (_isInvicible == invicible)
            return false;
        _isInvicible = invicible;
        return true;
    }

    public bool SetMaxHealth(float maxHealth)
    {
        _health = maxHealth;
        return true;
    }

    public bool AddMaxHealth(float amount)
    {
        if (amount <= 0)
            return false;
        _maxHealth += amount;
        _health += amount;

        OnHealthChange?.Invoke(_health - amount, _data.MaxHealth - amount, this);
        return true;
    }
}
