using System;
using UnityEngine;

/// <summary>
/// Represents the player in the scene. This component is responsible for binding all the systems available to this entity and managing events from the different systems.
/// </summary>
public class PlayerComponent : MonoBehaviour, IDisposable
{

    #region Fields

    [SerializeField]
    private PlayerControllerComponent _controller = null;

    [SerializeField]
    private PlayerMovementComponent _movement = null;

    [SerializeField]
    private PlayerDamageableComponent _damageable = null;

    [SerializeField]
    private PlayerDamagerComponent _damager = null;

    [SerializeField]
    private LevelComponent _leveller = null;

    [SerializeField]
    private UpgradesComponent _upgrader = null;

    [SerializeField]
    private ExpGemCollectorComponent _gemCollector = null;

    private bool _disposed = false;

    #endregion


    #region Lifecycle

    private void Awake()
    {
        if (_controller == null)
            _controller = GetComponent<PlayerControllerComponent>();

        if (_movement == null)
            _movement = GetComponent<PlayerMovementComponent>();

        if (_damageable == null)
            _damageable = GetComponent<PlayerDamageableComponent>();

        if (_damager == null)
            _damager = GetComponent<PlayerDamagerComponent>();

        if (_leveller == null)
            _leveller = GetComponent<LevelComponent>();

        if (_gemCollector == null)
            _gemCollector = GetComponent<ExpGemCollectorComponent>();

        if (_upgrader == null)
            _upgrader = GetComponent<UpgradesComponent>();
    }

    private void OnEnable()
    {
        SubscribeServices();
    }
    private void OnDisable()
    {
        UnsubscribeServices();
    }

    private void OnDestroy()
    {
        Dispose();
    }

    #endregion


    #region Public API

    public void Dispose()
    {
        if (!_disposed)
        {
            UnsubscribeServices();
            _disposed = true;
        }
    }

    #endregion

    #region Private API

    private void SubscribeServices()
    {
        // Health
        _damageable.OnHealthChange += HandleHealthChange;
        _damageable.OnDeath += HandleDeath;

        // Collectors
        _gemCollector.OnCollectItem += HandleCollectGem;
        _gemCollector.OnItemCollected += HandleGemCollected;

        // Level
        _leveller.OnLevelChange += HandleLevelChanged;
        _leveller.OnGainExp += HandleGainExp;

        // Upgrade
        _upgrader.OnUpgradeSelected += HandleUpgradeSelected;
        _upgrader.OnUpgradesPresented += HandleUpgradesPresented;
        _upgrader.OnUpgradeApplied += HandleUpgradeApplied;
    }

    private void UnsubscribeServices()
    {
        // Health
        _damageable.OnHealthChange -= HandleHealthChange;
        _damageable.OnDeath -= HandleDeath;

        // Collectors
        _gemCollector.OnCollectItem -= HandleCollectGem;
        _gemCollector.OnItemCollected -= HandleGemCollected;

        // Level
        _leveller.OnLevelChange -= HandleLevelChanged;
        _leveller.OnGainExp -= HandleGainExp;

        // Upgrade
        _upgrader.OnUpgradeSelected -= HandleUpgradeSelected;
        _upgrader.OnUpgradesPresented -= HandleUpgradesPresented;
        _upgrader.OnUpgradeApplied -= HandleUpgradeApplied;
    }

    private void HandleHealthChange(float previousHealth, float previousMaxHealth, BaseDamageableComponent damageable)
    {
    }

    private void HandleDeath(BaseDamageableComponent damageable)
    {
    }

    private void HandleGainExp(float gain, float exp)
    {
    }

    private void HandleLevelChanged(int level)
    {
        // Stop player update
        // Show upgrades
        // Play feedbacks

        _upgrader.PresentUpgradeChoices();

        //@todo remove
        _gemCollector.TriggerCollider.radius *= 1.2f;

        // @todo use tick system 
        Time.timeScale = 0;
    }

    /// <inheritdoc cref="CollectorComponent{T}.Collect(T)"/>
    private void HandleCollectGem(ExpGemComponent collectible)
    {
    }

    private void HandleGemCollected(ExpGemComponent collectible)
    {
    }

    /// <summary>
    /// Called when upgrades are presetned.
    /// </summary>
    /// <param name="upgrades"></param>
    private void HandleUpgradesPresented(UpgradeSO[] upgrades)
    {
    }

    /// <summary>
    /// Called when an upgrade has been selected.
    /// </summary>
    /// <param name="upgrade"></param>
    private void HandleUpgradeSelected(UpgradeSO upgrade)
    {
        switch (upgrade)
        {
            case UpgradeSO_IncreaseMoveSpeed:
                _upgrader.AppyUpgrade(upgrade, _movement, _leveller);
                break;

            case UpgradeSO_IncreaseAttackRange:
                _upgrader.AppyUpgrade(upgrade, _damager, _leveller);
                break;

            default:
                break;
        }
    }

    /// <summary>
    /// Called when an upgrade has been applied.
    /// </summary>
    /// <param name="upgrade"></param>
    /// <param name="success"></param>
    private void HandleUpgradeApplied(UpgradeSO upgrade, bool success)
    {
        //@todo remove this shit
        Time.timeScale = 1;
    }

    #endregion

}
