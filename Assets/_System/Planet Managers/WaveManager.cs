using System.Collections;
using UnityEditor.ShaderGraph.Internal;
using UnityEngine;

public class WaveManager : MonoBehaviour
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

    #endregion


    #region Lifecycle

    private void Awake()
    {
        if (_player == null)
            Debug.LogError($"{typeof(PlayerControllerComponent)} reference is missing.");
    }


    #endregion


    #region Public API  

    public int WaveIndex => _waveIndex;
    public Wave CurrentWave => _waves[_waveIndex];

    public int TotalWaves => _waves.Length;

    public bool IsAWaveRunning => _waveIndex >= 0 && _waveIndex < _waves.Length;

    public void Init(PlanetData data)
    {
        _planetData = data;
        _waveTimer = 0f;
        _waveTimer = 0f;
        _waitForNextWave = false;
    }

    public void NextWave()
    {
        Debug.Log("Next Wave");
        SetupWave(_waveIndex);
    }

    public void LaunchWaves()
    {
        SetupWave(0);
    }

    public bool SetupWave(int waveIndex)
    {
        if (waveIndex < 0 || waveIndex >= _waves.Length)
        {
            Debug.LogError($"Wave index {waveIndex} is out of range.");
            return false;
        }

        Wave wave = _waves[waveIndex];
        if (wave == null)
            return false;

        return SetupWave(wave);
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
        SetupWave(_waveIndex);

    }

    #endregion


    #region Private API

    private bool SetupWave(Wave wave)
    {
        if (wave == null)
            return false;

        _waveTimer = 0;

        OnWaveStart?.Invoke(CurrentWave);

        Debug.Log("Starting wave " + (_waveIndex + 1) + " / " + _waves.Length);

        return true;
    }

    #endregion
}
