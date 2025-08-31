using System;
using Unity.VisualScripting;
using UnityEngine;

/// <summary>
/// Handles the planet arena state, progression, victory/defeat conditions and manages waves and spawning.
/// </summary>
public class PlanetArenaManager : MonoBehaviour
{
    #region Delegates

    private delegate void PlanetTimerStartDelegate();

    private delegate void PlanetTimerUpdateDelegate(float delta, float time);

    private delegate void PlanetTimerStopDelegate();

    private delegate void PlanetTimerResetDelegate();

    #endregion


    #region Fields

    private event PlanetTimerStartDelegate OnPlanetTimerStart;

    private event PlanetTimerUpdateDelegate OnPlanetTimerUpdate;
    
    private event PlanetTimerStopDelegate OnPlanetTimerStop;
    
    private event PlanetTimerResetDelegate OnPlanetTimerReset;

    private PlanetData _planetData;

    private float _timer;
    private bool _isTimerRunning;

    // Handle planete states, progression, victory/defeat check
    // Planet Data (name, size, type, etc)

    // Wave manager 

    private WaveManager _waveManager;

    private EnemiesManager _enemiesManager;

    // Enemy Manager 
    // Spawner Manager

    #endregion


    #region Lifecycle

    private void Awake()
    {
        _waveManager = FindFirstObjectByType<WaveManager>();

        _enemiesManager = FindFirstObjectByType<EnemiesManager>();

        if (_waveManager == null)
            _waveManager = new WaveManager();

        if (_enemiesManager == null)
            _enemiesManager = new EnemiesManager();

        InitManagers();
    }

    #endregion


    #region Public API
    
    public void SetPlanetData(PlanetData data)
    {
        if (_planetData == null || data == _planetData)
            return;

        _planetData = data;
    }

    private void InitPlanet()
    {

    }



    #endregion


    #region Private API

    private void InitManagers()
    {
        _enemiesManager.Init(_planetData);
        _waveManager.Init(_planetData);
    }

    #endregion
}
