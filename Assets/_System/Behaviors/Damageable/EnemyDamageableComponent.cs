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

        Debug.Log($"Enemy took {amount} damage from {source.name}. Health: {_health}");

        //OnHealthChange.Invoke(_health + amount, _data.MaxHealth, this);

        if (_health <= 0)
        {
            Destroy(this.gameObject);
            Debug.Log("Enemy destroyed");
        }
        return true;
    }

    private void OnDestroy()
    {
        FindAnyObjectByType<TickManager>().Unregister(this.GetComponent<EnemyControllerComponent>());
    }
}