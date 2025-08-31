using System.Runtime.CompilerServices;
using Unity.VisualScripting;
using UnityEngine;

public class PlayerDamagerComponent : BaseDamagerComponent
{
    #region Fields

    public bool AllowAttack;

    #endregion


    #region Lifecycle

    protected override bool Init()
    {
        base.Init();
        return true;
    }

    #endregion


    #region Public API

    public override bool Attack(float damage)
    {
        if (!AllowAttack)
            return false;
        GetNearbyEnemies();

        return true;
    }

    public override bool CanAttack(float damage)
    {
        return AllowAttack;
    }

    #endregion


    #region Private API

    private void GetNearbyEnemies()
    {
        var colliders = Physics.OverlapSphere(transform.position, Data.AttackRadius, Data.TargetLayer);

        if (colliders.Length <= 0)
            return;

        colliders[0].GetComponent<EnemyDamageableComponent>().TakeDamage(this, Data.Damage);

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
