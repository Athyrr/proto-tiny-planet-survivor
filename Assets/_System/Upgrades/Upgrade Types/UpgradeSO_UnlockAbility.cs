using UnityEngine;

public class UpgradeSO_UnlockAbility : UpgradeSO
{

    #region Fields

    [SerializeField]
    private AbilitySO _ability = null;

    #endregion


    #region Public API
    
    public override bool Apply(params IUpgradableBehavior[] upgradables)
    {
        return false;
    }

    #endregion

}
