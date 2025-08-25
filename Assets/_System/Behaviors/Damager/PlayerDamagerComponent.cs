using System.Runtime.CompilerServices;
using UnityEngine;

public class PlayerDamagerComponent : BaseDamagerComponent
{
    #region Fields
    #endregion


    #region Lifecycle

    protected override bool Init()
    {
        base.Init();

        Debug.Log("Player Damager Init" + Data.Damage);
        return true;
    }

    #endregion


    #region Public API

    public override bool Attack(float damage)
    {
        Debug.Log("Player Attack");

        GetNearbyEnemies();

        return true;
    }

    #endregion

    #region Private API

    private void GetNearbyEnemies()
    {
        var colliders = Physics.OverlapSphere(transform.position, Data.AttackRadius, Data.TargetLayer);

        foreach (var c in colliders)
        {
            c.GetComponent<EnemyDamageableComponent>().TakeDamage(this, Data.Damage);
        }

    }

    #endregion


    #region Debug

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, Data.AttackRadius);
    }

    #endregion
}
