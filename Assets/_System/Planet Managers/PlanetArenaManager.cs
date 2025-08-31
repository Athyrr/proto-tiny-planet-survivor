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

    private PlanetData _planetData;

    private float _timer;
    private bool _isTimerRunning;

    // Wave manager 

    private WaveManager _waveManager;

    private EnemiesSpawnManager _enemiesManager;

    private Transform _player;

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
        _waveManager.LaunchWaves();
    }
    private void Update()
    {
        float delta = Time.deltaTime;
        if (_waveManager.IsAWaveRunning)
            _waveManager.UpdateWave(delta);
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


    #region Private API

    private void InitManagers()
    {
        var planet = FindFirstObjectByType<PlanetComponent>();
        var player = FindFirstObjectByType<PlayerControllerComponent>();

        _enemiesManager.Init(_planetData, player?.transform, planet);
        _waveManager.Init(_planetData);
    }

    #endregion
}
