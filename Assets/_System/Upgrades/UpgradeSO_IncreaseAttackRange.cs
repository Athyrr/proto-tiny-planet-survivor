using UnityEngine;

[CreateAssetMenu(fileName = "UpgradeSO_AttackRange", menuName = Constants.CreateDataAssetMenu + "/Upgrades/Attack Range")]
public class UpgradeSO_IncreaseAttackRange : UpgradeSO
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

            if (!TryCastUpgradableHas<PlayerDamagerComponent>(upgradable, out var damager))
                continue;

            float value = damager.Range;
            float calculated = CalcutalteUpgradeValue(value, _amount, _upgradeStrategy);
            damager.SetRange(calculated);

            //@todo each feedback listen on Upgrade delegate and update its renderer.
            SkillFeedbackComponent fb = FindFirstObjectByType<SkillFeedbackComponent>();
            fb.SetAttackRange(calculated);

            applied = true;
        }

        return applied;
    }

    #endregion

}
