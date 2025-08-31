using UnityEngine;

/// <summary>
/// 
/// </summary>
[System.Serializable]
public class Wave
{
    [Header("Wave Settings")]

    /// <summary>
    /// Wave duration before next wave starts in seconds.
    /// </summary>
    public float Duration;

    /// <summary>
    /// Delay beteween spawns in seconds.
    /// </summary>
    public float DelayBetweenSpawns;

    public EnemySpawnInfo[] EnemiesToSpawn;

    public AnimationCurve SpawnRateOverTime = AnimationCurve.Linear(0, 1, 1, 1);

    [System.Serializable]
    public struct EnemySpawnInfo
    {
        public GameObject EnemyPrefab;
        public int Count;

        public SpawnType SpawnType;

        public Transform[] SpawnPoints; // (if SpawnType is PlanetSpawnPoints)
        public float MinSpawnDistanceFromPlayer; // (if SpawnType is AroundPlayer)
        public float MaxSpawnDistanceFromPlayer; // (if SpawnType is AroundPlayer)
    }

    public enum SpawnType
    {
        AroundPlayer,
        RandomOnPlanet,
        PlanetSpawnPoints
    }
}
