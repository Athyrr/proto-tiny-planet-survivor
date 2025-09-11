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
        //OnHealthChange.Invoke(_health + amount, _data.MaxHealth, this);

        if (_health <= 0)
        {
            Kill();
        }
        return true;
    }

    private bool Kill()
    {
        // @todo un register TM + gem release on nme controller instead

        var tm = FindAnyObjectByType<TickManager>();
        var tickable = GetComponent<ITickable>();

        OnDeath?.Invoke(this);

        Instantiate(DamageableData.GemTest, transform.position, Quaternion.identity);

        if (tm == null || tickable == null)
            Destroy(this.gameObject);

        tm.Unregister(tickable);
        Destroy(this.gameObject);

        return true;
    }

}