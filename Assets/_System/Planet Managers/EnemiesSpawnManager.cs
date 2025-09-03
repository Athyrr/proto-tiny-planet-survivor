using System.Collections.Generic;
using System.Collections;
using UnityEngine;

/// <summary>
/// Handles spawning of enemies on the planet. Used by <see cref="PlanetArenaManager"/>.
/// </summary>
public class EnemiesSpawnManager : MonoBehaviour, IArenaManager
{
    #region Delegates

    public delegate void EnemyKilledDelegate(EnemyControllerComponent enemy);
    public event EnemyKilledDelegate OnEnemyKilled;

    #endregion


    #region Fields

    [SerializeField]
    [Min(1)]
    private int _maxEnemies = 10;

    private PlayerControllerComponent _player = null;
    private PlanetData _planetData = null;
    private PlanetComponent _planet;

    private List<EnemyControllerComponent> _enemies = new List<EnemyControllerComponent>();

    private Coroutine _spawnCoroutine = null;

    private bool _isManagerIntialized;

    public bool IsManagerIntialized => _isManagerIntialized;

    #endregion


    #region Public API

    public bool Initialize(PlanetData planetData, PlanetComponent planet, PlayerControllerComponent player)
    {
        _planetData = planetData;
        _player = player;
        _planet = planet;

        _isManagerIntialized =  _player != null && _planet != null;
        return _isManagerIntialized;
    }

    public void SpawnWaveEnemies(Wave wave)
    {
        if (!_isManagerIntialized)
        {
            Debug.LogError($"{nameof(EnemiesSpawnManager)}  is not initialized.");
            return;
        }

        if (wave == null || wave.EnemiesToSpawn == null || wave.EnemiesToSpawn.Length <= 0)
        {
            Debug.LogWarning("Wave data is invalid or empty.");
            return;
        }

        _spawnCoroutine = StartCoroutine(SpawnWaveEnemiesCoroutine(wave));
    }

    private IEnumerator SpawnWaveEnemiesCoroutine(Wave wave)
    {
        if (wave == null)
            Debug.LogWarning("Wave nulllllllllllllllllll");

        foreach (var spawnInfo in wave.EnemiesToSpawn)
        {
            for (int i = 0; i < spawnInfo.Count; i++)
            {
                if (!CanSpawn())
                    yield break;

                Vector3 spawnPosition = GetSpawnPosition(spawnInfo);
                SpawnEnemy(spawnInfo.EnemyPrefab, spawnPosition);

                yield return new WaitForSeconds(wave.DelayBetweenSpawns);
            }
        }
    }

    #endregion


    #region Private API

    private Vector3 GetSpawnPosition(Wave.EnemySpawnInfo spawnInfo)
    {
        Vector3 spawnPosition = Vector3.zero;

        switch (spawnInfo.SpawnType)
        {
            case Wave.SpawnType.AroundPlayer:
                {
                    spawnPosition = GetRandomPositionAroundPlayer(spawnInfo);
                    break;
                }
            case Wave.SpawnType.RandomOnPlanet:
                {
                    spawnPosition = GetRandomPositionOnPlanet();
                    break;
                }
            case Wave.SpawnType.PlanetSpawnPoints:
                {
                    spawnPosition = GetSpawnPointPosition(spawnInfo);
                    break;
                }
        }

        return _planet.GetSnappedPosition(spawnPosition);
    }

    private Vector3 GetRandomPositionAroundPlayer(Wave.EnemySpawnInfo spawnInfo)
    {

        if (_planet == null || _player == null)
        {
            Debug.LogError("Planet or Player reference is null!");
            return Vector3.zero;
        }

        float distance = Random.Range(spawnInfo.MinSpawnDistanceFromPlayer, spawnInfo.MaxSpawnDistanceFromPlayer);
        Vector3 direction = Random.onUnitSphere.normalized;
        return _planet.GetSurfaceStep(_player.transform.position, _player.transform.position + direction, distance);
    }

    // Random position on planet
    private Vector3 GetRandomPositionOnPlanet()
    {
        Vector2 randomPoint = Random.insideUnitCircle.normalized * _planet.Radius;
        return _planet.transform.position + new Vector3(randomPoint.x, 0, randomPoint.y);
    }

    private Vector3 GetSpawnPointPosition(Wave.EnemySpawnInfo spawnInfo)
    {
        if (spawnInfo.SpawnPoints == null || spawnInfo.SpawnPoints.Length <= 0)
        {
            Debug.LogWarning("No spawn points defined for PlanetSpawnPoints spawn type.");
            return GetRandomPositionOnPlanet();
        }
        int index = Random.Range(0, spawnInfo.SpawnPoints.Length);
        return spawnInfo.SpawnPoints[index].position;
    }

    private void SpawnEnemy(GameObject enemy, Vector3 position)
    {
        if (enemy == null)
            Debug.LogWarning("Nulllllllllll");

        var instance = Instantiate(enemy, position, Quaternion.identity); // @todo use Pooling instead

        var instanceComponent = instance.GetComponent<EnemyControllerComponent>();
        instanceComponent.Setup(_planet, _player.transform);


        _enemies.Add(instanceComponent);
        //Debug.Log($"Spawned enemy at {position}");
    }

    private bool CanSpawn()
    {
        return _enemies.Count < _maxEnemies;
    }

    #endregion
}
