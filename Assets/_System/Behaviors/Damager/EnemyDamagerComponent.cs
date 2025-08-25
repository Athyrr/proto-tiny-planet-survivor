using UnityEngine;

/// <summary>
/// 
/// </summary>
public class EnemyDamagerComponent : BaseDamagerComponent
{
    #region Fields



    #endregion


    #region Lifecycle

    protected override bool Init()
    {
        base.Init();

        Debug.Log("Enemy Damager Init" + Data.Damage);
        return true;
    }

    #endregion


    #region Public API

    public override bool Attack(float damage)
    {
        Debug.Log("Enemy Attack");
        return true;
    }

    #endregion
}
