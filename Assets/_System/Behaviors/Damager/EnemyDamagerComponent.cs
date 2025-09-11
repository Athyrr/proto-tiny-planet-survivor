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
        return true;
    }

    #endregion


    #region Public API

    public override bool Attack(float damage)
    {
        return true;
    }

    public override bool CanAttack(float damage)
    {
        return true;
    }

    #endregion
}
