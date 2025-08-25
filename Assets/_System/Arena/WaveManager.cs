using UnityEngine;

public class WaveManager : MonoBehaviour
{

    private int _waveCount = 0;
    private int _waveSize = 5;

    [SerializeField]
    private int _enemiesToSpawn = 0;

    [SerializeField]
    private EnemyControllerComponent _enemtPF;

    [SerializeField]
    private PlayerControllerComponent _player;



    private void Start()
    {
        //for (int i = 0; i < _enemiesToSpawn; i++)
        //{
        //    SpawnEnemy();
        //}
    }



    private void SpawnEnemy()
    {
        Vector3 spawnPos = _player.transform.position + Random.insideUnitSphere * 5f;

        Vector3 pos = new Vector3(spawnPos.x, 0f, spawnPos.z);
        var enemy = GameObject.Instantiate(_enemtPF, pos, Quaternion.identity);
        enemy.SetTarget(_player.transform);

    }

}
