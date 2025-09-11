using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeSO_CollectRange", menuName = Constants.CreateDataAssetMenu + "/Upgrades/Collect Range")]
public class UpgradeSO_IncreaseCollectRange : UpgradeSO
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

            if (!TryCastUpgradableHas<ExpGemCollectorComponent>(upgradable, out var collector))
                continue;

            float value = collector.CollectRange;
            float calculated = CalcutalteUpgradeValue(value, _amount, _upgradeStrategy);
            collector.SetCollectRange(calculated);

            applied = true;
        }

        return applied;
    }

    #endregion

}
