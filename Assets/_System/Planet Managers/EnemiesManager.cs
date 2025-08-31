using System.Collections.Generic;
using UnityEngine;

public class EnemiesManager : MonoBehaviour
{
    #region Delegates

    public delegate void EnemyKilledDelegate(EnemyControllerComponent enemy);
    public event EnemyKilledDelegate OnEnemyKilled;

    #endregion


    #region Fields

    private PlanetData _planetData = null;

    private List<EnemyControllerComponent> _enemies = new List<EnemyControllerComponent>();

    #endregion


    #region Lifecycle



    #endregion


    #region Public API

    public void Init(PlanetData data)
    {
        _planetData = data;
    }

    #endregion
}
