using System;
using System.Runtime.CompilerServices;
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
        // Collectors
        _gemCollector.OnCollectItem += HandleCollectGem;
        _gemCollector.OnItemCollected += OnGemCollected;


        // Level
        _leveller.OnLevelChange += HandleLevelChanged;
        _leveller.OnGainExp += HandleGainExp;

        // Health
        _damageable.OnHealthChange += HandleHealthChange;
        _damageable.OnDeath += HandleDeath;
    }

    private void UnsubscribeServices()
    {
        // Collectors
        _gemCollector.OnCollectItem -= HandleCollectGem;
        _gemCollector.OnItemCollected -= OnGemCollected;


        // Level
        _leveller.OnLevelChange -= HandleLevelChanged;
        _leveller.OnGainExp -= HandleGainExp;

        // Health
        _damageable.OnHealthChange -= HandleHealthChange;
        _damageable.OnDeath -= HandleDeath;
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
        _gemCollector.TriggerCollider.radius *= 1.2f;
    }

    /// <inheritdoc cref="CollectorComponent{T}.Collect(T)"/>
    private void HandleCollectGem(ExpGemComponent collectible)
    {
    }

    private void OnGemCollected(ExpGemComponent collectible)
    {
    }

    #endregion

}
