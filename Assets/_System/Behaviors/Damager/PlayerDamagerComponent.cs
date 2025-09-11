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

        if (this.TryGetComponentInChildren<SkillFeedbackComponent>(out var feedback))
        {
            feedback.PlayFeedback();
        }
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
        var colliders = Physics.OverlapSphere(transform.position, _range, Data.TargetLayer);

        if (colliders.Length <= 0)
            return;

        foreach (var col in colliders)
        {
            if (!col.TryGetComponent<EnemyDamageableComponent>(out var enemy))
                continue;

            enemy.TakeDamage(this, Data.Damage);
        }
    }

    #endregion


    #region Debug

    private void OnDrawGizmos()
    {
        Gizmos.color = Data.DebugColor;
        Gizmos.DrawWireSphere(transform.position, _range);
    }

    #endregion
}
