using UnityEngine;

/// <summary>
/// Handles the planet arena state, progression, victory/defeat conditions and manages waves and spawning.
/// </summary>
public class PlanetArenaManager : MonoBehaviour
{
    #region Delegates

    private delegate void PlanetTimerStartDelegate();
    private event PlanetTimerStartDelegate OnPlanetTimerStart;

    private delegate void PlanetTimerUpdateDelegate(float delta, float time);
    private event PlanetTimerUpdateDelegate OnPlanetTimerUpdate;

    private delegate void PlanetTimerStopDelegate();
    private event PlanetTimerStopDelegate OnPlanetTimerStop;

    private delegate void PlanetTimerResetDelegate();
    private event PlanetTimerResetDelegate OnPlanetTimerReset;

    #endregion


    #region Fields

    [SerializeField]
    private PlayerControllerComponent _player;

    [SerializeField]
    private PlanetComponent _planet;

    ///<inheritdoc cref="PlanetData"/>
    [SerializeField]
    private PlanetData _planetData;

    /// <inheritdoc cref="WaveManager"/>
    [SerializeField]
    private WaveManager _waveManager;

    ///<inheritdoc cref="EnemiesSpawnManager"/>
    [SerializeField]
    private EnemiesSpawnManager _enemiesManager;

    private float _timer;
    private bool _isTimerRunning;

    #endregion


    #region Lifecycle

    private void Awake()
    {
        _waveManager = FindFirstObjectByType<WaveManager>();
        _enemiesManager = FindFirstObjectByType<EnemiesSpawnManager>();

        if (_waveManager == null)
            _waveManager = new WaveManager();

        if (_enemiesManager == null)
            _enemiesManager = new EnemiesSpawnManager();

        InitManagers();
    }

    private void Start()
    {
        _waveManager.StartArenaWaves();
    }

    private void Update()
    {
        float delta = Time.deltaTime;

        if (_waveManager.IsRunningWave)
            _waveManager.UpdateWave(delta);
    }

    private void OnEnable()
    {
        if (_waveManager != null)
            _waveManager.OnWaveStart += HandleNextWave;
    }

    private void OnDisable()
    {
        if (_waveManager != null)
            _waveManager.OnWaveStart -= HandleNextWave;
    }

    #endregion


    #region Public API

    public void SetPlanetData(PlanetData data)
    {
        if (_planetData == null || data == _planetData)
            return;

        _planetData = data;
    }

    public void HandleNextWave(Wave wave)
    {
        _enemiesManager.SpawnWaveEnemies(wave);
    }


    #endregion


    #region Private API

    private void InitManagers()
    {
        if (_planet == null)
            _planet = FindFirstObjectByType<PlanetComponent>();

        if (_player == null)
            _player = FindFirstObjectByType<PlayerControllerComponent>();

        _enemiesManager.Initialize(_planetData, _planet, _player);
        _waveManager.Initialize(_planetData, _planet, _player);
    }

    #endregion
}
