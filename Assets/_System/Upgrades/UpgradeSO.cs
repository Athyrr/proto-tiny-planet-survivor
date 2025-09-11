using UnityEngine;

/// <summary>
/// 
/// </summary>
public abstract class UpgradeSO : ScriptableObject
{

    #region Fields

    [SerializeField]
    private string _name = string.Empty;

    [SerializeField]
    [TextArea(2, 4)]
    private string _description = string.Empty;

    [SerializeField]
    private Sprite _icon = null;

    #endregion


    #region Public API

    public string DisplayName => _name;
    public string Description => _description;
    public Sprite Icon => _icon;

    /// <summary>
    /// Applies the upgrade to the corresponding component.
    /// </summary>
    /// <param name="upgradables">The services/component to use for upgrading.</param>
    /// <returns></returns>
    public abstract bool Apply(params IUpgradableBehavior[] upgradables);

    #endregion

    #region Protected API

    /// <summary>
    /// Try to retrieve the type of an <see cref="IUpgradableBehavior"/>.
    /// </summary>
    /// <typeparam name="T">The type of the cast.</typeparam>
    /// <param name="upgradable"></param>
    /// <param name="casted">The casted type.</param>
    /// <returns></returns>
    protected bool TryCastUpgradableHas<T>(IUpgradableBehavior upgradable, out T casted) where T : IUpgradableBehavior
    {
        casted = default;

        if (upgradable == null)
            return default;

        if (upgradable is T type)
        {
            casted = type;
            return true;
        }

        return default;
    }

    /// <summary>
    /// Calculates the new value based on <see cref="UpgradeValueStrategy"/>.
    /// </summary>
    /// <param name="value"></param>
    /// <returns></returns>
    protected float CalcutalteUpgradeValue(float value, float amount, UpgradeValueStrategy strategy)
    {
        switch (strategy)
        {
            case UpgradeValueStrategy.Multiply:
                value *= amount;
                break;
            case UpgradeValueStrategy.Flat:
                value += amount;
                break;
            default:
                break;
        }
        return value;
    }

    #endregion

}

/// <summary>
/// The calculation strategy for the value.
/// </summary>
public enum UpgradeValueStrategy
{
    // Muilitply the value
    Multiply,

    // Add the value
    Flat
}
