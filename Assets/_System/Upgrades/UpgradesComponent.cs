using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Represents the ability select and apply 
/// </summary>
public class UpgradesComponent : MonoBehaviour
{
    // Upgrade selection
    // Dispatch events > Select upgrade
    // Calculate/Store upgrades availables depending on player level,previous upgrades, planet and/or waves state.

    #region Delegates

    public delegate void UpgradesPresentedDelegate(UpgradeSO[] upgrades);
    public event UpgradesPresentedDelegate OnUpgradesPresented;

    public delegate void UpgradeSelectionDelegate(UpgradeSO upgrade);
    public event UpgradeSelectionDelegate OnUpgradeSelected;

    public delegate void UpgradeAppliedDelegate(UpgradeSO upgrade, bool success);
    public event UpgradeAppliedDelegate OnUpgradeApplied;

    #endregion


    #region Fields

    [Header("Upgrade Database")]

    [Tooltip("@todo use a scriptable object as upgrades database instead of a list")]
    [SerializeField]
    private UpgradeSO[] _upgradeDatabase = null;

    [Header("Configuration")]

    [SerializeField]
    private int _upgradeChoicesCount = 3;

    [Header("Debug")]

    [SerializeField]
    private bool _drawDebug = false;


    private UpgradeSO _selectedUpgrade = null;

    #endregion


    #region Public API

    /// <summary>
    /// Presents differents upgrades to the player when it levels up.
    /// </summary>
    /// <returns></returns>
    public UpgradeSO[] PresentUpgradeChoices()
    {
        if (_upgradeDatabase == null || _upgradeDatabase.Length <= 0)
        {
            Debug.LogWarning("Upgrade database is empty or null!");
            return null;
        }

        int choiceCount = Mathf.Min(_upgradeDatabase.Length, _upgradeChoicesCount);
        UpgradeSO[] upgrades = new UpgradeSO[choiceCount];

        // Upgrade selection logic 
        //@todo improve that shit
        for (int i = 0; i < upgrades.Length; i++)
        {
            int randomIndex = Random.Range(0, _upgradeDatabase.Length);
            upgrades[i] = _upgradeDatabase[randomIndex];
        }

        OnUpgradesPresented?.Invoke(upgrades);
        return upgrades;
    }

    /// <summary>
    /// Selects one of the presented upgrades.
    /// </summary>
    /// <param name="upgrade"></param>
    /// <returns></returns>
    public bool SelectUpgrade(UpgradeSO upgrade)
    {
        if (upgrade == null)
            return false;

        _selectedUpgrade = upgrade;

        OnUpgradeSelected?.Invoke(upgrade);
        return true;
    }

    public bool AppyUpgrade(UpgradeSO upgrade, params IUpgradableBehavior[] upgradables)
    {
        if (upgrade == null)
        {
            Debug.LogWarning($"Failed to apply upgrade {upgrade.DisplayName}! No upgrade has  been selected!");
            return false;
        }

        var success = upgrade.Apply(upgradables);

        OnUpgradeApplied?.Invoke(upgrade, success);

        _selectedUpgrade = null;

        return success;
    }

    public bool AppyUpgrade(params IUpgradableBehavior[] upgradables)
    {
        return AppyUpgrade(_selectedUpgrade, upgradables);
    }

    public string ShowUpgradeDetails(UpgradeSO upgrade)
    {
        //@todo return a struct UpgradeDetail
        return string.Empty;
    }
    public string ShowUpgradeDetails()
    {
        return ShowUpgradeDetails(_selectedUpgrade);
    }


    #endregion

    #region Private API
    #endregion
}
