using UnityEngine;

public class EnemyDamageableComponent : BaseDamageableComponent
{
    public override bool Heal(float amount)
    {
        return true;
    }

    public override bool TakeDamage(BaseDamagerComponent source, float amount)
    {
        if (_isInvicible)
            return false;

        if (amount <= 0)
            return false;

        _health -= amount;

        //Debug.Log($"Enemy took {amount} damage from {source.name}. Health: {_health}");

        //OnHealthChange.Invoke(_health + amount, _data.MaxHealth, this);

        if (_health <= 0)
        {
            Kill();
        }
        return true;
    }

    private bool Kill()
    {
        var tm = FindAnyObjectByType<TickManager>();
        var tickable = GetComponent<ITickable>();

        if (tm == null || tickable == null)
            Destroy(this.gameObject);

        tm.Unregister(tickable);
        Destroy(this.gameObject);

        Debug.Log("Enemy destroyed");
        return true;
    }

}