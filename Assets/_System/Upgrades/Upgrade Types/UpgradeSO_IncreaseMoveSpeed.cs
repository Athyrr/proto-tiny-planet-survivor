using UnityEngine;

/// <summary>
/// 
/// </summary>
[CreateAssetMenu(fileName = "UpgradeSO_MoveSpeed", menuName = Constants.CreateDataAssetMenu + "/Upgrades/Move Speed")]
public class UpgradeSO_IncreaseMoveSpeed : UpgradeSO
{

    #region Fields

    [SerializeField]
    private UpgradeValueStrategy _upgradeStrategy = UpgradeValueStrategy.Multiply;

    [SerializeField]
    private float _amount = 1f;

    #endregion


    #region Public API

    ///<inheritdoc cref="UpgradeSO.Apply(IUpgradableBehavior[])"/>
    public override bool Apply(params IUpgradableBehavior[] upgradables)
    {
        if (upgradables == null || upgradables.Length == 0)
            return false;

        bool applied = false;

        for (int i = 0; i < upgradables.Length; i++)
        {
            var upgradable = upgradables[i];
            if (upgradable == null)
                continue;

            if (!TryCastUpgradableHas<PlayerMovementComponent>(upgradable, out var mover))
                continue;

            float value = mover.Speed;
            float calculated = CalcutalteUpgradeValue(value, _amount, _upgradeStrategy);
            mover.SetMoveSpeed(calculated);

            applied = true;
        }

        return applied;
    }

    #endregion

}


