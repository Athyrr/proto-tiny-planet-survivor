using System.Collections;
using UnityEngine;


/// <summary>
/// Handles the sequencing and management of enemy waves in the game.
/// </summary>
public class WaveManager : MonoBehaviour, IArenaManager
{
    #region Delegates

    public delegate void WaveStartDelegate(Wave wave);
    public event WaveStartDelegate OnWaveStart;

    public delegate void WaveUpdateDelegate(int waveIndex, float delta, float time);
    public event WaveUpdateDelegate OnWaveUpdate;

    public delegate void WaveEndDelegate(int waveIndex);
    public event WaveEndDelegate OnWaveEnd;

    public delegate void AllWavesCompletedDelegate();
    public event AllWavesCompletedDelegate OnAllWavesCompleted;

    #endregion


    #region Fields

    [Header("Refs")]

    [SerializeField]
    private PlayerControllerComponent _player;

    [Header("Waves Queue")]

    [SerializeField]
    private Wave[] _waves;

    [Header("Wave Settings")]

    [SerializeField]
    private float _timeBetweenWaves = 1f;

    private PlanetData _planetData;

    private float _waveTimer = 0f;

    private int _waveIndex = 0;

    private bool _waitForNextWave = false;

    private bool _hasArenaStarted = false;

    private bool _hasArenaFinished = false;

    ///<inheritdoc cref=" IArenaManager.IsManagerIntialized"/>
    private bool _isManagerIntialized;

    #endregion


    #region Lifecycle

    #endregion


    #region Public API  

    ///<inheritdoc cref=" IArenaManager.IsManagerIntialized"/>
    public bool IsManagerIntialized => _isManagerIntialized;

    public int WaveIndex => _waveIndex;

    public Wave CurrentWave => _waves[_waveIndex];

    public int TotalWaves => _waves.Length;

    public bool IsRunningWave => _waveIndex >= 0 && _waveIndex < _waves.Length;

    public bool HasArenaStarted => _hasArenaStarted;

    public bool Initialize(PlanetData planetData, PlanetComponent _, PlayerControllerComponent player)
    {
        _player = player;
        _planetData = planetData;

        _waveTimer = 0f;
        _waveTimer = 0f;
        _waitForNextWave = false;

        _isManagerIntialized =  _player != null;
        return _isManagerIntialized;
    }

    public void StartArenaWaves()
    {
        if (!IsManagerIntialized)
        {
            Debug.LogError($"{nameof(WaveManager)}  is not initialized.");
            return;
        }

        NextWave();
    }

    public void NextWave()
    {
        if (_hasArenaFinished)
            return;

        if (!_hasArenaStarted)
        {
            StartWave(0);
            _hasArenaStarted = true;
            _hasArenaFinished = false;
            Debug.Log("First Wave");

            return;
        }

        Debug.Log("Next Wave");
        StartWave(_waveIndex);
    }

    public void UpdateWave(float delta)
    {
        if (_waveIndex >= _waves.Length)
        {
            AllWavesCompleted();
            return;
        }

        Wave wave = _waves[_waveIndex];
        _waveTimer += delta;
        OnWaveUpdate?.Invoke(_waveIndex, delta, _waveTimer);

        //Debug.Log($"Wave {_waveIndex + 1} / {_waves.Length} - Time: {_waveTimer} / {wave.Duration}");

        if (_waveTimer >= wave.Duration)
        {
            OnWaveEnd?.Invoke(_waveIndex);

            _waveIndex++;
            if (_waveIndex >= _waves.Length)
            {
                AllWavesCompleted();
                return;
            }
            else
            {
                StartCoroutine(WaitForWaveCoroutine(_waveIndex));
                OnWaveStart?.Invoke(CurrentWave);
            }
        }
    }

    public void AllWavesCompleted()
    {
        Debug.Log("All waves completed!");
        OnAllWavesCompleted?.Invoke();
        _hasArenaFinished = true;
    }


    #endregion


    #region Private API

    private bool StartWave(Wave wave)
    {
        if (wave == null)
            return false;

        _waveTimer = 0;

        Debug.Log("Starting wave " + (_waveIndex + 1) + " / " + _waves.Length);
        OnWaveStart?.Invoke(CurrentWave);

        return true;
    }

    private bool StartWave(int waveIndex)
    {
        if (waveIndex < 0 || waveIndex >= _waves.Length)
        {
            Debug.LogError($"Wave index {waveIndex} is out of range.");
            return false;
        }

        Wave wave = _waves[waveIndex];
        if (wave == null)
            return false;

        return StartWave(wave);
    }

    private IEnumerator WaitForWaveCoroutine(int waveIndex)
    {
        float timer = 0f;
        Debug.Log("Waiting for next wave...");
        while (timer < _timeBetweenWaves)
        {
            timer += Time.deltaTime;
            yield return null;
        }
        StartWave(_waveIndex);

    }

    #endregion
}
